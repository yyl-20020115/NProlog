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

namespace Org.NProlog.Core.Predicate.Builtin.Compare;

/* TEST
%FAIL b@=<a
%TRUE b@=<b
%TRUE b@=<c
%FAIL b@=<1
%TRUE b@=<b(a)
*/
/**
 * <code>X@=&lt;Y</code> - term "less than or equal" test.
 * <p>
 * Succeeds when the term argument <code>X</code> is less than or equal to the term argument <code>Y</code>.
 * </p>
 */
public class TermLessThanOrEqual : AbstractSingleResultPredicate
{
    protected override bool Evaluate(Term arg1, Term arg2)
        => TermComparator.TERM_COMPARATOR.Compare(arg1, arg2) < 1;
}
