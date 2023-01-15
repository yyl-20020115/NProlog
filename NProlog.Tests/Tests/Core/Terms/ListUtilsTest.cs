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

namespace Org.NProlog.Core.Terms;

[TestClass]
public class ListUtilsTest : TestUtils
{
    [TestMethod]
    public void TestIsMemberTrue()
    {
        List list = TermFactory.List(Atom("x"), Atom("y"), Atom("z"));
        Assert.IsTrue(ListUtils.IsMember(Atom("x"), list));
        Assert.IsTrue(ListUtils.IsMember(Atom("y"), list));
        Assert.IsTrue(ListUtils.IsMember(Atom("z"), list));
    }

    [TestMethod]
    public void TestIsMemberFailure()
    {
        List list = TermFactory.List(Atom("x"), Atom("y"), Atom("z"));
        Assert.IsFalse(ListUtils.IsMember(Atom("w"), list));
    }

    [TestMethod]
    public void TestIsMemberEmptyList()
    {
        Assert.IsFalse(ListUtils.IsMember(Atom(), EmptyList.EMPTY_LIST));
    }

    [TestMethod]
    public void TestIsMemberVariable()
    {
        Atom x = Atom("x");
        List list = TermFactory.List(x, Atom("y"), Atom("z"));
        Variable v = Variable();
        Assert.IsTrue(ListUtils.IsMember(v, list));
        Assert.AreSame(x, v.Term);
    }

    [TestMethod]
    public void TestIsMemberVariablesAsArgumentsOfStructures()
    {
        Term list = ParseTerm("[p(a, B, 2),p(q, b, C),p(A, b, 5)]");
        Term element = ParseTerm("p(X,b,5)");
        Assert.IsTrue(ListUtils.IsMember(element, list));
        Assert.AreEqual("[p(a, B, 2),p(q, b, 5),p(A, b, 5)]", Write(list));
        Assert.AreEqual("p(q, b, 5)", Write(element));
    }

    [TestMethod]
    public void TestIsMemberVariableTail()
    {
        Variable tail = Variable();
        Term list = ListFactory.CreateList(new Term[] { Atom("x"), Atom("y"), Atom("z") }, tail);

        Assert.IsTrue(ListUtils.IsMember(Atom("x"), list));
        Assert.AreEqual(".(x, .(y, .(z, X)))", list.ToString());
        Assert.AreSame(tail, tail.Term);

        Assert.IsTrue(ListUtils.IsMember(Atom("y"), list));
        Assert.AreEqual(".(x, .(y, .(z, X)))", list.ToString());
        Assert.AreSame(tail, tail.Term);

        Assert.IsTrue(ListUtils.IsMember(Atom("z"), list));
        Assert.AreEqual(".(x, .(y, .(z, X)))", list.ToString());
        Assert.AreSame(tail, tail.Term);

        Atom q = Atom("q");
        Assert.IsTrue(ListUtils.IsMember(q, list));
        Assert.AreEqual(".(x, .(y, .(z, .(q, _))))", list.ToString());
        Assert.AreNotSame(tail, tail.Term);
        Assert.AreSame(TermType.LIST, tail.Type);
        Assert.AreSame(q, tail.GetArgument(0));
        Assert.AreSame(TermType.VARIABLE, tail.GetArgument(1).Type);

        Term newList = list.Term;
        Term newTail = tail.GetArgument(1);
        Atom w = Atom("w");
        Assert.IsTrue(ListUtils.IsMember(w, newList));
        Assert.AreEqual(".(x, .(y, .(z, .(q, .(w, _)))))", newList.ToString());
        Assert.AreNotSame(newTail, newTail.Term);
        Assert.AreSame(TermType.LIST, newTail.Type);
        Assert.AreSame(w, newTail.GetArgument(0));
        Assert.AreSame(TermType.VARIABLE, newTail.GetArgument(1).Type);
    }

    [TestMethod]
    public void TestIsMemberAtomTail()
    {
        Atom tail = Atom("test");
        Term list = ListFactory.CreateList(new Term[] { Atom("x"), Atom("y"), Atom("z") }, tail);

        Assert.IsTrue(ListUtils.IsMember(Atom("x"), list));
        Assert.AreEqual(".(x, .(y, .(z, test)))", list.ToString());
        Assert.AreSame(tail, tail.Term);

        Assert.IsTrue(ListUtils.IsMember(Atom("y"), list));
        Assert.AreEqual(".(x, .(y, .(z, test)))", list.ToString());
        Assert.AreSame(tail, tail.Term);

        Assert.IsTrue(ListUtils.IsMember(Atom("z"), list));
        Assert.AreEqual(".(x, .(y, .(z, test)))", list.ToString());
        Assert.AreSame(tail, tail.Term);

        try
        {
            Assert.IsTrue(ListUtils.IsMember(Atom("q"), list));
            Assert.Fail();
        }
        catch (PrologException e)
        {
            Assert.AreEqual("Expected empty list or variable but got: ATOM with value: test", e.Message);
        }
    }

    [TestMethod]
    public void TestIsMemberInvalidArgumentList()
    {
        try
        {
            ListUtils.IsMember(Atom(), Atom("a"));
            Assert.Fail();
        }
        catch (PrologException e)
        {
            Assert.AreEqual("Expected list or empty list but got: ATOM with value: a", e.Message);
        }
    }

    [TestMethod]
    public void TestToJavaUtilList()
    {
        Term[] arguments = CreateArguments();
        List prologList = (List)ListFactory.CreateList(arguments);
        List<Term> list = ListUtils.ToList(prologList);
        Assert.AreEqual(arguments.Length, list.Count);
        for (int i = 0; i < arguments.Length; i++)
        {
            Assert.AreSame(arguments[i], list[(i)]);
        }
    }

    [TestMethod]
    public void TestToJavaUtilListPartialList()
    {
        List list = (List)ListFactory.CreateList(CreateArguments(), Atom("tail"));
        Assert.IsNull(ListUtils.ToList(list));
    }

    [TestMethod]
    public void TestToJavaUtilListEmptyList()
    {
        List<Term> javaUtilList = ListUtils.ToList(EmptyList.EMPTY_LIST);
        Assert.IsTrue(javaUtilList.Count == 0);
    }

    [TestMethod]
    public void TestToJavaUtilListNonListArguments()
    {
        Assert.IsNull(ListUtils.ToList(Variable()));
        Assert.IsNull(ListUtils.ToList(Atom()));
        Assert.IsNull(ListUtils.ToList(Structure()));
        Assert.IsNull(ListUtils.ToList(IntegerNumber()));
        Assert.IsNull(ListUtils.ToList(DecimalFraction()));
        Assert.IsNull(ListUtils.ToList(new Variable()));
    }

    [TestMethod]
    public void TestToSortedUtilList()
    {
        Atom z = Atom("z");
        Atom a = Atom("a");
        Atom h = Atom("h");
        Atom q = Atom("q");
        // include multiple 'a's to test duplicates are not removed
        List list = (List)ListFactory.CreateList(new Term[] { z, a, a, h, a, q });
        List<Term> sortedList = ListUtils.ToSortedList(list);
        Assert.AreEqual(6, sortedList.Count);
        Assert.AreSame(a, sortedList[(0)]);
        Assert.AreSame(a, sortedList[(1)]);
        Assert.AreSame(a, sortedList[(2)]);
        Assert.AreSame(h, sortedList[(3)]);
        Assert.AreSame(q, sortedList[(4)]);
        Assert.AreSame(z, sortedList[(5)]);
    }

    [TestMethod]
    public void TestToSortedJavaUtilListEmptyList()
    {
        List<Term> javaUtilList = ListUtils.ToSortedList(EmptyList.EMPTY_LIST);
        Assert.IsTrue(javaUtilList.Count == 0);
    }

    [TestMethod]
    public void TestToSortedJavaUtilListEmptyListNonListArguments()
    {
        Assert.IsNull(ListUtils.ToSortedList(Variable()));
        Assert.IsNull(ListUtils.ToSortedList(Atom()));
        Assert.IsNull(ListUtils.ToSortedList(Structure()));
        Assert.IsNull(ListUtils.ToSortedList(IntegerNumber()));
        Assert.IsNull(ListUtils.ToSortedList(DecimalFraction()));
        Assert.IsNull(ListUtils.ToSortedList(new Variable()));
    }

    private static Term[] CreateArguments()
    {
        return new Term[] { Atom(), Structure(), IntegerNumber(), DecimalFraction(), Variable() };
    }
}
