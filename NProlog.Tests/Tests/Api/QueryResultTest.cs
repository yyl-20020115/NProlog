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
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Api;

[TestClass]
public class QueryResultTest
{
    [TestMethod]
    public void TestNoSolutions()
    {
        var r = new Prolog().ExecuteQuery("fail.");
        Assert.IsFalse(r.IsExhausted);
        Assert.IsFalse(r.Next());
        Assert.IsTrue(r.IsExhausted);
    }

    [TestMethod]
    public void TestOneSolution()
    {
        var r = new Prolog().ExecuteQuery("true.");
        Assert.IsFalse(r.IsExhausted);
        Assert.IsTrue(r.Next());
        Assert.IsTrue(r.IsExhausted);
        Assert.IsFalse(r.Next());
    }

    [TestMethod]
    public void TestMultipleSolutions()
    {
        var r = new Prolog().ExecuteQuery("repeat(3).");
        Assert.IsFalse(r.IsExhausted);
        Assert.IsTrue(r.Next());
        Assert.IsFalse(r.IsExhausted);
        Assert.IsTrue(r.Next());
        Assert.IsFalse(r.IsExhausted);
        Assert.IsTrue(r.Next());
        Assert.IsTrue(r.IsExhausted);
        Assert.IsFalse(r.Next());
    }

    [TestMethod]
    public void TestMultipleSolutionsWithVariable()
    {
        var p = new Prolog();
        p.ConsultReader(new StringReader("test(a, 1).test(b, 2).test(c, 3)."));
        var r = p.ExecuteQuery("test(X, Y).");
        Assert.IsFalse(r.IsExhausted);
        Assert.IsTrue(r.Next());
        Assert.AreEqual("a", r.GetAtomName("X"));
        Assert.AreEqual(1, r.GetLong("Y"));
        Assert.IsFalse(r.IsExhausted);
        Assert.IsTrue(r.Next());
        Assert.AreEqual("b", r.GetAtomName("X"));
        Assert.AreEqual(2, r.GetLong("Y"));
        Assert.IsFalse(r.IsExhausted);
        Assert.IsTrue(r.Next());
        Assert.AreEqual("c", r.GetAtomName("X"));
        Assert.AreEqual(3, r.GetLong("Y"));
        Assert.IsTrue(r.IsExhausted);
        Assert.IsFalse(r.Next());
    }

    [TestMethod]
    public void TestCutOnFirstAttempt()
    {
        QueryResult r = new Prolog().ExecuteQuery("repeat, !, fail.");
        Assert.IsFalse(r.IsExhausted);
        Assert.IsFalse(r.Next());
        Assert.IsTrue(r.IsExhausted);
    }

    [TestMethod]
    public void TestCutOnRetry()
    {
        QueryResult r = new Prolog().ExecuteQuery("repeat, !.");
        Assert.IsFalse(r.IsExhausted);
        Assert.IsTrue(r.Next());
        Assert.IsFalse(r.IsExhausted);
        Assert.IsFalse(r.Next());
        Assert.IsTrue(r.IsExhausted);
    }

    [TestMethod]
    public void TestPrologExceptionOnExecuteQuery()
    {
        Prolog p = new Prolog();
        StringReader sr = new StringReader("a(A) :- b(A). b(Z) :- c(Z, 5). c(X,Y) :- Z is X + Y, Z < 9.");
        p.ConsultReader(sr);
        QueryStatement s = p.CreateStatement("a(X).");
        try
        {
            // as a/1 only has a single clause then will try to evaluate as part of PredicateFactory.GetPredicate(), which is why exception occurs now rather than on a later call to .next()
            s.ExecuteQuery();
            Assert.Fail();
        }
        catch (PrologException e)
        {
            Assert.AreSame(typeof(PrologException), e.GetType()); // check it is not a sub-class
            Assert.AreEqual("Cannot get Numeric for term: X of type: VARIABLE", e.Message);
        }
    }

    [TestMethod]
    public void TestPrologExceptionOnNext()
    {
        Prolog p = new Prolog();
        p.ConsultReader(new StringReader("a(A) :- b(A). a(A) :- A = test. b(Z) :- c(Z, 5). c(X,Y) :- Z is X + Y, Z < 9."));
        QueryStatement s = p.CreateStatement("a(X).");
        QueryResult r = s.ExecuteQuery();
        try
        {
            // as a/1 only has multiple clauses then not try to evaluate as part of PredicateFactory.GetPredicate(), which is why exception only occurs on call to .next()
            r.Next();
            Assert.Fail();
        }
        catch (PrologException e)
        {
            Assert.AreSame(typeof(PrologException), e.GetType()); // check it is not a sub-class
            Assert.AreEqual("Cannot get Numeric for term: X of type: VARIABLE", e.Message);
        }
    }

    [TestMethod]
    public void TestGetTerm()
    {
        QueryResult r = new Prolog().ExecuteQuery("X = test(a, 1).");
        Assert.IsTrue(r.Next());
        Term expected = Structure.CreateStructure("test", new Term[] { new Atom("a"), new IntegerNumber(1) });
        var actual = r.GetTerm("X");
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void TestGetTermUnknownVariable()
    {
        QueryResult r = new Prolog().ExecuteQuery("X = test(a, 1).");
        Assert.IsTrue(r.Next());
        try
        {
            r.GetTerm("Y");
            Assert.Fail();
        }
        catch (PrologException e)
        {
            Assert.AreEqual("Unknown variable ID: Y. Query Contains the variables: [X]", e.Message);
        }
    }

    [TestMethod]
    public void TestGetTermBeforeNext()
    {
        QueryResult r = new Prolog().ExecuteQuery("X = test(a, 1).");
        try
        {
            r.GetTerm("X");
            Assert.Fail();
        }
        catch (PrologException e)
        {
            Assert.AreEqual("Query not yet evaluated. Call QueryResult.next() before attempting to get value of variables.", e.Message);
        }
    }

    [TestMethod]
    public void TestGetTermAfterFailure()
    {
        Prolog p = new Prolog();
        p.ConsultReader(new StringReader("a(1)."));
        QueryResult r = p.ExecuteQuery("a(X).");

        // assert first evaluation succeeds
        Assert.IsTrue(r.Next());
        Assert.AreEqual(1, r.GetLong("X"));

        // assert second evaluation fails and subsequent call to GetTerm throws an exception
        Assert.IsFalse(r.Next());
        try
        {
            r.GetTerm("X");
            Assert.Fail();
        }
        catch (PrologException e)
        {
            Assert.AreEqual("No more solutions. Last call to QueryResult.next() returned false.", e.Message);
        }
    }

    [TestMethod]
    public void TestGetAtomName()
    {
        QueryResult r = new Prolog().ExecuteQuery("X = a.");
        Assert.IsTrue(r.Next());
        Assert.AreEqual("a", r.GetAtomName("X"));
    }

    [TestMethod]
    public void TestGetDouble()
    {
        QueryResult r = new Prolog().ExecuteQuery("X = 1.5.");
        Assert.IsTrue(r.Next());
        Assert.AreEqual(1.5, r.GetDouble("X"), 0);
    }

    [TestMethod]
    public void TestGetLong()
    {
        QueryResult r = new Prolog().ExecuteQuery("X = 1.");
        Assert.IsTrue(r.Next());
        Assert.AreEqual(1L, r.GetLong("X"));
    }

    [TestMethod]
    public void TestGetVariableIds()
    {
        var r = new Prolog().ExecuteQuery("X = 1, Y=a, Z=[].");
        HashSet<string> expected = new();
        expected.Add("X");
        expected.Add("Y");
        expected.Add("Z");
        Assert.AreEqual(StringUtils.ToString(expected), StringUtils.ToString(r.GetVariableIds()));
    }
}
