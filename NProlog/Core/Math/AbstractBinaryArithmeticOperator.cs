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

namespace Org.NProlog.Core.Math;


/**
 * A template for {@code ArithmeticOperator}s that accept two arguments.
 */
public abstract class AbstractBinaryArithmeticOperator : AbstractArithmeticOperator
{

    public override Numeric Calculate(Numeric n1, Numeric n2) => ContainsFraction(n1, n2)
            ? new DecimalFraction(CalculateDouble(n1.Double, n2.Double))
            : IntegerNumberCache.ValueOf(CalculateLong(n1.Long, n2.Long));

    private static bool ContainsFraction(Numeric n1, Numeric n2) 
        => n1?.Type == TermType.FRACTION || n2?.Type == TermType.FRACTION;

    /** Returns the result of evaluating an arithmetic expression using the two arguments */
    protected abstract double CalculateDouble(double n1, double n2);

    /** Returns the result of evaluating an arithmetic expression using the two arguments */
    protected abstract long CalculateLong(long n1, long n2);
}
