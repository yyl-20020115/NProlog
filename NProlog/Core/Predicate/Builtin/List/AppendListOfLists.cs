/*
 * Copyright 2020 S. Webber
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
%TRUE Append([], [])
%TRUE Append([[]], [])
%TRUE Append([[a]], [a])
%TRUE Append([[a,b,c],[d,e,f,g,h]], [a,b,c,d,e,f,g,h])
%FAIL Append([[a,b,c],[d,e,f,g,h]], [a,b,c,d,e,f,g,x])

%?- Append([[a,b,c],[[d,e,f],x,y,z],[1,2,3],[]],X)
% X=[a,b,c,[d,e,f],x,y,z,1,2,3]

%?- Append([[a,b,c],[[d,e,f],x,y,z],[1,2,3],[]],[a,b,c,[d,e,f],x|X])
% X=[y,z,1,2,3]

%?- Append(a, X)
%ERROR Expected LIST but got: ATOM with value: a

%?- Append([a], X)
%ERROR Expected list but got: ATOM with value: a

% Note: unlike SWI Prolog, the first argument cannot be a partial list.
%?- Append([[a,b|X],[c,d]], Y)
%ERROR Expected list but got: LIST with value: .(a, .(b, X))
%?- Append([[a,b],[c,d|X]], Y)
%ERROR Expected list but got: LIST with value: .(c, .(d, X))
%?- Append([[a,b|X],[e,x|Y]], [a,b,c,d,e,x,y,z])
%ERROR Expected list but got: LIST with value: .(a, .(b, X))
%?- Append([[a,b|X],[e,x|Y]], [a,b,c,d,e,x,y|Z])
%ERROR Expected list but got: LIST with value: .(a, .(b, X))
*/
/**
 * <code>Append(ListOfLists, List)</code> - concatenates a list of lists.
 * <p>
 * The <code>Append(ListOfLists, List)</code> goal succeeds if the concatenation of lists contained in
 * <code>ListOfLists</code> matches the list <code>List</code>.
 * </p>
 */
public class AppendListOfLists : AbstractSingleResultPredicate
{

    protected override bool Evaluate(Term listOfLists, Term termToUnifyWith)
    {
        if (listOfLists.Type == TermType.EMPTY_LIST)
            return termToUnifyWith.Unify(EmptyList.EMPTY_LIST);

        TermUtils.AssertType(listOfLists, TermType.LIST);

        var input = ListUtils.ToList(listOfLists); // avoid converting to java list
        var output = new List<Term>();
        foreach (Term list in input)
        {
            var elements = ListUtils.ToList(list);
            if (elements == null)
                throw new PrologException("Expected list but got: " + list.Type + " with value: " + list);
            output.AddRange(elements);
        }
        return termToUnifyWith.Unify(ListFactory.CreateList(output));
    }
}
