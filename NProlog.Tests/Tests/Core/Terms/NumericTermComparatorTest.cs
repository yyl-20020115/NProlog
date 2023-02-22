/*
 * Copyright 2013 S. Webber
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a Copy of the License at
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
using Org.NProlog.Core.Kb;
using Org.NProlog.Core.Math;

namespace Org.NProlog.Core.Terms;

[TestClass]
public class NumericTermComparatorTest : TestUtils
{
    private readonly KnowledgeBase kb = TestUtils.CreateKnowledgeBase();
    private readonly ArithmeticOperators operators;

    public NumericTermComparatorTest()
    {
        operators = kb.ArithmeticOperators;
    }
    [TestMethod]
    public void TestCompareDecimalValues()
    {
        CompareDecimals(DecimalFraction(2.1), DecimalFraction(2.1));
        CompareDecimals(DecimalFraction(2.1), DecimalFraction(2.11));
        CompareDecimals(DecimalFraction(2.1), DecimalFraction(-2.1));
    }

    [TestMethod]
    public void TestCompareIntegerValues()
    {
        long[] values = { 0, 1, 2, 7, -1, -2, 7, int.MinValue, int.MaxValue, long.MinValue, long.MinValue + 1, long.MaxValue, long.MaxValue - 1 };
        for (int i1 = 0; i1 < values.Length; i1++)
        {
            for (int i2 = i1; i2 < values.Length; i2++)
            {
                ComparePrimitives(values[i1], values[i2]);
            }
        }
    }

    [TestMethod]
    public void TestMixedTypes()
    {
        CompareMixedTypes(DecimalFraction(2.0), IntegerNumber(2));
        CompareMixedTypes(IntegerNumber(2), DecimalFraction(2.0));

        CompareMixedTypes(DecimalFraction(1.9), IntegerNumber(2));
        CompareMixedTypes(DecimalFraction(2.1), IntegerNumber(2));

        CompareMixedTypes(IntegerNumber(2), DecimalFraction(2.0));
        CompareMixedTypes(IntegerNumber(2), DecimalFraction(2.0));
    }

    /** Demonstrate unexpected results that can occur due to loss of precision when comparing decimal fractions. */
    [TestMethod]
    public void TestRoundingErrors()
    {
        long a = long.MaxValue;
        long b = a - 1; // "b" is less than "a" but they are considered equal when compared as decimal fractions

        Assert.AreEqual(1, NumericTermComparator.Compare(IntegerNumber(a), IntegerNumber(b)));
        Assert.AreEqual(0, NumericTermComparator.Compare(DecimalFraction(a), IntegerNumber(b)));
        Assert.AreEqual(0, NumericTermComparator.Compare(IntegerNumber(a), DecimalFraction(b)));
        Assert.AreEqual(0, NumericTermComparator.Compare(DecimalFraction(a), DecimalFraction(b)));

        Assert.AreEqual(-1, NumericTermComparator.Compare(IntegerNumber(b), IntegerNumber(a)));
        Assert.AreEqual(0, NumericTermComparator.Compare(DecimalFraction(b), IntegerNumber(a)));
        Assert.AreEqual(0, NumericTermComparator.Compare(IntegerNumber(b), DecimalFraction(a)));
        Assert.AreEqual(0, NumericTermComparator.Compare(DecimalFraction(b), DecimalFraction(a)));
    }

    /**
     * NumericTermComparator provides an overloaded version of {@link NumericTermComparator#Compare(Term, Term)} that
     * also accepts a {@code KnowledgeBase} argument - this method tests that overloaded version.
     *
     * @see NumericTermComparator#Compare(Term, Term, KnowledgeBase)
     * @see #testStructuresRepresentingArithmeticOperators
     */
    [TestMethod]
    public void TestOverloadedCompareMethod()
    {
        Compare("1+1", "5-3", kb, 0);
        Compare("1.5", "3/2.0", kb, 0);
        Compare("7*5", "4*9", kb, -1); //35v36
        Compare("72", "8*9", kb, 0);
        Compare("72", "60+13", kb, -1);
        Compare("72", "74-3", kb, 1);
    }

    /**
     * Test that {@link NumericTermComparator#Compare(Term, Term, KnowledgeBase)} works with {@code Structure}s that
     * represent arithmetic expressions.
     */
    [TestMethod]
    public void TestStructuresRepresentingArithmeticOperators()
    {
        Structure addition = Structure("+", IntegerNumber(1), IntegerNumber(3));
        Structure subtraction = Structure("-", IntegerNumber(5), IntegerNumber(2));

        // test Compare(Term, Term) evaluates structures representing arithmetic expressions
        Assert.AreEqual(1, NumericTermComparator.Compare(addition, subtraction, operators));
        Assert.AreEqual(-1, NumericTermComparator.Compare(subtraction, addition, operators));
        Assert.AreEqual(0, NumericTermComparator.Compare(addition, addition, operators));

        // test Compare(Term, Term, KnowledgeBase) throws a PrologException if
        // a structure cannot be evaluated as an arithmetic expression
        try
        {
            NumericTermComparator.Compare(addition, Structure("-", IntegerNumber(5), Atom()), operators);
            Assert.Fail();
        }
        catch (PrologException e)
        {
            Assert.AreEqual("Cannot find arithmetic operator: test/0", e.Message);
        }
        try
        {
            NumericTermComparator.Compare(Structure("~", IntegerNumber(5), IntegerNumber(2)), subtraction, operators);
            Assert.Fail();
        }
        catch (PrologException e)
        {
            Assert.AreEqual("Cannot find arithmetic operator: ~/2", e.Message);
        }
    }

    private void ComparePrimitives(long i1, long i2)
    {
        CompareIntegers(IntegerNumber(i1), IntegerNumber(i2));
        CompareIntegers(IntegerNumber(i2), IntegerNumber(i1));

        CompareDecimals(DecimalFraction(i1), DecimalFraction(i2));
        CompareDecimals(DecimalFraction(i2), DecimalFraction(i1));

        CompareMixedTypes(DecimalFraction(i1), IntegerNumber(i2));
        CompareMixedTypes(IntegerNumber(i1), DecimalFraction(i2));
    }

    private static void CompareIntegers(IntegerNumber t1, IntegerNumber t2)
    {
        long i1 = t1.Long;
        long i2 = t2.Long;
        Assert.AreEqual(i1.CompareTo(i2), NumericTermComparator.Compare(t1, t2));
        Assert.AreEqual(i2.CompareTo(i1), NumericTermComparator.Compare(t2, t1));
    }

    private static void CompareDecimals(DecimalFraction t1, DecimalFraction t2)
    {
        double d1 = t1.Double;
        double d2 = t2.Double;
        Assert.AreEqual(d1.CompareTo(d2), NumericTermComparator.Compare(t1, t2));
        Assert.AreEqual(d2.CompareTo(d1), NumericTermComparator.Compare(t2, t1));
    }

    private static void CompareMixedTypes(Numeric t1, Numeric t2)
    {
        double d1 = t1.Double;
        double d2 = t2.Double;
        Assert.AreEqual(d1.CompareTo(d2), NumericTermComparator.Compare(t1, t2));
        Assert.AreEqual(d2.CompareTo(d1), NumericTermComparator.Compare(t2, t1));
    }

    private void Compare(string s1, string s2, KnowledgeBase kb, int expected)
    {
        Term t1 = TestUtils.ParseSentence(s1 + ".");
        Term t2 = TestUtils.ParseSentence(s2 + ".");
        Assert.AreEqual(expected, NumericTermComparator.Compare(t1, t2, operators));
        Assert.AreEqual(0 - expected, NumericTermComparator.Compare(t2, t1, operators));
    }
}
