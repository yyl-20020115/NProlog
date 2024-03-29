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
public class StructureTest : TestUtils
{
    [TestMethod]
    public void TestCreationWithArguments()
    {
        Term[] args = { Atom(), Structure(), IntegerNumber(), DecimalFraction(), Variable() };
        var p = Structure("test", args);
        Assert.AreEqual("test", p.Name);
        Assert.IsTrue(Enumerable.SequenceEqual(args, p.Args));
        //assertArrayEquals(args, p.Args);
        Assert.AreEqual(5, p.NumberOfArguments);
        for (int i = 0; i < args.Length; i++)
        {
            Assert.AreSame(args[i], p.GetArgument(i));
        }
        Assert.AreSame(TermType.STRUCTURE, p.Type);
        Assert.AreEqual("test(test, test(test), 1, 1.0, X)", p.ToString());
    }

    [TestMethod]
    public void TestGetValueNoVariables()
    {
        var p = Structure("p", Atom(), Structure("p", Atom()), List(IntegerNumber(), DecimalFraction()));
        var p2 = p.Term;
        Assert.AreSame(p, p2);
    }

    [TestMethod]
    public void TestGetValueUnassignedVariables()
    {
        var p = Structure("p", Variable(), Structure("p", Variable()), List(Variable(), Variable()));
        Assert.AreSame(p, p.Term);
    }

    [TestMethod]
    public void TestGetValueAssignedVariable()
    {
        var x = Variable("X");
        var p1 = Structure("p", Atom(), Structure("p", Atom(), x, IntegerNumber()), List(IntegerNumber(), DecimalFraction()));
        x.Unify(Atom());
        var p2 = p1.Term;
        Assert.AreNotSame(p1, p2);
        Assert.AreEqual(p1.ToString(), p2.ToString());
        AssertStrictEquality(p1, p2, true);
    }

    [TestMethod]
    public void TestGetBoundNoVariables()
    {
        var p = Structure("p", Atom(), Structure("p", Atom()), List(IntegerNumber(), DecimalFraction()));
        Assert.AreSame(p, p.Bound);
    }

    [TestMethod]
    public void TestGetBoundUnassignedVariables()
    {
        var p = Structure("p", Variable(), Structure("p", Variable()), List(Variable(), Variable()));
        Assert.AreSame(p, p.Bound);
    }

    [TestMethod]
    public void TestGetBoundAssignedVariable()
    {
        var x = Variable("X");
        var p = Structure("p", Atom(), Structure("p", Atom(), x, IntegerNumber()), List(IntegerNumber(), DecimalFraction()));
        x.Unify(Atom());
        Assert.AreSame(p, p.Bound);
    }

    [TestMethod]
    public void TestCreationList()
    {
        var t = Terms.Structure.CreateStructure(".", new Term[] { Atom("a"), Atom("b") });
        Assert.AreEqual(TermType.LIST, t.Type);
        Assert.IsTrue(t is LinkedTermList);
        var l = ParseSentence("[a | b].");
        Assert.AreEqual(l?.ToString(), t.ToString());
    }

    [TestMethod]
    public void TestUnifyWhenBothPredicatesHaveVariableArguments()
    {
        // test(x, Y)
        var p1 = Structure("test", new Atom("x"), new Variable("Y"));
        // test(X, y)
        var p2 = Structure("test", new Variable("X"), new Atom("y"));
        Assert.IsTrue(p1.Unify(p2));
        Assert.AreEqual("test(x, y)", p1.ToString());
        Assert.AreEqual(p1.ToString(), p2.ToString());
    }

    [TestMethod]
    public void TestUnifyWhenPredicateHasSameVariableTwiceAsArgument()
    {
        // test(x, y)
        var p1 = Structure("test", new Atom("x"), new Atom("y"));
        // test(X, X)
        var v = new Variable("X");
        var p2 = Structure("test", v, v);

        Assert.IsFalse(p2.Unify(p1));
        Assert.AreEqual("test(x, y)", p1.ToString());
        // Note: following is expected quirk - predicate doesn't automatically Backtrack on failure
        Assert.AreEqual("test(x, x)", p2.ToString());

        p2.Backtrack();
        Assert.AreEqual("test(X, X)", p2.ToString());

        Assert.IsFalse(p1.Unify(p2));
        Assert.AreEqual("test(x, y)", p1.ToString());
        // Note: following is expected quirk - predicate doesn't automatically Backtrack on failure
        Assert.AreEqual("test(x, x)", p2.ToString());

        p2.Backtrack();
        Assert.AreEqual("test(X, X)", p2.ToString());
    }

    [TestMethod]
    public void TestUnifyVariableThatIsPredicateArgument()
    {
        // test(X, X)
        var v = new Variable("X");
        var p = Structure("test", v, v);
        Assert.AreEqual("test(X, X)", p.ToString());
        Assert.IsTrue(v.Unify(new Atom("x")));
        Assert.AreEqual("test(x, x)", p.ToString());
    }

    [TestMethod]
    public void TestUnifyDifferentNamesSameArguments()
    {
        Term[] args = { Atom(), IntegerNumber(), DecimalFraction() };
        var p1 = Structure("test1", args);
        var p2 = Structure("test2", args);
        var p3 = Structure("test", args);
        AssertStrictEqualityAndUnify(p1, p2, false);
        AssertStrictEqualityAndUnify(p1, p3, false);
    }

    [TestMethod]
    public void TestSameNamesDifferentArguments()
    {
        Structure[] predicates = {
                  Structure("test1", new Atom("a"), new Atom("b"), new Atom("c")),
                  Structure("test2", new Atom("a"), new Atom("b"), new Atom("d")),
                  Structure("test3", new Atom("a"), new Atom("c"), new Atom("b")),
                  Structure("test4", new Atom("a"), new Atom("b"))};
        for (int i = 0; i < predicates.Length; i++)
        {
            for (int j = i; j < predicates.Length; j++)
            {
                if (i == j)
                {
                    // check they all Compare to a Copy of themselves
                    AssertStrictEqualityAndUnify(predicates[i], predicates[i].Copy(new()), true);
                }
                else
                {
                    AssertStrictEqualityAndUnify(predicates[i], predicates[j], false);
                }
            }
        }
    }

    [TestMethod]
    public void TestUnifyWrongType()
    {
        var p = Structure("1", new Term[] { Atom() });
        AssertStrictEqualityAndUnify(p, new Atom("1"), false);
        AssertStrictEqualityAndUnify(p, new IntegerNumber(1), false);
        AssertStrictEqualityAndUnify(p, new DecimalFraction(1), false);
    }

    [TestMethod]
    public void TestNoArguments()
    {
        try
        {
            Structure("test", EMPTY_ARRAY);
            Assert.Fail();
        }
        catch (ArgumentException e)
        {
            Assert.AreEqual("Cannot create structure with no arguments", e.Message);
        }
    }

    [TestMethod]
    public void TestCopyWithoutVariablesOrNestedArguments()
    {
        var p = Structure("test", Atom(), IntegerNumber(), DecimalFraction());
        var Copy = p.Copy(new());
        Assert.AreSame(p, Copy);
    }

    [TestMethod]
    public void TestCopyWithUnassignedVariables()
    {
        // create structure where some arguments are variables
        var name = "test";
        var a = Atom();
        var i = IntegerNumber();
        var d = DecimalFraction();
        var x = new Variable("X");
        var y = new Variable("Y");
        var original = Structure(name, a, x, i, y, d, x);

        // make a Copy
        Dictionary<Variable, Variable> sharedVariables = new();
        var Copy = original.Copy(sharedVariables);

        // Compare Copy to original
        Assert.AreEqual(2, sharedVariables.Count);
        Assert.AreEqual("X", sharedVariables[(x)].Id);
        Assert.AreNotEqual(x, sharedVariables[(x)]);
        Assert.AreEqual("Y", sharedVariables[(y)].Id);
        Assert.AreNotEqual(y, sharedVariables[(y)]);

        Assert.AreEqual(name, Copy.Name);
        Assert.AreEqual(6, Copy.NumberOfArguments);
        Assert.AreSame(a, Copy.Args[0]);
        Assert.AreSame(sharedVariables[(x)], Copy.Args[1]);
        Assert.AreSame(i, Copy.Args[2]);
        Assert.AreSame(sharedVariables[(y)], Copy.Args[3]);
        Assert.AreSame(d, Copy.Args[4]);
        Assert.AreSame(sharedVariables[(x)], Copy.Args[5]);
    }

    [TestMethod]
    public void TestCopyWithAssignedVariable()
    {
        var X = new Variable("X");
        var arg = Structure("p", X);
        var original = Structure("p", arg);

        Assert.AreSame(original, original.Term);

        Dictionary<Variable, Variable> sharedVariables = new();
        var copy1 = original.Copy(sharedVariables);
        Assert.AreNotSame(original, copy1);
        AssertStrictEquality(original, copy1, false);
        Assert.AreEqual(1, sharedVariables.Count);
        Assert.IsTrue(sharedVariables.ContainsKey(X));
        Assert.AreEqual(original.ToString(), copy1.ToString());

        X.Unify(Atom("a"));

        var copy2 = original.Copy(new());
        Assert.AreNotSame(original, copy2);
        AssertStrictEquality(original, copy2, true);
        Assert.AreEqual(original.ToString(), copy2.ToString());
        Assert.AreSame(copy2, copy2.Copy(new()));
        Assert.AreSame(copy2, copy2.Term);

        X.Backtrack();

        AssertStrictEquality(original, copy2, false);

        Assert.AreEqual("p(p(X))", original.ToString());
        Assert.AreEqual("p(p(a))", copy2.ToString());
    }

    [TestMethod]
    public void TestIsImmutable()
    {
        var v = Variable("X");
        var a = Atom("test");
        var p1 = Structure("p", Atom(), Structure("p", Atom(), v, IntegerNumber()), List(IntegerNumber(), DecimalFraction()));
        Assert.IsFalse(p1.IsImmutable);
        v.Unify(a);
        var p2 = p1.Copy(new());
        Assert.IsFalse(p1.IsImmutable);
        Assert.IsTrue(p2.IsImmutable);
        Assert.AreSame(v, p1.GetArgument(1).GetArgument(1));
        Assert.AreSame(a, p2.GetArgument(1).GetArgument(1));
    }

    [TestMethod]
    public void TestBacktrack()
    {
        var x = Variable("X");
        var y = Variable("Y");
        var z = Variable("Z");
        var a1 = Atom("test1");
        var a2 = Atom("test2");
        var a3 = Atom("test3");
        var p1 = Structure("p", x, Structure("p", a1, y));
        var p2 = Structure("p", z);
        x.Unify(p2);
        y.Unify(a2);
        z.Unify(a3);

        Assert.AreSame(p2, x.Bound);
        Assert.AreSame(a2, y.Bound);
        Assert.AreSame(a3, z.Bound);

        p1.Backtrack();

        // Note that backtracking unbounds X and Y from the "p2" structure
        // but it doesn't unbound the Z variable of the "p2" structure.
        Assert.AreSame(x, x.Bound);
        Assert.AreSame(y, y.Bound);
        Assert.AreSame(a3, z.Bound);
    }

    [TestMethod]
    public void TestHashCode()
    {
        var a = new Atom("a");
        var b = new Atom("b");
        var c = new Atom("c");
        var p = Structure("p", a, b, c);

        AssertHashCodeEquals(p, Structure("p", a, b, c));
        AssertHashCodeEquals(p, Structure("p", new Atom("a"), new Atom("b"), new Atom("c")));

        AssertHashCodeNotEquals(Structure("p", a, b), Structure("p", b, a));
        AssertHashCodeNotEquals(Structure("p", Structure("p", a, b), Structure("p", b, a)), //
                    Structure("p", Structure("p", b, a), Structure("p", a, b)));

        // assert order of args affects hashCode
        AssertHashCodeNotEquals(p, Structure("p", a, c, b));
        AssertHashCodeNotEquals(p, Structure("p", b, a, c));
        AssertHashCodeNotEquals(p, Structure("p", b, c, a));
        AssertHashCodeNotEquals(p, Structure("p", c, a, b));
        AssertHashCodeNotEquals(p, Structure("p", c, b, a));

        // assert number of args affects hashCode
        AssertHashCodeNotEquals(p, Structure("p", a));
        AssertHashCodeNotEquals(p, Structure("p", a, b, c, a));

        // assert functor affects hashCode
        AssertHashCodeNotEquals(p, Structure("P", a, b, c));
        AssertHashCodeNotEquals(p, Structure("pp", a, b, c));

        // assert arg types affects hashCode
        AssertHashCodeNotEquals(Structure("p", new DecimalFraction(1)), Structure("p", new IntegerNumber(1)));
    }

    private static void AssertStrictEqualityAndUnify(Term t1, Term t2, bool expectedResult)
    {
        AssertStrictEquality(t1, t2, expectedResult);
        Assert.IsTrue(t1.Unify(t2) == expectedResult);
    }
}
