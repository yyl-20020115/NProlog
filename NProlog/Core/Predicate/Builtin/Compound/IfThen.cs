/*
 * Copyright 2018 S. Webber
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
using Org.NProlog.Core.Predicate.Udp;
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Predicate.Builtin.Compound;


/* TEST
if_then_else_test(1).
if_then_else_test(2).
if_then_else_test(3).

%TRUE 2>1 -> true
%FAIL 2<1 -> true
%FAIL 2>1 -> fail

%?- if_then_else_test(X) -> if_then_else_test(X)
% X=1

%?- if_then_else_test(X) -> if_then_else_test(Y)
% X=1
% Y=1
% X=1
% Y=2
% X=1
% Y=3

%?- true -> X=a ; X=b
% X=a

%?- fail -> X=a ; X=b
% X=b

%?- (X=a, 1<2) -> Y=b; Y=c
% X=a
% Y=b

%?- (X=a, 1>2) -> Y=b; Y=c
% X=UNINSTANTIATED VARIABLE
% Y=c

%?- if_then_else_test(X) -> if_then_else_test(X) ; if_then_else_test(X)
% X=1

%?- (if_then_else_test(X), fail) -> if_then_else_test(X) ; if_then_else_test(X)
% X=1
% X=2
% X=3

%?- if_then_else_test(X) -> if_then_else_test(Y) ; Y=b
% X=1
% Y=1
% X=1
% Y=2
% X=1
% Y=3
*/
/**
 * <code>X-&gt;Y</code> - if <code>X</code> succeeds then <code>Y</code> is evaluated.
 * <p>
 * <b>Note:</b> The behaviour of this predicate changes when it is specified as the first argument of a structure of the
 * form <code>;/2</code>, i.e. the <i>"disjunction"</i> predicate. When a <code>-&gt;/2</code> predicate is the first
 * argument of a <code>;/2</code> predicate then the resulting behaviour is a <i>"if/then/else"</i> statement of the
 * form <code>((if-&gt;then);else)</code>.
 * </p>
 *
 * @see Disjunction
 */
public class IfThen : AbstractPredicateFactory, PreprocessablePredicateFactory
{
    protected override Predicate GetPredicate(Term conditionTerm, Term thenTerm)
    {
        var conditionPredicate = Predicates.GetPredicate(conditionTerm);
        // TODO should we need to call getTerm before calling getPredicate, or should getPredicate contain that logic?
        return conditionPredicate.Evaluate() ? Predicates.GetPredicate(thenTerm.Term) : PredicateUtils.FALSE;
    }


    public PredicateFactory Preprocess(Term term)
    {
        var condition = term.GetArgument(0);
        var action = term.GetArgument(1);
        if (PartialApplicationUtils.IsAtomOrStructure(condition) || PartialApplicationUtils.IsAtomOrStructure(action))
        {
            var p = Predicates;
            return new OptimisedIfThen(p.GetPreprocessedPredicateFactory(condition), p.GetPreprocessedPredicateFactory(action));
        }
        else
        {
            return this;
        }
    }

    public class OptimisedIfThen : PredicateFactory
    {
        private readonly PredicateFactory condition;
        private readonly PredicateFactory action;

        public OptimisedIfThen(PredicateFactory condition, PredicateFactory action)
        {
            this.condition = condition;
            this.action = action;
        }

        public virtual Predicate GetPredicate(Term[] args)
        {
            var conditionPredicate = condition.GetPredicate(args[0].Args);
            // TODO should we need to call getTerm before calling getPredicate, or should getPredicate contain that logic?
            return conditionPredicate.Evaluate() 
                ? action.GetPredicate(args[1].Term.Args) 
                : PredicateUtils.FALSE;
        }

        public virtual bool IsRetryable => action.IsRetryable;
    }
}
