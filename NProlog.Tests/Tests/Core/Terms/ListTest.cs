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
using System.Text;

namespace Org.NProlog.Core.Terms;

/**
 * @see TermTest
 */
//@RunWith(DataProviderRunner)
[TestClass]
public class ListTest : TestUtils
{
    private const int LONG_LIST_SIZE = 1000;

    [TestMethod]
    public void TestGetName()
    {
        var testList = new LinkedTermList(new Atom("a"), new Atom("b"));
        Assert.AreEqual(".", testList.Name);
    }

    [TestMethod]
    public void TestToString()
    {
        var testList = new LinkedTermList(new Atom("a"), new Atom("b"));
        Assert.AreEqual(".(a, b)", testList.ToString());
    }

    [TestMethod]
    public void TestMutableListGetTerm()
    {
        var testList = new LinkedTermList(new Atom("a"), new Atom("b"));
        var l = testList.Term;
        Assert.AreSame(testList, l);
    }

    [TestMethod]
    public void TestImmutableListGetTerm()
    {
        var a = new Atom("a");
        var b = new Atom("b");
        var c = new Atom("c");
        var x = new Variable("X");
        var y = new Variable("Y");
        var z = new Variable("Z");
        var sublist = ListFactory.CreateList(y, z);
        var originalList = ListFactory.CreateList(x, sublist);

        Assert.AreSame(originalList, originalList.Term);

        x.Unify(a);

        Assert.AreSame(x, originalList.GetArgument(0));
        Assert.AreSame(sublist, originalList.GetArgument(1));

        var newList = originalList.Term;
        Assert.AreNotSame(originalList, newList);
        Assert.AreSame(a, newList.GetArgument(0));
        Assert.AreSame(sublist, newList.GetArgument(1));

        z.Unify(c);

        newList = originalList.Term;
        Assert.AreNotSame(originalList, newList);
        Assert.AreSame(a, newList.GetArgument(0));
        Assert.AreNotSame(sublist, newList.GetArgument(1));
        Assert.AreSame(y, newList.GetArgument(1).GetArgument(0));
        Assert.AreSame(c, newList.GetArgument(1).GetArgument(1));

        x.Backtrack();
        z.Backtrack();
        y.Unify(b);

        newList = originalList.Term;
        Assert.AreNotSame(originalList, newList);
        Assert.AreSame(x, newList.GetArgument(0));
        Assert.AreNotSame(sublist, newList.GetArgument(1));
        Assert.AreSame(b, newList.GetArgument(1).GetArgument(0));
        Assert.AreSame(z, newList.GetArgument(1).GetArgument(1));
    }

    [TestMethod]
    public void TestGetType()
    {
        var testList = new LinkedTermList(new Atom("a"), new Atom("b"));
        Assert.AreSame(TermType.LIST, testList.Type);
    }

    [TestMethod]
    public void TestGetNumberOfArguments()
    {
        var testList = new LinkedTermList(new Atom("a"), new Atom("b"));
        Assert.AreEqual(2, testList.NumberOfArguments);
    }

    [TestMethod]
    public void TestGetArgument()
    {
        var head = new Atom("a");
        var tail = new Atom("b");
        var testList = new LinkedTermList(head, tail);
        Assert.AreSame(head, testList.GetArgument(0));
        Assert.AreSame(tail, testList.GetArgument(1));
    }

    [TestMethod]
    public void TestGetArgumentIndexOutOfBounds()
    {
        for (int index = -1; index <= 1; index++)
        {
            var testList = new LinkedTermList(new Atom("a"), new Atom("b"));
            try
            {
                testList.GetArgument(index);
                //Assert.Fail();
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
        var head = new Atom("a");
        var tail = new Atom("b");
        var testList = new LinkedTermList(head, tail);
        var args = testList.Args;
        Assert.AreEqual(2, args.Length);
        Assert.AreSame(head, args[0]);
        Assert.AreSame(tail, args[1]);
        Assert.AreNotSame(args, testList.Args);
    }

    [TestMethod]
    public void TestCopyNoVariableElements()
    {
        var testList = new LinkedTermList(new Atom("a"), new Atom("b"));
        Assert.AreSame(testList, testList.Copy(new()));
    }

    [TestMethod]
    public void TestCopyVariableElements()
    {
        var a = new Atom("a");
        var b = new Atom("b");
        var X = new Variable("X");
        var Y = new Variable("Y");
        var head = Structure("p", X);

        var original = new LinkedTermList(head, Y); // [p(X), Y]

        Assert.AreSame(original, original.Term);

        Dictionary<Variable, Variable> sharedVariables = new();
        var copy1 = original.Copy(sharedVariables);
        Assert.AreNotSame(original, copy1);
        AssertStrictEquality(original, copy1, false);
        Assert.AreEqual(2, sharedVariables.Count);
        Assert.IsTrue(sharedVariables.ContainsKey(X));
        Assert.IsTrue(sharedVariables.ContainsKey(Y));
        Assert.AreEqual(original.ToString(), copy1.ToString());

        Assert.IsTrue(X.Unify(a));
        Assert.IsTrue(Y.Unify(b));

        var copy2 = original.Copy(new());
        Assert.AreNotSame(original, copy2);
        AssertStrictEquality(original, copy2, true);
        Assert.AreEqual(original.ToString(), copy2.ToString());
        Assert.AreSame(copy2, copy2.Copy(new()));
        Assert.AreSame(copy2, copy2.Term);

        X.Backtrack();
        Y.Backtrack();

        AssertStrictEquality(original, copy2, false);

        Assert.AreEqual(".(p(X), Y)", original.ToString());
        Assert.AreEqual(".(p(a), b)", copy2.ToString());
    }

    [TestMethod]
    public void TestImmutableListCopy()
    {
        var a = new Atom("a");
        var b = new Atom("b");
        var c = new Atom("c");
        var x = new Variable("X");
        var y = new Variable("Y");
        var z = new Variable("Z");
        var sublist = ListFactory.CreateList(y, z);
        var originalList = ListFactory.CreateList(x, sublist);

        Dictionary<Variable, Variable> variables = new();
        var newList = originalList.Copy(variables);
        Assert.AreNotSame(originalList, newList);
        Assert.AreSame(variables[(x)], newList.GetArgument(0));
        Assert.AreSame(variables[(y)], newList.GetArgument(1).GetArgument(0));
        Assert.AreSame(variables[(z)], newList.GetArgument(1).GetArgument(1));

        x.Unify(a);

        Assert.AreSame(x, originalList.GetArgument(0));
        Assert.AreSame(sublist, originalList.GetArgument(1));

        variables = new();
        newList = originalList.Copy(variables);
        Assert.AreNotSame(originalList, newList);
        Assert.AreSame(a, newList.GetArgument(0));
        Assert.AreSame(variables[(y)], newList.GetArgument(1).GetArgument(0));
        Assert.AreSame(variables[(z)], newList.GetArgument(1).GetArgument(1));

        z.Unify(c);

        variables = new();
        newList = originalList.Copy(variables);
        Assert.AreNotSame(originalList, newList);
        Assert.AreSame(a, newList.GetArgument(0));
        Assert.AreNotSame(sublist, newList.GetArgument(1));
        Assert.AreSame(variables[(y)], newList.GetArgument(1).GetArgument(0));
        Assert.AreSame(c, newList.GetArgument(1).GetArgument(1));

        x.Backtrack();
        z.Backtrack();
        y.Unify(b);

        variables = new();
        newList = originalList.Copy(variables);
        Assert.AreNotSame(originalList, newList);
        Assert.AreSame(variables[(x)], newList.GetArgument(0));
        Assert.AreNotSame(sublist, newList.GetArgument(1));
        Assert.AreSame(b, newList.GetArgument(1).GetArgument(0));
        Assert.AreSame(variables[(z)], newList.GetArgument(1).GetArgument(1));
    }

    [TestMethod]
    public void TestImmutableHeadMutableTailCopy()
    {
        var x = new Variable("X");
        var a = new Atom("a");
        var b = new Atom("b");
        var sublist = ListFactory.CreateList(a, b);
        var originalList = ListFactory.CreateList(x, sublist);

        Dictionary<Variable, Variable> variables = new();
        var newList = originalList.Copy(variables);
        Assert.AreSame(variables[(x)], newList.GetArgument(0));
        Assert.AreSame(sublist, newList.GetArgument(1));
    }

    [TestMethod]
    public void TestGetValueNoVariableElements()
    {
        var testList = new LinkedTermList(new Atom("a"), new Atom("b"));
        Assert.AreSame(testList, testList.Term);
    }

    [TestMethod]
    public void TestListWithVariableArguments()
    {
        var a = new Atom("a");
        var b = new Atom("b");
        var X = new Variable("X");
        var Y = new Variable("Y");
        var l1 = new LinkedTermList(a, Y);
        var l2 = new LinkedTermList(X, b);

        AssertStrictEqualityUnifyAndBacktrack(l1, l2);
        AssertStrictEqualityUnifyAndBacktrack(l2, l1);
    }

    [TestMethod]
    public void TestUnifyWhenBothListsHaveVariableArguments1()
    {
        // [x, Y]
        var l1 = new LinkedTermList(new Atom("x"), new Variable("Y"));
        // [X, y]
        var l2 = new LinkedTermList(new Variable("X"), new Atom("y"));
        Assert.IsTrue(l1.Unify(l2));
        Assert.AreEqual(".(x, y)", l1.ToString());
        Assert.AreEqual(l1.ToString(), l2.ToString());
    }

    [TestMethod]
    public void TestUnifyWhenBothListsHaveVariableArguments2()
    {
        // [x, z]
        var l1 = new LinkedTermList(new Atom("x"), new Atom("z"));
        // [X, y]
        var l2 = new LinkedTermList(new Variable("X"), new Atom("y"));
        Assert.IsFalse(l1.Unify(l2));
        Assert.AreEqual(".(x, z)", l1.ToString());
        // Note: following is expected quirk - list doesn't automatically Backtrack on failure
        Assert.AreEqual(".(x, y)", l2.ToString());

        l2.Backtrack();
        Assert.AreEqual(".(X, y)", l2.ToString());
    }

    [TestMethod]
    public void TestUnifyWhenBothListsHaveVariableArguments3()
    {
        // [X, z]
        var l1 = new LinkedTermList(new Variable("X"), new Atom("z"));
        // [x, y]
        var l2 = new LinkedTermList(new Atom("x"), new Atom("y"));
        Assert.IsFalse(l1.Unify(l2));
        // Note: following is expected quirk - list doesn't automatically Backtrack on failure
        Assert.AreEqual(".(x, z)", l1.ToString());
        Assert.AreEqual(".(x, y)", l2.ToString());

        l1.Backtrack();
        Assert.AreEqual(".(X, z)", l1.ToString());
    }

    /** A long chain of lists where the majority of heads are immutable and all tails are not variables. */
    [TestMethod]
    public void TestLongList()
    {
        var bigListSyntaxBuilder1 = new StringBuilder("[");
        var bigListSyntaxBuilder2 = new StringBuilder("[");
        for (int i = 0; i < LONG_LIST_SIZE; i++)
        {
            if (i != 0)
            {
                bigListSyntaxBuilder1.Append(',');
                bigListSyntaxBuilder2.Append(',');
            }
            bigListSyntaxBuilder1.Append(i);
            // make one element in second list different than first
            if (i == LONG_LIST_SIZE / 4)
            {
                bigListSyntaxBuilder2.Append(i - 1);
            }
            else
            {
                bigListSyntaxBuilder2.Append(i);
            }
        }
        bigListSyntaxBuilder1.Append(']');
        bigListSyntaxBuilder2.Append(']');
        var bigListSyntax1 = bigListSyntaxBuilder1.ToString();
        var bigListSyntax2 = bigListSyntaxBuilder2.ToString();
        var t1 = (LinkedTermList)ParseSentence(bigListSyntax1 + ".");
        var t2 = (LinkedTermList)ParseSentence(bigListSyntax1 + ".");
        var t3 = (LinkedTermList)ParseSentence(bigListSyntax2 + ".");
        Assert.AreNotSame(t1, t2);
        // NOTE important to test write method doesn't throw stackoverflow
        Assert.AreEqual(bigListSyntax1, Write(t1));
        Assert.AreEqual(bigListSyntax2, Write(t3));
        // NOTE important to test ToString, Equals and Unify methods don't throw stackoverflow
        AssertMatch(t1, t1, true);
        AssertMatch(t1, t2, true);
        AssertMatch(t1, t3, false);
    }

    /** A long chain of lists where all the heads are variables and all tails are not variables. */
    [TestMethod]
    public void TestLongListWithMutableElements()
    {
        var bigListSyntaxBuilder1 = new StringBuilder("[");
        var bigListSyntaxBuilder2 = new StringBuilder("[");
        for (int i = 0; i < LONG_LIST_SIZE; i++)
        {
            if (i != 0)
            {
                bigListSyntaxBuilder1.Append(',');
                bigListSyntaxBuilder2.Append(',');
            }
            if (i == LONG_LIST_SIZE - 1)
            {
                bigListSyntaxBuilder1.Append('X');
            }
            else
            {
                bigListSyntaxBuilder1.Append(i);
            }
            if (i == LONG_LIST_SIZE - 2)
            {
                bigListSyntaxBuilder2.Append('Y');
            }
            else
            {
                bigListSyntaxBuilder2.Append(i);
            }
        }
        bigListSyntaxBuilder1.Append(']');
        bigListSyntaxBuilder2.Append(']');
        var bigListSyntax1 = bigListSyntaxBuilder1.ToString();
        var bigListSyntax2 = bigListSyntaxBuilder2.ToString();
        var t1 = (LinkedTermList)ParseSentence(bigListSyntax1 + ".");
        var t2 = (LinkedTermList)ParseSentence(bigListSyntax2 + ".");
        Assert.AreSame(t1, t1.Term);
        Assert.AreSame(t2, t2.Term);
        Assert.AreEqual(bigListSyntax1, Write(t1));
        Assert.AreEqual(bigListSyntax2, Write(t2));
        AssertStrictEquality(t1, t2, false);
        Assert.IsTrue(t1.Unify(t2));
        AssertStrictEquality(t1, t2, true);
        Assert.AreNotSame(t1, t1.Term);
        Assert.AreNotSame(t2, t2.Term);
        AssertStrictEquality(t1.Term, t2.Term, true);
        t1.Backtrack();
        t2.Backtrack();
        AssertStrictEquality(t1, t2, false);

        var t1Copy = t1.Copy(new());
        Assert.AreEqual(bigListSyntax1, Write(t1Copy));
        AssertStrictEquality(t1, t1Copy, false);
        var t2Copy = t2.Copy(new());
        Assert.AreEqual(bigListSyntax2, Write(t2Copy));
        AssertStrictEquality(t2, t2Copy, false);
    }

    /**
     * A long chain of lists where all heads and tails are variables.
     * <p>
     * Heads are unified to atoms and tails are unified with the next list in the chain.
     */
    [TestMethod]
    public void TestLongListWithVariableTails()
    {
        Term input = EmptyList.EMPTY_LIST;
        var atoms = new Term[LONG_LIST_SIZE];
        var lists = new Term[LONG_LIST_SIZE];
        for (int i = 0; i < LONG_LIST_SIZE; i++)
        {
            var atom = new Atom("atom" + i);
            atoms[i] = atom;
            var head = new Variable("H" + i);
            head.Unify(atom);
            var _tail = new Variable("T" + i);
            _tail.Unify(input);
            input = new LinkedTermList(head, _tail);
            lists[i] = input;
        }

        var output = input.Term;
        Assert.AreNotSame(input, output);
        AssertStrictEquality(input, output, true);
        for (int i = LONG_LIST_SIZE - 1; i > -1; i--)
        {
            Assert.AreSame(typeof(LinkedTermList), output.GetType());
            Assert.AreSame(atoms[i], output.GetArgument(0));
            output = output.GetArgument(1);
        }
        Assert.AreSame(EmptyList.EMPTY_LIST, output);

        var tail = input.GetArgument(1).Bound;
        input.Backtrack();
        Assert.AreSame(TermType.VARIABLE, input.GetArgument(0).Type);
        Assert.AreEqual("H" + (LONG_LIST_SIZE - 1), input.GetArgument(0).ToString());
        Assert.AreSame(TermType.VARIABLE, input.GetArgument(1).Type);
        Assert.AreEqual("T" + (LONG_LIST_SIZE - 1), input.GetArgument(1).ToString());

        bool first = true;
        for (int i = LONG_LIST_SIZE - 2; i > -1; i--)
        {
            Assert.AreSame(first ? typeof(LinkedTermList) : typeof(Variable), tail.GetType());
            first = false;
            Assert.AreSame(TermType.LIST, tail.Type);
            Assert.AreSame(atoms[i], tail.GetArgument(0).Bound);
            tail = tail.GetArgument(1);
        }
        Assert.AreSame(typeof(Variable), tail.GetType());
        Assert.AreSame(EmptyList.EMPTY_LIST, tail.Bound);
    }

    [TestMethod]
    public void TestIsImmutable()
    {
        var _atom = Atom("a");
        var number = IntegerNumber(42);
        var variable1 = Variable("X");
        var variable2 = Variable("Y");
        var immutableStructure = Structure("p", Atom("c"));
        var mutableStructure = Structure("p", Variable("Z"));

        // assert when both terms are mutable
        Assert.IsTrue(new LinkedTermList(_atom, number).IsImmutable);
        Assert.IsTrue(new LinkedTermList(_atom, _atom).IsImmutable);
        Assert.IsTrue(new LinkedTermList(immutableStructure, number).IsImmutable);
        Assert.IsTrue(new LinkedTermList(_atom, immutableStructure).IsImmutable);
        Assert.IsTrue(new LinkedTermList(immutableStructure, immutableStructure).IsImmutable);

        // assert when one at least one term is a variable
        Assert.IsFalse(new LinkedTermList(variable1, variable2).IsImmutable);
        Assert.IsFalse(new LinkedTermList(variable1, variable1).IsImmutable);
        Assert.IsFalse(new LinkedTermList(_atom, variable2).IsImmutable);
        Assert.IsFalse(new LinkedTermList(variable1, number).IsImmutable);
        Assert.IsFalse(new LinkedTermList(immutableStructure, variable2).IsImmutable);
        Assert.IsFalse(new LinkedTermList(variable1, immutableStructure).IsImmutable);

        // assert when one term is a mutable structure
        Assert.IsFalse(new LinkedTermList(_atom, mutableStructure).IsImmutable);
        Assert.IsFalse(new LinkedTermList(mutableStructure, number).IsImmutable);
        Assert.IsFalse(new LinkedTermList(mutableStructure, immutableStructure).IsImmutable);
        Assert.IsFalse(new LinkedTermList(immutableStructure, mutableStructure).IsImmutable);
        Assert.IsFalse(new LinkedTermList(mutableStructure, number).IsImmutable);
        Assert.IsFalse(new LinkedTermList(mutableStructure, mutableStructure).IsImmutable);
    }

    [TestMethod]
    public void TestIsImmutableAfterCopy()
    {
        var v = Variable("X");
        var a = Atom("test");
        var l1 = List(Atom(), Structure("p", Atom(), v, IntegerNumber()), List(IntegerNumber(), DecimalFraction()));
        Assert.IsFalse(l1.IsImmutable);
        v.Unify(a);
        var l2 = l1.Copy(new());
        Assert.IsFalse(l1.IsImmutable);
        Assert.IsTrue(l2.IsImmutable, l2.ToString());
        Assert.AreSame(v, l1.GetArgument(1).GetArgument(0)?.GetArgument(1));
        Assert.AreSame(a, l2.GetArgument(1).GetArgument(0)?.GetArgument(1));
    }

    [TestMethod]
    public void TestHashCode()
    {
        var w = new Atom("w");
        var x = new Atom("x");
        var y = new Atom("y");
        var z = new Atom("z");
        var l1 = new LinkedTermList(x, new LinkedTermList(y, z));

        Assert.AreEqual(l1.GetHashCode(), new LinkedTermList(x, new LinkedTermList(y, z)).GetHashCode());
        Assert.AreEqual(l1.GetHashCode(), new LinkedTermList(new Atom("x"), new LinkedTermList(new Atom("y"), new Atom("z"))).GetHashCode());

        // assert lists of same Length and elements do not have same hashcode if order is different
        Assert.AreNotEqual(l1.GetHashCode(), new LinkedTermList(x, new LinkedTermList(z, y)).GetHashCode());
        Assert.AreNotEqual(l1.GetHashCode(), new LinkedTermList(z, new LinkedTermList(y, x)).GetHashCode());
        Assert.AreNotEqual(l1.GetHashCode(), new LinkedTermList(z, new LinkedTermList(x, y)).GetHashCode());
        Assert.AreNotEqual(l1.GetHashCode(), new LinkedTermList(y, new LinkedTermList(y, z)).GetHashCode());
        Assert.AreNotEqual(l1.GetHashCode(), new LinkedTermList(y, new LinkedTermList(z, y)).GetHashCode());

        // assert lists of same Length do not have same hashcode if elements are different
        Assert.AreNotEqual(l1.GetHashCode(), new LinkedTermList(w, new LinkedTermList(y, z)).GetHashCode());
        Assert.AreNotEqual(l1.GetHashCode(), new LinkedTermList(x, new LinkedTermList(w, z)).GetHashCode());
        Assert.AreNotEqual(l1.GetHashCode(), new LinkedTermList(x, new LinkedTermList(y, w)).GetHashCode());
        Assert.AreNotEqual(l1.GetHashCode(), new LinkedTermList(x, new LinkedTermList(y, new Variable("z"))).GetHashCode());

        // assert lists of different Length do not have same hashcode
        Assert.AreNotEqual(l1.GetHashCode(), new LinkedTermList(x, y).GetHashCode());
    }

    private static void AssertMatch(LinkedTermList l1, LinkedTermList l2, bool expectMatch)
    {
        // NOTE important to test that ToString, Equals, hashCode and Unify methods don't throw stackoverflow
        AssertStrictEquality(l1, l2, expectMatch);
        Assert.AreEqual(expectMatch, l1.Equals(l2));
        Assert.AreEqual(expectMatch, l1.GetHashCode() == l2.GetHashCode());
        Assert.AreEqual(expectMatch, l1.Unify(l2));
        Assert.AreEqual(expectMatch, l1.ToString().Equals(l2.ToString()));
    }

    private static void AssertStrictEqualityUnifyAndBacktrack(LinkedTermList l1, LinkedTermList l2)
    {
        AssertStrictEquality(l1, l2, false);
        Assert.AreSame(l1, l1.Term);
        Assert.AreSame(l2, l2.Term);

        l1.Unify(l2);

        AssertStrictEquality(l1, l2, true);
        Assert.AreNotSame(l1, l1.Term);
        Assert.AreNotSame(l2, l2.Term);

        l1.Backtrack();
        l2.Backtrack();

        AssertStrictEquality(l1, l2, false);
        Assert.AreSame(l1, l1.Term);
        Assert.AreSame(l2, l2.Term);
    }
}
