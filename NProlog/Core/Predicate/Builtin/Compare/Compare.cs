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
using Org.NProlog.Core.Math;
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Predicate.Builtin.Compare;



/* TEST
%?- compare(X, a, z)
% X=<

%?- compare(X, a, a)
% X==

%?- compare(X, z, a)
% X=>

%FAIL compare(<, z, a)

%TRUE compare(>, z, a)

% All floating point numbers are less than all integers
%?- compare(X, 1.0, 1)
% X=<

%?- compare(X, a, Y)
% X=>
% Y=UNINSTANTIATED VARIABLE

%FAIL compare(=, X, Y)

%?- X=Y, compare(=, X, Y)
% X=UNINSTANTIATED VARIABLE
% Y=UNINSTANTIATED VARIABLE
*/
/**
 * <code>compare(X,Y,Z)</code> - compares arguments.
 * <p>
 * Compares the second and third arguments.
 * <ul>
 * <li>If second is greater than third then attempts to unify first argument with <code>&gt;</code></li>
 * <li>If second is less than third then attempts to unify first argument with <code>&lt;</code></li>
 * <li>If second is equal to third then attempts to unify first argument with <code>=</code></li>
 * </ul>
 */
public class Compare : AbstractSingleResultPredicate
{

    protected override bool Evaluate(Term result, Term t1, Term t2)
    {
        var i = TermComparator.TERM_COMPARATOR.Compare(t1, t2);
        var symbol = i < 0 ? "<" : i > 0 ? ">" : "=";
        return result.Unify(new Atom(symbol));
    }
}
