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

[TestClass]
public class ListFactoryTest : TestUtils
{
    [TestMethod]
    public void TestCreationWithoutTail()
    {
        Term[] args = CreateArguments();
        Term l = ListFactory.CreateList(args);

        foreach (Term arg in args)
        {
            TestIsList(l);
            Assert.AreEqual(arg, l.GetArgument(0));
            l = l.GetArgument(1);
        }

        Assert.AreSame(TermType.EMPTY_LIST, l.Type);
        Assert.AreSame(EmptyList.EMPTY_LIST, l);
    }

    [TestMethod]
    public void TestCreationWithTail()
    {
        Term[] args = CreateArguments();
        Term tail = new Atom("tail");
        Term l = ListFactory.CreateList(args, tail);

        foreach (Term arg in args)
        {
            TestIsList(l);
            Assert.AreEqual(arg, l.GetArgument(0));
            l = l.GetArgument(1);
        }

        Assert.AreSame(tail, l);
    }

    [TestMethod]
    public void TestCreationWithTailButNoHead()
    {
        Term tail = new Atom("tail");
        Assert.AreSame(tail, ListFactory.CreateList(new Term[0], tail));
    }

    /** Check {@link ListFactory#CreateList(Collection)} works the same as {@link ListFactory#CreateList(Term[])} */
    [TestMethod]
    public void TestCreationWithJavaCollection()
    {
        Term[] args = CreateArguments();
        //       Collection<Term> c = Arrays.asList(args);
        var c = args.ToArray();
        Term listFromArray = ListFactory.CreateList(args);
        Term listFromCollection = ListFactory.CreateList(c);
        Assert.AreEqual(listFromCollection, listFromArray);
    }

    [TestMethod]
    public void TestCreateListOfLengthNegative()
    {
        try
        {
            ListFactory.CreateListOfLength(-1);
            Assert.Fail();
        }
        catch (ArgumentException e)
        {
            Assert.AreEqual("Cannot create list of Length: -1", e.Message);
        }
    }

    [TestMethod]
    public void TestCreateListOfLengthZero()
    {
        Assert.AreSame(EmptyList.EMPTY_LIST, ListFactory.CreateListOfLength(0));
    }

    [TestMethod]
    public void TestCreateListOfLengthOne()
    {
        Term t = ListFactory.CreateListOfLength(1);
        Assert.AreSame(typeof(List), t.GetType());
        Assert.IsTrue(t.GetArgument(0).Type.isVariable);
        Assert.AreSame(EmptyList.EMPTY_LIST, t.GetArgument(1));
        Assert.AreEqual(".(E0, [])", t.ToString());
    }

    [TestMethod]
    public void TestCreateListOfLengthThree()
    {
        Term t = ListFactory.CreateListOfLength(3);
        Assert.AreSame(typeof(List), t.GetType());
        Assert.IsTrue(t.GetArgument(0).Type.isVariable);
        Assert.AreSame(typeof(List), t.GetArgument(1).GetType());
        Assert.AreEqual(".(E0, .(E1, .(E2, [])))", t.ToString());
    }

    [TestMethod]
    public void TestCreateListFromTwoAtoms()
    {
        Atom head = Atom("a");
        Atom tail = Atom("b");
        List t = ListFactory.CreateList(head, tail);
        Assert.AreSame(typeof(List), t.GetType());
        Assert.AreSame(head, t.GetArgument(0));
        Assert.AreSame(tail, t.GetArgument(1));
        Assert.IsTrue(t.IsImmutable);
    }

    [TestMethod]
    public void TestCreateListWithVariableHead()
    {
        Variable head = Variable("X");
        Atom tail = Atom("b");
        Term t = ListFactory.CreateList(head, tail);
        Assert.AreSame(typeof(List), t.GetType());
        Assert.AreSame(head, t.GetArgument(0));
        Assert.AreSame(tail, t.GetArgument(1));
        Assert.IsFalse(t.IsImmutable);
    }

    [TestMethod]
    public void TestCreateListWithVariableTail()
    {
        Atom head = Atom("a");
        Variable tail = Variable("Y");
        Term t = ListFactory.CreateList(head, tail);
        Assert.AreSame(typeof(List), t.GetType());
        Assert.AreSame(head, t.GetArgument(0));
        Assert.AreSame(tail, t.GetArgument(1));
        Assert.IsFalse(t.IsImmutable);
    }

    [TestMethod]
    public void TestCreateListWithVariableHeadAndTail()
    {
        Variable head = Variable("X");
        Variable tail = Variable("Y");
        Term t = ListFactory.CreateList(head, tail);
        Assert.AreSame(typeof(List), t.GetType());
        Assert.AreSame(head, t.GetArgument(0));
        Assert.AreSame(tail, t.GetArgument(1));
        Assert.IsFalse(t.IsImmutable);
    }

    private static Term[] CreateArguments()
    {
        return new Term[] { Atom(), Structure(), IntegerNumber(), DecimalFraction(), Variable() };
    }

    private static void TestIsList(Term l)
    {
        Assert.AreEqual(".", l.Name);
        Assert.AreEqual(TermType.LIST, l.Type);
        Assert.AreEqual(2, l.NumberOfArguments);
    }
}
