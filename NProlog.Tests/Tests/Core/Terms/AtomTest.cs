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
[TestClass]
public class AtomTest : TestUtils
{
    [TestMethod]
    public void TestGetName()
    {
        var a = new Atom("test");
        Assert.AreEqual("test", a.Name);
    }

    [TestMethod]
    public void TestToString()
    {
        var a = new Atom("test");
        Assert.AreEqual("test", a.ToString());
    }

    [TestMethod]
    public void TestGetTerm()
    {
        var a = Atom();
        var b = a.Term;
        Assert.AreSame(a, b);
    }

    [TestMethod]
    public void TestGetBound()
    {
        var a = Atom();
        Term b = a.Bound;
        Assert.AreSame(a, b);
    }

    [TestMethod]
    public void TestGetType()
    {
        var a = Atom();
        Assert.AreSame(TermType.ATOM, a.Type);
    }

    [TestMethod]
    public void TestGetNumberOfArguments()
    {
        var a = Atom();
        Assert.AreEqual(0, a.NumberOfArguments);
    }

    [TestMethod]
    //@DataProvider({"-1", "0", "1"})
    public void TestGetArgument()
    {
        for (int index = -1; index <= 1; index++)
        {
            try
            {
                var a = Atom();
                a.GetArgument(index);
                Assert.Fail();
            }
            catch (IndexOutOfRangeException e)
            {
                Assert.AreEqual("index:" + index, e.Message);
            }
        }
    }

    [TestMethod]
    public void TestGetArgs()
    {
        var a = Atom();
        Assert.AreSame(TermUtils.EMPTY_ARRAY, a.Args);
    }

    [TestMethod]
    public void TestHashCode()
    {
        var a = new Atom("test");

        Assert.AreEqual(a.GetHashCode(), new Atom("test").GetHashCode());
        Assert.AreNotEqual(a.GetHashCode(), new Atom("abcd").GetHashCode());
    }

    /** see {@link TermTest} */
    [TestMethod]
    public void TestEquals()
    {
        var a = new Atom("test");

        Assert.IsTrue(a.Equals(a));
        Assert.IsTrue(a.Equals(new Atom(a.Name)));

        Assert.AreNotEqual(new Atom("7"), new IntegerNumber(7));
        Assert.AreNotEqual(new Atom("7"), new DecimalFraction(7));
        Assert.AreNotEqual(a, new Atom(a.Name + " "));
        Assert.AreNotEqual(a, new Atom(a.Name + "x"));
        Assert.AreNotEqual(a, new Atom(" " + a.Name));
        Assert.AreNotEqual(a, new Atom("x" + a.Name));
        Assert.AreNotEqual(a, new Atom("TEST"));
        Assert.AreNotEqual(a, new Atom("tes"));
        Assert.AreNotEqual(a, Terms.Structure.CreateStructure(a.Name, new Term[] { a }));
        Assert.AreNotEqual(a, ListFactory.CreateList(a, a));
    }
}
