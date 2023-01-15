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
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Math;

[TestClass]
public class AbstractArithmeticOperatorTest : TestUtils
{
    // a non-abstract implementation of ArithmeticOperator (so we can create and test it)
    public class DummyArithmeticOperator : AbstractArithmeticOperator
    {
    }

    [TestMethod]
    public void TestWrongNumberOfArgumentsException()
    {
        for (int i = 0; i < 10; i++)
        {
            AssertWrongNumberOfArgumentsException(i);
        }
    }

    private void AssertWrongNumberOfArgumentsException(int numberOfArguments)
    {
        try
        {
            var c = new DummyArithmeticOperator();
            c.KnowledgeBase = (TestUtils.CreateKnowledgeBase());
            c.Calculate(CreateArgs(numberOfArguments, IntegerNumber()));
            Assert.Fail();
        }
        catch (Exception e)
        {
            string expectedMessage = "The ArithmeticOperator: Org.NProlog.Core.Math.AbstractArithmeticOperatorTest+DummyArithmeticOperator does not accept the number of arguments: "
                                     + numberOfArguments;
            Assert.AreEqual(expectedMessage, e.Message);
        }
    }
    public class AAO : AbstractArithmeticOperator
    {
        Numeric expected;
        public AAO(Numeric expected)
        {
            this.expected = expected;
        }
        public override Numeric Calculate(Numeric n1)
        {
            return expected;
        }
    };
    [TestMethod]
    public void TestOneArg()
    {
        Numeric expected = IntegerNumber(14);
        AbstractArithmeticOperator c = new AAO(expected);

        c.KnowledgeBase = (CreateKnowledgeBase());
        Assert.AreSame(expected, c.Calculate(new Term[] { IntegerNumber() }));
        Assert.AreSame(expected, c.Calculate(new Term[] { DecimalFraction() }));
    }
    public class AAO2 : AbstractArithmeticOperator
    {
        Numeric expected;
        public AAO2(Numeric expected)
        {
            this.expected = expected;
        }

        public override Numeric Calculate(Numeric n1, Numeric n2)
        {
            return expected;
        }
    }
    [TestMethod]
    public void TestTwoArgs()
    {
        Numeric expected = IntegerNumber(14);
        AbstractArithmeticOperator c = new AAO2(expected);
        c.KnowledgeBase = (CreateKnowledgeBase());
        Assert.AreSame(expected, c.Calculate(new Term[] { IntegerNumber(), IntegerNumber() }));
        Assert.AreSame(expected, c.Calculate(new Term[] { DecimalFraction(), DecimalFraction() }));
        Assert.AreSame(expected, c.Calculate(new Term[] { IntegerNumber(), DecimalFraction() }));
        Assert.AreSame(expected, c.Calculate(new Term[] { DecimalFraction(), IntegerNumber() }));
    }
    public class AAO3 : AbstractArithmeticOperator
    {
        public override Numeric Calculate(Numeric n1)
        {
            return n1;
        }
    }

    [TestMethod]
    public void TestInvalidArgument()
    {
        AbstractArithmeticOperator c = new AAO3();
        c.KnowledgeBase = (CreateKnowledgeBase());

        AssertUnexpectedAtom(c, Atom());
        AssertUnexpectedVariable(c, Variable());
    }
    public class AAO4 : AbstractArithmeticOperator
    {
        public override Numeric Calculate(Numeric n1, Numeric n2)
        {
            return n1;
        }

    }
    [TestMethod]
    public void TestInvalidArguments()
    {
        AbstractArithmeticOperator c = new AAO4();

        c.KnowledgeBase = (CreateKnowledgeBase());

        AssertUnexpectedAtom(c, Atom(), Atom());
        AssertUnexpectedAtom(c, IntegerNumber(), Atom());
        AssertUnexpectedAtom(c, Atom(), IntegerNumber());
        AssertUnexpectedVariable(c, Variable(), Variable());
        AssertUnexpectedVariable(c, IntegerNumber(), Variable());
        AssertUnexpectedVariable(c, Variable(), IntegerNumber());
    }

    private void AssertUnexpectedAtom(AbstractArithmeticOperator c, params Term[] args)
    {
        try
        {
            c.Calculate(args);
            Assert.Fail();
        }
        catch (PrologException e)
        {
            Assert.AreEqual("Cannot find arithmetic operator: test/0", e.Message);
        }
    }

    private void AssertUnexpectedVariable(AbstractArithmeticOperator c, params Term[] args)
    {
        try
        {
            c.Calculate(args);
            Assert.Fail();
        }
        catch (PrologException e)
        {
            Assert.AreEqual("Cannot get Numeric for term: X of type: VARIABLE", e.Message);
        }
    }
    public class AAO5 : AbstractArithmeticOperator
    {
        public override Numeric Calculate(Numeric n1)
        {
            return new IntegerNumber(n1.Long + 5);
        }

    }
    [TestMethod]
    public void TestArithmeticFunctionArgument()
    {
        AbstractArithmeticOperator c = new AAO5();
        c.KnowledgeBase = (TestUtils.CreateKnowledgeBase());
        Structure arithmeticFunction = Structure("*", IntegerNumber(3), IntegerNumber(7));
        Numeric result = c.Calculate(new Term[] { arithmeticFunction });
        Assert.AreEqual(26, result.Long); // 26 = (3*7)+5
    }
    public class AAO6 : AbstractArithmeticOperator
    {
        public override Numeric Calculate(Numeric n1, Numeric n2)
        {
            return new IntegerNumber(n1.Long - n2.Long);
        }
    }
    [TestMethod]
    public void TestArithmeticFunctionArguments()
    {
        AbstractArithmeticOperator c = new AAO6();
        c.KnowledgeBase = (CreateKnowledgeBase());
        Structure f1 = Structure("*", IntegerNumber(3), IntegerNumber(7));
        Structure f2 = Structure("/", IntegerNumber(12), IntegerNumber(2));
        Numeric result = c.Calculate(new Term[] { f1, f2 });
        Assert.AreEqual(15, result.Long); // 15 = (3*7)-(12/2)
    }
    public class AAO7 : AbstractArithmeticOperator
    {

        public override Numeric Calculate(Numeric n1)
        {
            return new IntegerNumber(-(n1.Long * 2));
        }

    }
    [TestMethod]
    public void TestPreprocessOneArgument()
    {
        AbstractArithmeticOperator c = new AAO7();
        c.KnowledgeBase = (CreateKnowledgeBase());
        Assert.AreEqual(IntegerNumber(-84), c.Preprocess(Structure("dummy", IntegerNumber(42))));
        Assert.AreSame(c, c.Preprocess(Structure("dummy", Variable())));
        // TODO test PreprocessedUnaryOperator
        Assert.AreEqual("PreprocessedUnaryOperator",
                    c.Preprocess(Structure("dummy", Structure("+", IntegerNumber(), Variable()))).GetType().Name);
    }
    public class AAO8 : AbstractArithmeticOperator
    {
        public override Numeric Calculate(Numeric n1, Numeric n2)
        {
            return new IntegerNumber(n1.Long - n2.Long + 42);
        }

    }

    [TestMethod]
    public void TestPreprocessTwoArguments()
    {
        AbstractArithmeticOperator c = new AAO8();
        c.KnowledgeBase = (CreateKnowledgeBase());
        Assert.AreEqual(IntegerNumber(47), c.Preprocess(Structure("dummy", IntegerNumber(8), IntegerNumber(3))));
        Assert.AreSame(c, c.Preprocess(Structure("dummy", Variable(), Variable())));
        // TODO test PreprocessedBinaryOperator
        Assert.AreEqual("PreprocessedBinaryOperator",
                    c.Preprocess(Structure("dummy", Variable(), Structure("+", IntegerNumber(), Variable()))).GetType().Name);
        Assert.AreEqual("PreprocessedBinaryOperator",
                    c.Preprocess(Structure("dummy", Structure("+", IntegerNumber(), Variable()), Variable())).GetType().Name);
        Assert.AreEqual("PreprocessedBinaryOperator",
                    c.Preprocess(Structure("dummy", Structure("+", IntegerNumber(), Variable()), Structure("+", IntegerNumber(), Variable()))).GetType().Name);
    }

    public class AAO9 : AbstractArithmeticOperator
    {
        public override bool IsPure => false;

    }
    [TestMethod]
    public void TestPreprocessNotPure()
    {
        AbstractArithmeticOperator c = new AAO9();
        Assert.AreSame(c, c.Preprocess(Structure("dummy", IntegerNumber(42))));
        Assert.AreSame(c, c.Preprocess(Structure("dummy", Variable())));
    }
    public class AAO10 : AbstractArithmeticOperator
    {

    }

    [TestMethod]
    public void TestIsPure()
    {
        AbstractArithmeticOperator c = new AAO10();

        Assert.IsTrue(c.IsPure);

    }
}
