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
using NProlog.Tests.Tests.Core.Math;
using Org.NProlog.Core.Exceptions;
using Org.NProlog.Core.Kb;
using Org.NProlog.Core.Predicate;
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Math;

[TestClass]
public class ArithmeticOperatorsTest : TestUtils
{
    private readonly KnowledgeBase kb = TestUtils.CreateKnowledgeBase();
    private const string dummyOperatorName = "dummy_arithmetic_operator";
    private readonly PredicateKey dummyOperatorKey = new PredicateKey(dummyOperatorName, 1);
    private const int dummyTermArgument = 7;
    private readonly Structure dummyTerm = Structure(dummyOperatorName, IntegerNumber(dummyTermArgument));

    [TestMethod]
    public void TestGetNumericIntegerNumber()
    {
        var c = CreateOperators();
        var i = IntegerNumber(1);
        Assert.AreSame(i, c.GetNumeric(i));
    }

    [TestMethod]
    public void TestGetNumericDecimalFraction()
    {
        var c = CreateOperators();
        var d = DecimalFraction(17.6);
        Assert.AreSame(d, c.GetNumeric(d));
    }

    [TestMethod]
    public void TestGetNumericException()
    {
        var c = CreateOperators();
        try
        {
            c.GetNumeric(Variable("X"));
            Assert.Fail();
        }
        catch (PrologException e)
        {
            Assert.AreEqual("Cannot get Numeric for term: X of type: VARIABLE", e.Message);
        }
    }

    [TestMethod]
    public void TestGetNumericPredicate()
    {
        var c = CreateOperators();

        // try to use arithmetic operator by a name that there is no match for (expect exception)
        try
        {
            c.GetNumeric(dummyTerm);
            Assert.Fail();
        }
        catch (PrologException e)
        {
            Assert.AreEqual("Cannot find arithmetic operator: dummy_arithmetic_operator/1", e.Message);
        }

        // Add new arithmetic operator
        c.AddArithmeticOperator(dummyOperatorKey, typeof(DummyArithmeticOperatorDefaultConstructor));

        // assert that the factory is now using the newly added arithmetic operator
        var n = c.GetNumeric(dummyTerm);
        Assert.AreSame(typeof(IntegerNumber), n.GetType());
        Assert.AreEqual(dummyTermArgument + 1, n.Long);
    }

    [TestMethod]
    public void TestGetArithmeticOperator()
    {
        // Add operator
        var c = CreateOperators();
        c.AddArithmeticOperator(dummyOperatorKey, typeof(DummyArithmeticOperatorDefaultConstructor));

        // assert getArithmeticOperator returns the newly added operator
        Assert.AreSame(typeof(DummyArithmeticOperatorDefaultConstructor), c.GetArithmeticOperator(dummyOperatorKey).GetType());
        Assert.AreSame(c.GetArithmeticOperator(dummyOperatorKey), c.GetArithmeticOperator(dummyOperatorKey));
    }

    [TestMethod]
    public void TestGetPreprocessedArithmeticOperatorNumeric()
    {
        var c = CreateOperators();

        var i = new IntegerNumber(7);
        ArithmeticOperator preprocessed = c.GetPreprocessedArithmeticOperator(i);

        Assert.AreSame(i, preprocessed);
    }

    [TestMethod]
    public void TestGetPreprocessedArithmeticOperatorVariable()
    {
        var c = CreateOperators();

        Assert.IsNull(c.GetPreprocessedArithmeticOperator(Variable()));
    }

    [TestMethod]
    public void TestGetPreprocessedArithmeticOperatorUnknownStructure()
    {
        var c = CreateOperators();

        Structure expression = Structure(dummyOperatorName, IntegerNumber(7));
        Assert.IsNull(c.GetPreprocessedArithmeticOperator(expression));
    }

    [TestMethod]
    public void TestGetPreprocessedArithmeticOperatorOperatorNotPreprocessable()
    {
        var mockOperator = new MockArithmeticOperator();
        var c = CreateOperators();
        c.AddArithmeticOperator(dummyOperatorKey, mockOperator);

        var preprocessed = c.GetPreprocessedArithmeticOperator(Structure(dummyOperatorName, IntegerNumber(7)));

        Assert.AreSame(mockOperator, preprocessed);
        VerifyNoInteractions(mockOperator);
    }

    [TestMethod]
    public void TestGetPreprocessedArithmeticOperatorOperatorPreprocessable()
    {
        var expression = Structure(dummyOperatorName, IntegerNumber(7));
        var mockPreprocessableOperator = new MockPreprocessableArithmeticOperator();
        var mockPreprocessedOperator = new MockPreprocessableArithmeticOperator();
        When(mockPreprocessableOperator.Preprocess(expression)).ThenReturn(mockPreprocessedOperator);
        var c = CreateOperators();
        c.AddArithmeticOperator(dummyOperatorKey, mockPreprocessableOperator);

        ArithmeticOperator preprocessed = c.GetPreprocessedArithmeticOperator(expression);

        Assert.AreSame(mockPreprocessableOperator, preprocessed);
        Verify(mockPreprocessableOperator).Preprocess(expression);
        VerifyNoMoreInteractions(mockPreprocessableOperator, mockPreprocessedOperator);
    }

    [TestMethod]
    public void TestAddExistingOperatorName()
    {
        var c = CreateOperators();

        // Add new arithmetic operator class name
        c.AddArithmeticOperator(dummyOperatorKey, typeof(DummyArithmeticOperatorDefaultConstructor).Name);

        // attempt to Add arithmetic operator again
        // (should Assert.Fail now a arithmetic operator with the same name already exists in the factoty)
        try
        {
            c.AddArithmeticOperator(dummyOperatorKey, typeof(DummyArithmeticOperatorDefaultConstructor).Name);
            Assert.Fail("could re-Add arithmetic operator named: " + dummyOperatorName);
        }
        catch (PrologException)
        {
            // expected;
        }
        try
        {
            c.AddArithmeticOperator(dummyOperatorKey, new DummyArithmeticOperatorDefaultConstructor());
            Assert.Fail("could re-Add arithmetic operator named: " + dummyOperatorName);
        }
        catch (PrologException)
        {
            // expected;
        }
    }

    [TestMethod]
    public void TestAddExistingOperatorInstance()
    {
        var c = CreateOperators();

        // Add new arithmetic operator instance
        c.AddArithmeticOperator(dummyOperatorKey, new DummyArithmeticOperatorDefaultConstructor());

        // attempt to Add arithmetic operator again
        // (should Assert.Fail now a arithmetic operator with the same name already exists in the factoty)
        try
        {
            c.AddArithmeticOperator(dummyOperatorKey, typeof(DummyArithmeticOperatorDefaultConstructor).Name);
            Assert.Fail("could re-Add arithmetic operator named: " + dummyOperatorName);
        }
        catch (PrologException)
        {
            // expected;
        }
        try
        {
            c.AddArithmeticOperator(dummyOperatorKey, new DummyArithmeticOperatorDefaultConstructor());
            Assert.Fail("could re-Add arithmetic operator named: " + dummyOperatorName);
        }
        catch (PrologException)
        {
            // expected;
        }
    }

    [TestMethod]
    public void TestAddOperatorError()
    {
        var c = CreateOperators();

        // Add new arithmetic operator with invalid name
        c.AddArithmeticOperator(dummyOperatorKey, "an invalid class name");
        try
        {
            c.GetNumeric(dummyTerm);
            Assert.Fail();
        }
        catch (Exception e)
        {
            // expected as specified class name is invalid
            Assert.AreEqual("Could not create new ArithmeticOperator using: an invalid class name", e.Message);
        }
    }

    /** Test using a static method to Add a arithmetic operator that does not have a public no arg constructor. */
    [TestMethod]
    public void TestAddOperatorUsingStaticMethod()
    {
        var c = CreateOperators();
        var className = typeof(DummyArithmeticOperatorPublicConstructor).FullName;
        var allName = className + "/GetInstance";
        c.AddArithmeticOperator(dummyOperatorKey, allName);
        Numeric n;
        try
        {
            n = c.GetNumeric(dummyTerm);
            Assert.AreSame(typeof(IntegerNumber), n.GetType());
            Assert.AreEqual(dummyTermArgument * 3, n.Long);
        }
        catch (Exception e)
        {
            // expected as specified class name is invalid
            Assert.AreEqual("Could not create new ArithmeticOperator using: " + allName, e.Message);
        }

    }

    private ArithmeticOperators CreateOperators()
    {
        return new ArithmeticOperators(kb);
    }

}
/** ArithmeticOperator used to test that new arithmetic operators can be added to the factory. */
public class DummyArithmeticOperatorDefaultConstructor : ArithmeticOperator, KnowledgeBaseConsumer
{
    KnowledgeBase kb;

    public KnowledgeBase KnowledgeBase { get=>kb; set=>kb=value; }
    public DummyArithmeticOperatorDefaultConstructor()
    {

    }
    /**
     * @return an IntegerNumber with a value of the first input argument + 1
     */

    public Numeric Calculate(Term[] args)
    {
        if (kb == null)
        {
            // KnowledgeBase= should be called by ArithmeticOperators when it creates an instance of this class
            throw new SystemException("KnowledgeBase not set on " + this);
        }
        long input = TermUtils.CastToNumeric(args[0]).Long;
        long output = input + 1;
        return new IntegerNumber(output);
    }


    public void SetKnowledgeBase(KnowledgeBase kb)
    {
        this.kb = kb;
    }
}

/** ArithmeticOperator used to test that new arithmetic operators can be created using a static method. */
public class DummyArithmeticOperatorPublicConstructor : ArithmeticOperator, KnowledgeBaseConsumer
{
    KnowledgeBase kb;

    public KnowledgeBase KnowledgeBase { get => this.kb; set => this.kb = value; }

    public static DummyArithmeticOperatorPublicConstructor GetInstance()
    {
        return new DummyArithmeticOperatorPublicConstructor();
    }

    private DummyArithmeticOperatorPublicConstructor()
    {
        // private as want to test creation using GetInstance static method
    }

    /**
     * @return an IntegerNumber with a value of the first input argument + 1
     */

    public Numeric Calculate(Term[] args)
    {
        if (kb == null)
        {
            // KnowledgeBase= should be called by ArithmeticOperators when it creates an instance of this class
            throw new SystemException("KnowledgeBase not set on " + this);
        }
        long input = TermUtils.CastToNumeric(args[0]).Long;
        long output = input * 3;
        return new IntegerNumber(output);
    }


    public void SetKnowledgeBase(KnowledgeBase kb)
    {
        this.kb = kb;
    }
}
