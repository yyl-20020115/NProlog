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
%?- flatten([a,[[b]],[c]], X)
% X=[a,b,c]

%?- flatten([a,b,c], X)
% X=[a,b,c]

%?- flatten([[[[a]]],[],[],[]], X)
% X=[a]

%?- flatten([a], X)
% X=[a]

%?- flatten(a, X)
% X=[a]

%?- flatten([[[[]]],[],[],[]], X)
% X=[]

%?- flatten([], X)
% X=[]

%?- flatten([a|b], X)
% X=[a,b]

%?- flatten([a|[]], X)
% X=[a]

%?- flatten([[a|b],[c,d|e],[f|[]],g|h], X)
% X=[a,b,c,d,e,f,g,h]

%?- flatten([p([[a]]),[[[p(p(x))]],[p([a,b,c])]]], X)
% X=[p([[a]]),p(p(x)),p([a,b,c])]

%FAIL flatten([a,b,c], [c,b,a])
%FAIL flatten([a,b,c], [a,[b],c])

%?- flatten([a,b,[c|X],d|Y], Z)
% X=UNINSTANTIATED VARIABLE
% Y=UNINSTANTIATED VARIABLE
% Z=[a,b,c,X,d,Y]
*/
/**
 * <code>flatten(X,Y)</code> - flattens a nested list.
 * <p>
 * Flattens the nested list represented by <code>X</code> and attempts to unify it with <code>Y</code>.
 */
public class Flatten : AbstractSingleResultPredicate
{

    protected override bool Evaluate(Term original, Term expected)
    {
        var flattenedVersion = original.Type switch
        {
            var tt when tt == TermType.LIST => ListFactory.CreateList(FlattenList(original)),
            var tt when tt == TermType.EMPTY_LIST => original,
            _ => ListFactory.CreateList(original, EmptyList.EMPTY_LIST),
        };
        return expected.Unify(flattenedVersion);
    }

    private List<Term> FlattenList(Term input)
    {
        List<Term> result = new();
        var next = input;
        while (next.Type == TermType.LIST)
        {
            var head = next.GetArgument(0);
            if (head.Type == TermType.LIST)
            {
                result.AddRange(FlattenList(head));
            }
            else if (head.Type != TermType.EMPTY_LIST)
            {
                result.Add(head);
            }

            next = next.GetArgument(1);
        }
        if (next.Type != TermType.EMPTY_LIST)
        {
            result.Add(next);
        }
        return result;
    }
}
