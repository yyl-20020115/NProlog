/*
 * Copyright 2020 S. Webber
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
using Org.NProlog.Core.Predicate;
using Org.NProlog.Core.Predicate.Udp;
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Api;

[TestClass]
public class QueryPlanTest : TestUtils
{
    /**
     * Confirm that a QueryPlan can be evaluated multiple times.
     */
    [TestMethod]
    public void TestPlanReusable()
    {
        var prolog = new Prolog();
        prolog.ConsultReader(new StringReader("test(a).test(b).test(c)."));
        var plan = prolog.CreatePlan("test(X).");

        var result1 = plan.ExecuteQuery();
        var result2 = plan.ExecuteQuery();
        var statement = plan.CreateStatement();
        statement.SetAtomName("X", "b");
        var result3 = statement.ExecuteQuery();

        Assert.IsTrue(result1.Next());
        Assert.AreEqual("a", result1.GetAtomName("X"));

        Assert.IsTrue(result2.Next());
        Assert.AreEqual("a", result2.GetAtomName("X"));

        Assert.IsTrue(result3.Next());
        Assert.IsFalse(result3.Next());

        Assert.IsTrue(result1.Next());
        Assert.AreEqual("b", result1.GetAtomName("X"));

        Assert.IsTrue(result1.Next());
        Assert.AreEqual("c", result1.GetAtomName("X"));

        Assert.IsTrue(result2.Next());
        Assert.AreEqual("b", result2.GetAtomName("X"));

        Assert.IsTrue(result2.Next());
        Assert.AreEqual("c", result2.GetAtomName("X"));
    }

    /**
     * Confirm that QUeryPlan calls PreprocessablePredicateFactory.Preprocess(Term).
     */
    [TestMethod]
    public void TestPreprocessed()
    {
        var mockPreprocessablePredicateFactory = new MockPreprocessablePredicateFactory();
        var mockPredicateFactory = new MockPredicateFactory();
        var prolog = new Prolog();
        prolog.AddPredicateFactory(new PredicateKey("test", 0), mockPreprocessablePredicateFactory);

        When(mockPreprocessablePredicateFactory?.Preprocess(new Atom("test"))).ThenReturn(mockPredicateFactory);
        When(mockPredicateFactory?.GetPredicate(new Term[0])).ThenReturn(PredicateUtils.TRUE);

        var plan = prolog.CreatePlan("test.");
        Verify(mockPreprocessablePredicateFactory)?.Preprocess(new Atom("test"));

        plan.ExecuteOnce();
        plan.ExecuteOnce();
        plan.ExecuteOnce();
        Verify(mockPredicateFactory, Times(3))?.GetPredicate(new Term[0]);

        VerifyNoMoreInteractions(mockPreprocessablePredicateFactory, mockPredicateFactory);
    }

    /**
     * Example of evaluating the same query as both a QueryPlan and a QueryStatement.
     * <p>
     * The QueryPlan version is able to determine that it is exhausted while the QueryStatement version does not.
     */
    [TestMethod]
    public void TestComparePlanToStatement()
    {
        var prolog = new Prolog();
        prolog.ConsultReader(new StringReader("test(1, a).test(_, b).test(3, c)."));
        var query = "test(2, X).";

        var plan = prolog.CreatePlan(query);
        var statement = prolog.CreateStatement(query);

        var planResult = plan.ExecuteQuery();
        var statementResult = statement.ExecuteQuery();

        Assert.IsTrue(planResult.Next());
        Assert.IsTrue(statementResult.Next());

        Assert.AreEqual("b", planResult.GetAtomName("X"));
        Assert.AreEqual("b", statementResult.GetAtomName("X"));

        Assert.IsTrue(planResult.IsExhausted);
        Assert.IsFalse(statementResult.IsExhausted);
    }

    [TestMethod]
    public void TestExecuteOnce()
    {
        var mockPredicateFactory = new MockPredicateFactory();
        When(mockPredicateFactory?.GetPredicate(System.Array.Empty<Term>())).ThenReturn(PredicateUtils.TRUE);
        var prolog = new Prolog();
        prolog.AddPredicateFactory(new PredicateKey("mock", 0), mockPredicateFactory);

        var plan = prolog.CreatePlan("repeat, mock.");

        plan.ExecuteOnce();
        Verify(mockPredicateFactory)?.GetPredicate(new Term[0]);
        VerifyNoMoreInteractions(mockPredicateFactory);
    }

    [TestMethod]
    public void TestExecuteOnceNoSolution()
    {
        var p = new Prolog().CreatePlan("true, true, fail.");
        try
        {
            p.ExecuteOnce();
            Assert.Fail();
        }
        catch (PrologException prologException)
        {
            Assert.AreEqual("Failed to find a solution for: ,(,(true, true), fail)", prologException.Message);
        }
    }
}
