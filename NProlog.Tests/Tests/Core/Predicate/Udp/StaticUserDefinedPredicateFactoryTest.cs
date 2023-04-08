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
using Org.NProlog.Core.Kb;
using Org.NProlog.Core.Parser;
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Predicate.Udp;

/**
 * Tests {@link StaticUserDefinedPredicateFactory}.
 * <p>
 * See also system tests in src/test/prolog/udp/predicate-meta-data
 */
[TestClass]
public class StaticUserDefinedPredicateFactoryTest : TestUtils
{
    private static readonly string[] RECURSIVE_PREDICATE_SYNTAX = { "concatenate([],L,L).", "concatenate([X|L1],L2,[X|L3]) :- concatenate(L1,L2,L3)." };
    private static readonly string[] NON_RECURSIVE_PREDICATE_SYNTAX = { "p(X,Y,Z) :- repeat(3), X<Y.", "p(X,Y,Z) :- X is Y+Z.", "p(X,Y,Z) :- X=a." };

    [TestMethod]
    public void TestSingleFact()
    {
        AssertSingleRuleAlwaysTruePredicate("p.");
        AssertSingleRuleAlwaysTruePredicate("p(_).");
        AssertSingleRuleAlwaysTruePredicate("p(X).");
        AssertSingleRuleAlwaysTruePredicate("p(A,B,C).");
        AssertSingleRuleAlwaysTruePredicate("p(A,_,C).");
    }

    private static void AssertSingleRuleAlwaysTruePredicate(string term)
    {
        var pf = GetActualPredicateFactory(ToTerms(term));
        Assert.AreSame(typeof(SingleNonRetryableRulePredicateFactory), pf.GetType());
        var p = pf.GetPredicate(TermUtils.EMPTY_ARRAY);
        Assert.IsTrue(p.Evaluate());
        Assert.IsFalse(p.CouldReevaluationSucceed);
        Assert.IsFalse(pf.IsRetryable);
    }

    [TestMethod]
    public void TestSingleNonRetryableRule()
    {
        AssertSingleNonRetryableRulePredicateFactory("p(A,_,C) :- fail.");
        AssertSingleNonRetryableRulePredicateFactory("p(A,_,C) :- true.");
        AssertSingleNonRetryableRulePredicateFactory("p(A,_,C) :- nl.");
        AssertSingleNonRetryableRulePredicateFactory("p(A,_,C) :- write(A), nl.");
    }

    [TestMethod]
    public void TestSingleRuleAlwaysCutsOnBacktrack()
    {
        
        AssertSingleNonRetryableRulePredicateFactory("p(A,_,C) :- !.");
        //AssertSingleNonRetryableRulePredicateFactory("p(A,_,C) :- repeat, !.");
        //AssertSingleNonRetryableRulePredicateFactory("p(A,_,C) :- nl, !.");
        //AssertSingleNonRetryableRulePredicateFactory("p(A,_,C) :- !, nl.");
        //AssertSingleNonRetryableRulePredicateFactory("p(A,_,C) :- nl, !, nl.");
        //AssertSingleNonRetryableRulePredicateFactory("p(A,_,C) :- repeat, !, nl.");
    }

    private static void AssertSingleNonRetryableRulePredicateFactory(string term)
    {
        var pf = GetActualPredicateFactory(ToTerms(term));
        Assert.AreEqual(typeof(SingleNonRetryableRulePredicateFactory), pf.GetType());
        var p = pf.GetPredicate(EMPTY_ARRAY);
        Assert.IsFalse(p.CouldReevaluationSucceed);
        Assert.IsFalse(pf.IsRetryable);
    }

    [TestMethod]
    public void TestRepeatSetAmount()
    {
        AssertRepeatSetAmount("p(_).");
        AssertRepeatSetAmount("p(X).");
        AssertRepeatSetAmount("p(A,B,C).");
        AssertRepeatSetAmount("p(A,_,C).");
    }

    private static void AssertRepeatSetAmount(string term)
    {
        var clauses = ToTerms(term, term, term);
        int expectedSuccessfulEvaluations = clauses.Length;
        var pf = GetActualPredicateFactory(clauses);
        // Note that use to return specialised "MultipleRulesAlwaysTruePredicate" object for predicates of this style
        // but now use generic "InterpretedUserDefinedPredicatePredicateFactory" as seemed overly complex to support
        // this special case when it is so rarely used.
        var p = pf.GetPredicate(CreateArgs(clauses[0]));
        Assert.AreSame(typeof(InterpretedUserDefinedPredicate), p.GetType());
        Assert.IsTrue(p.CouldReevaluationSucceed);
        for (int i = 0; i < expectedSuccessfulEvaluations; i++)
        {
            Assert.IsTrue(p.CouldReevaluationSucceed);
            Assert.IsTrue(p.Evaluate());
        }
        Assert.IsFalse(p.CouldReevaluationSucceed);
        Assert.IsFalse(p.Evaluate());
        Assert.IsTrue(pf.IsRetryable);
    }

    [TestMethod]
    public void TestSingleFactWithSingleImmutableArgumentPredicate()
    {
        var pf = GetActualPredicateFactory("p(a).");
        Assert.AreSame(typeof(SingleNonRetryableRulePredicateFactory), pf.GetType());
        Assert.IsFalse(pf.IsRetryable);
    }

    [TestMethod]
    public void TestMultipleFactsWithSingleImmutableArgumentPredicate()
    {
        var pf = GetActualPredicateFactory("p(a).", "p(b).", "p(c).");
        AssertLinkedHashMapPredicateFactory(pf);
        Assert.IsTrue(pf.IsRetryable);
    }

    [TestMethod]
    public void TestMultipleFactsWithSingleImmutableArgumentPredicateDuplicates()
    {
        var pf = GetActualPredicateFactory("p(a).", "p(a).", "p(a).");
        AssertSingleIndexPredicateFactory(pf);
        Assert.IsTrue(pf.IsRetryable);
    }

    [TestMethod]
    public void TestMultipleFactsWithSingleImmutableArgumentPredicateDifferentTypes()
    {
        string[] clauses = { "p(a).", "p(1).", "p(1.0).", "p(x(a)).", "p([]).", "p([a,b])." };
        var pf = GetActualPredicateFactory(clauses);
        AssertLinkedHashMapPredicateFactory(pf);
        Assert.IsTrue(pf.IsRetryable);
    }

    [TestMethod]
    public void TestSingleFactWithMultipleImmutableArgumentsPredicate()
    {
        var pf = GetActualPredicateFactory("p(a,b,c).");
        Assert.AreSame(typeof(SingleNonRetryableRulePredicateFactory), pf.GetType());
        Assert.IsFalse(pf.IsRetryable);
    }

    [TestMethod]
    public void TestMultipleFactsWithMultipleImmutableArgumentsPredicate()
    {
        var pf = GetActualPredicateFactory("p(a,b,c).", "p(1,2,3).", "p(x,y,z).");
        AssertIndexablePredicateFactory(pf);
        Assert.IsTrue(pf.IsRetryable);
    }

    [TestMethod]
    public void TestMultipleFactsWithNoArgumentsPredicate()
    {
        var pf = GetActualPredicateFactory("p.", "p.", "p.");
        Assert.IsTrue(pf.IsRetryable);
        var p = pf.GetPredicate(TermUtils.EMPTY_ARRAY);
        Assert.AreSame(typeof(InterpretedUserDefinedPredicate), p.GetType());
        Assert.IsTrue(p.Evaluate());
        Assert.IsTrue(p.Evaluate());
        Assert.IsTrue(p.Evaluate());
        Assert.IsFalse(p.Evaluate());
    }

    [TestMethod]
    public void TestIndexablePredicate()
    {
        // has mutable arg so not treated as facts but some args are indexable
        var clauses = ToTerms("p(a,b,c).", "p(1,2,3).", "p(x,y,Z).");
        var pf = GetActualPredicateFactory(clauses);
        Assert.AreEqual("IndexablePredicateFactory", pf.GetType().Name);
        Assert.IsTrue(pf.IsRetryable);
    }

    [TestMethod]
    public void TestNotIndexablePredicate()
    {
        // not args are indexable as none are always immutable
        var pf = GetActualPredicateFactory("p(a,b,c).", "p(1,2,3).", "p(X,Y,Z).");
        Assert.AreEqual("NotIndexablePredicateFactory", pf.GetType().Name);
        Assert.IsTrue(pf.IsRetryable);
    }

    [TestMethod]
    public void TestNeverSucceedsPredicateFactory()
    {
        var pf = GetActualPredicateFactory("p(a,b,c).", "p(1,2,3).", "p(x,y,Z).");
        Assert.AreEqual("IndexablePredicateFactory", pf.GetType().Name);
        Assert.IsTrue(pf.IsRetryable);
        var term = Structure("p", Atom("q"), Atom("w"), Atom("e"));
        var preprocessedPredicateFactory = ((PreprocessablePredicateFactory)pf).Preprocess(term);
        Assert.AreSame(typeof(NeverSucceedsPredicateFactory), preprocessedPredicateFactory.GetType());
        Assert.AreSame(PredicateUtils.FALSE, preprocessedPredicateFactory.GetPredicate(term.Args));
        Assert.IsFalse(preprocessedPredicateFactory.IsRetryable);
    }

    [TestMethod]
    public void TestInterpretedTailRecursivePredicateFactory()
    {
        var pf = GetActualPredicateFactory(ToTerms(RECURSIVE_PREDICATE_SYNTAX));
        Assert.AreSame(typeof(InterpretedTailRecursivePredicateFactory), pf.GetType());
        Assert.IsTrue(pf.IsRetryable);
    }

    [TestMethod]
    public void TestInterpretedUserDefinedPredicate()
    {
        var pf = GetActualPredicateFactory(ToTerms(NON_RECURSIVE_PREDICATE_SYNTAX));
        Assert.AreSame(typeof(InterpretedUserDefinedPredicate), pf.GetPredicate(CreateArgs(3)).GetType());
        Assert.IsTrue(pf.IsRetryable);
    }

    [TestMethod]
    public void TestRetryableRule()
    {
        var pf = GetActualPredicateFactory("x(X) :- var(X), !, repeat.");
        AssertSingleRetryableRulePredicateFactory(pf);
    }

    [TestMethod]
    public void TestConjunctionContainingVariables()
    {
        var pf = GetActualPredicateFactory("and(X,Y) :- X, Y.");
        AssertSingleRetryableRulePredicateFactory(pf);
        Assert.IsTrue(pf.IsRetryable);
    }

    [TestMethod]
    public void TestVariableAntecedent()
    {
        var pf = GetActualPredicateFactory("true(X) :- X.");
        AssertSingleRetryableRulePredicateFactory(pf);
    }

    [TestMethod]
    public void TestAddFirst()
    {
        var kb = CreateKnowledgeBase(PROLOG_DEFAULT_PROPERTIES);
        var t = ParseSentence("test(X).");
        var clauseModel = ClauseModel.CreateClauseModel(t);
        var f = new StaticUserDefinedPredicateFactory(kb, PredicateKey.CreateForTerm(t));
        try
        {
            f.AddFirst(clauseModel);
        }
        catch (PrologException e)
        {
            Assert.AreEqual(
                "Cannot add clause to already defined user defined predicate as it is not dynamic: test/1 clause: test(X)",
                e.Message);
        }
    }

    [TestMethod]
    public void TestAddLast()
    {
        var kb = CreateKnowledgeBase(PROLOG_DEFAULT_PROPERTIES);
        var t = ParseSentence("test(a).");
        var f = new StaticUserDefinedPredicateFactory(kb, PredicateKey.CreateForTerm(t));

        // ok to Add clause as predicate not yet compiled
        var firstClause = ClauseModel.CreateClauseModel(t);
        f.AddLast(firstClause);

        f.Compile();

        // no longer ok to Add clause as predicate has been compiled
        var secondClause = ClauseModel.CreateClauseModel(TestUtils.ParseSentence("test(z)."));
        try
        {
            f.AddFirst(secondClause);
        }
        catch (PrologException e)
        {
            Assert.AreEqual("Cannot add clause to already defined user defined predicate as it is not dynamic: test/1 clause: test(z)", e.Message);
        }
    }

    private static void AssertSingleRetryableRulePredicateFactory(PredicateFactory p) => Assert.AreSame(typeof(SingleRetryableRulePredicateFactory), p.GetType());

    private static void AssertIndexablePredicateFactory(PredicateFactory p) => Assert.AreEqual("IndexablePredicateFactory", p.GetType().Name);

    private static void AssertSingleIndexPredicateFactory(PredicateFactory p) => Assert.AreEqual("SingleIndexPredicateFactory", p.GetType().Name);

    private static void AssertLinkedHashMapPredicateFactory(PredicateFactory p)
    => Assert.AreEqual("LinkedHashMapPredicateFactory", p.GetType().Name);

    private static Term[] CreateArgs(Term term) => CreateArgs(term.NumberOfArguments);

    private static Term[] CreateArgs(int numArgs) => Arrays.Fill(new Term[numArgs], Atom());

    private static PredicateFactory GetActualPredicateFactory(params string[] clauses) => GetActualPredicateFactory(ToTerms(clauses));

    private static PredicateFactory GetActualPredicateFactory(Term[] clauses)
    {
        var kb = CreateKnowledgeBase(PROLOG_DEFAULT_PROPERTIES);
        StaticUserDefinedPredicateFactory? f = null;
        foreach (var clause in clauses)
        {
            var clauseModel = ClauseModel.CreateClauseModel(clause);
            if (f == null)
            {
                var key = PredicateKey.CreateForTerm(clauseModel.Consequent);
                f = new StaticUserDefinedPredicateFactory(kb, key);
            }
            f.AddLast(clauseModel);
        }
        return f?.GetActualPredicateFactory();
    }

    private static Term[] ToTerms(params string[] clausesSyntax)
    {
        var clauses = new Term[clausesSyntax.Length];
        for (int i = 0; i < clauses.Length; i++)
            clauses[i] = ParseSentence(clausesSyntax[i]);
        return clauses;
    }
}
