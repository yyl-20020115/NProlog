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
using Org.NProlog.Core.Kb;
using Org.NProlog.Core.Math;

namespace Org.NProlog.Core.Terms;

[TestClass]
public class TermUtilsTest : TestUtils
{
    [TestMethod]
    public void TestEmptyArray()
    {
        Assert.AreEqual(0, TermUtils.EMPTY_ARRAY.Length);

    }

    [TestMethod]
    public void TestCopy()
    {
        // setup input terms
        Atom a = Atom("a");
        Variable x = Variable("X");
        Variable y = Variable("Y");
        Variable z = Variable("Z");
        Assert.IsTrue(x.Unify(a));
        Structure p = Structure("p", x, y);
        Term[] input = { a, p, x, y, z };

        // perform Copy
        Term[] output = TermUtils.Copy(input);

        // check result
        Assert.AreEqual(input.Length, output.Length);

        Assert.AreSame(a, output[0]);

        Term t = output[1];
        Assert.AreSame(TermType.STRUCTURE, t.Type);
        Assert.AreSame(p.Name, t.Name);
        Assert.AreEqual(2, t.NumberOfArguments);
        Assert.AreSame(a, t.GetArgument(0));
        Term copyOfY = t.GetArgument(1);
        AssertVariable(copyOfY, "Y");

        Assert.AreSame(a, output[2]);

        Assert.AreSame(copyOfY, output[3]);

        AssertVariable(output[4], "Z");
    }

    private static void AssertVariable(Term t, string id)
    {
        Assert.AreSame(TermType.VARIABLE, t.Type);
        Assert.AreSame(t, t.Term);
        Assert.AreEqual(id, ((Variable)t).Id);
    }

    [TestMethod]
    public void TestBacktrack()
    {
        // setup input terms
        Atom a = Atom("a");
        Atom b = Atom("b");
        Atom c = Atom("c");
        Variable x = Variable("X");
        Variable y = Variable("Y");
        Variable z = Variable("Z");
        Assert.IsTrue(x.Unify(a));
        Assert.IsTrue(y.Unify(b));
        Assert.IsTrue(z.Unify(c));
        Term[] original = { x, a, b, y, c, z };
        Term[] input = { x, a, b, y, c, z };

        // perform the Backtrack
        TermUtils.Backtrack(input);

        // assert variables have backtracked
        Assert.AreSame(x, x.Term);
        Assert.AreSame(y, y.Term);
        Assert.AreSame(z, z.Term);

        // assert array was not manipulated
        for (int i = 0; i < input.Length; i++)
        {
            Assert.AreSame(original[i], input[i]);
        }
    }

    [TestMethod]
    public void TestUnifySuccess()
    {
        // setup input terms
        Variable x = Variable("X");
        Variable y = Variable("Y");
        Variable z = Variable("Z");
        Atom a = Atom("a");
        Atom b = Atom("b");
        Atom c = Atom("c");
        Term[] input1 = { x, b, z };
        Term[] input2 = { a, y, c };

        // attempt unification
        Assert.IsTrue(TermUtils.Unify(input1, input2));

        // assert all variables unified to atoms
        Assert.AreSame(a, x.Term);
        Assert.AreSame(b, y.Term);
        Assert.AreSame(c, z.Term);
    }

    [TestMethod]
    public void TestUnifyFailure()
    {
        // setup input terms
        Variable x = Variable("X");
        Variable y = Variable("Y");
        Variable z = Variable("Z");
        Atom a = Atom("a");
        Atom b = Atom("b");
        Atom c = Atom("c");
        Term[] input1 = { x, b, z, b };
        Term[] input2 = { a, y, c, a };

        // attempt unification
        Assert.IsFalse(TermUtils.Unify(input1, input2));

        // assert all variables in input1 were backed tracked
        Assert.AreSame(x, x.Term);
        Assert.AreSame(z, z.Term);

        // as javadocs states, terms passed in second argument to Unify may not be backtracked
        Assert.AreSame(b, y.Term);
    }

    [TestMethod]
    public void TestGetAllVariablesInTerm()
    {
        Variable q = Variable("Q");
        Variable r = Variable("R");
        Variable s = Variable("S");
        Variable t = Variable("T");
        Variable v = Variable("V");
        Variable w = Variable("W");
        Variable x = Variable("X");
        Variable y = Variable("Y");
        Variable z = Variable("Z");
        Variable anon = new Variable();
        Variable[] variables = { q, r, s, t, v, w, x, y, z, anon };
        Structure input = Structure("p1", x, v, anon, EmptyList.EMPTY_LIST, y, q, IntegerNumber(1), Structure("p2", y, DecimalFraction(1.5), w), List(s, y, IntegerNumber(7), r, t),
                    z);
        HashSet<Variable> result = TermUtils.GetAllVariablesInTerm(input);
        Assert.AreEqual(variables.Length, result.Count);
        foreach (Variable variable in variables)
        {
            Assert.IsTrue(result.Contains(variable));
        }
    }

    [TestMethod]
    public void TestIntegerNumberCastToNumeric()
    {
        IntegerNumber i = IntegerNumber();
        Assert.AreSame(i, TermUtils.CastToNumeric(i));
    }

    [TestMethod]
    public void TestDecimalFractionCastToNumeric()
    {
        DecimalFraction d = DecimalFraction();
        Assert.AreSame(d, TermUtils.CastToNumeric(d));
    }

    [TestMethod]
    public void TestAtomCastToNumeric()
    {
        try
        {
            Atom a = Atom("1");
            TermUtils.CastToNumeric(a);
            Assert.Fail();
        }
        catch (PrologException e)
        {
            Assert.AreEqual("Expected Numeric but got: ATOM with value: 1", e.Message);
        }
    }

    [TestMethod]
    public void TestVariableCastToNumeric()
    {
        Variable v = Variable();
        try
        {
            TermUtils.CastToNumeric(v);
            Assert.Fail();
        }
        catch (PrologException e)
        {
            Assert.AreEqual("Expected Numeric but got: VARIABLE with value: X", e.Message);
        }
        IntegerNumber i = IntegerNumber();
        v.Unify(i);
        Assert.AreSame(i, TermUtils.CastToNumeric(v));
    }

    [TestMethod]
    public void TestStructureCastToNumeric()
    {
        // test that, even if it represents an arithmetic expression,
        // a structure causes an exception when passed to CastToNumeric
        Structure arithmeticExpression = Structure("*", IntegerNumber(3), IntegerNumber(7));
        try
        {
            TermUtils.CastToNumeric(arithmeticExpression);
            Assert.Fail();
        }
        catch (PrologException e)
        {
            Assert.AreEqual("Expected Numeric but got: STRUCTURE with value: *(3, 7)", e.Message);
        }
    }

    [TestMethod]
    public void TestIntegerNumberToLong()
    {
        KnowledgeBase kb = TestUtils.CreateKnowledgeBase();
        ArithmeticOperators operators = kb.ArithmeticOperators;
        Assert.AreEqual(int.MaxValue, TermUtils.ToLong(operators, IntegerNumber(int.MaxValue)));
        Assert.AreEqual(1, TermUtils.ToLong(operators, IntegerNumber(1)));
        Assert.AreEqual(0, TermUtils.ToLong(operators, IntegerNumber(0)));
        Assert.AreEqual(int.MinValue, TermUtils.ToLong(operators, IntegerNumber(int.MinValue)));
    }

    [TestMethod]
    public void TestArithmeticFunctionToLong()
    {
        KnowledgeBase kb = TestUtils.CreateKnowledgeBase();
        ArithmeticOperators operators = kb.ArithmeticOperators;
        Structure arithmeticExpression = Structure("*", IntegerNumber(3), IntegerNumber(7));
        Assert.AreEqual(21, TermUtils.ToLong(operators, arithmeticExpression));
    }

    [TestMethod]
    public void TestToLongExceptions()
    {
        KnowledgeBase kb = TestUtils.CreateKnowledgeBase();
        AssertTestToLongException(kb, Atom("test"), "Cannot find arithmetic operator: test/0");
        AssertTestToLongException(kb, Structure("p", IntegerNumber(1), IntegerNumber(1)), "Cannot find arithmetic operator: p/2");
        AssertTestToLongException(kb, DecimalFraction(0), "Expected integer but got: FRACTION with value: 0.0");
        AssertTestToLongException(kb, Structure("+", DecimalFraction(1.0), DecimalFraction(1.0)), "Expected integer but got: FRACTION with value: 2.0");
    }

    private static void AssertTestToLongException(KnowledgeBase kb, Term t, string expectedExceptionMessage)
    {
        ArithmeticOperators operators = kb.ArithmeticOperators;
        try
        {
            TermUtils.ToLong(operators, t);
            Assert.Fail();
        }
        catch (PrologException e)
        {
            Assert.AreEqual(expectedExceptionMessage, e.Message);
        }
    }

    [TestMethod]
    public void TestGetAtomName()
    {
        Atom a = Atom("testAtomName");
        Assert.AreEqual("testAtomName", TermUtils.GetAtomName(a));
    }

    [TestMethod]
    public void TestGetAtomNameException()
    {
        Structure p = Structure("testAtomName", Atom());
        try
        {
            TermUtils.GetAtomName(p);
            Assert.Fail();
        }
        catch (PrologException e)
        {
            Assert.AreEqual("Expected an atom but got: STRUCTURE with value: testAtomName(test)", e.Message);
        }
    }

    [TestMethod]
    public void AssertType()
    {
        TermUtils.AssertType(Atom("testAtomName"), TermType.ATOM);
        try
        {
            TermUtils.AssertType(Atom("testAtomName"), TermType.LIST);
            Assert.Fail();
        }
        catch (PrologException e)
        {
            Assert.AreEqual("Expected LIST but got: ATOM with value: testAtomName", e.Message);
        }
    }

    [TestMethod]
    public void TestToInt()
    {
        AssertToInt(0);
        AssertToInt(1);
        AssertToInt(-1);
        AssertToInt(int.MaxValue);
        AssertToInt(int.MinValue);
    }

    private static void AssertToInt(long n)
    {
        Assert.AreEqual(n, TermUtils.ToInt(IntegerNumber(n)));
    }

    [TestMethod]
    public void TestToIntException()
    {
        AssertToIntException(int.MaxValue + 1L);
        AssertToIntException(int.MinValue - 1L);
        AssertToIntException(long.MaxValue);
        AssertToIntException(long.MinValue);
    }

    private static void AssertToIntException(long n)
    {
        try
        {
            TermUtils.ToInt(IntegerNumber(n));
            Assert.Fail();
        }
        catch (PrologException e)
        {
            Assert.AreEqual("Value cannot be cast to an int without losing precision: " + n, e.Message);
        }
    }
}
