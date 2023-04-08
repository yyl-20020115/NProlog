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
 * Provides methods for comparing instances of {@link Numeric}.
 *
 * @see #compare(Term, Term, ArithmeticOperators)
 * @see TermComparator
 */
public class NumericTermComparator
{
    /**
     * Singleton instance
     */
    public static readonly NumericTermComparator NUMERIC_TERM_COMPARATOR = new();

    /**
     * Private constructor to force use of {@link #NUMERIC_TERM_COMPARATOR}
     */
    private NumericTermComparator() { }

    /**
     * Compares the two arguments, representing arithmetic expressions, for order.
     * <p>
     * Returns a negative integer, zero, or a positive integer as the numeric value represented by the first argument is
     * less than, equal to, or greater than the second.
     * <p>
     * Unlike {@link #compare(Numeric, Numeric)} this method will work for arguments that represent arithmetic
     * expressions (e.g. a {@link Structure} of the form {@code +(1,2)}) as well as {@link Numeric} terms.
     *
     * @param t1 the first term to be compared
     * @param t2 the second term to be compared
     * @return a negative integer, zero, or a positive integer as the first term is less than, equal to, or greater than
     * the second
     * @throws PrologException if either argument does not represent an arithmetic expression
     * @see #compare(Numeric, Numeric)
     * @see ArithmeticOperators#getNumeric(Term)
     */
    public static int Compare(Term? t1, Term? t2, ArithmeticOperators operators)
        => Compare(operators.GetNumeric(t1), operators.GetNumeric(t2));

    /**
     * Compares two arguments, representing {@link Numeric} terms, for order.
     * <p>
     * Returns a negative integer, zero, or a positive integer as the numeric value represented by the first argument is
     * less than, equal to, or greater than the second.
     *
     * @param n1 the first term to be compared
     * @param n2 the second term to be compared
     * @return a negative integer, zero, or a positive integer as the first term is less than, equal to, or greater than
     * the second
     * @see #compare(Term, Term, ArithmeticOperators)
     */
    public static int Compare(Numeric n1, Numeric n2) 
        => n1.Type == TermType.INTEGER && n2.Type == TermType.INTEGER
        ? Compare(n1.Long, n2.Long) 
        : Compare(n1.Double, n2.Double);

    public static int Compare(long n1, long n2) => n1.CompareTo(n2);
    public static int Compare(double n1, double n2) => n1.CompareTo(n2);
}
