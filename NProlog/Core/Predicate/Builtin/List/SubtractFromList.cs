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

namespace Org.NProlog.Core.Predicate.Builtin.List;

/* TEST
%TRUE subtract([a,b,c,d,e,f], [a,s,d,f], [b,c,e])
%TRUE subtract([a,b,a,a,d,c,d,e,f], [a,s,d,f], [b,c,e])
%TRUE subtract([a,b,a,a,d,c,d,e,f], [], [a,b,a,a,d,c,d,e,f])
%TRUE subtract([], [a,s,d,f], [])
%TRUE subtract([], [], [])

%?- subtract([a,a,a,a], [X], Z)
% X=a
% Z=[]

%?- subtract([a,a,a,a,b], [X], Z)
% X=a
% Z=[b]

%?- subtract([p(A),p(B),p(C)], [p(a)],Z)
% A=a
% B=a
% C=a
% Z=[]

%?- subtract([p(a,B,c,e)], [p(A,b,C,e)], Z)
% A=a
% B=b
% C=c
% Z=[]

%?- subtract([p(a,B,c,x)], [p(A,b,C,e)], Z)
% A=UNINSTANTIATED VARIABLE
% B=UNINSTANTIATED VARIABLE
% C=UNINSTANTIATED VARIABLE
% Z=[p(a, B, c, x)]

%?- subtract([p(a,B), p(A,b)], [p(A,B)], Z)
% A=a
% B=b
% Z=[]

%FAIL subtract(X, [], [])
%FAIL subtract([], X, [])
%FAIL subtract(X, Y, [])
*/
/**
 * <code>subtract(X,Y,Z)</code> - removes elements from a list.
 * <p>
 * <code>subtract(X,Y,Z)</code> removes the elements in the list represented by <code>Y</code> from the list represented
 * by <code>X</code> and attempts to unify the result with <code>Z</code>.
 */
public class SubtractFromList : AbstractSingleResultPredicate
{

    protected override bool Evaluate(Term original, Term itemsToRemove, Term result)
    {
        var originalAsList = ListUtils.ToList(original);
        var itemsToRemoveAsList = ListUtils.ToList(itemsToRemove);

        if (originalAsList == null || itemsToRemoveAsList == null)
            return false;
        foreach (var item in originalAsList.ToArray())
            if (ShouldBeRemoved(item, itemsToRemoveAsList))
                originalAsList.Remove(item);

        return result.Unify(ListFactory.CreateList(originalAsList));
    }

    private static bool ShouldBeRemoved(Term item, List<Term> itemsToRemoveAsList)
    {
        foreach (var itemToRemove in itemsToRemoveAsList)
            if (IsUnified(item, itemToRemove))
                return true;
        return false;
    }

    private static bool IsUnified(Term item, Term itemToRemove)
    {
        item = item.Term;
        itemToRemove = itemToRemove.Term;

        if (item.Unify(itemToRemove))
            return true;
        else
        {
            item.Backtrack();
            itemToRemove.Backtrack();
            return false;
        }
    }
}
