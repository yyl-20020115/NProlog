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
 * A template for {@code ArithmeticOperator}s that accept exactly one argument.
 */
public abstract class AbstractUnaryArithmeticOperator : AbstractArithmeticOperator
{

    public override Numeric Calculate(Numeric? n) => n?.Type == TermType.FRACTION
            ? new DecimalFraction(CalculateDouble(n.Double))
            : IntegerNumberCache.ValueOf(CalculateLong(n.Long));

    /** Returns the result of evaluating an arithmetic expression using the specified argument */
    protected abstract double CalculateDouble(double n);

    /** Returns the result of evaluating an arithmetic expression using the specified argument */
    protected abstract long CalculateLong(long n);
}
