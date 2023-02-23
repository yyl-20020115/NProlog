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
using Org.NProlog.Core.Terms;
using System.Diagnostics.CodeAnalysis;

namespace Org.NProlog.Core.Predicate.Builtin.List;


/* TEST
%?- sort([q,w,e,r,t,y], X)
% X=[e,q,r,t,w,y]

%TRUE sort([q,w,e,r,t,y], [e,q,r,t,w,y])
%FAIL sort([q,w,e,r,t,y], [q,w,e,r,t,y])
%FAIL sort([q,w,e,r,t,y], [e,q,t,r,w,y])

%?- sort([q,w,e,r,t,y], [A,B,C,D,E,F])
% A=e
% B=q
% C=r
% D=t
% E=w
% F=y

%?- sort([], X)
% X=[]

%?- sort([a], X)
% X=[a]

%FAIL sort(a, X)
%FAIL sort([a,b,c|T], X)

%?- sort([h,e,l,l,o], X)
% X=[e,h,l,o]

%FAIL sort([h,e,l,l,o], [e,h,l,l,o])
*/
/**
 * <code>sort(X,Y)</code> - sorts a list and removes duplicates.
 * <p>
 * Attempts to unify <code>Y</code> with a sorted version of the list represented by <code>X</code>, with duplicates
 * removed.
 * </p>
 */
public class SortAsSet : AbstractSingleResultPredicate
{

    protected override bool Evaluate(Term unsorted, Term sorted)
    {
        var elements = ListUtils.ToSortedList(unsorted);
        if (elements == null)
        {
            return false;
        }
        else
        {
            RemoveDuplicates(elements);
            return sorted.Unify(ListFactory.CreateList(elements));
        }
    }

    public class Eq : IEqualityComparer<Term>
    {
        public bool Equals(Term? x, Term? y) => TermUtils.TermsEqual(x, y);

        public int GetHashCode([DisallowNull] Term obj) => obj.GetHashCode();
    }
    private static void RemoveDuplicates(List<Term> elements)
    {
        var dist = elements.Distinct(new Eq()).ToList();
        elements.Clear();
        elements.AddRange(dist);
    }
}
