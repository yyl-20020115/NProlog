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
using Org.NProlog.Api;
using Org.NProlog.Core.Exceptions;
using Org.NProlog.Core.Predicate.Udp;
using System.Text;

namespace Org.NProlog.Core.Predicate;

/**
 * Tests of attempting to replace or update an already defined predicate.
 * <p>
 * See: https://github.com/s-webber/prolog/issues/195
 */
[TestClass]
public class PredicatesTest : TestUtils
{
    private readonly PredicateKey KEY = new ("test", 2);

    [TestMethod]
    public void TestCannotReplacePredicateFactoryWithAnotherPredicateFactory()
    {
        var prolog = new Prolog();

        // given that a build-in predicate is associated with the key
        prolog.AddPredicateFactory(KEY, new MockPredicateFactory());

        // attempting to associate another built-in predicate with the key should cause an exception
        try
        {
            prolog.AddPredicateFactory(KEY, new MockPredicateFactory());
            Assert.Fail();
        }
        catch (PrologException e)
        {
            Assert.AreEqual("Already defined: test/2", e.Message);
        }
    }

    [TestMethod]
    public void TestCannotReplacePredicateFactoryWithNonDynamicUserDefinedPredicate()
    {
        var prolog = new Prolog();

        // given that a build-in predicate is associated with the key
        prolog.AddPredicateFactory(KEY, new MockPredicateFactory());

        // attempting to Add user defined clauses for the key should cause an exception
        try
        {
            prolog.ConsultReader(new StringReader("test(a, b)."));
            Assert.Fail();
        }
        catch (PrologException e)
        {
            Assert.AreEqual("Cannot replace already defined built-in predicate: test/2", e.InnerException.Message);
        }
    }

    [TestMethod]
    public void TestCannotReplacePredicateFactoryWithDynamicUserDefinedPredicate()
    {
        var prolog = new Prolog();

        // given that a build-in predicate is associated with the key
        prolog.AddPredicateFactory(KEY, new MockPredicateFactory());

        // attempting to Add dynamic user defined clauses for the key should cause an exception
        try
        {
            prolog.ConsultReader(new StringReader("?- dynamic(test/2). test(a, b)."));
            Assert.Fail();
        }
        catch (PrologException e)
        {
            Assert.AreEqual("Cannot replace already defined built-in predicate: test/2", e.InnerException.Message);
        }
    }

    [TestMethod]
    public void TestCannotReplacePredicateFactoryWithAssertedPredicate()
    {
        var prolog = new Prolog();

        // given that a build-in predicate is associated with the key
        prolog.AddPredicateFactory(KEY, new MockPredicateFactory());

        // attempting to assert clauses for the key should cause an exception
        try
        {
            prolog.ExecuteOnce("assert(test(a, b)).");
            Assert.Fail();
        }
        catch (PrologException e)
        {
            Assert.AreEqual("Cannot replace already defined built-in predicate: test/2", e.Message);
        }
    }

    [TestMethod]
    public void TestCannotReplaceNonDynamicUserDefinedPredicateWithPredicateFactory()
    {
        var prolog = new Prolog();

        // given that a non-dynamic user defined predicate is associated with the key
        prolog.ConsultReader(new StringReader("test(a, b)."));

        // attempting to associate a built-in predicate with the key should cause an exception
        try
        {
            prolog.AddPredicateFactory(KEY, new MockPredicateFactory());
            Assert.Fail();
        }
        catch (PrologException e)
        {
            Assert.AreEqual("Already defined: test/2", e.Message);
        }
    }

    [TestMethod]
    public void TestCannotUpdateNonDynamicUserDefinedPredicateWithNonDynamicUserDefinedPredicate()
    {
        var prolog = new Prolog();

        // given that a non-dynamic user defined predicate is associated with the key
        prolog.ConsultReader(new StringReader("test(a, b)."));

        // attempting to Add more user defined clauses for the key should cause an exception
        try
        {
            prolog.ConsultReader(new StringReader("test(c, d)."));
            Assert.Fail();
        }
        catch (PrologException e)
        {
            var expected = "Cannot Append to already defined user defined predicate as it is not dynamic. You can set the predicate to dynamic by adding the following line to start of the file that the predicate is defined in:\n?- dynamic(test/2).";
            Assert.AreEqual(expected, e.InnerException.Message);
        }
    }

    [TestMethod]
    public void TestCannotUpdateNonDynamicUserDefinedPredicateWithDynamicUserDefinedPredicate()
    {
        var prolog = new Prolog();

        // given that a non-dynamic user defined predicate is associated with the key
        prolog.ConsultReader(new StringReader("test(a, b)."));

        // attempting to Add more user defined clauses for the key should cause an exception
        try
        {
            prolog.ConsultReader(new StringReader("?- dynamic(test/2). test(c, d)."));
            Assert.Fail();
        }
        catch (PrologException e)
        {
            Assert.AreEqual("Predicate has already been defined and is not dynamic: test/2", e.InnerException.Message);
        }
    }

    [TestMethod]
    public void TestCannotUpdateNonDynamicUserDefinedPredicateWithAssertedPredicate()
    {
        var prolog = new Prolog();

        // given that a non-dynamic user defined predicate is associated with the key
        prolog.ConsultReader(new StringReader("test(a, b)."));

        // attempting to Add more user defined clauses for the key should cause an exception
        try
        {
            prolog.ExecuteOnce("assert(test(a, b)).");
            Assert.Fail();
        }
        catch (PrologException e)
        {
            Assert.AreEqual("Cannot add clause to already defined user defined predicate as it is not dynamic: test/2 clause: test(a, b)", e.Message);
        }
    }

    [TestMethod]
    public void TestCannotReplaceDynamicUserDefinedPredicateWithPredicateFactory()
    {
        var prolog = new Prolog();

        // given that a dynamic user defined predicate is associated with the key
        prolog.ConsultReader(new StringReader("?- dynamic(test/2). test(a, b)."));

        // attempting to associate a built-in predicate with the key should cause an exception
        try
        {
            prolog.AddPredicateFactory(KEY, new MockPredicateFactory());
            Assert.Fail();
        }
        catch (PrologException e)
        {
            Assert.AreEqual("Already defined: test/2", e.Message);
        }
    }

    [TestMethod]
    public void TestCanUpdateDynamicUserDefinedPredicateWithUserDefinedPredicate()
    {
        var prolog = new Prolog();

        // given that a dynamic user defined predicate is associated with the key
        prolog.ConsultReader(new StringReader("?- dynamic(test/2). test(a, b)."));

        // querying it should find the defined clause
        var r = prolog.ExecuteQuery("test(X, Y).");
        Assert.IsTrue(r.Next());
        Assert.AreEqual("a", r.GetAtomName("X"));
        Assert.AreEqual("b", r.GetAtomName("Y"));
        Assert.IsFalse(r.Next());

        // attempting to Add more user defined clauses for the key should succeed, as was declared dynamic when first consulted
        prolog.ConsultReader(new StringReader("test(c, d). test(e, f)."));

        // querying it should find the original defined clause and the subsequently defined clauses
        r = prolog.ExecuteQuery("test(X, Y).");
        Assert.IsTrue(r.Next());
        Assert.AreEqual("a", r.GetAtomName("X"));
        Assert.AreEqual("b", r.GetAtomName("Y"));
        Assert.IsTrue(r.Next());
        Assert.AreEqual("c", r.GetAtomName("X"));
        Assert.AreEqual("d", r.GetAtomName("Y"));
        Assert.IsTrue(r.Next());
        Assert.AreEqual("e", r.GetAtomName("X"));
        Assert.AreEqual("f", r.GetAtomName("Y"));
        Assert.IsFalse(r.Next());
    }

    [TestMethod]
    public void TestCanUpdateDynamicUserDefinedPredicateWithDynamicUserDefinedPredicate()
    {
        var prolog = new Prolog();

        // given that a dynamic user defined predicate is associated with the key
        prolog.ConsultReader(new StringReader("?- dynamic(test/2). test(a, b)."));

        // querying it should find the defined clause
        var r = prolog.ExecuteQuery("test(X, Y).");
        Assert.IsTrue(r.Next());
        Assert.AreEqual("a", r.GetAtomName("X"));
        Assert.AreEqual("b", r.GetAtomName("Y"));
        Assert.IsFalse(r.Next());

        // attempting to Add more user defined clauses for the key should succeed, as was declared dynamic when first consulted
        prolog.ConsultReader(new StringReader("?- dynamic(test/2). test(c, d). test(e, f)."));

        // querying it should find the original defined clause and the subsequently defined clauses
        r = prolog.ExecuteQuery("test(X, Y).");
        Assert.IsTrue(r.Next());
        Assert.AreEqual("a", r.GetAtomName("X"));
        Assert.AreEqual("b", r.GetAtomName("Y"));
        Assert.IsTrue(r.Next());
        Assert.AreEqual("c", r.GetAtomName("X"));
        Assert.AreEqual("d", r.GetAtomName("Y"));
        Assert.IsTrue(r.Next());
        Assert.AreEqual("e", r.GetAtomName("X"));
        Assert.AreEqual("f", r.GetAtomName("Y"));
        Assert.IsFalse(r.Next());
    }

    [TestMethod]
    public void TestCanUpdateAssertedPredicateWithUserDefinedPredicate()
    {
        var prolog = new Prolog();

        // given that a clause has been asserted for the key
        prolog.ExecuteOnce("assert(test(a, b)).");

        // querying it should find the defined clause
        var r = prolog.ExecuteQuery("test(X, Y).");
        Assert.IsTrue(r.Next());
        Assert.AreEqual("a", r.GetAtomName("X"));
        Assert.AreEqual("b", r.GetAtomName("Y"));
        Assert.IsFalse(r.Next());

        // attempting to Add more user defined clauses for the key should succeed, as was first created via an assert so is dynamic
        prolog.ConsultReader(new StringReader("test(c, d). test(e, f)."));

        // querying it should find the original defined clause and the subsequently defined clauses
        r = prolog.ExecuteQuery("test(X, Y).");
        Assert.IsTrue(r.Next());
        Assert.AreEqual("a", r.GetAtomName("X"));
        Assert.AreEqual("b", r.GetAtomName("Y"));
        Assert.IsTrue(r.Next());
        Assert.AreEqual("c", r.GetAtomName("X"));
        Assert.AreEqual("d", r.GetAtomName("Y"));
        Assert.IsTrue(r.Next());
        Assert.AreEqual("e", r.GetAtomName("X"));
        Assert.AreEqual("f", r.GetAtomName("Y"));
        Assert.IsFalse(r.Next());
    }

    [TestMethod]
    public void TestCannotReplaceAssertedPredicateWithPredicateFactory()
    {
        var prolog = new Prolog();

        // given that a clause has been asserted for the key
        prolog.ExecuteOnce("assert(test(a, b)).");

        // attempting to associate a built-in predicate with the key should cause an exception
        try
        {
            prolog.AddPredicateFactory(KEY, new MockPredicateFactory());
            Assert.Fail();
        }
        catch (PrologException e)
        {
            Assert.AreEqual("Already defined: test/2", e.Message);
        }
    }

    /**
     * Test scenario described in https://github.com/s-webber/prolog/issues/195
     * <p>
     * <pre>
     * 1. Consult resource containing facts that have been defined as dynamic, and a rule that uses those facts.
     * 2. Consult another resource that Contains an additional fact for the predicate defined in step 1.
     * 3. Query the rule defined in step to confirm it uses that facts defined in both steps 1 and 2.
     * </pre>
     */
    [TestMethod]
    public void TestAppendToAlreadyDefinedClauseUsedByRule()
    {
        var prolog = new Prolog();

        // given that a dynamic user defined predicate is associated with the key
        var input1 = new StringBuilder();
        input1.Append("?- dynamic(test/2)."); // define as dynamic so can be updated by later consultations
        input1.Append("test(a,1).");
        input1.Append("test(b,2).");
        input1.Append("test(c,3).");
        input1.Append("test(d,4).");
        input1.Append("test(e,5).");
        input1.Append("test(f,6).");
        input1.Append("test(g,7).");
        input1.Append("test(h,8).");
        input1.Append("test(i,9).");
        input1.Append("testRule(X) :- test(X, Y), Y mod 2 =:= 0.");
        prolog.ConsultReader(new StringReader(input1.ToString()));

        // querying it should find the defined clause
        var plan = prolog.CreatePlan("testRule(X).");
        var r = plan.ExecuteQuery();
        Assert.IsTrue(r.Next());
        Assert.AreEqual("b", r.GetAtomName("X"));
        Assert.IsTrue(r.Next());
        Assert.AreEqual("d", r.GetAtomName("X"));
        Assert.IsTrue(r.Next());
        Assert.AreEqual("f", r.GetAtomName("X"));
        Assert.IsTrue(r.Next());
        Assert.AreEqual("h", r.GetAtomName("X"));
        Assert.IsFalse(r.Next());

        Assert.IsTrue(Enumerable.SequenceEqual(new string[] { "b", "d", "f", "h" }.ToList(), plan.CreateStatement().FindAllAsAtomName()));

        // attempting to Add more user defined clauses for the key should succeed, as was declared dynamic when first consulted
        var input2 = new StringBuilder();
        input2.Append("test(a,1).");

        prolog.ConsultReader(new StringReader("test(j,10)."));

        // querying it should find the original defined clause and the subsequently defined clauses
        r = plan.ExecuteQuery();
        Assert.IsTrue(r.Next());
        Assert.AreEqual("b", r.GetAtomName("X"));
        Assert.IsTrue(r.Next());
        Assert.AreEqual("d", r.GetAtomName("X"));
        Assert.IsTrue(r.Next());
        Assert.AreEqual("f", r.GetAtomName("X"));
        Assert.IsTrue(r.Next());
        Assert.AreEqual("h", r.GetAtomName("X"));
        Assert.IsTrue(r.Next());
        Assert.AreEqual("j", r.GetAtomName("X"));
        Assert.IsFalse(r.Next());

        Assert.IsTrue(Enumerable.SequenceEqual(new string[] { "b", "d", "f", "h", "j" }.ToList(), plan.CreateStatement().FindAllAsAtomName()));
    }

    [TestMethod]
    public void TestGetPredicate()
    {
        var p = CreateKnowledgeBase().Predicates;
        Assert.AreSame(PredicateUtils.TRUE, p.GetPredicate(Atom("true")));
        Assert.AreSame(PredicateUtils.FALSE, p.GetPredicate(Atom("does_not_exist")));
    }
}
