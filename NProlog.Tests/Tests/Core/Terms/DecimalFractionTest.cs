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
namespace Org.NProlog.Core.Terms;

/**
 * @see TermTest
 */
//@RunWith(DataProviderRunner)
[TestClass]
public class DecimalFractionTest : TestUtils
{
    private const double DELTA = 0;

    [TestMethod]
    public void TestGetName()
    {
        Assert.AreEqual("0.0", new DecimalFraction(0).Name);
        Assert.AreEqual(double.MaxValue.ToString(), new DecimalFraction(double.MaxValue).Name);
        Assert.AreEqual("-7.0", new DecimalFraction(-7).Name);
    }

    [TestMethod]
    public void TestToString()
    {
        Assert.AreEqual("0.0", new DecimalFraction(0).ToString());
        Assert.AreEqual(double.MaxValue.ToString(), new DecimalFraction(double.MaxValue).ToString());
        //CHG:-7.0
        Assert.AreEqual("-7.0", new DecimalFraction(-7).ToString());
    }

    [TestMethod]
    public void TestGetTerm()
    {
        var d1 = new DecimalFraction(0);
        var d2 = d1.Term;
        Assert.AreSame(d1, d2);
    }

    [TestMethod]
    public void TestGetBound()
    {
        var d1 = new DecimalFraction(0);
        var d2 = d1.Bound;
        Assert.AreSame(d1, d2);
    }

    [TestMethod]
    public void TestGetLong()
    {
        Assert.AreEqual(0, new DecimalFraction(0).Long);
        //NOTICE: this is wrong because both double and long has 64 bits
        //NOTICE: but they have different range therefore different precision.
        //NOTICE: can not be equal on C#/C++ (just is exceptional)
        Assert.AreNotEqual((long.MaxValue), new DecimalFraction(long.MaxValue).Long);
        Assert.AreEqual(-7, new DecimalFraction(-7.01).Long);
        Assert.AreEqual(-1, new DecimalFraction(-1.01).Long);
    }

    [TestMethod]
    public void TestCalculate()
    {
        var d1 = new DecimalFraction(0);
        var d2 = d1.Calculate(EMPTY_ARRAY);
        Assert.AreSame(d1, d2);
    }

    [TestMethod]
    public void TestGetDouble()
    {
        Assert.AreEqual(0.0, new DecimalFraction(0).Double, DELTA);
        Assert.AreEqual(double.MaxValue, new DecimalFraction(double.MaxValue).Double, DELTA);
        Assert.AreEqual(-7.01, new DecimalFraction(-7.01).Double, DELTA);
    }

    [TestMethod]
    public void TestGetType()
    {
        var d = DecimalFraction();
        Assert.AreSame(TermType.FRACTION, d.Type);
    }

    [TestMethod]
    public void TestGetNumberOfArguments()
    {
        var d = DecimalFraction();
        Assert.AreEqual(0, d.NumberOfArguments);
    }

    [TestMethod]
    //@DataProvider({"-1", "0", "1"})
    public void TestGetArgument()
    {
        for(int index = -1;index<=1;index++)
        try
        {
            var d = DecimalFraction();
            d.GetArgument(index);
            Assert.Fail();
        }
        catch (IndexOutOfRangeException e)
        {
            Assert.AreEqual("index:" + index, e.Message);
        }
    }

    [TestMethod]
    public void TestGetArgs()
    {
        var d = DecimalFraction();
        Assert.AreSame(TermUtils.EMPTY_ARRAY, d.Args);
    }

    [TestMethod]
    public void TestHashCode()
    {
        var n = new DecimalFraction(7);

        Assert.AreEqual(n.GetHashCode(), new DecimalFraction(7).GetHashCode());
        Assert.AreNotEqual(n.GetHashCode(), new DecimalFraction(7.1).GetHashCode());
    }

    /** see {@link TermTest} */
    [TestMethod]
    public void TestEquals()
    {
        var n = new DecimalFraction(7);

        Assert.IsTrue(n.Equals(n));
        Assert.IsTrue(n.Equals(new DecimalFraction(7)));

        Assert.AreNotEqual(n, new DecimalFraction(6));
        Assert.AreNotEqual(n, new DecimalFraction(8));
        Assert.AreNotEqual(n, new DecimalFraction(6.9));
        Assert.AreNotEqual(n, new DecimalFraction(7.1));
        Assert.AreNotEqual(n, new DecimalFraction(0));
        Assert.AreNotEqual(n, new DecimalFraction(-7));
        Assert.AreNotEqual(n, new IntegerNumber(7));
        Assert.AreNotEqual(n, new Atom("7"));
        Assert.AreNotEqual(n, Terms.Structure.CreateStructure("7", new Term[] { n }));
        Assert.AreNotEqual(n, ListFactory.CreateList(n, n));
    }
}
