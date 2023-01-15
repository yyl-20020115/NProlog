/*
 * Copyright 2023 S. Webber
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
namespace Org.NProlog.Core.Predicate.Builtin.Clp;

[TestClass]

public class CommonExpressionTest {
   [TestMethod]
   public void testMinus() {
      ExpressionFactory f = CommonExpression.minus();
      Expression e = f.createExpression(new Expression[] {new FixedValue(7)});

      Assert.AreSame(Minus, e.GetType());
      Assert.AreEqual(-7L, e.getMax(null));
      Assert.AreEqual(-7L, e.getMin(null));
   }

   [TestMethod]
   public void testAbsolute() {
      ExpressionFactory f = CommonExpression.absolute();
      Expression e = f.createExpression(new Expression[] {new FixedValue(-7)});

      Assert.AreSame(Absolute, e.GetType());
      Assert.AreEqual(7L, e.getMax(null));
      Assert.AreEqual(7L, e.getMin(null));
   }

   [TestMethod]
   public void testAdd() {
      ExpressionFactory f = CommonExpression.Add();
      Expression e = f.createExpression(new Expression[] {new FixedValue(7), new FixedValue(3)});

      Assert.AreSame(Add, e.GetType());
      Assert.AreEqual(10L, e.getMax(null));
      Assert.AreEqual(10L, e.getMin(null));
   }

   [TestMethod]
   public void testSubtract() {
      ExpressionFactory f = CommonExpression.subtract();
      Expression e = f.createExpression(new Expression[] {new FixedValue(7), new FixedValue(3)});

      Assert.AreSame(Subtract, e.GetType());
      Assert.AreEqual(4L, e.getMax(null));
      Assert.AreEqual(4L, e.getMin(null));
   }

   [TestMethod]
   public void testMultiply() {
      ExpressionFactory f = CommonExpression.multiply();
      Expression e = f.createExpression(new Expression[] {new FixedValue(7), new FixedValue(3)});

      Assert.AreSame(Multiply, e.GetType());
      Assert.AreEqual(21L, e.getMax(null));
      Assert.AreEqual(21L, e.getMin(null));
   }

   [TestMethod]
   public void testDivide() {
      ExpressionFactory f = CommonExpression.divide();
      Expression e = f.createExpression(new Expression[] {new FixedValue(27), new FixedValue(3)});

      Assert.AreSame(Divide, e.GetType());
      Assert.AreSame(9L, e.getMax(null));
      Assert.AreSame(9L, e.getMin(null));
   }

   [TestMethod]
   public void testMaximum() {
      ExpressionFactory f = CommonExpression.maximum();
      Expression e = f.createExpression(new Expression[] {new FixedValue(123), new FixedValue(456)});

      Assert.AreSame(Maximum, e.GetType());
      Assert.AreEqual(456L, e.getMax(null));
      Assert.AreEqual(456L, e.getMin(null));
   }

   [TestMethod]
   public void testMinimum() {
      ExpressionFactory f = CommonExpression.minimum();
      Expression e = f.createExpression(new Expression[] {new FixedValue(123), new FixedValue(456)});

      Assert.AreSame(Minimum, e.GetType());
      Assert.AreEqual(123L, e.getMax(null));
      Assert.AreEqual(123L, e.getMin(null));
   }
}
