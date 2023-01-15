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
public class IntegerNumberTest : TestUtils
{
    private static readonly double DELTA = 0;

    [TestMethod]
    public void TestGetName()
    {
        Assert.AreEqual("0", new IntegerNumber(0).Name);
        Assert.AreEqual(long.MaxValue.ToString(), new IntegerNumber(long.MaxValue).Name);
        Assert.AreEqual("-7", new IntegerNumber(-7).Name);
    }

    [TestMethod]
    public void TestToString()
    {
        Assert.AreEqual("0", new IntegerNumber(0).ToString());
        Assert.AreEqual(long.MaxValue.ToString(), new IntegerNumber(long.MaxValue).ToString());
        Assert.AreEqual("-7", new IntegerNumber(-7).ToString());
    }

    [TestMethod]
    public void TestGetTerm()
    {
        IntegerNumber i1 = new IntegerNumber(0);
        IntegerNumber i2 = i1.Term;
        Assert.AreSame(i1, i2);
    }

    [TestMethod]
    public void TestGetBound()
    {
        IntegerNumber i1 = new IntegerNumber(0);
        Term i2 = i1.Bound;
        Assert.AreSame(i1, i2);
    }

    [TestMethod]
    public void TestCalculate()
    {
        IntegerNumber i1 = new IntegerNumber(0);
        IntegerNumber i2 = i1.Calculate(TermUtils.EMPTY_ARRAY);
        Assert.AreSame(i1, i2);
    }

    [TestMethod]
    public void TestGetLong()
    {
        Assert.AreEqual(0, new IntegerNumber(0).Long);
        Assert.AreEqual(long.MaxValue, new IntegerNumber(long.MaxValue).Long);
        Assert.AreEqual(-7, new IntegerNumber(-7).Long);
    }

    [TestMethod]
    public void TestGetDouble()
    {
        Assert.AreEqual(0.0, new IntegerNumber(0).Double, DELTA);
        Assert.AreEqual(int.MaxValue, new IntegerNumber(int.MaxValue).Double, DELTA);
        Assert.AreEqual(-7.0, new IntegerNumber(-7).Double, DELTA);
    }

    [TestMethod]
    public void TestGetType()
    {
        IntegerNumber i = IntegerNumber();
        Assert.AreSame(TermType.INTEGER, i.Type);
    }

    [TestMethod]
    public void TestGetNumberOfArguments()
    {
        IntegerNumber i = IntegerNumber();
        Assert.AreEqual(0, i.NumberOfArguments);
    }

    [TestMethod]
    public void TestGetArgument()
    {
        for (int index = -1; index <= 1; index++)
            try
            {
                IntegerNumber i = IntegerNumber();
                i.GetArgument(index);
                Assert.Fail();
            }
            catch (IndexOutOfRangeException e)
            {
                Assert.AreEqual("index:"+index, e.Message);
            }
    }

    [TestMethod]
    public void TestGetArgs()
    {
        IntegerNumber i = IntegerNumber();
        Assert.AreSame(TermUtils.EMPTY_ARRAY, i.Args);
    }

    [TestMethod]
    public void TestHashCode()
    {
        IntegerNumber n = new IntegerNumber(7);

        Assert.AreEqual(n.GetHashCode(), new IntegerNumber(7).GetHashCode());
        Assert.AreNotEqual(n.GetHashCode(), new IntegerNumber(8).GetHashCode());
    }

    /** see {@link TermTest} */
    [TestMethod]
    public void TestEquals()
    {
        IntegerNumber n = new IntegerNumber(7);

        Assert.IsTrue(n.Equals(n));
        Assert.IsTrue(n.Equals(new IntegerNumber(7)));

        Assert.AreNotEqual(n, new IntegerNumber(6));
        Assert.AreNotEqual(n, new IntegerNumber(8));
        Assert.AreNotEqual(n, new IntegerNumber(0));
        Assert.AreNotEqual(n, new IntegerNumber(-7));
        Assert.AreNotEqual(n, new DecimalFraction(7));
        Assert.AreNotEqual(n, new Atom("7"));
        Assert.AreNotEqual(n, Terms.Structure.CreateStructure("7", new Term[] { n }));
        Assert.AreNotEqual(n, ListFactory.CreateList(n, n));
    }
}
