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
using Org.NProlog.Core.Kb;
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Predicate.Udp;

[TestClass]
public class MultipleRulesWithSingleImmutableArgumentPredicateTest : TestUtils
{
    private static readonly string FUNCTOR = "test";

    private KnowledgeBase kb;
    private PredicateFactory testObject;

    [TestInitialize]
    public void Init()
    {
        string[] atomNames = { "a", "b", "c", "c", "c", "c", "c", "d", "e", "b", "f" };

        kb = TestUtils.CreateKnowledgeBase(TestUtils.PROLOG_DEFAULT_PROPERTIES);
        PredicateKey key = new PredicateKey(FUNCTOR, 1);
        StaticUserDefinedPredicateFactory pf = new StaticUserDefinedPredicateFactory(kb, key);
        foreach (string atomName in atomNames)
        {
            ClauseModel clause = ClauseModel.CreateClauseModel(Structure(FUNCTOR, Atom(atomName)));
            pf.AddLast(clause);
        }
        kb.Predicates.AddUserDefinedPredicate(pf);
        Assert.AreSame(pf, kb.Predicates.GetPredicateFactory(key));
        testObject = pf.GetActualPredicateFactory();
    }

    [TestMethod]
    public void TestSuceedsNever()
    {
        Assert.AreSame(SucceedsNeverPredicate.SINGLETON, testObject.GetPredicate(new Term[] { Atom("z") }));
    }

    [TestMethod]
    public void TestSucceedsOnce()
    {
        Assert.AreSame(SucceedsOncePredicate.SINGLETON, testObject.GetPredicate(new Term[] { Atom("a") }));
        Assert.AreSame(SucceedsOncePredicate.SINGLETON, testObject.GetPredicate(new Term[] { Atom("d") }));
        Assert.AreSame(SucceedsOncePredicate.SINGLETON, testObject.GetPredicate(new Term[] { Atom("e") }));
        Assert.AreSame(SucceedsOncePredicate.SINGLETON, testObject.GetPredicate(new Term[] { Atom("f") }));
    }

    [TestMethod]
    public void TestSucceedsMany()
    {
        AssertSucceedsMany(Atom("b"), 2);
        AssertSucceedsMany(Atom("c"), 5);
        Assert.AreNotSame(testObject.GetPredicate(new Term[] { Atom("b") }), testObject.GetPredicate(new Term[] { Atom("b") }));
    }

    private void AssertSucceedsMany(Term arg, int expectedSuccesses)
    {
        Predicate p = testObject.GetPredicate(new Term[] { arg });
        Assert.AreSame(typeof(InterpretedUserDefinedPredicate), p.GetType()); // TODO Add assertClass to TestUtils
        for (int i = 0; i < expectedSuccesses; i++)
        {
            Assert.IsTrue(p.CouldReevaluationSucceed);
            Assert.IsTrue(p.Evaluate());
        }
        Assert.IsFalse(p.Evaluate());
    }

    [TestMethod]
    public void TestSpyPointEnabledFails()
    {
        var o = new SimplePrologListener();
        kb.PrologListeners.AddListener(o);

        kb.SpyPoints.TraceEnabled = (true);

        Predicate p = testObject.GetPredicate(new Term[] { Atom("z") });
        Assert.IsFalse(p.Evaluate());
        Assert.AreSame(typeof(SucceedsNeverPredicate), p.GetType());
        Assert.AreEqual("CALLtest(z)FAILtest(z)", o.GetResult());
    }

    [TestMethod]
    public void TestSpyPointEnabledSucceedsOnce()
    {
        var o = new SimplePrologListener();
        kb.PrologListeners.AddListener(o);

        kb.SpyPoints.TraceEnabled = (true);

        Predicate p = testObject.GetPredicate(new Term[] { Atom("a") });
        Assert.IsTrue(p.Evaluate());
        Assert.IsFalse(p.CouldReevaluationSucceed);
        Assert.AreSame(typeof(SucceedsOncePredicate), p.GetType());
        Assert.AreEqual("CALLtest(a)EXITtest(a)", o.GetResult());
    }

    [TestMethod]
    public void TestSpyPointEnabledSucceedsMany()
    {
        SimplePrologListener o = new SimplePrologListener();
        kb.PrologListeners.AddListener(o);

        kb.SpyPoints.TraceEnabled = (true);

        Predicate p = testObject.GetPredicate(new Term[] { Atom("c") });
        Assert.IsTrue(p.Evaluate());
        Assert.IsTrue(p.CouldReevaluationSucceed);
        Assert.IsTrue(p.Evaluate());
        Assert.IsTrue(p.CouldReevaluationSucceed);
        Assert.IsTrue(p.Evaluate());
        Assert.IsTrue(p.CouldReevaluationSucceed);
        Assert.IsTrue(p.Evaluate());
        Assert.IsTrue(p.CouldReevaluationSucceed);
        Assert.IsTrue(p.Evaluate());
        Assert.IsFalse(p.CouldReevaluationSucceed);
        Assert.AreSame(typeof(InterpretedUserDefinedPredicate), p.GetType());
        Assert.AreEqual("CALLtest(c)EXITtest(c)REDOtest(c)EXITtest(c)REDOtest(c)EXITtest(c)REDOtest(c)EXITtest(c)REDOtest(c)EXITtest(c)", o.GetResult());
    }
}
