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
using Org.NProlog.Core.Kb;
using Org.NProlog.Core.Parser;
using Org.NProlog.Core.Predicate;
using Org.NProlog.Core.Predicate.Udp;
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Api;

[TestClass]
public class QueryStatementTest : TestUtils
{
    private readonly KnowledgeBase kb = TestUtils.CreateKnowledgeBase();

    [TestMethod]
    public void TestSetTerm()
    {
        var s = new QueryStatement(kb, "X = Y.");
        var term = Core.Terms.Structure.CreateStructure("test", new Term[] { new Atom("a") });
        s.SetTerm("Y", term);
        Assert.AreSame(term, s.FindFirstAsTerm());
    }

    [TestMethod]
    public void TestSetAtomName()
    {
        var s = new QueryStatement(kb, "X = Y.");
        s.SetAtomName("Y", "a");
        Assert.AreEqual(new Atom("a"), s.FindFirstAsTerm());
    }

    [TestMethod]
    public void TestSetDouble()
    {
        var s = new QueryStatement(kb, "X = Y.");
        s.SetDouble("Y", 42.5);
        Assert.AreEqual(new DecimalFraction(42.5), s.FindFirstAsTerm());
    }

    [TestMethod]
    public void TestSetLong()
    {
        var s = new QueryStatement(kb, "X = Y.");
        s.SetLong("Y", 42);
        Assert.AreEqual(new IntegerNumber(42), s.FindFirstAsTerm());
    }

    [TestMethod]
    public void TestSetListOfTermsVarargsVersion()
    {
        var s = new QueryStatement(kb, "X = Y.");
        var term1 = Core.Terms.Structure.CreateStructure("test", new Term[] { new Atom("a") });
        var term2 = new Atom("a");
        var term3 = new IntegerNumber(1);
        s.SetListOfTerms("Y", term1, term2, term3);
        Assert.AreEqual(new LinkedTermList(term1, new LinkedTermList(term2, new LinkedTermList(term3, EmptyList.EMPTY_LIST))), s.FindFirstAsTerm());
    }

    [TestMethod]
    public void TestSetListOfTermsListVersion()
    {
        var s = new QueryStatement(kb, "X = Y.");
        var term1 = Core.Terms.Structure.CreateStructure("test", new Term[] { new Atom("a") });
        var term2 = new Atom("a");
        var term3 = new IntegerNumber(1);
        s.SetListOfTerms("Y", term1, term2, term3);
        Assert.AreEqual(new LinkedTermList(term1, new LinkedTermList(term2, new LinkedTermList(term3, EmptyList.EMPTY_LIST))), s.FindFirstAsTerm());
    }

    [TestMethod]
    public void TestSetListOfAtomNamesVarargsVersion()
    {
        var s = new QueryStatement(kb, "X = Y.");
        s.SetListOfAtomNames("Y", "a", "b", "c");
        Assert.AreEqual(new LinkedTermList(new Atom("a"), new LinkedTermList(new Atom("b"), new LinkedTermList(new Atom("c"), EmptyList.EMPTY_LIST))), s.FindFirstAsTerm());
    }

    [TestMethod]
    public void TestSetListOfAtomNamesListVersion()
    {
        var s = new Prolog().CreateStatement("X = Y.");
        s.SetListOfAtomNames("Y", "a", "b", "c");
        Assert.AreEqual(new LinkedTermList(new Atom("a"), new LinkedTermList(new Atom("b"), new LinkedTermList(new Atom("c"), EmptyList.EMPTY_LIST))), s.FindFirstAsTerm());
    }

    [TestMethod]
    public void TestSetListOfDoubles_varargs_version()
    {
        var s = new QueryStatement(kb, "X = Y.");
        s.SetListOfDoubles("Y", 42.5, 180.2, -7.0);
        Assert.AreEqual(new LinkedTermList(new DecimalFraction(42.5), new LinkedTermList(new DecimalFraction(180.2), new LinkedTermList(new DecimalFraction(-7.0), EmptyList.EMPTY_LIST))), s.FindFirstAsTerm());
    }

    [TestMethod]
    public void TestSetListOfDoubles_list_version()
    {
        var s = new Prolog().CreateStatement("X = Y.");
        s.SetListOfDoubles("Y", 42.5, 180.2, -7.0);
        Assert.AreEqual(new LinkedTermList(new DecimalFraction(42.5), new LinkedTermList(new DecimalFraction(180.2), new LinkedTermList(new DecimalFraction(-7.0), EmptyList.EMPTY_LIST))), s.FindFirstAsTerm());
    }

    [TestMethod]
    public void TestSetListOfLongs_varargs_version()
    {
        var s = new QueryStatement(kb, "X = Y.");
        s.SetListOfLongs("Y", 42, 180, -7);
        Assert.AreEqual(new LinkedTermList(new IntegerNumber(42), new LinkedTermList(new IntegerNumber(180), new LinkedTermList(new IntegerNumber(-7), EmptyList.EMPTY_LIST))), s.FindFirstAsTerm());
    }

    [TestMethod]
    public void TestSetListOfLongs_list_version()
    {
        var s = new QueryStatement(kb, "X = Y.");
        s.SetListOfLongs("Y", 42L, 180L, -7L);
        Assert.AreEqual(new LinkedTermList(new IntegerNumber(42), new LinkedTermList(new IntegerNumber(180), new LinkedTermList(new IntegerNumber(-7), EmptyList.EMPTY_LIST))), s.FindFirstAsTerm());
    }

    [TestMethod]
    public void TestNotReusable()
    {
        var s = new QueryStatement(kb, "X = Y.");
        s.ExecuteQuery();
        try
        {
            s.ExecuteQuery();
            Assert.Fail();
        }
        catch (PrologException e)
        {
            Assert.AreEqual("This QueryStatement has already been evaluated. If you want to reuse the same query then consider using a QueryPlan. See: Prolog.CreatePlan(string)",
                        e.Message);
        }
    }

    [TestMethod]
    public void TestUnknownVariable()
    {
        var s = new Prolog().CreateStatement("X = Y.");
        try
        {
            s.SetTerm("Z", new Atom("a"));
            Assert.Fail();
        }
        catch (PrologException e)
        {
            Assert.AreEqual("Do not know about variable named: Z in query: =(X, Y)", e.Message);
        }
    }

    [TestMethod]
    public void TestAlreadySetVariable()
    {
        var s = new QueryStatement(kb, "X = Y.");
        s.SetTerm("X", new Atom("a"));
        try
        {
            s.SetTerm("X", new Atom("b"));
            Assert.Fail();
        }
        catch (PrologException e)
        {
            Assert.AreEqual("Cannot set: X to: b as has already been set to: a", e.Message);
        }
    }

    [TestMethod]
    public void TestInvalidQuery()
    {
        try
        {
            var s = new QueryStatement(kb, "X");
            Assert.Fail();
        }
        catch (ParserException e)
        {
            Assert.AreEqual("Unexpected end of stream Line: X", e.Message);
        }
    }

    [TestMethod]
    public void TestMoreThanOneSentenceInQuery()
    {
        try
        {
            var s = new QueryStatement(kb, "X is 1. Y is 2.");
            Assert.Fail();

        }
        catch (PrologException e)
        {
            Assert.AreEqual("PrologException caught parsing: X is 1. Y is 2.", e.Message);
            Assert.AreEqual("More input found after . in X is 1. Y is 2.", e.InnerException?.Message??"");
        }
    }

    [TestMethod]
    public void TestExecuteOnce()
    {
        var mockPredicateFactory = new MockPredicateFactory();
        When(mockPredicateFactory.GetPredicate(System.Array.Empty<Term>())).ThenReturn(PredicateUtils.TRUE);
        kb.Predicates.AddPredicateFactory(new PredicateKey("mock", 0), mockPredicateFactory);

        var s = new QueryStatement(kb, "repeat, mock.");
        s.ExecuteOnce();

        Verify(mockPredicateFactory)?.GetPredicate(System.Array.Empty<Term>());
        VerifyNoMoreInteractions(mockPredicateFactory);
    }

    [TestMethod]
    public void TestExecuteOnceNoSolution()
    {
        var s = new QueryStatement(kb, "true, true, fail.");
        try
        {
            s.ExecuteOnce();
            Assert.Fail();
        }
        catch (PrologException prologException)
        {
            Assert.AreEqual("Failed to find a solution for: ,(,(true, true), fail)", prologException.Message);
        }
    }
}
