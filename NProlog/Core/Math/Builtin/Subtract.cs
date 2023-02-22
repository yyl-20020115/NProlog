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
namespace Org.NProlog.Core.Math.Builtin;


/* TEST
%LINK prolog-arithmetic
*/
/**
 * <code>-</code> - performs subtraction.
 */
public class Subtract : AbstractBinaryArithmeticOperator
{
    /** Returns the difference of the two arguments */

    protected override double CalculateDouble(double n1, double n2)
        => n1 - n2;

    /** Returns the difference of the two arguments */

    protected override long CalculateLong(long n1, long n2)
        => n1 - n2;
}
