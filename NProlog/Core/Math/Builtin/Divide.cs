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

namespace Org.NProlog.Core.Math.Builtin;


/* TEST
%LINK prolog-arithmetic
*/
/**
 * <code>/</code> - performs division.
 */
public class Divide : AbstractArithmeticOperator
{

    public override Numeric Calculate(Numeric n1, Numeric n2)
    {
        if (ContainsFraction(n1, n2))
            return DivideFractions(n1, n2);
        else
        {
            var dividend = n1.Long;
            var divisor = n2.Long;
            // e.g. 6 / 2 = 3
            // e.g. 7 / 2 = 3.5
            return dividend % divisor == 0 ? IntegerNumberCache.ValueOf(dividend / divisor)
                : DivideFractions(n1, n2);
        }
    }

    private static bool ContainsFraction(Numeric n1, Numeric n2)
        => n1.Type == TermType.FRACTION || n2.Type == TermType.FRACTION;

    private static DecimalFraction DivideFractions(Numeric n1, Numeric n2)
        => new (n1.Double / n2.Double);
}
