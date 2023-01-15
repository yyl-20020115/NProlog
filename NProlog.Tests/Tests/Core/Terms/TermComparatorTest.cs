/*
 * Copyright 2013-2014 S. Webber
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
namespace Org.NProlog.Core.Terms;

[TestClass]
public class TermComparatorTest : TestUtils
{
    /**
     * selection of terms ordered in lowest precedence first order
     * <p>
     * Note: only one variable and no ANONYMOUS_VARIABLE as ANONYMOUS_VARIABLE and "variable against variable"
     * comparisons tested separately.
     */
    private static readonly Term[] TERMS_ORDERED_IN_LOWEST_PRECEDENCE = {Variable("A"),
       DecimalFraction(-2.1), DecimalFraction(-1.9), DecimalFraction(0), DecimalFraction(1),
       IntegerNumber(-2), IntegerNumber(0), IntegerNumber(1),
       EmptyList.EMPTY_LIST,
       Atom("a"), Atom("z"),
       Structure("a", Atom("b")), Structure("b", Atom("a")), Structure("b", Structure("a", Atom())),
       Structure("!", Atom("a"), Atom("b")),
       List(Atom("a"), Atom("b")), List(Atom("b"), Atom("a")), List(Atom("b"), Atom("a"), Atom("b")), List(Atom("c"), Atom("a")), List(Structure("a", Atom()), Atom("b")),
       Structure("a", Atom("a"), Atom("b")), Structure("a", Atom("a"), Atom("z")), Structure("a", Atom("a"), Structure("z", Atom()))};

    [TestMethod]
    public void TestCompareTerms()
    {
        for (int i = 0; i < TERMS_ORDERED_IN_LOWEST_PRECEDENCE.Length; i++)
        {
            Term t1 = TERMS_ORDERED_IN_LOWEST_PRECEDENCE[i];
            TestEqual(t1, t1);
            for (int z = i + 1; z < TERMS_ORDERED_IN_LOWEST_PRECEDENCE.Length; z++)
            {
                Term t2 = TERMS_ORDERED_IN_LOWEST_PRECEDENCE[z];
                TestIsGreater(t2, t1);

                Term v1 = Variable("X");
                Term v2 = Variable("Y");
                v1.Unify(t1);
                v2.Unify(t2);
                TestIsGreater(v2, t1);
                TestIsGreater(t2, v1);
                TestIsGreater(v2, v1);
            }
        }
    }

    [TestMethod]
    public void TestVariablesAssignedToEachOther()
    {
        Atom a = Atom("a");
        Variable x = new Variable("X");
        Variable y = new Variable("Y");
        Variable z = new Variable("Z");

        TestNotEqual(z, y);
        TestNotEqual(z, x);
        TestNotEqual(y, x);

        x.Unify(z);

        TestNotEqual(z, y);
        TestEqual(z, x);
        TestNotEqual(x, y);

        x.Unify(Atom("a"));

        TestEqual(x, z);
        TestEqual(x, a);
        TestEqual(z, a);
        TestIsGreater(x, y);
        TestIsGreater(z, y);

        y.Unify(x);
        TestEqual(x, y);
        TestEqual(x, z);
        TestEqual(y, z);
        TestEqual(z, a);
    }

    private static void TestNotEqual(Term t1, Term t2)
    {
        Assert.IsTrue(TermComparator.TERM_COMPARATOR.Compare(t1, t2) != 0, t1 + " " + t2);
        Assert.IsTrue(TermComparator.TERM_COMPARATOR.Compare(t2, t1) != 0, t2 + " " + t1);
    }

    private static void TestEqual(Term t1, Term t2)
    {
        Assert.AreEqual(0, TermComparator.TERM_COMPARATOR.Compare(t1, t2), t1 + " " + t2);
        Assert.AreEqual(0, TermComparator.TERM_COMPARATOR.Compare(t2, t1), t2 + " " + t1);
    }

    private static void TestIsGreater(Term t1, Term t2)
    {
        Assert.IsTrue(TermComparator.TERM_COMPARATOR.Compare(t1, t2) > 0, t1 + " " + t2);
        Assert.IsTrue(TermComparator.TERM_COMPARATOR.Compare(t2, t1) < 0, t2 + " " + t1);
    }
}
