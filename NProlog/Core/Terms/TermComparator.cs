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
using Org.NProlog.Core.Math;

namespace Org.NProlog.Core.Terms;

/**
 * An implementation of {@code Comparator} for comparing instances of {@link Term}.
 *
 * @see #compare(Term, Term)
 * @see NumericTermComparator
 * @see TermUtils#termsEqual(Term, Term)
 */
public class TermComparator : IComparer<Term>
{
    /**
     * Singleton instance
     */
    public static readonly TermComparator TERM_COMPARATOR = new();

    /**
     * Private constructor to force use of {@link #TERM_COMPARATOR}
     */
    private TermComparator() { }

    /**
     * Compares the two arguments for order.
     * <p>
     * Returns a negative integer, zero, or a positive integer as the first argument is less than, equal to, or greater
     * than the second.
     * <p>
     * The criteria for deciding the order of terms is as follows:
     * <ul>
     * <li>All uninstantiated variables are less than all floating point numbers, which are less than all integers, which
     * are less than all atoms, which are less than all structures (including lists).</li>
     * <li>Comparison of two integer or two floating point numbers is done using {@link NumericTermComparator}.</li>
     * <li>Comparison of two atoms is done by comparing the {@code string} values they represent using
     * {@code string.compareTo(string)}.</li>
     * <li>One structure is less than another if it has a lower arity (number of arguments). If two structures have the
     * same arity then they are ordered by comparing their functors (names) (determined by
     * {@code string.compareTo(string)}). If two structures have the same arity and functor then they are ordered by
     * comparing their arguments in order. The first corresponding arguments that differ determines the order of the two
     * structures.</li>
     * </ul>
     *
     * @param t1 the first term to be compared
     * @param t2 the second term to be compared
     * @return a negative integer, zero, or a positive integer as the first term is less than, equal to, or greater than
     * the second
     */

    public int Compare(Term? t1, Term? t2)
    {
        var v1 = t1.Term;
        var v2 = t2.Term;

        // if the both arguments refer to the same object then must be identical
        // this deals with the case where both arguments are empty lists
        // or both are an anonymous variable
        if (v1.Term == v2.Term) return 0;

        var type1 = v1.Type;
        var type2 = v2.Type;

        return type1.IsStructure && type2.IsStructure
            ? CompareStructures(v1, v2)
            : type1 != type2
                ? type1.Precedence > type2.Precedence ? 1 : -1
                : type1 switch
                {
                    var tt when (tt == TermType.FRACTION || tt == TermType.INTEGER)
                        => NumericTermComparator.Compare(TermUtils.CastToNumeric(v1), TermUtils.CastToNumeric(t2)),
                    var tt when (tt == TermType.ATOM) => t1.Name.CompareTo(t2.Name),
                    var tt when (tt == TermType.VARIABLE || tt == TermType.CLP_VARIABLE) => v1.GetHashCode() > v2.GetHashCode() ? 1 : -1,// NOTE: uses object's hashCode which is not guaranteed, so may get different results in different JVMs
                    _ => throw new PrologException($"Unknown TermType: {type1}"),
                };
    }

    private int CompareStructures(Term t1, Term t2)
    {
        // compare number of arguments
        var t1Length = t1.NumberOfArguments;
        var t2Length = t2.NumberOfArguments;
        if (t1Length != t2Length)
            return t1Length > t2Length ? 1 : -1;

        // compare predicate names
        var nameComparison = t1.Name.CompareTo(t2.Name);
        if (nameComparison != 0)
            return nameComparison;

        // compare arguments one at a time
        for (var i = 0; i < t1Length; i++)
        {
            var argComparison = Compare(t1.GetArgument(i), t2.GetArgument(i));
            if (argComparison != 0)
                return argComparison;
        }

        // if still cannot separate then consider them identical
        return 0;
    }
}
