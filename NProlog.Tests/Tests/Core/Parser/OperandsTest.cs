/*
 * Copyright 2013-2014 S. Webber
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

namespace Org.NProlog.Core.Parser;

[TestClass]
public class OperandsTest
{
    private static readonly string[] ASSOCIATIVITES = { "fx", "fy", "xfx", "xfy", "yfx", "xf", "yf" };

    [TestMethod]
    public void TestInvalidAssociativity()
    {
        try
        {
            new Operands().AddOperand("test", "yfy", 100);
            Assert.Fail();
        }
        catch (PrologException e)
        {
            Assert.AreEqual("Cannot add operand with associativity of: yfy as the only values allowed are: [xfx, xfy, yfx, fx, fy, xf, yf]", e.Message);
        }
    }

    [TestMethod]
    public void TestDuplicate()
    {
        var operands = new Operands();
        operands.AddOperand("test", "xfx", 100);
        // test can re-Add if the same precedence and associativity
        operands.AddOperand("test", "xfx", 100);
        try
        {
            // test can not re-Add if different precedence
            operands.AddOperand("test", "xfx", 101);
            Assert.Fail();
        }
        catch (PrologException e)
        {
            // expected
        }
        try
        {
            // test can not re-Add if different associativity
            operands.AddOperand("test", "xfy", 100);
            Assert.Fail();
        }
        catch (PrologException e)
        {
            // expected
        }
        operands.AddOperand("test", "fx", 100);
    }

    [TestMethod]
    public void TestOperands()
    {
        var operands = new Operands();

        List<TestOperand> testCases = new();

        // Add 3 operands for each type of associativity
        int ctr = 1;
        foreach (var associativity in ASSOCIATIVITES)
        {
            for (int i = 0; i < 3; i++)
            {
                var t = new TestOperand("name" + ctr, associativity, ctr);
                testCases.Add(t);
                Assert.IsFalse(operands.IsDefined(t.name));
                operands.AddOperand(t.name, t.associativity, t.priority);
                Assert.IsTrue(operands.IsDefined(t.name));
                ctr++;
            }
        }

        foreach (var t in testCases)
        {
            AssertOperand(operands, t);
        }
    }

    private static void AssertOperand(Operands o, TestOperand t)
    {
        Assert.IsTrue(o.IsDefined(t.name));
        Assert.AreEqual(t.Prefix, o.Prefix(t.name));
        Assert.AreEqual(t.Infix, o.Infix(t.name));
        Assert.AreEqual(t.Postfix, o.Postfix(t.name));
        Assert.AreEqual(t.Fx, o.Fx(t.name));
        Assert.AreEqual(t.Fy, o.Fy(t.name));
        Assert.AreEqual(t.Xfx, o.Xfx(t.name));
        Assert.AreEqual(t.Xfy, o.Xfy(t.name));
        Assert.AreEqual(t.Yfx, o.Yfx(t.name));
        Assert.AreEqual(t.Xf, o.Xf(t.name));
        Assert.AreEqual(t.Yf, o.Yf(t.name));

        try
        {
            Assert.AreEqual(t.priority, o.GetPrefixPriority(t.name));
            Assert.IsTrue(t.Prefix);
        }
        catch (NullReferenceException e)
        {
            Assert.IsFalse(t.Prefix);
        }
        try
        {
            Assert.AreEqual(t.priority, o.GetInfixPriority(t.name));
            Assert.IsTrue(t.Infix);
        }
        catch (NullReferenceException e)
        {
            Assert.IsFalse(t.Infix);
        }
        try
        {
            Assert.AreEqual(t.priority, o.GetPostfixPriority(t.name));
            Assert.IsTrue(t.Postfix);
        }
        catch (NullReferenceException e)
        {
            Assert.IsFalse(t.Postfix);
        }
    }

    public class TestOperand
    {
        public readonly string name;
        public readonly string associativity;
        public readonly int priority;

        public TestOperand(string name, string associativity, int priority)
        {
            this.name = name;
            this.associativity = associativity;
            this.priority = priority;
        }

        public bool Prefix => Fx || Fy;

        public bool Infix => Xfx || Xfy || Yfx;

        public bool Postfix => Xf || Yf;

        public bool Fx => "fx" == associativity;

        public bool Fy => "fy" == associativity;

        public bool Xfx => "xfx" == associativity;

        public bool Xfy => "xfy" == associativity;

        public bool Yfx => "yfx" == associativity;

        public bool Xf => "xf" == associativity;

        public bool Yf => "yf" == associativity;
    }
}
