/*
 * Copyright 2022 S. Webber
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
%?- pairs_keys([a-y, c-x, b-z], L)
% L = [a,c,b]

%?- pairs_keys([a-y, a-x, a-z], L)
% L = [a,a,a]

%?- pairs_values([a-y, c-x, b-z], L)
% L = [y,x,z]

%?- pairs_values([a-y, c-y, b-y], L)
% L = [y,y,y]
 */
/**
 * <code>pairs_keys(Pairs,Keys)</code> / <code>pairs_values(Pairs,Values)</code> - get keys or values from list of Key-Value pairs.
 */
public class PairsElements : AbstractSingleResultPredicate
{
    public static PairsElements Keys() => new(0);

    public static PairsElements Values() => new(1);

    private readonly int argumentIdx;

    private PairsElements(int argumentIdx) 
        => this.argumentIdx = argumentIdx;

    protected override bool Evaluate(Term pairs, Term values)
    {
        var tail = pairs;
        List<Term> selected = new();
        while (tail.Type == TermType.LIST)
        {
            var head = tail.GetArgument(0);
            if (!PartialApplicationUtils.IsKeyValuePair(head))
                throw new PrologException("Expected every element of list to be a compound term with a functor of - and two arguments but got: " + head);
            selected.Add(head.GetArgument(argumentIdx));
            tail = tail.GetArgument(1);
        }

        if (tail.Type != TermType.EMPTY_LIST)
            throw new PrologException("Expected first element to be a ground list but got: " + pairs);

        return values.Unify(ListFactory.CreateList(selected));
    }
}
