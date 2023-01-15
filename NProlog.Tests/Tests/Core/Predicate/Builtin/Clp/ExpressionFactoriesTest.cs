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
public class ExpressionFactoriesTest {
   private static readonly Structure DUMMY_TERM = structure("dummy_clp_expression", integerNumber(7));
   private static readonly PredicateKey DUMMY_KEY = PredicateKey.CreateForTerm(DUMMY_TERM);

   private readonly KnowledgeBase kb = TestUtils.CreateKnowledgeBase();

   [TestMethod]
   public void testToExpressionClpVariable() {
      ExpressionFactories expressions = new ExpressionFactories(kb);
      ClpVariable v = new ClpVariable();
      HashSet<ClpVariable> vars = new HashSet<>();

      Assert.AreSame(v, expressions.toExpression(v, vars));
      Assert.AreEqual(1, vars.Count);
      Assert.IsTrue(vars.Contains(v));
   }

   [TestMethod]
   public void testToExpressionVariable() {
      ExpressionFactories expressions = new ExpressionFactories(kb);
      Variable projogVariable = new Variable();
      HashSet<ClpVariable> vars = new ();

      ClpVariable clpVariable = (ClpVariable) expressions.toExpression(projogVariable, vars);
      Assert.AreEqual(1, vars.Count);
      Assert.IsTrue(vars.Contains(clpVariable));
      Assert.AreSame(clpVariable, projogVariable.Term);
   }

   [TestMethod]
   public void testToExpressionIntegerNumber() {
      ExpressionFactories expressions = new ExpressionFactories(kb);
      IntegerNumber i = integerNumber(42);

      FixedValue expression = (FixedValue) expressions.toExpression(i, Collections.emptySet());
      Assert.AreSame(i.Long, expression.getMin(null));
   }

   [TestMethod]
   public void testToExpressionDecimalFraction() {
      ExpressionFactories expressions = new ExpressionFactories(kb);
      DecimalFraction d = new DecimalFraction(1.5);

      try {
         expressions.toExpression(d, Collections.emptySet());
         Assert.Fail();
      } catch (PrologException e) {
         Assert.AreEqual("Cannot get CLP expression for term: 1.5 of type: FRACTION", e.Message);
      }
   }

   [TestMethod]
   public void testToExpressionUnknownAtom() {
      ExpressionFactories expressions = new ExpressionFactories(kb);
      Atom a = new Atom("a");

      try {
         expressions.toExpression(a, Collections.emptySet());
         Assert.Fail();
      } catch (PrologException e) {
         Assert.AreEqual("Cannot find CLP expression: a/0", e.Message);
      }
   }

   [TestMethod]
   public void testToExpressionPredicate() {
      ExpressionFactories expressions = new ExpressionFactories(kb);

      // try to use CLP expression factory by a name that there is no match for (expect exception)
      try {
         expressions.toExpression(DUMMY_TERM, Collections.emptySet());
         Assert.Fail();
      } catch (PrologException e) {
         Assert.AreEqual("Cannot find CLP expression: dummy_clp_expression/1", e.Message);
      }

      // Add new CLP expression factory
      expressions.addExpressionFactory(DUMMY_KEY, DummyClpExpressionDefaultConstructor.Name);

      // assert that the factory is now using the newly added CLP expression factory
      Expression expression = expressions.toExpression(DUMMY_TERM, Collections.emptySet());
      Assert.AreSame(DummyClpExpressionDefaultConstructor.DUMMY_EXPRESSION, expression);
   }

   [TestMethod]
   public void testAddDuplicate() {
      ExpressionFactories expressions = new ExpressionFactories(kb);
      expressions.addExpressionFactory(DUMMY_KEY, DummyClpExpressionDefaultConstructor.Name);

      try {
         expressions.addExpressionFactory(DUMMY_KEY, DummyClpExpressionDefaultConstructor.Name);
         Assert.Fail();
      } catch (PrologException e) {
         Assert.AreEqual("Already defined CLP expression: dummy_clp_expression/1", e.Message);
      }
   }

   public static class DummyClpExpressionDefaultConstructor : ExpressionFactory {
      private readonly static Expression DUMMY_EXPRESSION = new FixedValue(180);

      
      public Expression createExpression(Expression[] args) {
         Assert.AreEqual(1, args.Length);
         long expectedValue = ((IntegerNumber) DUMMY_TERM.GetArgument(0)).Long;
         Assert.AreEqual(expectedValue, ((FixedValue) args[0]).getMin(null));
         return DUMMY_EXPRESSION;
      }
   }
}
