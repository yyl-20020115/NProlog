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
using Org.NProlog.Core.Terms;
using static Org.NProlog.Core.Predicate.Udp.ClauseActionFactory;

namespace Org.NProlog.Core.Predicate.Udp;

[TestClass]
public class ClauseActionFactoryTest : TestUtils
{
    private readonly KnowledgeBase kb =new();
    private readonly PredicateFactory mockPredicateFactory;
    private readonly Predicate mockPredicate1;
    private readonly Predicate mockPredicate2;
    //[TestInitialize]
    public ClauseActionFactoryTest()
    {
        mockPredicate1 = new MockPredicate();
        mockPredicate2 = new MockPredicate();

        mockPredicateFactory = new MockPredicateFactory();
        When(mockPredicateFactory.GetPredicate(EMPTY_ARRAY)).ThenReturn(mockPredicate1, mockPredicate2);

        kb = KnowledgeBaseUtils.CreateKnowledgeBase();
        KnowledgeBaseUtils.Bootstrap(kb);
        kb.Predicates.AddPredicateFactory(new PredicateKey("test", 0), mockPredicateFactory);
    }

    [TestCleanup]
    public void After()
    {
        VerifyNoInteractions(mockPredicate1, mockPredicate2);
        VerifyNoMoreInteractions(mockPredicateFactory);
    }

    [TestMethod]
    public void TestAlwaysMatchedFactIsRetryable()
    {
        var a = Create<AlwaysMatchedFact>("p.");
        Assert.IsFalse(a.IsRetryable);
    }

    [TestMethod]
    public void TestAlwaysMatchedFactIsAlwaysCutOnBacktrack()
    {
        var a = Create<AlwaysMatchedFact>("p.");
        Assert.IsFalse(a.IsAlwaysCutOnBacktrack);
    }

    [TestMethod]
    public void TestAlwaysMatchedFactGetPredicateNoArguments()
    {
        var a = Create<AlwaysMatchedFact>("p.");
        Assert.AreSame(PredicateUtils.TRUE, a.GetPredicate(EMPTY_ARRAY));
    }

    [TestMethod]
    public void TestAlwaysMatchedFactGetPredicateDistinctVariableArguments()
    {
        var a = Create<AlwaysMatchedFact>("p(X,Y,Z).");
        Assert.AreSame(PredicateUtils.TRUE, a.GetPredicate(EMPTY_ARRAY));
    }

    [TestMethod]
    public void TestImmutableFactIsRetryable()
    {
        var a = Create<ImmutableFact>("p(a,b,c).");
        Assert.IsFalse(a.IsRetryable);
    }

    [TestMethod]
    public void TestImmutableFactIsAlwaysCutOnBacktrack()
    {
        var a = Create<ImmutableFact>("p(a,b,c).");
        Assert.IsFalse(a.IsAlwaysCutOnBacktrack);
    }

    [TestMethod]
    public void TestImmutableFactGetPredicateQueryArgsMatchClause()
    {
        var a = Create<ImmutableFact>("p(a,b,c).");
        Assert.AreSame(PredicateUtils.TRUE, a.GetPredicate(Array(Atom("a"), Atom("b"), Atom("c"))));
    }

    [TestMethod]
    public void TestImmutableFactGetPredicateQueryArgsDontMatchClause()
    {
        var a = Create<ImmutableFact>("p(a,b,c).");
        Assert.AreSame(PredicateUtils.FALSE, a.GetPredicate(Array(Atom("a"), Atom("b"), Atom("z"))));
    }

    [TestMethod]
    public void TestImmutableFactGetPredicateQueryArgsAllDistinctVariables()
    {
        var a = Create<ImmutableFact>("p(a,b,c).");

        Variable x = new("X");
        Variable y = new("Y");
        Variable z = new("Z");
        Assert.AreSame(PredicateUtils.TRUE, a.GetPredicate(Array(x, y, z)));
        Assert.AreEqual(Atom("a"), x.Term);
        Assert.AreEqual(Atom("b"), y.Term);
        Assert.AreEqual(Atom("c"), z.Term);
    }

    [TestMethod]
    public void TestImmutableFactGetPredicateQueryArgsMixtureOfAtomAndDistinctVariables()
    {
        var a = Create<ImmutableFact>("p(a,b,c).");

        Variable x = new("X");
        Variable y = new("Y");
        Assert.AreSame(PredicateUtils.TRUE, a.GetPredicate(Array(Atom("a"), x, y)));
        Assert.AreEqual(Atom("b"), x.Term);
        Assert.AreEqual(Atom("c"), y.Term);
    }

    [TestMethod]
    public void TestImmutableFactGetPredicateSharedVariablesDontMatch()
    {
        var a = Create<ImmutableFact>("p(a,b,c).");

        Variable x = new("X");
        Variable y = new("Y");
        Assert.AreSame(PredicateUtils.FALSE, a.GetPredicate(Array(x, y, x)));
    }

    [TestMethod]
    public void TestImmutableFactGetPredicateSharedVariablesMatch()
    {
        var a = Create<ImmutableFact>("p(a,b,a).");

        Variable x = new("X");
        Variable y = new("Y");
        Assert.AreSame(PredicateUtils.TRUE, a.GetPredicate(Array(x, y, x)));
        Assert.AreEqual(Atom("a"), x.Term);
        Assert.AreEqual(Atom("b"), y.Term);
    }

    [TestMethod]
    public void TestMutableFactIsRetryable()
    {
        var a = Create<MutableFact>("p(a,X,c).");
        Assert.IsFalse(a.IsRetryable);
    }

    [TestMethod]
    public void TestMutableFactIsAlwaysCutOnBacktrack()
    {
        var a = Create<MutableFact>("p(a,X,c).");
        Assert.IsFalse(a.IsAlwaysCutOnBacktrack);
    }

    [TestMethod]
    public void TestMutableFactGetPredicateQueryArgsUnifyWithClause()
    {
        var a = Create<MutableFact>("p(a,X,c).");
        Assert.AreSame(PredicateUtils.TRUE, a.GetPredicate(Array(Atom("a"), Atom("b"), Atom("c"))));
        Assert.AreSame(PredicateUtils.TRUE, a.GetPredicate(Array(Atom("a"), Atom("d"), Atom("c"))));
    }

    [TestMethod]
    public void TestMutableFactGetPredicateQueryArgsDontUnifyWithClause()
    {
        var a = Create<MutableFact>("p(a,X,c).");
        Assert.AreSame(PredicateUtils.FALSE, a.GetPredicate(Array(Atom("a"), Atom("b"), Atom("d"))));
    }

    [TestMethod]
    public void TestMutableFactGetPredicateQueryArgsSharedVariableDoesntUnifyWithClause()
    {
        var a = Create<MutableFact>("p(a,X,c).");
        Variable x = new("X");
        Assert.AreSame(PredicateUtils.FALSE, a.GetPredicate(Array(x, Atom("b"), x)));
    }

    [TestMethod]
    public void TestMutableFactGetPredicateQueryArgsSharedVariableUnifyWithClause()
    {
        var a = Create<MutableFact>("p(a,X,a).");
        Variable x = new("X");
        Assert.AreSame(PredicateUtils.TRUE, a.GetPredicate(Array(x, Atom("b"), x)));
        Assert.AreEqual(Atom("a"), x.Term);
    }

    [TestMethod]
    public void TestMutableFactGetPredicateQueryArgsDontUnifyWithClauseSharedVariable()
    {
        var a = Create<MutableFact>("p(X,b,X).");
        Assert.AreSame(PredicateUtils.FALSE, a.GetPredicate(Array(Atom("a"), Atom("b"), Atom("c"))));
    }

    [TestMethod]
    public void TestMutableFactGetPredicateQueryArgsUnifyWithClauseSharedVariable()
    {
        var a = Create<MutableFact>("p(X,b,X).");
        Assert.AreSame(PredicateUtils.TRUE, a.GetPredicate(Array(Atom("a"), Atom("b"), Atom("a"))));
    }

    [TestMethod]
    public void TestMutableFactGetPredicateQueryArgsVariableUnifiesWithClauseVariable()
    {
        var a = Create<MutableFact>("p(a,X,c).");
        Variable x = new ("X");
        Assert.AreSame(PredicateUtils.TRUE, a.GetPredicate(Array(Atom("a"), x, Atom("c"))));
        Assert.AreSame(TermType.VARIABLE, x.Term.Type);
        // assert query variable has been unified with clause variable
        Assert.AreNotSame(x, x.Term);
    }

    [TestMethod]
    public void TestMutableFactGetPredicateQueryArgsVariableUnifiesWithClauseAtom()
    {
        var a = Create<MutableFact>("p(a,X,c).");
        Variable x = new ("X");
        Assert.AreSame(PredicateUtils.TRUE, a.GetPredicate(Array(Atom("a"), Atom("b"), x)));
        Assert.AreEqual(Atom("c"), x.Term);
    }

    [TestMethod]
    public void TestVariableAntecedantIsRetryable()
    {
        var a = Create<VariableAntecedantClauseAction>("p(X) :- X.");
        Assert.IsTrue(a.IsRetryable);
    }

    [TestMethod]
    public void TestVariableAntecedantIsAlwaysCutOnBacktrack()
    {
        var a = Create<VariableAntecedantClauseAction>("p(X) :- X.");
        Assert.IsFalse(a.IsAlwaysCutOnBacktrack);
    }

    [TestMethod]
    public void TestVariableAntecedantGetPredicateUnassignedVariable()
    {
        var a = Create<VariableAntecedantClauseAction>("p(X) :- X.");
        try
        {
            a.GetPredicate(Array(new Variable("Z")));
            Assert.Fail();
        }
        catch (PrologException e)
        {
            Assert.AreEqual("Expected an atom or a predicate but got a VARIABLE with value: X", e.Message);
        }
    }

    [TestMethod]
    public void TestVariableAntecedantGetPredicateUnknownPredicate()
    {
        var a = Create<VariableAntecedantClauseAction>("p(X) :- X.");
        Assert.AreSame(PredicateUtils.FALSE, a.GetPredicate(Array(Atom("an_unknown_predicate"))));
    }

    [TestMethod]
    public void TestVariableAntecedantGetPredicateQueryArgsDontUnifyWithClause()
    {
        var a = Create<VariableAntecedantClauseAction>("p(X,a) :- X.");
        Assert.AreSame(PredicateUtils.FALSE, a.GetPredicate(Array(Atom("test"), (Atom("b")))));
    }

    [TestMethod]
    public void TestVariableAntecedantGetPredicateKnownPredicate()
    {
        var queryArgs = Array(Atom("test"));

        var a = Create<VariableAntecedantClauseAction>("p(X) :- X.");
        Assert.AreNotSame(mockPredicate1, a.GetPredicate(queryArgs));
        Assert.AreNotSame(mockPredicate2, a.GetPredicate(queryArgs));

        Verify(mockPredicateFactory, Times(2))?.GetPredicate(EMPTY_ARRAY);
    }

    [TestMethod]
    public void TestVariableAntecedantGetPredicateWithDifferentQueryArgs()
    {
        var pf1 = new MockPredicateFactory();
        var p1 = new MockPredicate();
        When(pf1.GetPredicate(EMPTY_ARRAY)).ThenReturn(p1);
        kb.Predicates.AddPredicateFactory(new PredicateKey("test1", 0), pf1);

        var pf2 = new MockPredicateFactory();
        var p2 = new MockPredicate();
        When(pf2.GetPredicate(EMPTY_ARRAY)).ThenReturn(p2);
        kb.Predicates.AddPredicateFactory(new PredicateKey("test2", 0), pf2);

        var a = Create<VariableAntecedantClauseAction>("p(X) :- X.");
        Assert.AreNotSame(p1, a.GetPredicate(Array(Atom("test1"))));
        Assert.AreNotSame(p2, a.GetPredicate(Array(Atom("test2"))));

        Verify(pf1, Times(1))?.GetPredicate(EMPTY_ARRAY);
        Verify(pf2, Times(1))?.GetPredicate(EMPTY_ARRAY);
        VerifyNoMoreInteractions(pf1, pf2, p1, p2);
    }

    [TestMethod]
    public void TestZeroArgConsequentRuleIsRetryableUnknownPredicate()
    {
        var a = Create<ZeroArgConsequentRule>("p :- an_unknown_predicate.");
        Assert.IsTrue(a.IsRetryable);
    }

    [TestMethod]
    public void TestZeroArgConsequentRuleIsAlwaysCutOnBacktrackUnknownPredicate()
    {
        var a = Create<ZeroArgConsequentRule>("p :- an_unknown_predicate.");
        Assert.IsFalse(a.IsAlwaysCutOnBacktrack);
    }

    [TestMethod]
    public void TestZeroArgConsequentRuleIsRetryableTrue()
    {
        When(mockPredicateFactory?.IsRetryable).ThenReturn(true);

        var a = Create<ZeroArgConsequentRule>("p :- test.");
        Assert.IsFalse(a.IsRetryable);

        Confirm(Verify(mockPredicateFactory)?.IsRetryable);
    }

    [TestMethod]
    public void TestZeroArgConsequentRuleIsRetryableFalse()
    {
        When(mockPredicateFactory?.IsRetryable).ThenReturn(false);

        var a = Create<ZeroArgConsequentRule>("p :- test.");
        Assert.IsFalse(a.IsRetryable);

        Confirm(Verify(mockPredicateFactory)?.IsRetryable);
    }

    [TestMethod]
    public void TestZeroArgConsequentRuleIsAlwaysCutOnBacktrackTrue()
    {
        When(mockPredicateFactory?.IsAlwaysCutOnBacktrack).ThenReturn(true);

        var a = Create<ZeroArgConsequentRule>("p :- test.");
        Assert.IsFalse(a.IsAlwaysCutOnBacktrack);
        _ = Verify(mockPredicateFactory)?.IsAlwaysCutOnBacktrack;
    }

    [TestMethod]
    public void TestZeroArgConsequentRuleIsAlwaysCutOnBacktrackFalse()
    {
        When(mockPredicateFactory?.IsAlwaysCutOnBacktrack).ThenReturn(false);

        var a = Create<ZeroArgConsequentRule>("p :- test.");
        Assert.IsFalse(a.IsAlwaysCutOnBacktrack);
        _ = Verify(mockPredicateFactory)?.IsAlwaysCutOnBacktrack;
    }

    [TestMethod]
    public void TestZeroArgConsequentRuleGetPredicate()
    {
        var a = Create<ZeroArgConsequentRule>("p :- test.");
        Assert.AreNotSame(mockPredicate1, a.GetPredicate(EMPTY_ARRAY));
        Assert.AreNotSame(mockPredicate2, a.GetPredicate(EMPTY_ARRAY));

        Verify(mockPredicateFactory, Times(2))?.GetPredicate(EMPTY_ARRAY);
    }

    // TODO p :- test(X). p(X) :- test(X). p(a) :- test(X).
    // TODO testImmutableConsequentRule_getPredicate_antecedent_mutable

    [TestMethod]
    public void TestZeroArgConsequentRuleGetPredicateAntecedentMutable()
    {
        var pf = new MockPredicateFactory();
        kb?.Predicates.AddPredicateFactory(new PredicateKey("test", 5), pf);

        //ArgumentCaptor<Term[]> captor = ArgumentCaptor.forClass(Term[]);
        var p1 = new MockPredicate();
        var p2 = new MockPredicate();
        //when(pf.GetPredicate(captor.capture())).thenReturn(p1, p2);

        var a = Create<ZeroArgConsequentRule>("p :- test(X,y,X,p(X),Z).");
        Assert.AreNotSame(p1, a.GetPredicate(EMPTY_ARRAY));
        Assert.AreNotSame(p2, a.GetPredicate(EMPTY_ARRAY));

        //var allValues = new List<Term[]>();//
        //                                            //captor.getAllValues();
        //Assert.AreEqual(0, allValues.Count);

        //var values1 = allValues[(0)];
        //Assert.AreEqual(Atom("y"), values1[1]);
        //Assert.AreSame(values1[0], values1[2]);
        //Assert.AreSame(values1[0], values1[3].GetArgument(0));
        //Assert.AreNotSame(values1[0], values1[4]);

        //var values2 = allValues[(1)];
        //Assert.AreNotSame(values1[0], values2[0]);
        //Assert.AreSame(values1[1], values2[1]);
        //Assert.AreNotSame(values1[2], values2[2]);
        //Assert.AreNotSame(values1[3], values2[3]);
        //Assert.AreNotSame(values1[4], values2[4]);

        Verify(pf, Times(2))?.GetPredicate(Any<Term>());
        VerifyNoMoreInteractions(pf, p1, p2);
    }

    [TestMethod]
    public void TestImmutableConsequentRuleIsRetryableUnknownPredicate()
    {
        var a = Create<ImmutableConsequentRule>("p(a,b,c) :- an_unknown_predicate.");
        Assert.IsTrue(a.IsRetryable);
    }

    [TestMethod]
    public void TestImmutableConsequentRuleIsAlwaysCutOnBacktrackUnknownPredicate()
    {
        var a = Create<ImmutableConsequentRule>("p(a,b,c) :- an_unknown_predicate.");
        Assert.IsFalse(a.IsAlwaysCutOnBacktrack);
    }

    [TestMethod]
    public void TestImmutableConsequentRuleIsRetryableTrue()
    {
        When(mockPredicateFactory?.IsRetryable).ThenReturn(true);

        var a = Create<ImmutableConsequentRule>("p(a,b,c) :- test.");
        Assert.IsFalse(a.IsRetryable);
        _ = Verify(mockPredicateFactory)?.IsRetryable;
    }

    [TestMethod]
    public void TestImmutableConsequentRuleIsRetryableFalse()
    {
        When(mockPredicateFactory?.IsRetryable).ThenReturn(false);

        var a = Create<ImmutableConsequentRule>("p(a,b,c) :- test.");
        Assert.IsFalse(a.IsRetryable);
        _ = Verify(mockPredicateFactory)?.IsRetryable;
    }

    [TestMethod]
    public void TestImmutableConsequentRuleIsAlwaysCutOnBacktrackTrue()
    {
        When(mockPredicateFactory?.IsAlwaysCutOnBacktrack).ThenReturn(true);

        var a = Create<ImmutableConsequentRule>("p(a,b,c) :- test.");
        Assert.IsFalse(a.IsAlwaysCutOnBacktrack);
        _ = Verify(mockPredicateFactory)?.IsAlwaysCutOnBacktrack;
    }

    [TestMethod]
    public void TestImmutableConsequentRuleIsAlwaysCutOnBacktrackFalse()
    {
        When(mockPredicateFactory?.IsAlwaysCutOnBacktrack).ThenReturn(false);

        var a = Create<ImmutableConsequentRule>("p(a,b,c) :- test.");
        Assert.IsFalse(a?.IsAlwaysCutOnBacktrack);
        _ = Verify(mockPredicateFactory)?.IsAlwaysCutOnBacktrack;
    }

    [TestMethod]
    public void TestImmutableConsequentRuleGetPredicateQueryArgsMatchClause()
    {
        var a = Create<ImmutableConsequentRule>("p(a,b,c) :- test.");
        var queryArgs = Array(Atom("a"), Atom("b"), Atom("c"));
        Assert.AreNotEqual(mockPredicate1, a.GetPredicate(queryArgs));
        Assert.AreNotEqual(mockPredicate2, a.GetPredicate(queryArgs));

        Verify(mockPredicateFactory, Times(2))?.GetPredicate(EMPTY_ARRAY);
    }

    [TestMethod]
    public void TestImmutableConsequentRuleGetPredicateQueryArgsDontMatchClause()
    {
        var a = Create<ImmutableConsequentRule>("p(a,b,c) :- test.");
        Assert.AreSame(PredicateUtils.FALSE, a.GetPredicate(Array(Atom("a"), Atom("b"), Atom("z"))));
    }

    [TestMethod]
    public void TestImmutableConsequentRuleGetPredicateQueryArgsAllDistinctVariables()
    {
        var a = Create<ImmutableConsequentRule>("p(a,b,c) :- test.");

        var x = new Variable("X");
        var y = new Variable("Y");
        var z = new Variable("Z");
        Assert.AreNotEqual(mockPredicate1, a.GetPredicate(Array(x, y, z)));
        Assert.AreEqual(Atom("a"), x.Term);
        Assert.AreEqual(Atom("b"), y.Term);
        Assert.AreEqual(Atom("c"), z.Term);

        Verify(mockPredicateFactory)?.GetPredicate(EMPTY_ARRAY);
    }

    [TestMethod]
    public void TestImmutableConsequentRuleGetPredicateQueryArgsMixtureOfAtomAndDistinctVariables()
    {
        var a = Create<ImmutableConsequentRule>("p(a,b,c) :- test.");

        var x = new Variable("X");
        var y = new Variable("Y");
        Assert.AreNotSame(mockPredicate1, a.GetPredicate(Array(Atom("a"), x, y)));
        Assert.AreEqual(Atom("b"), x.Term);
        Assert.AreEqual(Atom("c"), y.Term);

        Verify(mockPredicateFactory)?.GetPredicate(EMPTY_ARRAY);
    }

    [TestMethod]
    public void TestImmutableConsequentRuleGetPredicateSharedVariablesDontMatch()
    {
        var a = Create<ImmutableConsequentRule>("p(a,b,c) :- test.");

        var x = new Variable("X");
        var y = new Variable("Y");
        Assert.AreSame(PredicateUtils.FALSE, a.GetPredicate(Array(x, y, x)));
    }

    [TestMethod]
    public void TestImmutableConsequentRuleGetPredicateSharedVariablesMatch()
    {
        var a = Create<ImmutableConsequentRule>("p(a,b,a) :- test.");

        var x = new Variable("X");
        var y = new Variable("Y");
        Assert.AreNotSame(mockPredicate1, a.GetPredicate(Array(x, y, x)));
        Assert.AreEqual(Atom("a"), x.Term);
        Assert.AreEqual(Atom("b"), y.Term);

        Verify(mockPredicateFactory)?.GetPredicate(EMPTY_ARRAY);
    }

    [TestMethod]
    public void TestMutableRuleIsRetryableUnknownPredicate()
    {
        var a = Create<MutableRule>("p(a,X,c) :- an_unknown_predicate.");
        Assert.IsTrue(a.IsRetryable);
    }

    [TestMethod]
    public void TestMutableRuleIsAlwaysCutOnBacktrackUnknownPredicate()
    {
        var a = Create<MutableRule>("p(a,X,c) :- an_unknown_predicate.");
        Assert.IsFalse(a.IsAlwaysCutOnBacktrack);
    }

    [TestMethod]
    public void TestMutableRuleIsRetryableTrue()
    {
        When(mockPredicateFactory?.IsRetryable).ThenReturn(true);

        var a = Create<MutableRule>("p(a,X,c) :- test.");
        Assert.IsFalse(a.IsRetryable);
        _ = Verify(mockPredicateFactory)?.IsRetryable;
    }

    [TestMethod]
    public void TestMutableRuleIsRetryableFalse()
    {
        When(mockPredicateFactory?.IsRetryable).ThenReturn(false);

        var a = Create<MutableRule>("p(a,X,c) :- test.");
        Assert.IsFalse(a.IsRetryable);
        _ = Verify(mockPredicateFactory)?.IsRetryable;
    }

    [TestMethod]
    public void TestMutableRuleIsAlwaysCutOnBacktrackTrue()
    {
        When(mockPredicateFactory?.IsAlwaysCutOnBacktrack).ThenReturn(true);

        var a = Create<MutableRule>("p(a,X,c) :- test.");
        Assert.IsFalse(a.IsAlwaysCutOnBacktrack);

        Confirm(Verify(mockPredicateFactory)?.IsAlwaysCutOnBacktrack);
    }

    [TestMethod]
    public void TestMutableRuleIsAlwaysCutOnBacktrackFalse()
    {
        When(mockPredicateFactory?.IsAlwaysCutOnBacktrack).ThenReturn(false);

        var a = Create<MutableRule>("p(a,X,c) :- test.");
        Assert.IsFalse(a.IsAlwaysCutOnBacktrack);
        _ = Verify(mockPredicateFactory)?.IsAlwaysCutOnBacktrack;
    }

    [TestMethod]
    public void TestMutableRuleGetPredicateDistinctVariableArguments()
    {
        var a = Create<MutableRule>("p(X,Y,Z) :- test.");

        Variable v1 = new("A");
        Variable v2 = new("B");
        Variable v3 = new("C");

        Assert.AreNotSame(mockPredicate1, a.GetPredicate(Array(v1, v2, v3)));

        // assert query variables have been unified with clause variables
        Assert.AreSame(TermType.VARIABLE, v1.Term.Type);
        Assert.AreNotSame(v1, v1.Term);
        Assert.AreEqual("X", ((Variable)v1.Term).Id);

        Assert.AreSame(TermType.VARIABLE, v2.Term.Type);
        Assert.AreNotSame(v2, v2.Term);
        Assert.AreEqual("Y", ((Variable)v2.Term).Id);

        Assert.AreSame(TermType.VARIABLE, v3.Term.Type);
        Assert.AreNotSame(v3, v3.Term);
        Assert.AreEqual("Z", ((Variable)v3.Term).Id);

        Verify(mockPredicateFactory)?.GetPredicate(EMPTY_ARRAY);
    }

    [TestMethod]
    public void TestMutableRuleGetPredicateQueryArgsUnifyWithClause()
    {
        var a = Create<MutableRule>("p(a,X,c) :- test.");
        Assert.AreNotSame(mockPredicate1, a.GetPredicate(Array(Atom("a"), Atom("b"), Atom("c"))));
        Assert.AreNotSame(mockPredicate2, a.GetPredicate(Array(Atom("a"), Atom("d"), Atom("c"))));
        Verify(mockPredicateFactory, Times(2))?.GetPredicate(EMPTY_ARRAY);
    }

    [TestMethod]
    public void TestMutableRuleGetPredicateQueryArgsDontUnifyWithClause()
    {
        var a = Create<MutableRule>("p(a,X,c) :- test.");
        Assert.AreSame(PredicateUtils.FALSE, a.GetPredicate(Array(Atom("a"), Atom("b"), Atom("d"))));
    }

    [TestMethod]
    public void TestMutableRuleGetPredicateQueryArgsSharedVariableDoesntUnifyWithClause()
    {
        var a = Create<MutableRule>("p(a,X,c) :- test.");
        Variable x = new("X");
        Assert.AreSame(PredicateUtils.FALSE, a.GetPredicate(Array(x, Atom("b"), x)));
    }

    [TestMethod]
    public void TestMutableRuleGetPredicateQueryArgsSharedVariableUnifyWithClause()
    {
        var a = Create<MutableRule>("p(a,X,a) :- test.");
        var x = new Variable("X");
        Assert.AreNotSame(mockPredicate1, a.GetPredicate(Array(x, Atom("b"), x)));
        Assert.AreEqual(Atom("a"), x.Term);
        Verify(mockPredicateFactory)?.GetPredicate(EMPTY_ARRAY);
    }

    [TestMethod]
    public void TestMutableRuleGetPredicateQueryArgsDontUnifyWithClauseSharedVariable()
    {
        var a = Create<MutableRule>("p(X,b,X) :- test.");
        Assert.AreSame(PredicateUtils.FALSE, a.GetPredicate(Array(Atom("a"), Atom("b"), Atom("c"))));
    }

    [TestMethod]
    public void TestMutableRuleGetPredicateQueryArgsUnifyWithClauseSharedVariable()
    {
        var a = Create<MutableRule>("p(X,b,X) :- test.");
        Assert.AreNotSame(mockPredicate1, a.GetPredicate(Array(Atom("a"), Atom("b"), Atom("a"))));
        Verify(mockPredicateFactory)?.GetPredicate(EMPTY_ARRAY);
    }

    [TestMethod]
    public void TestMutableRuleGetPredicateQueryArgsVariableUnifiesWithClauseVariable()
    {
        var a = Create<MutableRule>("p(a,X,c) :- test.");
        Variable variable = new ("A");
        Assert.AreNotSame(mockPredicate1, a.GetPredicate(Array(Atom("a"), variable, Atom("c"))));
        // assert query variable has been unified with clause variable
        Assert.AreSame(TermType.VARIABLE, variable.Term.Type);
        Assert.AreNotSame(variable, variable.Term);
        Assert.AreEqual("X", ((Variable)variable.Term).Id);
        Verify(mockPredicateFactory)?.GetPredicate(EMPTY_ARRAY);
    }

    [TestMethod]
    public void TestMutableRuleGetPredicateQueryArgsVariableUnifiesWithClauseAtom()
    {
        var a = Create<MutableRule>("p(a,X,c) :- test.");
        Variable x = new ("X");
        Assert.AreNotSame(mockPredicate1, a.GetPredicate(Array(Atom("a"), Atom("b"), x)));
        Assert.AreEqual(Atom("c"), x.Term);
        Verify(mockPredicateFactory)?.GetPredicate(EMPTY_ARRAY);
    }

    [TestMethod]
    public void TestIsMatch()
    {
        var a = Create<MutableFact>("p(X,b,Y).");
        Variable x = new ("X");

        Assert.IsTrue(IsMatch(a, new Term[] { x, x, x }));
        Assert.AreSame(x, x.Term);

        Assert.IsFalse(IsMatch(a, new Term[] { x, new Atom("c"), x }));
        Assert.AreSame(x, x.Term);

        Assert.IsTrue(IsMatch(a, new Term[] { new Atom("a"), new Atom("b"), new Atom("c") }));

        Assert.IsTrue(IsMatch(a, new Term[] { new Atom("c"), new Atom("b"), new Atom("a") }));
    }


    private T Create<T>(string syntax)
    {
        var type = typeof(T);
        var model = CreateClauseModel(syntax);
        var result = CreateClauseAction(kb, model);
        AssertType(type, result);
        Assert.AreSame(model, result.Model);
        return (T)result;
    }
}
