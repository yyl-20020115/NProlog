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
using Org.NProlog.Core.Kb;
using Org.NProlog.Core.Predicate.Builtin.List;
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Predicate.Builtin.Compound;



/* TEST
%FAIL \+ true
%TRUE \+ fail

% Note: "not" is a synonym for "\+".
%FAIL not(true)
%TRUE not(fail)

%?- \+ [A,B,C,9]=[1,2,3,4], A=6, B=7, C=8
% A=6
% B=7
% C=8

%?- \+ ((X=Y,1>2)), X=1, Y=2
% X=1
% Y=2

test1(X,Y) :- \+ ((X=Y,1>2)), X=1, Y=2.

%?- test1(X,Y)
% X=1
% Y=2

test2(X) :- \+ \+ X=1, X=2.

%?- test2(X)
% X=2

%FAIL test2(1)
%FAIL test2(2)
*/
/**
 * <code>\+ X</code> - "not".
 * <p>
 * The <code>\+ X</code> goal succeeds if an attempt to satisfy the goal represented by the term <code>X</code> fails.
 * The <code>\+ X</code> goal fails if an attempt to satisfy the goal represented by the term <code>X</code> succeeds.
 * </p>
 */
public class Not : AbstractSingleResultPredicate, PreprocessablePredicateFactory
{

    protected override bool Evaluate(Term t) => EvaluateNot(t, Predicates.GetPredicateFactory(t));

    private static bool EvaluateNot(Term t, PredicateFactory pf)
    {
        var p = pf.GetPredicate(t.Args);
        if (!p.Evaluate())
        {
            t.Backtrack();
            return true;
        }
        else
        {
            return false;
        }
    }


    public virtual PredicateFactory Preprocess(Term term)
    {
        var arg = term.GetArgument(0);
        return PartialApplicationUtils.IsAtomOrStructure(arg) ? new OptimisedNot(Predicates.GetPreprocessedPredicateFactory(arg)) : this;
    }

    public class OptimisedNot : AbstractSingleResultPredicate
    {
        private readonly PredicateFactory pf;

        public OptimisedNot(PredicateFactory pf)
        {
            this.pf = Objects.RequireNonNull(pf);
        }


        protected override bool Evaluate(Term arg) => EvaluateNot(arg, pf);
    }
}
