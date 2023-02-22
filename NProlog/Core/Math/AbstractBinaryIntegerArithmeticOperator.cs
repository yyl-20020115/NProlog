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
using Org.NProlog.Core.Exceptions;
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Math;

/**
 * A template for {@code ArithmeticOperator}s that accept two arguments of type {@link TermType#INTEGER}.
 */
public abstract class AbstractBinaryIntegerArithmeticOperator : AbstractArithmeticOperator
{
    public override Numeric Calculate(Numeric n1, Numeric n2) 
        => IntegerNumberCache.ValueOf(CalculateLong(ToLong(n1), ToLong(n2)));

    public static long ToLong(Numeric n) 
        => n.Type == TermType.INTEGER ? n.Long 
        : throw new PrologException("Expected integer but got: " + n.Type + " with value: " + n);

    /** Returns the result of evaluating an arithmetic expression using the two arguments */
    protected abstract long CalculateLong(long n1, long n2);
}
