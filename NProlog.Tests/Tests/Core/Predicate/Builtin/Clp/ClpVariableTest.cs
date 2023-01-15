/*
 * Copyright 2022 S. Webber
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

namespace Org.NProlog.Core.Predicate.Builtin.Clp;

[TestClass]

public class ClpVariableTest
{
    [TestMethod]
    public void TestGetName()
    {
        var v = new ClpVariable();
        var n = v.Name;
    }

    [TestMethod]
    public void testGetArgs()
    {
        var v = new ClpVariable();
        var n = v.Args;
    }

    [TestMethod]
    public void testGetArgument()
    {
        ClpVariable v = new ClpVariable();
        v.GetArgument(0);
    }

    [TestMethod]
    public void testGetNumberOfArguments()
    {
        ClpVariable v = new ClpVariable();
        Assert.AreEqual(0, v.NumberOfArguments);
    }

    [TestMethod]
    public void testGetType()
    {
        ClpVariable v = new ClpVariable();
        CoreConstraintStore environment = new CoreConstraintStore();

        Assert.AreSame(TermType.CLP_VARIABLE, v.Type);

        v.setMin(environment, 7);
        Assert.AreSame(TermType.CLP_VARIABLE, v.Type);

        v.setMax(environment, 8);
        Assert.AreSame(TermType.CLP_VARIABLE, v.Type);

        v.setMin(environment, 8);
        Assert.AreSame(TermType.INTEGER, v.Type);
    }

    [TestMethod]
    public void testIsImmutable()
    {
        ClpVariable v = new ClpVariable();
        CoreConstraintStore environment = new CoreConstraintStore();

        Assert.IsFalse(v.isImmutable());

        v.setMin(environment, 7);
        Assert.IsFalse(v.isImmutable());
        Assert.IsFalse(v.Term.isImmutable());

        v.setMax(environment, 8);
        Assert.IsFalse(v.isImmutable());
        Assert.IsFalse(v.Term.isImmutable());

        v.setMin(environment, 8);
        Assert.IsFalse(v.isImmutable());
        Assert.IsTrue(v.Term.isImmutable());
    }

    [TestMethod]
    public void testUnify_integer()
    {
        ClpVariable v = new ClpVariable();
        CoreConstraintStore e = new CoreConstraintStore();

        long min = 7;
        long max = 9;

        v.setMin(e, min);
        v.setMax(e, max);
        v = v.Term;

        // test successful Unify with every value within range
        for (long i = min; i <= max; i++)
        {
            IntegerNumber n = new IntegerNumber(i);

            Assert.IsTrue(n.Unify(v));
            Assert.AreEqual(TermType.INTEGER, v.Type);
            Assert.AreEqual(i, v.Long);

            v.Backtrack();
            Assert.AreEqual(TermType.CLP_VARIABLE, v.Type);

            Assert.IsTrue(v.Unify(n));
            Assert.AreEqual(TermType.INTEGER, v.Type);
            Assert.AreEqual(i, v.Long);

            v.Backtrack();
            Assert.AreEqual(TermType.CLP_VARIABLE, v.Type);
        }

        // test unsuccessful Unify with values either side of range
        IntegerNumber maxPlus1 = new IntegerNumber(max + 1);
        Assert.IsFalse(maxPlus1.Unify(v));
        Assert.IsFalse(v.Unify(maxPlus1));

        IntegerNumber minMinus1 = new IntegerNumber(min - 1);
        Assert.IsFalse(minMinus1.Unify(v));
        Assert.IsFalse(v.Unify(minMinus1));
    }

    [TestMethod]
    public void testUnify_variable()
    {
        ClpVariable v1 = new ClpVariable();
        Variable v2 = new Variable();

        Assert.IsTrue(v1.Unify(v2));
        Assert.AreSame(v1, v2.Term);

        v2.Backtrack();
        Assert.AreSame(v2, v2.Term);

        Assert.IsTrue(v2.Unify(v1));
        Assert.AreSame(v1, v2.Term);
    }

    [TestMethod]
    public void testUnify_clpVariable_self()
    {
        ClpVariable v1 = new ClpVariable();

        Assert.IsTrue(v1.Unify(v1));
        Assert.AreSame(v1, v1.Term);

        v1.Backtrack();
        Assert.AreSame(v1, v1.Term);
    }

    [TestMethod]
    public void testUnify_clpVariable_both_unbound()
    {
        var v1 = new ClpVariable();
        var v2 = new ClpVariable();

        Assert.IsTrue(v1.Unify(v2));
        Assert.AreSame(v1, v2.Term);

        v2.Backtrack();
        Assert.AreSame(v2, v2.Term);

        Assert.IsTrue(v2.Unify(v1));
        Assert.AreSame(v2, v1.Term);
    }

    [TestMethod]
    public void testUnify_clpVariable_overlap()
    {
        CoreConstraintStore e = new CoreConstraintStore();

        ClpVariable v1 = new ClpVariable();
        v1.setMin(e, 10);
        v1.setMax(e, 12);
        v1 = v1.Term;

        ClpVariable v2 = new ClpVariable();
        v2.setMin(e, 11);
        v2.setMax(e, 14);
        v2 = v2.Term;

        Assert.IsTrue(v1.Unify(v2));
        Assert.AreNotSame(v1, v1.Term);
        Assert.AreNotSame(v2, v1.Term);
        Assert.AreNotSame(v1, v2.Term);
        Assert.AreNotSame(v2, v2.Term);
        Assert.AreSame(v1.Term, v2.Term);
        Assert.AreEqual(11, v1.getMin(e));
        Assert.AreEqual(12, v1.getMax(e));
        Assert.AreEqual(11, v2.getMin(e));
        Assert.AreEqual(12, v2.getMax(e));

        v1.Backtrack();
        Assert.AreEqual(10, v1.getMin(e));
        Assert.AreEqual(12, v1.getMax(e));
        Assert.AreEqual(11, v2.getMin(e));
        Assert.AreEqual(12, v2.getMax(e));

        v2.Backtrack();
        Assert.AreEqual(10, v1.getMin(e));
        Assert.AreEqual(12, v1.getMax(e));
        Assert.AreEqual(11, v2.getMin(e));
        Assert.AreEqual(14, v2.getMax(e));
    }

    [TestMethod]
    public void testUnify_clpVariable_fail()
    {
        CoreConstraintStore e = new CoreConstraintStore();

        ClpVariable v1 = new ClpVariable();
        v1.setMin(e, 10);
        v1.setMax(e, 11);

        ClpVariable v2 = new ClpVariable();
        v2.setMin(e, 12);
        v2.setMax(e, 14);

        Assert.IsFalse(v1.Unify(v2));
        Assert.AreEqual(10, v1.getMin(e));
        Assert.AreEqual(11, v1.getMax(e));
        Assert.AreEqual(12, v2.getMin(e));
        Assert.AreEqual(14, v2.getMax(e));
    }

    [TestMethod]
    public void testUnify_decimal()
    {
        ClpVariable v = new ClpVariable();
        DecimalFraction d = new DecimalFraction(7);

        Assert.IsFalse(v.Unify(d));
        Assert.IsFalse(d.Unify(v));

        v.setMin(new CoreConstraintStore(), 7);
        v.setMax(new CoreConstraintStore(), 7);

        Assert.AreEqual(d.Double, v.Double, 0);

        Assert.IsFalse(v.Unify(d));
        Assert.IsFalse(d.Unify(v));
    }

    [TestMethod]
    public void testUnify_atom()
    {
        ClpVariable v = new ClpVariable();
        Atom a = new Atom("a");
        Assert.IsFalse(v.Unify(a));
        Assert.IsFalse(a.Unify(v));
    }

    [TestMethod]
    public void testUnify_structure()
    {
        ClpVariable v = new ClpVariable();
        Term s = Structure.CreateStructure("test", new Term[] { new Atom("a") });
        Assert.IsFalse(v.Unify(s));
        Assert.IsFalse(s.Unify(v));
    }

    [TestMethod]
    public void testUnify_list()
    {
        ClpVariable v = new ClpVariable();
        Term list = ListFactory.CreateList(new Atom("a"), EmptyList.EMPTY_LIST);
        Assert.IsFalse(v.Unify(list));
        Assert.IsFalse(list.Unify(v));
    }

    [TestMethod]
    public void testUnify_emptyList()
    {
        ClpVariable v = new ClpVariable();
        Assert.IsFalse(v.Unify(EmptyList.EMPTY_LIST));
        Assert.IsFalse(EmptyList.EMPTY_LIST.Unify(v));
    }

    [TestMethod]
    public void testBacktrack()
    {
        ClpVariable v = new ClpVariable();
        CoreConstraintStore environment = new CoreConstraintStore();

        Assert.AreSame(v, v.Term);
        Assert.AreEqual(long.MinValue, v.getMin(environment));
        Assert.AreEqual(long.MaxValue, v.getMax(environment));

        v.Backtrack(); // has no affect, nothing to Backtrack

        Assert.AreSame(v, v.Term);
        Assert.AreEqual(long.MinValue, v.getMin(environment));
        Assert.AreEqual(long.MaxValue, v.getMax(environment));

        v.setMin(environment, 7);

        ClpVariable other1 = v.Term;
        Assert.AreNotSame(v, other1);
        Assert.AreEqual(7, v.getMin(environment));
        Assert.AreEqual(long.MaxValue, v.getMax(environment));
        Assert.AreEqual(7, other1.getMin(environment));
        Assert.AreEqual(long.MaxValue, other1.getMax(environment));

        v.setMax(environment, 12);

        ClpVariable other2 = v.Term;
        Assert.AreSame(other2, other1.Term);
        Assert.AreNotSame(v, other2);
        Assert.AreNotSame(other1, other2);
        Assert.AreEqual(7, v.getMin(environment));
        Assert.AreEqual(12, v.getMax(environment));
        Assert.AreEqual(7, other1.getMin(environment));
        Assert.AreEqual(12, other1.getMax(environment));
        Assert.AreEqual(7, other2.getMin(environment));
        Assert.AreEqual(12, other2.getMax(environment));

        v.Backtrack(); // Backtrack set minimum to 7, set maximum to 12

        Assert.AreSame(v, v.Term);
        Assert.AreEqual(long.MinValue, v.getMin(environment));
        Assert.AreEqual(long.MaxValue, v.getMax(environment));
        Assert.AreEqual(7, other1.getMin(environment));
        Assert.AreEqual(12, other1.getMax(environment));
        Assert.AreEqual(7, other2.getMin(environment));
        Assert.AreEqual(12, other2.getMax(environment));

        other1.Backtrack(); // Backtrack set maximum to 12

        Assert.AreSame(other1, other1.Term);
        Assert.AreEqual(7, other1.getMin(environment));
        Assert.AreEqual(long.MaxValue, other1.getMax(environment));
        Assert.AreEqual(7, other2.getMin(environment));
        Assert.AreEqual(12, other2.getMax(environment));
    }

    [TestMethod]
    public void testBacktrack_structure_example_1()
    {
        ClpVariable v = new ClpVariable();
        v.setMin(new CoreConstraintStore(), 0);
        v.setMax(new CoreConstraintStore(), 0);

        Term structure = Structure.CreateStructure("test", new Term[] { v });

        Assert.AreNotSame(v, v.Term);

        structure.Backtrack();

        Assert.AreSame(v, v.Term);
    }

    [TestMethod]
    public void testBacktrack_structure_example_2()
    {
        ClpVariable v = new ClpVariable();

        Term structure = Structure.CreateStructure("test", new Term[] { v });

        v.setMin(new CoreConstraintStore(), 0);
        v.setMax(new CoreConstraintStore(), 0);

        Assert.AreNotSame(v, v.Term);

        structure.Backtrack();

        Assert.AreSame(v, v.Term);
    }

    [TestMethod]
    public void testBacktrack_structure_example_3()
    {
        ClpVariable v = new ClpVariable();

        Term structure = Structure.CreateStructure("test", new Term[] { v });

        v.setMin(new CoreConstraintStore(), 0);

        Assert.AreNotSame(v, v.Term);

        structure.Backtrack();

        Assert.AreSame(v, v.Term);
    }

    [TestMethod]
    public void testBacktrack_list_example_1()
    {
        ClpVariable v1 = new ClpVariable();
        v1.setMin(new CoreConstraintStore(), 0);
        v1.setMax(new CoreConstraintStore(), 0);

        ClpVariable v2 = new ClpVariable();
        v2.setMin(new CoreConstraintStore(), 0);
        v2.setMax(new CoreConstraintStore(), 0);

        Term list = ListFactory.CreateList(v1, v2);

        Assert.AreNotSame(v1, v1.Term);
        Assert.AreNotSame(v2, v2.Term);

        list.Backtrack();

        Assert.AreSame(v1, v1.Term);
        Assert.AreSame(v2, v2.Term);
    }

    [TestMethod]
    public void testBacktrack_list_example_2()
    {
        ClpVariable v1 = new ClpVariable();
        ClpVariable v2 = new ClpVariable();

        Term list = ListFactory.CreateList(v1, v2);

        v1.setMin(new CoreConstraintStore(), 0);
        v1.setMax(new CoreConstraintStore(), 0);
        v2.setMin(new CoreConstraintStore(), 0);
        v2.setMax(new CoreConstraintStore(), 0);

        Assert.AreNotSame(v1, v1.Term);
        Assert.AreNotSame(v2, v2.Term);

        list.Backtrack();

        Assert.AreSame(v1, v1.Term);
        Assert.AreSame(v2, v2.Term);
    }

    [TestMethod]
    public void testBacktrack_list_example_3()
    {
        ClpVariable v1 = new ClpVariable();
        ClpVariable v2 = new ClpVariable();

        Term list = ListFactory.CreateList(v1, v2);

        v1.setMin(new CoreConstraintStore(), 0);
        v2.setMax(new CoreConstraintStore(), 0);

        Assert.AreNotSame(v1, v1.Term);
        Assert.AreNotSame(v2, v2.Term);

        list.Backtrack();

        Assert.AreSame(v1, v1.Term);
        Assert.AreSame(v2, v2.Term);
    }

    [TestMethod]
    public void testNumeric()
    {
        ClpVariable v = new ClpVariable();
        CoreConstraintStore environment = new CoreConstraintStore();
        v.setMin(environment, 7);
        v.setMax(environment, 7);

        Assert.AreEqual(7, v.Long);
        Assert.AreEqual(7d, v.Double, 0);
    }

    [TestMethod]
    public void testNumeric_exception()
    {
        ClpVariable v = new ClpVariable();
        string expected = "Cannot use CLP_VARIABLE as a number as has more than one possible value: -9223372036854775808..9223372036854775807";

        try
        {
            v.Long;
            Assert.Fail();
        }
        catch (PrologException e)
        {
            Assert.AreEqual(expected, e.Message);
        }
        try
        {
            v.Double;
            Assert.Fail();
        }
        catch (PrologException e)
        {
            Assert.AreEqual(expected, e.Message);
        }
    }

    [TestMethod]
    public void testCalculate()
    {
        ClpVariable v = new ClpVariable();
        Assert.AreSame(v, v.Calculate(null));
    }

    [TestMethod]
    public void testConstraints()
    {
        ClpVariable v = new ClpVariable();
        Constraint c1 = mock(Constraint);
        Constraint c2 = mock(Constraint);
        Constraint c3 = mock(Constraint);

        Assert.IsTrue(v.Constraints.Count == 0);

        v.addConstraint(c1);
        v.addConstraint(c2);
        Assert.AreEqual(Arrays.asList(c1, c2), v.Constraints);

        // duplicates are allowed - but maybe they shouldn't be? TODO
        v.addConstraint(c2);
        Assert.AreEqual(Arrays.asList(c1, c2, c2), v.Constraints);

        // altering Copy of constraints doesn't alter ClpVariable
        v.
        // altering Copy of constraints doesn't alter ClpVariable
        Constraints.clear();
        Assert.AreEqual(Arrays.asList(c1, c2, c2), v.Constraints);

        // altering ClpVariable doesn't alter Copy of constraints
        List<Constraint> constraints = v.Constraints;
        v.addConstraint(c3);
        Assert.AreEqual(Arrays.asList(c1, c2, c2), constraints);
        Assert.AreEqual(Arrays.asList(c1, c2, c2, c3), v.Constraints);

        Assert.AreNotSame(v.Constraints, v.Constraints);
        Assert.AreEqual(v.Constraints, v.Constraints);
    }

    [TestMethod]
    public void testSetMax()
    {
        ClpVariable v = new ClpVariable();
        CoreConstraintStore environment = new CoreConstraintStore();
        v.setMin(environment, 7);

        Assert.AreSame(ExpressionResult.VALID, v.setMax(environment, 15));
        Assert.AreEqual(7, v.getMin(environment));
        Assert.AreEqual(15, v.getMax(environment));

        Assert.AreSame(ExpressionResult.VALID, v.setMax(environment, 15));
        Assert.AreEqual(7, v.getMin(environment));
        Assert.AreEqual(15, v.getMax(environment));

        Assert.AreSame(ExpressionResult.VALID, v.setMax(environment, 16));
        Assert.AreEqual(7, v.getMin(environment));
        Assert.AreEqual(15, v.getMax(environment));

        Assert.AreSame(ExpressionResult.VALID, v.setMax(environment, 8));
        Assert.AreEqual(7, v.getMin(environment));
        Assert.AreEqual(8, v.getMax(environment));

        Assert.AreSame(ExpressionResult.VALID, v.setMax(environment, 7));
        Assert.AreEqual(7, v.getMin(environment));
        Assert.AreEqual(7, v.getMax(environment));

        Assert.AreSame(ExpressionResult.INVALID, v.setMax(environment, 6));
    }

    [TestMethod]
    public void testSetMin()
    {
        ClpVariable v = new ClpVariable();
        CoreConstraintStore environment = new CoreConstraintStore();
        v.setMax(environment, 15);

        Assert.AreSame(ExpressionResult.VALID, v.setMin(environment, 7));
        Assert.AreEqual(7, v.getMin(environment));
        Assert.AreEqual(15, v.getMax(environment));

        Assert.AreSame(ExpressionResult.VALID, v.setMin(environment, 7));
        Assert.AreEqual(7, v.getMin(environment));
        Assert.AreEqual(15, v.getMax(environment));

        Assert.AreSame(ExpressionResult.VALID, v.setMin(environment, 6));
        Assert.AreEqual(7, v.getMin(environment));
        Assert.AreEqual(15, v.getMax(environment));

        Assert.AreSame(ExpressionResult.VALID, v.setMin(environment, 14));
        Assert.AreEqual(14, v.getMin(environment));
        Assert.AreEqual(15, v.getMax(environment));

        Assert.AreSame(ExpressionResult.VALID, v.setMin(environment, 15));
        Assert.AreEqual(15, v.getMin(environment));
        Assert.AreEqual(15, v.getMax(environment));

        Assert.AreSame(ExpressionResult.INVALID, v.setMin(environment, 16));
    }

    [TestMethod]
    public void testSetNot()
    {
        ClpVariable v = new ClpVariable();
        CoreConstraintStore environment = new CoreConstraintStore();
        v.setMin(environment, 6);
        v.setMax(environment, 9);

        Assert.AreSame(ExpressionResult.VALID, v.setNot(environment, 6));
        Assert.AreEqual(7, v.getMin(environment));
        Assert.AreEqual(9, v.getMax(environment));

        Assert.AreSame(ExpressionResult.VALID, v.setNot(environment, 6));
        Assert.AreEqual(7, v.getMin(environment));
        Assert.AreEqual(9, v.getMax(environment));

        Assert.AreSame(ExpressionResult.VALID, v.setNot(environment, 5));
        Assert.AreEqual(7, v.getMin(environment));
        Assert.AreEqual(9, v.getMax(environment));

        Assert.AreSame(ExpressionResult.VALID, v.setNot(environment, 10));
        Assert.AreEqual(7, v.getMin(environment));
        Assert.AreEqual(9, v.getMax(environment));
        Assert.AreEqual("7..9", v.ToString());

        Assert.AreSame(ExpressionResult.VALID, v.setNot(environment, 8));
        Assert.AreEqual(7, v.getMin(environment));
        Assert.AreEqual(9, v.getMax(environment));
        Assert.AreEqual("{7, 9}", v.ToString());

        Assert.AreSame(ExpressionResult.VALID, v.setNot(environment, 9));
        Assert.AreEqual(7, v.getMin(environment));
        Assert.AreEqual(7, v.getMax(environment));
        Assert.AreEqual("7", v.ToString());

        Assert.AreSame(ExpressionResult.INVALID, v.setNot(environment, 7));
    }

    [TestMethod]
    public void testSetMin_updated()
    {
        ClpVariable v = new ClpVariable();
        CoreConstraintStore environment = new CoreConstraintStore();
        Constraint constraint = mock(Constraint);
        when(constraint.enforce(environment)).thenReturn(ConstraintResult.FAILED);
        v.addConstraint(constraint);

        Assert.AreSame(ExpressionResult.VALID, v.setMin(environment, 6));

        verifyNoMoreInteractions(constraint);

        Assert.IsFalse(environment.resolve());

        verify(constraint).enforce(environment);
    }

    [TestMethod]
    public void testSetNot_updated()
    {
        ClpVariable v = new ClpVariable();
        CoreConstraintStore environment = new CoreConstraintStore();
        v.setMin(environment, 0);
        v.setMax(environment, 10);
        Constraint constraint = mock(Constraint);
        when(constraint.enforce(environment)).thenReturn(ConstraintResult.MATCHED);
        v.Term.addConstraint(constraint);

        Assert.AreSame(ExpressionResult.VALID, v.setNot(environment, 6));

        verifyNoMoreInteractions(constraint);

        Assert.IsTrue(environment.resolve());

        verify(constraint).enforce(environment);
    }

    [TestMethod]
    public void testSetMax_updated()
    {
        ClpVariable v = new ClpVariable();
        CoreConstraintStore environment = new CoreConstraintStore();
        Constraint constraint = mock(Constraint);
        when(constraint.enforce(environment)).thenReturn(ConstraintResult.UNRESOLVED);
        v.addConstraint(constraint);

        Assert.AreSame(ExpressionResult.VALID, v.setMax(environment, 6));

        verifyNoMoreInteractions(constraint);

        Assert.IsTrue(environment.resolve());

        verify(constraint).enforce(environment);
    }

    [TestMethod]
    public void testSetMin_no_change()
    {
        ClpVariable v = new ClpVariable();
        CoreConstraintStore environment = new CoreConstraintStore();
        Constraint constraint = mock(Constraint);
        v.addConstraint(constraint);

        Assert.AreSame(ExpressionResult.VALID, v.setMin(environment, long.MinValue));

        environment.resolve();

        verifyNoMoreInteractions(constraint);
    }

    [TestMethod]
    public void testSetMax_no_change()
    {
        ClpVariable v = new ClpVariable();
        CoreConstraintStore environment = new CoreConstraintStore();
        Constraint constraint = mock(Constraint);
        v.Term.addConstraint(constraint);

        Assert.AreSame(ExpressionResult.VALID, v.setMax(environment, long.MaxValue));

        environment.resolve();

        verifyNoMoreInteractions(constraint);
    }

    [TestMethod]
    public void testSetNot_no_change_outside_range()
    {
        ClpVariable v = new ClpVariable();
        CoreConstraintStore environment = new CoreConstraintStore();
        v.setMin(environment, 0);
        v.setMin(environment, 10);
        Constraint constraint = mock(Constraint);
        v.Term.addConstraint(constraint);

        Assert.AreSame(ExpressionResult.VALID, v.setNot(environment, 11));

        environment.resolve();

        verifyNoMoreInteractions(constraint);
    }

    [TestMethod]
    public void testSetNot_no_change_unbound()
    {
        ClpVariable v = new ClpVariable();
        CoreConstraintStore environment = new CoreConstraintStore();
        v.setMin(environment, 0);
        v.setMin(environment, 10);
        Constraint constraint = mock(Constraint);
        v.Term.Term.addConstraint(constraint);

        Assert.AreSame(ExpressionResult.VALID, v.setNot(environment, 11));

        environment.resolve();

        verifyNoMoreInteractions(constraint);
    }

    [TestMethod]
    public void testSetMin_Failed()
    {
        ClpVariable v = new ClpVariable();
        CoreConstraintStore environment = new CoreConstraintStore();
        v.setMax(environment, 5);
        Constraint constraint = mock(Constraint);
        v.Term.addConstraint(constraint);

        Assert.AreSame(ExpressionResult.INVALID, v.setMin(environment, 6));

        environment.resolve();

        verifyNoMoreInteractions(constraint);
    }

    [TestMethod]
    public void testSetMax_Failed()
    {
        ClpVariable v = new ClpVariable();
        CoreConstraintStore environment = new CoreConstraintStore();
        v.setMin(environment, 7);
        Constraint constraint = mock(Constraint);
        v.Term.addConstraint(constraint);

        Assert.AreSame(ExpressionResult.INVALID, v.setMax(environment, 6));

        environment.resolve();

        verifyNoMoreInteractions(constraint);
    }

    [TestMethod]
    public void testSetNot_Failed()
    {
        ClpVariable v = new ClpVariable();
        CoreConstraintStore environment = new CoreConstraintStore();
        v.setMin(environment, 6);
        v.setMax(environment, 6);
        Constraint constraint = mock(Constraint);
        v.Term.addConstraint(constraint);

        Assert.AreSame(ExpressionResult.INVALID, v.setNot(environment, 6));

        environment.resolve();

        verifyNoMoreInteractions(constraint);
    }

    [TestMethod]
    public void testReifyMatched()
    {
        ClpVariable v = new ClpVariable();
        CoreConstraintStore environment = new CoreConstraintStore();
        v.setMin(environment, 1);
        v.setMax(environment, 1);
        Assert.AreSame(ConstraintResult.MATCHED, v.reify(environment));
    }

    [TestMethod]
    public void testReifyFailed()
    {
        ClpVariable v = new ClpVariable();
        CoreConstraintStore environment = new CoreConstraintStore();
        v.setMin(environment, 0);
        v.setMax(environment, 0);
        Assert.AreSame(ConstraintResult.FAILED, v.reify(environment));
    }

    [TestMethod]
    public void testReifyUnresolved()
    {
        ClpVariable v = new ClpVariable();
        CoreConstraintStore environment = new CoreConstraintStore();
        v.setMin(environment, 0);
        v.setMax(environment, 1);
        Assert.AreSame(ConstraintResult.UNRESOLVED, v.reify(environment));
    }

    [TestMethod]
    public void testReifyToHigh()
    {
        ClpVariable v = new ClpVariable();
        CoreConstraintStore environment = new CoreConstraintStore();
        v.setMin(environment, 2);
        v.setMax(environment, 2);
        try
        {
            v.reify(environment);
            Assert.Fail();
        }
        catch (IllegalStateException e)
        {
            Assert.AreEqual("Expected 0 or 1 but got 2", e.Message);
        }

    }

    [TestMethod]
    public void testReifyToLow()
    {
        ClpVariable v = new ClpVariable();
        CoreConstraintStore environment = new CoreConstraintStore();
        v.setMin(environment, -1);
        v.setMax(environment, -1);
        try
        {
            v.reify(environment);
            Assert.Fail();
        }
        catch (IllegalStateException e)
        {
            Assert.AreEqual("Expected 0 or 1 but got -1", e.Message);
        }
    }

    [TestMethod]
    public void testEnforceMatched()
    {
        ClpVariable v = new ClpVariable();
        CoreConstraintStore environment = new CoreConstraintStore();
        v.setMin(environment, 1);
        v.setMax(environment, 1);
        Assert.AreSame(ConstraintResult.MATCHED, v.enforce(environment));
    }

    [TestMethod]
    public void testEnforceMatchedAndUpdated()
    {
        ClpVariable v = new ClpVariable();
        CoreConstraintStore environment = new CoreConstraintStore();
        v.setMin(environment, long.MinValue);
        v.setMax(environment, long.MaxValue);
        Assert.AreSame(ConstraintResult.MATCHED, v.enforce(environment));
        Assert.AreEqual(1, v.getMin(environment));
        Assert.AreEqual(1, v.getMax(environment));
    }

    [TestMethod]
    public void testEnforceFailed()
    {
        ClpVariable v = new ClpVariable();
        CoreConstraintStore environment = new CoreConstraintStore();
        v.setMin(environment, 0);
        v.setMax(environment, 0);
        Assert.AreSame(ConstraintResult.FAILED, v.enforce(environment));
    }

    [TestMethod]
    public void testEnforceToHigh()
    {
        ClpVariable v = new ClpVariable();
        CoreConstraintStore environment = new CoreConstraintStore();
        v.setMin(environment, 2);
        v.setMax(environment, 2);
        try
        {
            v.enforce(environment);
            Assert.Fail();
        }
        catch (IllegalStateException e)
        {
            Assert.AreEqual("Expected 0 or 1", e.Message);
        }
    }

    [TestMethod]
    public void testEnforceToLow()
    {
        ClpVariable v = new ClpVariable();
        CoreConstraintStore environment = new CoreConstraintStore();
        v.setMin(environment, -1);
        v.setMax(environment, -1);
        try
        {
            v.enforce(environment);
            Assert.Fail();
        }
        catch (IllegalStateException e)
        {
            Assert.AreEqual("Expected 0 or 1", e.Message);
        }
    }

    [TestMethod]
    public void testPreventMatched()
    {
        ClpVariable v = new ClpVariable();
        CoreConstraintStore environment = new CoreConstraintStore();
        v.setMin(environment, 0);
        v.setMax(environment, 0);
        Assert.AreSame(ConstraintResult.MATCHED, v.prevent(environment));
    }

    [TestMethod]
    public void testPreventMatchedAndUpdated()
    {
        ClpVariable v = new ClpVariable();
        CoreConstraintStore environment = new CoreConstraintStore();
        v.setMin(environment, long.MinValue);
        v.setMax(environment, long.MaxValue);
        Assert.AreSame(ConstraintResult.MATCHED, v.prevent(environment));
        Assert.AreEqual(0, v.getMin(environment));
        Assert.AreEqual(0, v.getMax(environment));
    }

    [TestMethod]
    public void testPreventFailed()
    {
        ClpVariable v = new ClpVariable();
        CoreConstraintStore environment = new CoreConstraintStore();
        v.setMin(environment, 1);
        v.setMax(environment, 1);
        Assert.AreSame(ConstraintResult.FAILED, v.prevent(environment));
    }

    [TestMethod]
    public void testPreventToHigh()
    {
        ClpVariable v = new ClpVariable();
        CoreConstraintStore environment = new CoreConstraintStore();
        v.setMin(environment, 2);
        v.setMax(environment, 2);
        try
        {
            v.prevent(environment);
            Assert.Fail();
        }
        catch (IllegalStateException e)
        {
            Assert.AreEqual("Expected 0 or 1", e.Message);
        }
    }

    [TestMethod]
    public void testPreventToLow()
    {
        ClpVariable v = new ClpVariable();
        CoreConstraintStore environment = new CoreConstraintStore();
        v.setMin(environment, -1);
        v.setMax(environment, -1);
        try
        {
            v.prevent(environment);
            Assert.Fail();
        }
        catch (IllegalStateException e)
        {
            Assert.AreEqual("Expected 0 or 1", e.Message);
        }
    }

    [TestMethod]
    public void testWalk()
    {
        // given
        ClpVariable testObject = new ClpVariable();

        Consumer<Expression> consumer = mock(Consumer);

        // when
        testObject.walk(consumer);

        // then
        verify(consumer).accept(testObject);
        verifyNoMoreInteractions(consumer);
    }

    [TestMethod]
    public void testReplace_null()
    {
        // given
        ClpVariable testObject = new ClpVariable();

        Function<LeafExpression, LeafExpression> function = mock(Function);
        when(testObject.replace(function)).thenReturn(null);

        // when
        Expression replacement = testObject.replace(function);
        Assert.AreSame(testObject, replacement);

        // then
        verify(function).apply(testObject);
        verifyNoMoreInteractions(function);
    }

    [TestMethod]
    public void testReplace_replacement()
    {
        // given
        ClpVariable testObject = new ClpVariable();
        org.prolog.clp.Variable expectedReplacement = new ClpConstraintStore.Builder().createVariable();

        Function<LeafExpression, LeafExpression> function = mock(Function);
        when(testObject.replace(function)).thenReturn(expectedReplacement);

        // when
        Expression replacement = testObject.replace(function);
        Assert.AreSame(expectedReplacement, replacement);

        // then
        verify(function).apply(testObject);
        verifyNoMoreInteractions(function);
    }

    [TestMethod]
    public void testCopy_no_bitset()
    {
        CoreConstraintStore e = new CoreConstraintStore();
        Constraint c1 = mock(Constraint);
        Constraint c2 = mock(Constraint);

        ClpVariable original = new ClpVariable();
        original.addConstraint(c1);
        original.setMin(e, 8);
        original.setMax(e, 12);
        original = original.Term;

        ClpVariable Copy = original.Copy();
        Assert.AreNotSame(Copy, original);
        Assert.AreSame(Copy, original.Term);

        Copy.addConstraint(c2);

        original.setMin(e, 9);
        original.setMax(e, 11);

        original.Backtrack();

        Assert.AreEqual(8, original.getMin(e));
        Assert.AreEqual(12, original.getMax(e));
        Assert.AreEqual("8..12", original.ToString());
        Assert.AreEqual(Arrays.asList(c1), original.Constraints);
        Assert.AreEqual(9, Copy.getMin(e));
        Assert.AreEqual(11, Copy.getMax(e));
        Assert.AreEqual("9..11", Copy.ToString());
        Assert.AreEqual(Arrays.asList(c1, c2), Copy.Term.getConstraints());
    }

    [TestMethod]
    public void testCopy_bitset()
    {
        CoreConstraintStore e = new CoreConstraintStore();
        Constraint c1 = mock(Constraint);
        Constraint c2 = mock(Constraint);

        ClpVariable original = new ClpVariable();
        original.addConstraint(c1);
        original.setMin(e, 7);
        original.setMax(e, 15);
        original.setNot(e, 12);
        original = original.Term;

        ClpVariable Copy = original.Copy();
        Assert.AreNotSame(Copy, original);
        Assert.AreSame(Copy, original.Term);

        Copy.addConstraint(c2);

        original.setMin(e, 8);
        original.setMax(e, 14);
        original.setNot(e, 11);

        original.Backtrack();

        Assert.AreEqual(7, original.getMin(e));
        Assert.AreEqual(15, original.getMax(e));
        Assert.AreEqual("{7, 8, 9, 10, 11, 13, 14, 15}", original.ToString());
        Assert.AreEqual(Arrays.asList(c1), original.Constraints);
        Assert.AreEqual(8, Copy.getMin(e));
        Assert.AreEqual(14, Copy.getMax(e));
        Assert.AreEqual("{8, 9, 10, 13, 14}", Copy.ToString());
        Assert.AreEqual(Arrays.asList(c1, c2), Copy.Term.getConstraints());
    }

    [TestMethod]
    public void testTermCopy()
    {
        ClpVariable v = new ClpVariable();
        try
        {
            v.Copy(new());
            Assert.Fail();
        }
        catch (PrologException e)
        {
            Assert.AreEqual("CLP_VARIABLE does not support Copy, so is not suitable for use in this scenario", e.Message);
        }

        CoreConstraintStore e = new CoreConstraintStore();
        v.setMin(e, 8);
        v.setMax(e, 8);

        Assert.AreSame(v.Term, v.Copy(new()));
    }

    [TestMethod]
    public void testTermCompare()
    {
        ClpVariable clpVariable1 = new ClpVariable();
        ClpVariable clpVariable2 = new ClpVariable();
        clpVariable2.setMin(new CoreConstraintStore(), 8);
        clpVariable2.setMax(new CoreConstraintStore(), 8);

        Atom a = new Atom("a");
        EmptyList el = EmptyList.EMPTY_LIST;
        Term l = ListFactory.CreateList(a, el);
        IntegerNumber i7 = new IntegerNumber(7);
        IntegerNumber i10 = new IntegerNumber(10);
        DecimalFraction d7 = new DecimalFraction(7.5);
        DecimalFraction d10 = new DecimalFraction(10.5);
        Term s = Structure.CreateStructure("test", new Term[] { i7, i10, d7, d10 });
        Variable v1 = new Variable();
        Variable v2 = new Variable();
        v2.Unify(new IntegerNumber(9));
        List<Term> list = Arrays.asList(a, v1, v2, el, l, i7, i10, d7, d10, s, clpVariable1, clpVariable2);
        Collections.shuffle(list);

        list.sort(TermComparator.TERM_COMPARATOR);

        Assert.AreEqual(list, Arrays.asList(v1, clpVariable1, d7, d10, i7, clpVariable2, v2, i10, el, a, l, s));
    }

    [TestMethod]
    public void testUnboundDoesNotEqualClpVariable()
    {
        ClpVariable v = new ClpVariable();

        Assert.IsTrue(v.Equals(v));
        Assert.IsFalse(v.Equals(new ClpVariable()));
    }

    [TestMethod]
    public void testUnboundDoesNotEqualInteger()
    {
        IntegerNumber i = new IntegerNumber(7);
        ClpVariable v = new ClpVariable();

        Assert.IsFalse(v.Equals(i));
        Assert.IsFalse(i.Equals(v));
    }

    [TestMethod]
    public void testUnboundDoesNotEqualIntegerMatchingMin()
    {
        IntegerNumber i = new IntegerNumber(7);

        ClpVariable v = new ClpVariable();
        Assert.AreSame(TermType.CLP_VARIABLE, v.Type);
        v.setMin(new CoreConstraintStore(), i.Long);
        v.setMax(new CoreConstraintStore(), i.Long + 1);
        Assert.AreSame(TermType.CLP_VARIABLE, v.Type);
        Assert.IsFalse(v.isImmutable());
        Assert.IsFalse(v.Term.isImmutable());

        Assert.IsFalse(v.Equals(i));
        Assert.IsFalse(i.Equals(v));

        Assert.IsFalse(v.Term.Equals(i));
        Assert.IsFalse(i.Equals(v.Term));
    }

    [TestMethod]
    public void testUnboundDoesNotEqualIntegerMatchingMax()
    {
        IntegerNumber i = new IntegerNumber(7);

        ClpVariable v = new ClpVariable();
        Assert.AreSame(TermType.CLP_VARIABLE, v.Type);
        v.setMin(new CoreConstraintStore(), i.Long - 1);
        v.setMax(new CoreConstraintStore(), i.Long);
        Assert.AreSame(TermType.CLP_VARIABLE, v.Type);
        Assert.IsFalse(v.isImmutable());
        Assert.IsFalse(v.Term.isImmutable());

        Assert.IsFalse(v.Equals(i));
        Assert.IsFalse(i.Equals(v));

        Assert.IsFalse(v.Term.Equals(i));
        Assert.IsFalse(i.Equals(v.Term));
    }

    [TestMethod]
    public void testBoundNotEqualsInteger()
    {
        IntegerNumber i = new IntegerNumber(7);

        ClpVariable v = new ClpVariable();
        Assert.AreSame(TermType.CLP_VARIABLE, v.Type);
        v.setMin(new CoreConstraintStore(), i.Long + 1);
        v.setMax(new CoreConstraintStore(), i.Long + 1);
        Assert.AreSame(TermType.INTEGER, v.Type);
        Assert.IsFalse(v.isImmutable());
        Assert.IsTrue(v.Term.isImmutable());

        Assert.IsFalse(v.Equals(i));
        Assert.IsFalse(i.Equals(v));

        Assert.IsFalse(v.Term.Equals(i));
        Assert.IsFalse(i.Equals(v.Term));
        Assert.AreNotEqual(i.GetHashCode(), v.Term.GetHashCode());
    }

    [TestMethod]
    public void testBoundEqualsInteger()
    {
        IntegerNumber i = new IntegerNumber(7);

        ClpVariable v = new ClpVariable();
        Assert.AreSame(TermType.CLP_VARIABLE, v.Type);
        v.setMin(new CoreConstraintStore(), i.Long);
        v.setMax(new CoreConstraintStore(), i.Long);
        Assert.AreSame(TermType.INTEGER, v.Type);
        Assert.IsFalse(v.isImmutable());
        Assert.IsTrue(v.Term.isImmutable());

        Assert.IsFalse(v.Equals(i));
        Assert.IsFalse(i.Equals(v));

        Assert.IsTrue(v.Term.Equals(i));
        Assert.IsTrue(i.Equals(v.Term));
        Assert.AreEqual(i.GetHashCode(), v.Term.GetHashCode());
    }

    [TestMethod]
    public void testBoundDoesNotEqualClpVariableMatchingMin()
    {
        int value = 42;

        ClpVariable v1 = new ClpVariable();
        v1.setMin(new CoreConstraintStore(), value);
        v1.setMax(new CoreConstraintStore(), value);

        ClpVariable v2 = new ClpVariable();
        v2.setMin(new CoreConstraintStore(), value);
        v2.setMax(new CoreConstraintStore(), value + 1);

        Assert.IsFalse(v1.Equals(v2));
        Assert.IsFalse(v2.Equals(v1));

        Assert.IsFalse(v1.Equals(v2.Term));
        Assert.IsFalse(v2.Equals(v1.Term));

        Assert.IsFalse(v1.Term.Equals(v2.Term));
        Assert.IsFalse(v2.Term.Equals(v1.Term));
    }

    [TestMethod]
    public void testBoundDoesNotEqualClpVariableMatchingMax()
    {
        int value = 42;

        ClpVariable v1 = new ClpVariable();
        v1.setMin(new CoreConstraintStore(), value);
        v1.setMax(new CoreConstraintStore(), value);

        ClpVariable v2 = new ClpVariable();
        v2.setMin(new CoreConstraintStore(), value - 1);
        v2.setMax(new CoreConstraintStore(), value);

        Assert.IsFalse(v1.Equals(v2));
        Assert.IsFalse(v2.Equals(v1));

        Assert.IsFalse(v1.Equals(v2.Term));
        Assert.IsFalse(v2.Equals(v1.Term));

        Assert.IsFalse(v1.Term.Equals(v2.Term));
        Assert.IsFalse(v2.Term.Equals(v1.Term));
    }

    [TestMethod]
    public void testBoundNotEqualsClpVariable()
    {
        int value = 42;

        ClpVariable v1 = new ClpVariable();
        v1.setMin(new CoreConstraintStore(), value);
        v1.setMax(new CoreConstraintStore(), value);

        ClpVariable v2 = new ClpVariable();
        v2.setMin(new CoreConstraintStore(), value + 1);
        v2.setMax(new CoreConstraintStore(), value + 1);

        Assert.IsFalse(v1.Equals(v2));
        Assert.IsFalse(v2.Equals(v1));

        Assert.IsFalse(v1.Equals(v2.Term));
        Assert.IsFalse(v2.Equals(v1.Term));

        Assert.IsFalse(v1.Term.Equals(v2.Term));
        Assert.IsFalse(v2.Term.Equals(v1.Term));
    }

    [TestMethod]
    public void testBoundEqualsClpVariable()
    {
        int value = 42;

        ClpVariable v1 = new ClpVariable();
        v1.setMin(new CoreConstraintStore(), value);
        v1.setMax(new CoreConstraintStore(), value);

        ClpVariable v2 = new ClpVariable();
        v2.setMin(new CoreConstraintStore(), value);
        v2.setMax(new CoreConstraintStore(), value);

        Assert.IsFalse(v1.Equals(v2));
        Assert.IsFalse(v2.Equals(v1));

        Assert.IsFalse(v1.Equals(v2.Term));
        Assert.IsFalse(v2.Equals(v1.Term));

        Assert.IsTrue(v1.Term.Equals(v2.Term));
        Assert.IsTrue(v2.Term.Equals(v1.Term));
        Assert.AreEqual(v1.Term.GetHashCode(), v2.Term.GetHashCode());
    }
}
