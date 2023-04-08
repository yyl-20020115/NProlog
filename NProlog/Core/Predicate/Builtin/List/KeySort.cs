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
using Org.NProlog.Core.Exceptions;
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Predicate.Builtin.List;




/* TEST
%?- keysort([a - 1,b - 3,c - 2], X)
% X=[a - 1,b - 3,c - 2]

%?- keysort([c - 2,a - 1,b - 3], X)
% X=[a - 1,b - 3,c - 2]

%TRUE keysort([c - 2,a - 1,b - 3], [a - 1,b - 3,c - 2])
%FAIL keysort([c - 2,a - 1,b - 3], [c - 2,a - 1,b - 3])

% Duplicates are <i>not</i> removed.
%?- keysort([a - 1,a - 9,a - 1,z - 1, q - 3, z - 1], X)
% X=[a - 1,a - 9,a - 1,q - 3,z - 1,z - 1]

% Keys are sorted using the standard ordering of terms.
%?- keysort([Variable - v,1.0 - v,1 - v,atom - v, [] - v,structure(a) - v,[list] - v], X)
% X=[Variable - v,1.0 - v,1 - v,[] - v,atom - v,structure(a) - v,[list] - v]
% Variable=UNINSTANTIATED VARIABLE

%?- keysort([[list] - v,structure(a) - v,[] - v,atom - v,1 - v,1.0 - v,Variable - v], X)
% X=[Variable - v,1.0 - v,1 - v,[] - v,atom - v,structure(a) - v,[list] - v]
% Variable=UNINSTANTIATED VARIABLE

% Both the first and second arguments can contain variables.
%?- keysort([c - Q,a - W,b - E],[R - 1,T - 2,Y - 3])
% Q=3
% W=1
% E=2
% R=a
% T=b
% Y=c
*/
/**
 * <code>keysort(X,Y)</code> - sorts a list of key/value pairs.
 * <p>
 * Sorts the list <code>X</code>, containing <i>key/value pairs</i>, and attempts to unify the result with
 * <code>Y</code>. Key/value pairs are compound terms with a functor of <code>-</code> and two arguments. The first
 * argument is the <i>key</i> and the second argument is the <i>value</i>. It is the key of the key/value pairs that is
 * used to sort the elements contained in <code>X</code>. (Note: duplicates are <i>not</i> removed.)
 */
public class KeySort : AbstractSingleResultPredicate
{

    public class TermComparer : Comparer<Term>
    {
        public override int Compare(Term? x, Term? y)
            => (x==null ||y == null)?0: TermComparator.TERM_COMPARATOR.Compare(x.GetArgument(0), y.GetArgument(0));
    }
    private static readonly TermComparer KEY_VALUE_PAIR_COMPARATOR = new();

    protected override bool Evaluate(Term original, Term result)
    {
        var elements = ListUtils.ToList(original) ?? throw new PrologException("Expected first argument to be a fully instantied list but got: " + original);
        AssertKeyValuePairs(elements);
        elements.Sort(KEY_VALUE_PAIR_COMPARATOR);
        return result.Unify(ListFactory.CreateList(elements));
    }

    private static bool AssertKeyValuePairs(List<Term> elements)
    {
        foreach (var t in elements)
            if (!PartialApplicationUtils.IsKeyValuePair(t))
                throw new PrologException("Expected every element of list to be a compound term with a functor of - and two arguments but got: " + t);
        return true;
    }
}
