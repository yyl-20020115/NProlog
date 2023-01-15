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

/**
 * @see TermTest
 */
[TestClass]
public class VariableTest : TestUtils
{
    [TestMethod]
    public void TestUnassignedVariableMethods()
    {
        var v = new Variable("X");

        Assert.AreEqual(v, v);
        Assert.AreEqual("X", v.Id);
        Assert.AreEqual("X", v.ToString());
        Assert.AreSame(v, v.Term);
        Assert.AreSame(v, v.Bound);
        Assert.IsTrue(TermUtils.TermsEqual(v, v));

        try
        {
            var m = v.Name;
            Assert.Fail();
        }
        catch (NullReferenceException)
        {
        }
        try
        {
            var m = v.Args;
            Assert.Fail();
        }
        catch (NullReferenceException)
        {
        }
        try
        {
            var m = v.NumberOfArguments;
            Assert.Fail();
        }
        catch (NullReferenceException)
        {
        }
        try
        {
            v.GetArgument(0);
            Assert.Fail();
        }
        catch (NullReferenceException)
        {
        }
        try
        {
            TermUtils.CastToNumeric(v);
            Assert.Fail();
        }
        catch (PrologException e)
        {
            Assert.AreEqual("Expected Numeric but got: VARIABLE with value: X", e.Message);
        }

        Assert.IsTrue(v.Unify(v));

        // just check Backtrack doesn't throw an exception
        v.Backtrack();
    }

    [TestMethod]
    public void TestUnifyVariables1()
    {
        var x = new Variable("X");
        var y = new Variable("Y");
        AssertStrictEquality(x, y, false);
        Assert.IsTrue(x.Unify(y));
        AssertStrictEquality(x, y, true);
        x.Backtrack();
        AssertStrictEquality(x, y, false);
    }

    [TestMethod]
    public void TestUnifyVariables2()
    {
        var a = Atom();
        var x = new Variable("X");
        var y = new Variable("Y");
        Assert.IsTrue(y.Unify(a));
        Assert.IsTrue(x.Unify(y));
        Assert.AreSame(a, x.Term);
        Assert.AreSame(a, x.Bound);
        x.Backtrack();
        Assert.AreSame(x, x.Term);
        Assert.AreSame(a, y.Term);
        Assert.AreSame(x, x.Bound);
        Assert.AreSame(a, y.Bound);
    }

    [TestMethod]
    public void TestUnifyVariables3()
    {
        var a = Atom();
        var x = new Variable("X");
        var y = new Variable("Y");
        Assert.IsTrue(x.Unify(y));
        Assert.IsTrue(y.Unify(a));
        Assert.AreSame(a, x.Term);
        Assert.AreSame(a, x.Bound);
    }

    [TestMethod]
    public void TestVariablesUnifiedToTheSameTerm()
    {
        var a = Atom();
        var x = new Variable("X");
        var y = new Variable("Y");
        AssertStrictEquality(x, y, false);
        Assert.IsTrue(x.Unify(a));
        Assert.IsTrue(y.Unify(a));
        AssertStrictEquality(x, y, true);
        x.Backtrack();
        AssertStrictEquality(x, y, false);
        Assert.AreSame(x, x.Term);
        Assert.AreSame(a, y.Term);
        Assert.AreSame(x, x.Bound);
        Assert.AreSame(a, y.Bound);
    }

    [TestMethod]
    public void TestVariableUnifiedToMutableTerm()
    {
        var x = new Variable("X");
        var y = new Variable("Y");
        var a = new Atom("a");
        var b = new Atom("b");
        var p = Structure("p1", Structure("p2", x, Atom(), x), y);

        var result = new Variable("Result");
        Assert.AreSame(result, result.Term);
        Assert.AreSame(result, result.Bound);

        Assert.IsTrue(result.Unify(p));
        Assert.AreSame(p, result.Term);
        Assert.AreSame(p, result.Bound);

        Assert.IsTrue(x.Unify(a));
        Assert.AreNotSame(p, result.Term);
        //AreSame
        Assert.AreEqual(Structure("p1", Structure("p2", a, Atom(), a), y), result.Term);
        Assert.AreSame(p, result.Bound);

        Assert.IsTrue(y.Unify(b));
        Assert.AreNotSame(p, result.Term);
        Assert.AreEqual(Structure("p1", Structure("p2", a, Atom(), a), b), result.Term);
        Assert.AreSame(p, result.Bound);

        x.Backtrack();
        y.Backtrack();
        Assert.AreSame(p, result.Term);
        Assert.AreSame(p, result.Bound);

        result.Backtrack();
        Assert.AreSame(result, result.Term);
        Assert.AreSame(result, result.Bound);
    }

    [TestMethod]
    public void TestCopy()
    {
        var v = Variable();
        Dictionary<Variable, Variable> sharedVariables = new();
        Term Copy = v.Copy(sharedVariables);
        Assert.AreEqual(1, sharedVariables.Count);
        Assert.AreSame(Copy, sharedVariables[(v)]);
        Assert.IsFalse(TermUtils.TermsEqual(v, Copy));
        Assert.IsTrue(v.Unify(Copy));
        Assert.IsTrue(TermUtils.TermsEqual(v, Copy));
    }

    /**
     * Tests that, when {@link Variable#Copy(Dictionary)} is called on a variable whose "Copy" (contained in the specified Dictionary)
     * is already instantiated, the term the "Copy" is instantiated with gets returned rather than the "Copy" itself.
     * <p>
     * This behaviour is required for things like
     * {@link org.prolog.core.udp.interpreter.InterpretedTailRecursivePredicate} to work.
     */
    [TestMethod]
    public void TestCopy2()
    {
        var v = Variable();
        var a = Atom();
        var s1 = Structure("name", v);
        var s2 = Structure("name", v);

        Dictionary<Variable, Variable> sharedVariables = new();

        var c1 = s1.Copy(sharedVariables);
        Assert.IsTrue(c1.Unify(Structure("name", a)));

        var c2 = s2.Copy(sharedVariables);
        // check that the single argument of the newly copied structure is the atom itself
        // rather than a variable assigned to the atom
        Assert.AreSame(a, c2.GetArgument(0));
        // check that, while backtracking does affect the first copied structure,
        // it does not alter the second copied structure
        c1.Backtrack();
        c2.Backtrack();
        Assert.AreSame(typeof(Variable), c1.GetArgument(0).GetType());
        Assert.AreSame(a, c2.GetArgument(0));
    }

    [TestMethod]
    public void TestIsImmutable()
    {
        var v = new Variable("X");
        Assert.IsFalse(v.IsImmutable);
        var a = Atom();
        Assert.IsTrue(v.Unify(a));
        Assert.IsFalse(v.IsImmutable);
    }

    [TestMethod]
    public void TestUnifyAnonymousVariable()
    {
        var v = Variable();
        var anon = new Variable();
        Assert.IsTrue(v.Unify(anon));
        Assert.AreSame(anon, v.Term);
        Assert.AreSame(anon, v.Bound);
    }

    [TestMethod]
    public void TestUnifyWithSelf()
    {
        var x = new Variable("X");
        x.Unify(x);
        Assert.AreSame(x, x.Term);
    }

    [TestMethod]
    public void TestUnifyPair()
    {
        var v = new Variable("V");
        var w = new Variable("W");
        v.Unify(w);
        w.Unify(v);
        Assert.AreSame(w, v.Term);
        Assert.AreSame(w, v.Term);
    }

    [TestMethod]
    public void TestUnifyCyclicChain()
    {
        var v = new Variable("V");
        var w = new Variable("W");
        var x = new Variable("X");
        var y = new Variable("Y");
        var z = new Variable("Z");

        v.Unify(w);
        w.Unify(x);
        x.Unify(y);
        y.Unify(z);
        z.Unify(x);

        Assert.AreSame(z, v.Term);
        Assert.AreSame(z, w.Term);
        Assert.AreSame(z, x.Term);
        Assert.AreSame(z, y.Term);
        Assert.AreSame(z, z.Term);

        v.Unify(v);
        v.Unify(x);
        v.Unify(w);
        v.Unify(y);
        v.Unify(z);
        w.Unify(v);
        w.Unify(w);
        w.Unify(x);
        w.Unify(y);
        w.Unify(z);
        x.Unify(v);
        x.Unify(w);
        x.Unify(x);
        x.Unify(y);
        x.Unify(z);
        y.Unify(v);
        y.Unify(w);
        y.Unify(x);
        y.Unify(y);
        y.Unify(z);
        z.Unify(v);
        z.Unify(w);
        z.Unify(x);
        z.Unify(y);
        z.Unify(z);

        Assert.AreSame(z, v.Term);
        Assert.AreSame(z, w.Term);
        Assert.AreSame(z, x.Term);
        Assert.AreSame(z, y.Term);
        Assert.AreSame(z, z.Term);

        var u = new Variable("U");
        u.Unify(x);
        Assert.AreSame(z, u.Term);

        var t = new Variable("T");
        x.Unify(t);
        Assert.AreSame(t, t.Term);
        Assert.AreSame(t, u.Term);
        Assert.AreSame(t, v.Term);
        Assert.AreSame(t, w.Term);
        Assert.AreSame(t, x.Term);
        Assert.AreSame(t, y.Term);
        Assert.AreSame(t, z.Term);
    }

    [TestMethod]
    public void TestVariableChain()
    {
        var v1 = Variable();
        var v2 = v1;
        for (int i = 0; i < 10000; i++)
        {
            var tmpVar = Variable("V" + i);
            v2.Unify(tmpVar);
            v2 = tmpVar;
        }
        Assert.AreNotEqual(v1, v2);
        var t = Structure("name", Atom("a"), Atom("b"), Atom("c"));
        Assert.IsTrue(v2.Unify(t));
        Assert.AreNotEqual(v1, v2);
        AssertStrictEquality(v1, v2, true);
        AssertStrictEquality(v1, t, true);
        AssertStrictEquality(v2, t, true);

        Assert.AreSame(t, v1.Term);
        Assert.AreSame(t, v1.Bound);
        Assert.AreSame(t, v1.Copy(null));
        Assert.AreEqual(t.ToString(), v1.ToString());
        Assert.AreSame(t.Name, v1.Name);
        Assert.AreSame(t.Type, v1.Type);
        Assert.AreEqual(t.NumberOfArguments, v1.NumberOfArguments);
        Assert.AreSame(t.Args, v1.Args);
        Assert.AreSame(t.GetArgument(0), v1.GetArgument(0));
        Assert.IsTrue(TermUtils.TermsEqual(t, v1));
        Assert.IsTrue(TermUtils.TermsEqual(v1, v1));
        Assert.IsTrue(t.Unify(v1));
        Assert.IsTrue(v1.Unify(t));
        Assert.IsFalse(v1.Unify(Atom()));
        Assert.IsFalse(Atom().Unify(v1));

        v2.Backtrack();
        Assert.AreSame(v2, v1.Term);
        Assert.AreSame(v2, v1.Bound);

        v1.Backtrack();
        Assert.AreSame(v1, v1.Term);
        Assert.AreSame(v1, v1.Bound);
    }

    [TestMethod]
    public void TestInfiniteTerm()
    {
        //NOTICE: this kills itself
        return;
        var v = Variable("X");
        var t = Structure("name", v);
        Assert.IsTrue(v.Unify(t));

        Assert.AreSame(t, v.Bound);
        Assert.AreSame(t, t.Bound);

        try
        {
            v.Copy(new());
            Assert.Fail();
        }
        catch (StackOverflowException)
        {
        }
        try
        {
            t.Copy(new());
            Assert.Fail();
        }
        catch (StackOverflowException)
        {
        }
        try
        {
            var m = v.Term;
            Assert.Fail();
        }
        catch (StackOverflowException)
        {
        }
        try
        {
            var m = t.Term;
            Assert.Fail();
        }
        catch (StackOverflowException)
        {
        }
        try
        {
            v.ToString();
            Assert.Fail();
        }
        catch (StackOverflowException)
        {
        }
        try
        {
            t.ToString();
            Assert.Fail();
        }
        catch (StackOverflowException)
        {
        }
        Assert.IsTrue(true);
    }

    [TestMethod]
    public void TestAnonymous()
    {
        Assert.IsTrue(new Variable("_").IsAnonymous);
        Assert.IsTrue(new Variable().IsAnonymous);
    }

    [TestMethod]
    public void TestNotAnonymous()
    {
        Assert.IsFalse(new Variable("__").IsAnonymous);
        Assert.IsFalse(new Variable("_1").IsAnonymous);
        Assert.IsFalse(new Variable("_X").IsAnonymous);
        Assert.IsFalse(new Variable("X").IsAnonymous);
        Assert.IsFalse(new Variable("XYZ").IsAnonymous);
        Assert.IsFalse(new Variable("X_").IsAnonymous);
        Assert.IsFalse(new Variable("X_Y").IsAnonymous);
        Assert.IsFalse(new Variable("_X_Y_").IsAnonymous);
    }

    [TestMethod]
    public void TestAnonymousId()
    {
        Assert.AreEqual("_", Terms.Variable.ANONYMOUS_VARIABLE_ID);
        Assert.AreEqual("_", new Variable().Id);
    }

    [TestMethod]
    public void TestVariablesEqualToSelf()
    {
        var v = new Variable("X");
        int originalHashCode = v.GetHashCode();

        // an uninstantiated variable is equal to itself
        Assert.AreEqual(v, v);
        AssertStrictEquality(v, v, true);

        // instantiate variable
        v.Unify(new Atom("test"));

        // an instantiated variable is equal to itself and has its original hashcode
        Assert.AreEqual(v, v);
        AssertStrictEquality(v, v, true);
        Assert.AreEqual(originalHashCode, v.GetHashCode());

        v.Backtrack();

        // after backtracking an uninstantiated variable is still equal to itself and has its original hashcode
        Assert.AreEqual(v, v);
        AssertStrictEquality(v, v, true);
        Assert.AreEqual(originalHashCode, v.GetHashCode());
    }

    [TestMethod]
    public void TestVariablesNotEqualWhenUnassigned()
    {
        var v1 = new Variable("X");

        var v2 = new Variable("X");
        Assert.AreNotEqual(v1, v2);
        Assert.AreNotEqual(v1.GetHashCode(), v2.GetHashCode());
        AssertStrictEquality(v1, v2, false);

        var v3 = new Variable("Y");
        Assert.AreNotEqual(v1, v3);
        Assert.AreNotEqual(v1.GetHashCode(), v3.GetHashCode());
        AssertStrictEquality(v1, v3, false);
    }

    [TestMethod]
    public void TestVariablesNotEqualWhenUnifiedWithEachOther()
    {
        var v1 = new Variable("X");
        var v2 = new Variable("Y");
        v1.Unify(v2);

        Assert.AreNotEqual(v1, v2);
        Assert.AreNotEqual(v1.GetHashCode(), v2.GetHashCode());
        AssertStrictEquality(v1, v2, true);
    }

    [TestMethod]
    public void TestVariablesNotEqualWhenUnifiedWithEachOtherAndSomethingElse()
    {
        var v1 = new Variable("X");
        var v2 = new Variable("Y");
        var a = new Atom("test");
        v1.Unify(v2);
        v2.Unify(a);

        Assert.AreNotEqual(v1, v2);
        Assert.AreNotEqual(v1.GetHashCode(), v2.GetHashCode());
        AssertStrictEquality(v1, v2, true);
        AssertStrictEquality(v1, a, true);
        AssertStrictEquality(v2, a, true);
    }

    [TestMethod]
    public void TestVariablesNotEqualWhenBothUnifiedToSameTerm()
    {
        var v1 = new Variable("X");
        var v2 = new Variable("Y");
        var a = new Atom("test");
        v1.Unify(a);
        v2.Unify(a);

        Assert.AreNotEqual(v1, v2);
        Assert.AreNotEqual(v1.GetHashCode(), v2.GetHashCode());
        AssertStrictEquality(v1, v2, true);
        AssertStrictEquality(v1, a, true);
        AssertStrictEquality(v2, a, true);
    }

    [TestMethod]
    public void TestVariableNotEqualToUnifiedAtom()
    {
        AssertVariableNotEqualToUnifiedTerm(new Atom("test"));
    }

    [TestMethod]
    public void TestVariableNotEqualToUnifiedInteger()
    {
        AssertVariableNotEqualToUnifiedTerm(new IntegerNumber(7));
    }

    [TestMethod]
    public void TestVariableNotEqualToUnifiedFraction()
    {
        AssertVariableNotEqualToUnifiedTerm(new DecimalFraction(7));
    }

    [TestMethod]
    public void TestVariableNotEqualToUnifiedStructure()
    {
        AssertVariableNotEqualToUnifiedTerm(Terms.Structure.CreateStructure("test", new Term[] { Atom() }));
    }

    [TestMethod]
    public void TestVariableNotEqualToUnifiedList()
    {
        AssertVariableNotEqualToUnifiedTerm(new List(Atom(), Atom()));
    }

    [TestMethod]
    public void TestVariableNotEqualToUnifiedEmptyList()
    {
        AssertVariableNotEqualToUnifiedTerm(EmptyList.EMPTY_LIST);
    }

    private static void AssertVariableNotEqualToUnifiedTerm(Term t)
    {
        var v = new Variable(t.Name);
        Assert.IsTrue(v.Unify(t));

        AssertStrictEquality(v, t, true);
        Assert.IsFalse(v.Equals(t));
        Assert.IsFalse(t.Equals(v));
        Assert.AreNotEqual(v.GetHashCode(), t.GetHashCode());
    }
}
