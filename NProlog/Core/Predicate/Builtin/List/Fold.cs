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
using Org.NProlog.Core.Exceptions;
using Org.NProlog.Core.Predicate.Udp;
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Predicate.Builtin.List;




/* TEST
multiple_result_predicate(X,Y,Z) :-  Z is X+Y.
multiple_result_predicate(X,Y,Z) :-  Z is X*Y.

single_result_predicate(X,Y,Z) :-  Z is X+Y.

%?- foldl(single_result_predicate, [2,4,7], 0, X)
% X=13

%?- foldl(single_result_predicate, [2,4,7], 42, X)
% X=55

%TRUE foldl(single_result_predicate, [2,4,7], 0, 13)
%FAIL foldl(single_result_predicate, [2,4,7], 0, 12)

%?- foldl(single_result_predicate, [], 7, X)
% X=7

%?- foldl(single_result_predicate, [3], 7, X)
% X=10

%?- foldl(multiple_result_predicate, [2,4,7], 42, X)
% X=55
% X=336
% X=183
% X=1232
% X=95
% X=616
% X=343
% X=2352

%?- foldl(multiple_result_predicate, [1,2,3], 0, X)
% X=6
% X=9
% X=5
% X=6
% X=5
% X=6
% X=3
% X=0

%?- foldl(multiple_result_predicate, [1,2,3], 0, 6)
%YES
%YES
%YES
%NO

%?- foldl(multiple_result_predicate, [], 7, X)
% X=7

%?- foldl(multiple_result_predicate, [3], 7, X)
% X=10
% X=21

four_arg_predicate(1,X,Y,Z) :-  Z is X+Y.
four_arg_predicate(2,X,Y,Z) :-  Z is X-Y.
four_arg_predicate(3,X,Y,Z) :-  Z is X+Y.
four_arg_predicate(3,X,Y,Z) :-  Z is X-Y.
four_arg_predicate(a,1,2,3).
four_arg_predicate(a,x,y,3).
four_arg_predicate(a,5,3,12).
four_arg_predicate(a,999,_,99999).
four_arg_predicate(b,_,_,_).

%?- foldl(four_arg_predicate(1), [2,4,7], 0, X)
% X=13

%?- foldl(four_arg_predicate(2), [2,4,7], 0, X)
% X=5

%?- foldl(four_arg_predicate(3), [2,4,7], 0, X)
% X=13
% X=1
% X=9
% X=5
% X=13
% X=1
% X=9
% X=5

%?- foldl(four_arg_predicate(3), [2,4,7], 0, 5)
%YES
%YES

%?- foldl(four_arg_predicate(a), [B,C], A, X)
% A=2
% B=1
% C=5
% X=12
% A=2
% B=1
% C=999
% X=99999
% A=y
% B=x
% C=5
% X=12
% A=y
% B=x
% C=999
% X=99999
% A=3
% B=5
% C=999
% X=99999
% A=UNINSTANTIATED VARIABLE
% B=999
% C=999
% X=99999

%FAIL foldl(four_arg_predicate(3), [2,4,7], 0, 14)

%FAIL foldl(four_arg_predicate(4), [2,4,7], 0, X)

% Note: Unlike SWI Prolog, fails on first evaluation if the second argument is not a concrete list.
%?- foldl(single_result_predicate, [2,4,7|T], 0, X)
%ERROR Expected concrete list but got: .(2, .(4, .(7, T)))
%?- foldl(single_result_predicate, L, 0, X)
%ERROR Expected concrete list but got: L
*/
/**
 * <code>foldl(PredicateName, Values, Start, Result)</code> - combines elements of a list into a single term.
 * <p>
 * See <a href="https://en.wikipedia.org/wiki/Fold_(higher-order_function)">Wikipedia</a>.
 */
public class Fold : AbstractPredicateFactory, PreprocessablePredicateFactory
{
    /** The arity of the predicate represented by the first argument. */
    private static readonly int FIRST_ARG_ARITY = 3;


    public virtual PredicateFactory Preprocess(Term arg)
    {
        var action = arg.GetArgument(0);
        if (PartialApplicationUtils.IsAtomOrStructure(action))
        {
            var pf = PartialApplicationUtils.GetPreprocessedPartiallyAppliedPredicateFactory(
                Predicates, action, FIRST_ARG_ARITY);
            return new OptimisedFold(pf, action);
        }
        else
        {
            return this;
        }
    }

    private class OptimisedFold : PredicateFactory
    {
        readonly PredicateFactory pf;
        readonly Term action;

        public OptimisedFold(PredicateFactory pf, Term action)
        {
            this.pf = pf;
            this.action = action;
        }


        public virtual Predicate GetPredicate(Term[] args)
            => GetFoldPredicate(pf, action, args[1], args[2], args[3]);


        public bool IsRetryable => pf.IsRetryable;
    }


    protected override Predicate GetPredicate(Term atom, Term values, Term start, Term result)
    {
        var pf = PartialApplicationUtils.GetPartiallyAppliedPredicateFactory(Predicates, atom, FIRST_ARG_ARITY);
        return GetFoldPredicate(pf, atom, values, start, result);
    }

    private static Predicate GetFoldPredicate(PredicateFactory pf, Term action, Term values, Term start, Term result)
    {
        var list = ToList(values);

        if (list.Count == 0)
        {
            return PredicateUtils.ToPredicate(result.Unify(start));
        }
        else if (pf.IsRetryable)
        {
            return new Retryable(pf, action, list, start, result);
        }
        else
        {
            bool success = EvaluateFold(pf, action, list, start, result);
            return PredicateUtils.ToPredicate(success);
        }
    }

    private static List<Term> ToList(Term values)
    {
        var list = ListUtils.ToList(values);
        if (list == null)
        {
            throw new PrologException("Expected concrete list but got: " + values);
        }
        return list;
    }

    private static bool EvaluateFold(PredicateFactory pf, Term action, List<Term> values, Term start, Term result)
    {
        var output = start;
        foreach (Term next in values)
        {
            var previous = output;
            output = new Variable("FoldAccumulator");
            var p = PartialApplicationUtils.GetPredicate(pf, action, next, previous, output);
            if (!p.Evaluate())
            {
                return false;
            }
        }

        return result.Unify(output);
    }

    private class Retryable : Predicate
    {
        private readonly PredicateFactory pf;
        private readonly Term action;
        private readonly List<Term> values;
        private readonly Term start;
        private readonly Term result;
        private readonly List<Predicate> predicates;
        private readonly List<Variable> accumulators;
        private readonly List<Term> backtrack1;
        private readonly List<Term> backtrack2;
        private int idx;

        public Retryable(PredicateFactory pf, Term action, List<Term> values, Term start, Term result)
        {
            this.pf = pf;
            this.action = action;
            this.values = values;
            this.start = start;
            this.result = result;
            this.accumulators = new(values.Count);
            this.predicates = new(values.Count);
            this.backtrack1 = new(values.Count);
            this.backtrack2 = new(values.Count);
        }


        public virtual bool Evaluate()
        {
            result.Backtrack();

            while (idx > -1)
            {
                bool success;
                if (predicates.Count == idx)
                {
                    var x = values[(idx)].Term;
                    var y = idx == 0 ? start.Term : accumulators[(idx - 1)].Term;
                    var v = new Variable("FoldAccumulator" + idx);
                    var p = PartialApplicationUtils.GetPredicate(pf, action, x, y, v);
                    success = p.Evaluate();

                    accumulators.Add(v);
                    predicates.Add(p);
                    backtrack1.Add(x);
                    backtrack2.Add(y);
                }
                else
                {
                    var p = predicates[(idx)];
                    success = p.CouldReevaluationSucceed && p.Evaluate();
                }

                if (success)
                {
                    if (idx < values.Count - 1)
                    {
                        idx++;
                    }
                    else if (result.Unify(accumulators[(idx)]))
                    {
                        return true;
                    }
                }
                else
                {
                    predicates.RemoveAt(idx);
                    var ac = accumulators[idx];
                    ac.Backtrack();
                    accumulators.RemoveAt(idx);
                    var b1 = backtrack1[idx];
                    b1.Backtrack();
                    backtrack1.RemoveAt(idx);
                    var b2 = backtrack2[idx];
                    b2.Backtrack();
                    backtrack2.RemoveAt(idx);
                    idx--;
                }
            }
            return false;
        }


        public virtual bool CouldReevaluationSucceed
        {
            get
            {
                if (predicates.Count == 0)
                { // if empty then has not been evaluated yet
                    return true;
                }

                foreach (var p in predicates)
                    if (p.CouldReevaluationSucceed) return true;
                return false;
            }
        }
    }
}
