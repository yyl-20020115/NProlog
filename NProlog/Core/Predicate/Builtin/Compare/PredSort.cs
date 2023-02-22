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
using Org.NProlog.Core.Predicate.Builtin.List;
using Org.NProlog.Core.Predicate.Udp;
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Predicate.Builtin.Compare;

/* TEST
%?- predsort(compare, [s,d,f,a,a,a,z], X)
% X=[a,a,a,d,f,s,z]

%TRUE predsort(compare, [s,d,f,a,a,a,z], [a,a,a,d,f,s,z])
%FAIL predsort(compare, [s,d,f,a,a,a,z], [s,d,f,a,a,a,z])

%TRUE predsort(compare, [], [])

compare_desc(X,Y,Z) :- Y@<Z, X='>'.
compare_desc(X,Y,Z) :- Y@>Z, X='<'.
compare_desc(X,Y,Z) :- Y==Z, X='='.

compare_desc(asc,X,Y,Z) :- Y@<Z, X='>'.
compare_desc(asc,X,Y,Z) :- Y@>Z, X='<'.
compare_desc(asc,X,Y,Z) :- Y==Z, X='='.
compare_desc(desc,X,Y,Z) :- Y@<Z, X='<'.
compare_desc(desc,X,Y,Z) :- Y@>Z, X='>'.
compare_desc(desc,X,Y,Z) :- Y==Z, X='='.

%?- predsort(compare_desc, [s,d,f,a,a,a,z], X)
% X=[z,s,f,d,a,a,a]

% Note: This behaviour is different than the SWI version. SWI version removes duplicates.
%?- predsort(compare_desc(asc), [s,d,f,a,a,a,z], X)
% X=[z,s,f,d,a,a,a]

compare_retryable('>',_,_).
compare_retryable('<',_,_).
compare_retryable('=',_,_).
% Note: This behaviour is different than the SWI version. SWI version backtracks to find alternative solutions.
%?- predsort(compare_retryable, [s,z], X)
% X=[s,z]
*/
/**
 * <code>predsort(X,Y,Z)</code> - sorts a list using the specified predicate.
 * <p>
 * Sorts the list represented by <code>Y</code> using the predicate represented by <code>X</code> - and attempts to
 * unify the result with <code>Z</code>. The predicate represented by <code>X</code> must indicate whether the second
 * argument is equal, less than or greater than the third argument - by unifying the first argument with an atom which
 * has the value <code>=</code>, <code>&lt;</code> or <code>&gt;</code>.
 * </p>
 */
public class PredSort : AbstractSingleResultPredicate, PreprocessablePredicateFactory
{
    // The SWI version of this predicate removes duplicates and backtracks to find alternative solutions.
    // TODO Either change this version to behave the same or update documentation to make it clear how the behaviour of this version differs from SWI.

    /** The arity of the predicate represented by the first argument. */
    private const int FIRST_ARG_ARITY = 3;


    protected override bool Evaluate(Term predicateName, Term input, Term sorted)
    {
        var pf = PartialApplicationUtils.GetPartiallyAppliedPredicateFactory(
            Predicates, predicateName, FIRST_ARG_ARITY);
        return EvaluatePredSort(pf, predicateName, input, sorted);
    }

    private static bool EvaluatePredSort(PredicateFactory pf, Term predicateName, Term input, Term sorted)
    {
        var list = ListUtils.ToList(input);
        if (list == null)
        {
            return false;
        }

        //      Collections.sort(list,);
        list.Sort(new PredSortComparator(pf, predicateName));
        return sorted.Unify(ListFactory.CreateList(list));
    }

    public class PredSortComparator : IComparer<Term>
    {
        private readonly PredicateFactory pf;
        private readonly Term predicateName;

        public PredSortComparator(PredicateFactory pf, Term predicateName)
        {
            this.pf = pf;
            this.predicateName = predicateName;
        }


        public int Compare(Term? o1, Term? o2)
        {
            var result = new Variable("PredSortResult");
            var p = PartialApplicationUtils.GetPredicate(pf, predicateName, result, o1, o2);
            if (p.Evaluate())
            {
                var delta = TermUtils.GetAtomName(result);
                return delta switch
                {
                    "<" => -1,
                    ">" => 1,
                    "=" => 0,
                    _ => throw new ArgumentException(delta),
                };
            }
            else
            {
                throw new InvalidOperationException(predicateName + " " + result + " " + o1 + " " + o2); // TODO
            }
        }
    }


    public PredicateFactory Preprocess(Term term)
    {
        var goal = term.GetArgument(1);
        return goal.Type == TermType.ATOM
            ? new PreprocessedPredSort(PartialApplicationUtils.GetPreprocessedPartiallyAppliedPredicateFactory(Predicates, goal, FIRST_ARG_ARITY), goal)
            : this;
    }

    public class PreprocessedPredSort : PredicateFactory
    {
        private readonly PredicateFactory pf;
        private readonly Term predicateName;

        public PreprocessedPredSort(PredicateFactory pf, Term predicateName)
        {
            this.pf = pf;
            this.predicateName = predicateName;
        }

        public Predicate GetPredicate(Term[] args) => PredicateUtils.ToPredicate(
            EvaluatePredSort(pf, predicateName, args[1], args[2]));

        public bool IsRetryable => false;
    }
}
