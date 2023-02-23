/*
 * Copyright 2013 S. Webber
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using Org.NProlog.Core.Predicate.Udp;
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Predicate.Builtin.Compound;


/* TEST
%TRUE true, true
%FAIL true, fail
%FAIL fail, true
%FAIL fail, fail

%TRUE true, true, true
%FAIL true, fail, fail
%FAIL fail, true, fail
%FAIL fail, fail, true
%FAIL true, true, fail
%FAIL true, fail, true
%FAIL fail, true, true
%FAIL fail, fail, fail

b :- true.
c :- true.
d :- true.
y :- true.
a :- b,c,d.
x :- y,z.
%TRUE a
%FAIL x

p2(1) :- true.
p2(2) :- true.
p2(3) :- true.

p3(a) :- true.
p3(b) :- true.
p3(c) :- true.

p4(1, b, [a,b,c]) :- true.
p4(3, c, [1,2,3]) :- true.
p4(X, Y, [q,w,e,r,t,y]) :- true.

p1(X, Y, Z) :- p2(X), p3(Y), p4(X,Y,Z).

%?- p1(X, Y, Z)
% X=1
% Y=a
% Z=[q,w,e,r,t,y]
% X=1
% Y=b
% Z=[a,b,c]
% X=1
% Y=b
% Z=[q,w,e,r,t,y]
% X=1
% Y=c
% Z=[q,w,e,r,t,y]
% X=2
% Y=a
% Z=[q,w,e,r,t,y]
% X=2
% Y=b
% Z=[q,w,e,r,t,y]
% X=2
% Y=c
% Z=[q,w,e,r,t,y]
% X=3
% Y=a
% Z=[q,w,e,r,t,y]
% X=3
% Y=b
% Z=[q,w,e,r,t,y]
% X=3
% Y=c
% Z=[1,2,3]
% X=3
% Y=c
% Z=[q,w,e,r,t,y]

%?- p2(X), p2(X), p2(X)
% X=1
% X=2
% X=3

%FAIL p2(X), p3(X), p2(X)
*/
/**
 * <code>X,Y</code> - conjunction.
 * <p>
 * <code>X,Y</code> specifies a conjunction of goals. <code>X,Y</code> succeeds if <code>X</code> succeeds <i>and</i>
 * <code>Y</code> succeeds. If <code>X</code> succeeds and <code>Y</code> fails then an attempt is made to re-satisfy
 * <code>X</code>. If <code>X</code> fails the entire conjunction fails.
 * </p>
 */
public class Conjunction : AbstractPredicateFactory, PreprocessablePredicateFactory
{

    protected override Predicate GetPredicate(Term arg1, Term arg2)
    {
        var firstPredicate = Predicates.GetPredicateFactory(arg1).GetPredicate(arg1.Args);
        return firstPredicate.Evaluate()
          ? new ConjunctionPredicate(firstPredicate, Predicates.GetPredicateFactory(arg2), arg2)
          : PredicateUtils.FALSE;
    }

    public virtual PredicateFactory Preprocess(Term term)
    {
        var firstArg = term.GetArgument(0);
        var secondArg = term.GetArgument(1);
        if (firstArg.Type.IsVariable || secondArg.Type.IsVariable) return this;

        var firstPredicateFactory = Predicates.GetPreprocessedPredicateFactory(firstArg);
        var secondPredicateFactory = Predicates.GetPreprocessedPredicateFactory(secondArg);
        return firstPredicateFactory.IsRetryable || secondPredicateFactory.IsRetryable
            ? new OptimisedRetryableConjuction(firstPredicateFactory, secondPredicateFactory)
            : new OptimisedSingletonConjuction(firstPredicateFactory, secondPredicateFactory);
    }

    public class OptimisedRetryableConjuction : AbstractPredicateFactory
    {
        private readonly PredicateFactory firstPredicateFactory;
        private readonly PredicateFactory secondPredicateFactory;

        public OptimisedRetryableConjuction(PredicateFactory firstPredicateFactory, PredicateFactory secondPredicateFactory)
        {
            this.firstPredicateFactory = firstPredicateFactory;
            this.secondPredicateFactory = secondPredicateFactory;
        }

        protected override Predicate GetPredicate(Term arg1, Term arg2)
        {
            var firstPredicate = firstPredicateFactory.GetPredicate(arg1.Args);
            return firstPredicate.Evaluate() ? new ConjunctionPredicate(firstPredicate, secondPredicateFactory, arg2) : PredicateUtils.FALSE;
        }

        public bool IsAlwaysCutOnBacktrack
            => secondPredicateFactory.IsAlwaysCutOnBacktrack 
            || firstPredicateFactory.IsAlwaysCutOnBacktrack 
            && !secondPredicateFactory.IsRetryable;
    }

    public class OptimisedSingletonConjuction : AbstractSingleResultPredicate
    {
        private readonly PredicateFactory firstPredicateFactory;
        private readonly PredicateFactory secondPredicateFactory;

        public OptimisedSingletonConjuction(PredicateFactory firstPredicateFactory, PredicateFactory secondPredicateFactory)
        {
            this.firstPredicateFactory = firstPredicateFactory;
            this.secondPredicateFactory = secondPredicateFactory;
        }

        protected override bool Evaluate(Term arg1, Term arg2)
            => firstPredicateFactory.GetPredicate(arg1.Args).Evaluate() && secondPredicateFactory.GetPredicate(arg2.Term.Args).Evaluate();
    }

    public class ConjunctionPredicate : Predicate
    {
        private readonly Predicate firstPredicate;
        private readonly PredicateFactory secondPredicateFactory;
        private readonly Term originalSecondArgument;
        private Predicate secondPredicate;
        private Term copySecondArgument;

        public ConjunctionPredicate(Predicate firstPredicate, PredicateFactory secondPredicateFactory, Term secondArgument)
        {
            this.firstPredicate = firstPredicate;
            this.secondPredicateFactory = secondPredicateFactory;
            this.originalSecondArgument = secondArgument;
        }

        public virtual bool Evaluate()
        {
            do
            {
                if (secondPredicate == null)
                {
                    copySecondArgument = originalSecondArgument.Term;
                    secondPredicate = secondPredicateFactory.GetPredicate(copySecondArgument.Args);
                    if (secondPredicate.Evaluate()) return true;
                }
                else if (secondPredicate.CouldReevaluationSucceed && secondPredicate.Evaluate())
                {
                    return true;
                }

                secondPredicate = null;
                TermUtils.Backtrack(copySecondArgument.Args);
            } while (firstPredicate.CouldReevaluationSucceed && firstPredicate.Evaluate());

            return false;
        }

        public virtual bool CouldReevaluationSucceed => firstPredicate.CouldReevaluationSucceed
                  || (secondPredicate != null && secondPredicate.CouldReevaluationSucceed)
                  || (copySecondArgument == null && secondPredicateFactory.IsRetryable)
            ;
    }
}
