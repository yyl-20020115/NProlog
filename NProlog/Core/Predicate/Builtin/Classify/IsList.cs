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
using Org.NProlog.Core.Predicate.Builtin.Construct;
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Predicate.Builtin.Classify;


/* TEST
%TRUE is_list([1.0,2.0,3.0])
%TRUE is_list([])
%TRUE is_list([a|[]])

%FAIL is_list([a|b])
%FAIL is_list([a|X])
%FAIL is_list(X)
*/
/**
 * <code>is_list(X)</code> - checks that a term is a list.
 * <p>
 * <code>is_list(X)</code> succeeds if <code>X</code> currently stands for a list.
 * </p>
 */
public class IsList : AbstractSingleResultPredicate {

    protected override bool Evaluate(Term arg) => arg.Type switch
    {
        var tt when tt == TermType.EMPTY_LIST => true,
        var tt when tt == TermType.LIST => IsDeepList(arg),
        _ => false
    };

    protected static bool IsDeepList(Term arg)
    {
        var tail = arg;
        while ((tail = tail.GetArgument(1)).Type == TermType.LIST) ;
        return tail.Type == TermType.EMPTY_LIST;
    }
}
