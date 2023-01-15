/*
 * Copyright 2021 S. Webber
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
using Org.NProlog.Core.Terms;

namespace Org.NProlog;

[TestClass]
public class TermFactoryTest
{
    [TestMethod]
    public void TestAtom()
    {
        Atom a = TermFactory.Atom();
        Assert.AreEqual("test", a.Name);
    }

    [TestMethod]
    public void TestAtomByName()
    {
        string name = "testAtom" + DateTime.Now.Millisecond;// DateTime.Now.Millisecond;
        Atom a = TermFactory.Atom(name);
        Assert.AreEqual(name, a.Name);
    }

    [TestMethod]
    public void TestStructure()
    {
        Structure s = TermFactory.Structure();
        Assert.AreEqual("test", s.Name);
        Assert.AreEqual(1, s.NumberOfArguments);
        Assert.AreEqual(new Atom("test"), s.GetArgument(0));
    }

    [TestMethod]
    public void TestStructureByNameAndArgs()
    {
        string name = "testStructure" + DateTime.Now.Millisecond;
        Term arg1 = new Atom("first argument");
        Term arg2 = new Atom("second argument");
        Term arg3 = new Atom("third argument");
        Structure s = TermFactory.Structure(name, arg1, arg2, arg3);
        Assert.AreEqual(name, s.Name);
        Assert.AreEqual(3, s.NumberOfArguments);
        Assert.AreSame(arg1, s.GetArgument(0));
        Assert.AreSame(arg2, s.GetArgument(1));
        Assert.AreSame(arg3, s.GetArgument(2));
    }

    [TestMethod]
    public void TestList()
    {
        Term arg1 = new Atom("first argument");
        Term arg2 = new Atom("second argument");
        Term arg3 = new Atom("third argument");
        List list = TermFactory.List(arg1, arg2, arg3);
        Assert.AreSame(arg1, list.GetArgument(0));
        list = (List)list.GetArgument(1);
        Assert.AreSame(arg2, list.GetArgument(0));
        list = (List)list.GetArgument(1);
        Assert.AreSame(arg3, list.GetArgument(0));
        Assert.AreSame(EmptyList.EMPTY_LIST, list.GetArgument(1));
    }

    [TestMethod]
    public void TestIntegerNumber()
    {
        IntegerNumber i = TermFactory.IntegerNumber();
        Assert.AreEqual(1, i.Long);
    }

    [TestMethod]
    public void TestIntegerNumberByValue()
    {
        long value = Random.Shared.NextInt64();
        IntegerNumber i = TermFactory.IntegerNumber(value);
        Assert.AreEqual(value, i.Long);
    }

    [TestMethod]
    public void TestDecimalFraction()
    {
        DecimalFraction d = TermFactory.DecimalFraction();
        Assert.AreEqual(1.0, d.Double, 0);
    }

    [TestMethod]
    public void TestDecimalFractionByValue()
    {
        double value = Random.Shared.NextDouble();
        DecimalFraction d = TermFactory.DecimalFraction(value);
        Assert.AreEqual(value, d.Double, 0);
    }

    [TestMethod]
    public void TestVariable()
    {
        Variable v = TermFactory.Variable();
        Assert.AreEqual("X", v.Id);
    }

    [TestMethod]
    public void TestVariableById()
    {
        string id = "testVariable" + DateTime.Now.Millisecond;// DateTime.Now.Millisecond;
        Variable v = TermFactory.Variable(id);
        Assert.AreEqual(id, v.Id);
    }
}
