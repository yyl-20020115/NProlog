/*
 * Copyright 2012 - 2018 S. Webber
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
using Org.NProlog.Core.Predicate.Builtin.List;
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Predicate.Builtin.Compound;


/* TEST
%?- true; true
%YES
%YES
%TRUE_NO true; fail
%TRUE fail; true
%FAIL fail; fail

%?- true; true; true
%YES
%YES
%YES
%TRUE_NO true; fail; fail
%TRUE_NO fail; true; fail
%TRUE fail; fail; true
%?- true; true; fail
%YES
%YES
%NO
%?- true; fail; true
%YES
%YES
%?- fail; true; true
%YES
%YES
%FAIL fail; fail; fail

a :- true.
b :- true.
c :- true.
d :- true.
%?- a;b;c
%YES
%YES
%YES
%?- a;b;z
%YES
%YES
%NO
%?- a;y;c
%YES
%YES
%TRUE_NO a;y;z
%?- x;b;c
%YES
%YES
%TRUE_NO x;b;z
%TRUE x;y;c
%FAIL x;y;z

p2(1) :- true.
p2(2) :- true.
p2(3) :- true.

p3(a) :- true.
p3(b) :- true.
p3(c) :- true.

p4(1, b, [a,b,c]) :- true.
p4(3, c, [1,2,3]) :- true.
p4(X, Y, [q,w,e,r,t,y]) :- true.

p1(X, Y, Z) :- p2(X); p3(Y); p4(X,Y,Z).

%?- p1(X, Y, Z)
% X=1
% Y=UNINSTANTIATED VARIABLE
% Z=UNINSTANTIATED VARIABLE
% X=2
% Y=UNINSTANTIATED VARIABLE
% Z=UNINSTANTIATED VARIABLE
% X=3
% Y=UNINSTANTIATED VARIABLE
% Z=UNINSTANTIATED VARIABLE
% X=UNINSTANTIATED VARIABLE
% Y=a
% Z=UNINSTANTIATED VARIABLE
% X=UNINSTANTIATED VARIABLE
% Y=b
% Z=UNINSTANTIATED VARIABLE
% X=UNINSTANTIATED VARIABLE
% Y=c
% Z=UNINSTANTIATED VARIABLE
% X=1
% Y=b
% Z=[a,b,c]
% X=3
% Y=c
% Z=[1,2,3]
% X=UNINSTANTIATED VARIABLE
% Y=UNINSTANTIATED VARIABLE
% Z=[q,w,e,r,t,y]

%?- p2(X); p2(X); p2(X)
% X=1
% X=2
% X=3
% X=1
% X=2
% X=3
% X=1
% X=2
% X=3

%?- p2(X); p3(X); p2(X)
% X=1
% X=2
% X=3
% X=a
% X=b
% X=c
% X=1
% X=2
% X=3

%?- X=12; X=27; X=56
% X=12
% X=27
% X=56

%?- p2(X); X=12; p3(X); X=27; p2(X)
% X=1
% X=2
% X=3
% X=12
% X=a
% X=b
% X=c
% X=27
% X=1
% X=2
% X=3
*/
/**
 * <code>X;Y</code> - disjunction.
 * <p>
 * <code>X;Y</code> specifies a disjunction of goals. <code>X;Y</code> succeeds if either <code>X</code> succeeds
 * <i>or</i> <code>Y</code> succeeds. If <code>X</code> fails then an attempt is made to satisfy <code>Y</code>. If
 * <code>Y</code> fails the entire disjunction fails.
 * </p>
 * <p>
 * <b>Note:</b> The behaviour of this predicate changes when its first argument is of the form <code>-&gt;/2</code>,
 * i.e. the <i>"if/then"</i> predicate. When a <code>-&gt;/2</code> predicate is the first argument of a
 * <code>;/2</code> predicate then the resulting behaviour is a <i>"if/then/else"</i> statement of the form
 * <code>((if-&gt;then);else)</code>.
 * </p>
 *
 * @see IfThen
 */
public class Disjunction : AbstractPredicateFactory, PreprocessablePredicateFactory
{
    public virtual PredicateFactory Preprocess(Term term)
    {
        var arg1 = term.GetArgument(0);
        var arg2 = term.GetArgument(1);
        if (PartialApplicationUtils.IsAtomOrStructure(arg1) && PartialApplicationUtils.IsAtomOrStructure(arg2))
        {
            if (Predicates.GetPredicateFactory(arg1) is IfThen)
            {
                var conditionTerm = arg1.GetArgument(0);
                var thenTerm = arg1.GetArgument(1);
                var condition = Predicates.GetPreprocessedPredicateFactory(conditionTerm);
                var thenPf = Predicates.GetPreprocessedPredicateFactory(thenTerm);
                var elsePf = Predicates.GetPreprocessedPredicateFactory(arg2);
                return new OptimisedIfThenElse(condition, thenPf, elsePf);
            }
            else
            {
                var pf1 = Predicates.GetPreprocessedPredicateFactory(arg1);
                var pf2 = Predicates.GetPreprocessedPredicateFactory(arg2);
                return new OptimisedDisjunction(pf1, pf2,this);
            }
        }
        return this;
    }

    public class OptimisedDisjunction : PredicateFactory
    {
        private readonly Disjunction disjunction;
        private readonly PredicateFactory pf1;
        private readonly PredicateFactory pf2;
        public OptimisedDisjunction(PredicateFactory pf1, PredicateFactory pf2, Disjunction disjunction)
        {
            this.pf1 = pf1;
            this.pf2 = pf2;
            this.disjunction = disjunction;
        }


        public virtual Predicate GetPredicate(Term[] args) 
            => new DisjunctionPredicate(pf1, pf2, args[0], args[1],this.disjunction);

        public bool IsRetryable => true;
    }

    public class OptimisedIfThenElse : PredicateFactory
    {
        private readonly PredicateFactory condition;
        private readonly PredicateFactory thenPf;
        private readonly PredicateFactory elsePf;

        public OptimisedIfThenElse(PredicateFactory condition, PredicateFactory thenPf, PredicateFactory elsePf)
        {
            this.condition = condition;
            this.thenPf = thenPf;
            this.elsePf = elsePf;
        }

        public virtual Predicate GetPredicate(Term[] args)
        {
            var ifThenTerm = args[0];
            var conditionTerm = ifThenTerm.GetArgument(0);
            var conditionPredicate = condition.GetPredicate(conditionTerm.Args);
            if (conditionPredicate.Evaluate())
            {
                return thenPf.GetPredicate(ifThenTerm.GetArgument(1).Term.Args);
            }
            else
            {
                conditionTerm.Backtrack();
                return elsePf.GetPredicate(args[1].Args);
            }
        }

        public bool IsRetryable 
            => thenPf.IsRetryable || elsePf.IsRetryable;

        public bool IsAlwaysCutOnBacktrack
            => thenPf.IsAlwaysCutOnBacktrack && elsePf.IsAlwaysCutOnBacktrack;
    }

    protected override Predicate GetPredicate(Term firstArg, Term secondArg) 
        => Predicates?.GetPredicateFactory(firstArg) is IfThen
          ? CreateIfThenElse(firstArg, secondArg)
          : CreateDisjunction(firstArg, secondArg);

    private Predicate CreateIfThenElse(Term ifThenTerm, Term elseTerm)
    {
        var conditionTerm = ifThenTerm.GetArgument(0);
        var thenTerm = ifThenTerm.GetArgument(1);

        var p = Predicates;
        var conditionPredicate = p.GetPredicate(conditionTerm);
        if (conditionPredicate.Evaluate())
        {
            return p.GetPredicate(thenTerm.Term);
        }
        else
        {
            conditionTerm.Backtrack();
            return p.GetPredicate(elseTerm);
        }
    }

    private DisjunctionPredicate CreateDisjunction(Term firstArg, Term secondArg) 
        => new (null, null, firstArg, secondArg, this);

    public class DisjunctionPredicate : Predicate
    {
        private readonly Disjunction disjunction;
        private readonly PredicateFactory? pf1;
        private readonly PredicateFactory? pf2;
        private readonly Term inputArg1;
        private readonly Term inputArg2;
        private Predicate? firstPredicate;
        private Predicate? secondPredicate;

        public DisjunctionPredicate(PredicateFactory? pf1, PredicateFactory? pf2, Term inputArg1, Term inputArg2, Disjunction disjunction)
        {
            this.pf1 = pf1;
            this.pf2 = pf2;
            this.inputArg1 = inputArg1;
            this.inputArg2 = inputArg2;
            this.disjunction = disjunction;
        }

        public virtual bool Evaluate()
        {
            if (firstPredicate == null)
            {
                firstPredicate = GetPredicate(pf1, inputArg1);
                if (firstPredicate.Evaluate())
                    return true;
            }
            else if (secondPredicate == null && firstPredicate.CouldReevaluationSucceed && firstPredicate.Evaluate())
            {
                return true;
            }

            if (secondPredicate == null)
            {
                inputArg1.Backtrack();
                secondPredicate = GetPredicate(pf2, inputArg2);
                return secondPredicate.Evaluate();
            }
            else
            {
                return secondPredicate.CouldReevaluationSucceed && secondPredicate.Evaluate();
            }
        }

        private Predicate GetPredicate(PredicateFactory pf, Term t) 
            => pf == null ? this.disjunction.Predicates.GetPredicate(t.Term) : pf.GetPredicate(t.Term.Args);

        public virtual bool CouldReevaluationSucceed 
            => secondPredicate == null || secondPredicate.CouldReevaluationSucceed;
    }
}
