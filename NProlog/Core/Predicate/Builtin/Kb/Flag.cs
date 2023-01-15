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

namespace Org.NProlog.Core.Predicate.Builtin.Kb;



/* TEST
%?- flag(p(a), X, 2)
% X=0
%?- flag(p(a), X, X*10)
% X=2
%FAIL flag(p(a), 2, 7)
%?- flag(p(a), X, 5)
% X=20
%TRUE flag(p(b), 5, 7)

%FAIL flag(p, 1, 1)
%TRUE flag(p, 0, 1)

%?- flag(a(a), X, 25)
% X=0
%?- flag(a(b), X, X+1)
% X=25
%?- flag(a(c), X, X+1)
% X=26
%FAIL flag(a(d), 26, 33)
%TRUE flag(a(d), 27, 33)
*/
/**
 * <code>flag(X,Y,Z)</code> - associates a key with a value.
 * <p>
 * The first argument must be an atom or structure. The name and arity of the first argument is used to construct the
 * key. The second argument is the value currently associated with the key. If there is not currently a value associated
 * with the key then it will default to 0. The third argument is the new value to associate with the key. The third
 * argument must be a numeric value.
 */
public class Flag : AbstractSingleResultPredicate
{
    private readonly Dictionary<PredicateKey, Numeric> flags = new();


    protected override bool Evaluate(Term key, Term oldValue, Term newValue)
    {
        var pk = PredicateKey.CreateForTerm(key);
        lock (flags)
        {
            Numeric n = GetOrCreate(pk);

            if (oldValue.Unify(n))
            {
                Put(pk, newValue);
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    private Numeric GetOrCreate(PredicateKey pk)
    {
        if (!flags.TryGetValue(pk, out var n))
        {
            flags.Add(pk, n = IntegerNumberCache.ZERO);
        }
        return n;
    }

    private void Put(PredicateKey pk, Term value)
    {
        flags.Add(pk, ArithmeticOperators.GetNumeric(value));
    }
}
