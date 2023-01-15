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
using Org.NProlog.Core.Kb;
using Org.NProlog.Core.Predicate;
using Org.NProlog.Core.Terms;
using Org.NProlog.Core.Predicate.Udp;

namespace Org.NProlog.Core.Event;

[TestClass]
public class SpyPointsTest : TestUtils
{
    private readonly KnowledgeBase kb = TestUtils.CreateKnowledgeBase();

    [TestMethod]
    public void TestGetSameSpyPointForSamePredicateKey()
    {
        SpyPoints testObject = new SpyPoints(kb);
        PredicateKey key = CreateKey("test", 2);
        Assert.AreSame(testObject.GetSpyPoint(key), testObject.GetSpyPoint(key));


    }

    [TestMethod]
    public void TestGetSameSpyPointWhenPredicateKeysEqual()
    {
        SpyPoints testObject = new SpyPoints(kb);
        PredicateKey key1 = CreateKey("test", 2);
        PredicateKey key2 = CreateKey("test", 2);
        Assert.AreNotSame(key1, key2);
        Assert.AreSame(testObject.GetSpyPoint(key1), testObject.GetSpyPoint(key2));
    }

    [TestMethod]
    public void TestGetDifferentSpyPointWhenPredicateKeysNotEqual()
    {
        SpyPoints testObject = new SpyPoints(kb);
        PredicateKey key1 = CreateKey("test1", 0);
        PredicateKey key2 = CreateKey("test1", 1);
        PredicateKey key3 = CreateKey("test1", 2);
        PredicateKey key4 = CreateKey("test2", 2);
        Assert.AreNotSame(testObject.GetSpyPoint(key1), testObject.GetSpyPoint(key2));
        Assert.AreNotSame(testObject.GetSpyPoint(key2), testObject.GetSpyPoint(key3));
        Assert.AreNotSame(testObject.GetSpyPoint(key3), testObject.GetSpyPoint(key4));


    }

    [TestMethod]
    public void TestGetPredicateKey()
    {
        SpyPoints testObject = new SpyPoints(kb);
        PredicateKey key1 = CreateKey("test1", 2);
        PredicateKey key2 = CreateKey("test1", 2);
        PredicateKey key3 = CreateKey("test1", 1);
        PredicateKey key4 = CreateKey("test2", 1);
        Assert.AreNotSame(key1, key2);

        Assert.AreSame(key1, testObject.GetSpyPoint(key1).PredicateKey);
        Assert.AreSame(key1, testObject.GetSpyPoint(key2).PredicateKey);
        Assert.AreSame(key3, testObject.GetSpyPoint(key3).PredicateKey);
        Assert.AreSame(key4, testObject.GetSpyPoint(key4).PredicateKey);
    }

    [TestMethod]
    public void TestGetSpyPoints()
    {
        SpyPoints testObject = new SpyPoints(kb);
        PredicateKey[] keys = { CreateKey("test1", 0), CreateKey("test1", 2), CreateKey("test2", 2) };
        SpyPoints.SpyPoint[] spyPoints = new SpyPoints.SpyPoint[keys.Length];

        Assert.IsTrue(testObject.GetSpyPoints().Count == 0);
        for (int i = 0; i < keys.Length; i++)
        {
            spyPoints[i] = testObject.GetSpyPoint(keys[i]);
            Assert.AreEqual(i + 1, testObject.GetSpyPoints().Count);
        }

        for (int i = 0; i < keys.Length; i++)
        {
            Assert.AreSame(spyPoints[i], testObject.GetSpyPoints()[(keys[i])]);
        }
    }

    [TestMethod]
    public void TestSetSpyPointOnlyAltersSingleSpyPoint()
    {
        SpyPoints testObject = new SpyPoints(kb);
        PredicateKey key1 = CreateKey("test1", 2);
        PredicateKey key2 = CreateKey("test2", 2);
        SpyPoints.SpyPoint sp1 = testObject.GetSpyPoint(key1);
        SpyPoints.SpyPoint sp2 = testObject.GetSpyPoint(key2);

        Assert.IsFalse(sp1.Set);
        Assert.IsFalse(sp2.Set);

        testObject.SetSpyPoint(key1, true);
        Assert.IsTrue(sp1.Set);
        Assert.IsFalse(sp2.Set);

        testObject.SetSpyPoint(key1, true);
        Assert.IsTrue(sp1.Set);
        Assert.IsFalse(sp2.Set);

        testObject.SetSpyPoint(key1, false);
        Assert.IsFalse(sp1.Set);
        Assert.IsFalse(sp2.Set);
    }

    [TestMethod]
    public void TestSetSpyPointWorksEvenBeforeGetSpyPointCalled()
    {
        SpyPoints testObject = new SpyPoints(kb);
        PredicateKey key = CreateKey("test", 2);
        testObject.SetSpyPoint(key, true);
        Assert.IsTrue(testObject.GetSpyPoint(key).Set);
    }

    [TestMethod]
    public void TestSpyPointAffectedBySetTraceEnabledCallAfterItWasCreated()
    {
        SpyPoints testObject = new SpyPoints(kb);
        SpyPoints.SpyPoint sp = testObject.GetSpyPoint(CreateKey("test", 1));
        Assert.IsFalse(sp.IsEnabled);
        testObject.TraceEnabled = true;
        Assert.IsTrue(sp.IsEnabled);
        testObject.TraceEnabled = false;
        Assert.IsFalse(sp.IsEnabled);
    }

    [TestMethod]
    public void TestSpyPointAffectedBySetTraceEnabledCallBeforeItWasCreated()
    {
        SpyPoints testObject = new SpyPoints(kb);
        testObject.TraceEnabled = true;
        SpyPoints.SpyPoint sp = testObject.GetSpyPoint(CreateKey("test", 1));
        Assert.IsTrue(sp.IsEnabled);
        testObject.TraceEnabled = false;
        Assert.IsFalse(sp.IsEnabled);
    }

    [TestMethod]
    public void TestSetTraceEnabledIndependantOfSetSpyPoint()
    {
        SpyPoints testObject = new SpyPoints(kb);
        PredicateKey key = CreateKey("test", 2);

        SpyPoints.SpyPoint sp = testObject.GetSpyPoint(key);
        Assert.IsTrue(sp.Set == false && sp.IsEnabled == false);

        testObject.TraceEnabled = true;
        Assert.IsTrue(sp.Set == false && sp.IsEnabled == true);

        testObject.TraceEnabled = false;
        Assert.IsTrue(sp.Set == false && sp.IsEnabled == false);

        testObject.SetSpyPoint(key, true);
        Assert.IsTrue(sp.Set == true && sp.IsEnabled == true);

        testObject.TraceEnabled = true;
        Assert.IsTrue(sp.Set == true && sp.IsEnabled == true);

        testObject.TraceEnabled = false;
        Assert.IsTrue(sp.Set == true && sp.IsEnabled == true);

        testObject.TraceEnabled = true;
        Assert.IsTrue(sp.Set == true && sp.IsEnabled == true);

        testObject.SetSpyPoint(key, false);
        Assert.IsTrue(sp.Set == false && sp.IsEnabled == true);
    }

    [TestMethod]
    public void TestSpyPointUpdatesObserver()
    {
        // Add a listener to the KnowledgeBase's PrologListeners
        // so we can keep track of ProjogEvent objects created by the SpyPoint
        SimplePrologListener listener = new SimplePrologListener();
        kb.PrologListeners.AddListener(listener);

        PredicateKey key = CreateKey("test", 1);
        DynamicUserDefinedPredicateFactory pf = new DynamicUserDefinedPredicateFactory(kb, key);
        pf.AddFirst(ClauseModel.CreateClauseModel(Structure("test", new Variable())));
        kb.Predicates.AddUserDefinedPredicate(pf);

        SpyPoints testObject = new SpyPoints(kb);
        SpyPoints.SpyPoint sp = testObject.GetSpyPoint(key);

        // make a number of log calls to the spy point -
        // the observer should not be updated with any of them as the spy point is not set
        Assert.IsFalse(sp.Set);
        sp.LogCall(this, new Term[] { Atom("a") });
        sp.LogExit(this, new Term[] { Atom("b") }, 1);
        sp.LogFail(this, new Term[] { Atom("c") });
        sp.LogRedo(this, new Term[] { Atom("d") });
        Assert.IsTrue(listener.Count == 0);

        // set the spy point and then make a number of log calls to the spy point -
        // the observer should now be updated with each call in the order they are made
        testObject.SetSpyPoint(key, true);
        sp.LogCall(this, new Term[] { Atom("z") });
        sp.LogExit(this, new Term[] { List(Atom("a"), Variable("X")) }, 0);
        sp.LogFail(this, new Term[] { Structure("c", EmptyList.EMPTY_LIST, Atom("z"), IntegerNumber(1)) });
        sp.LogRedo(this, new Term[] { new Variable() });
        Assert.AreEqual(4, listener.Count);
        AssertPrologEvent(listener.Get(0), "CALL", "test(z)");
        AssertPrologEvent(listener.Get(1), "EXIT", "test([a,X])");
        AssertPrologEvent(listener.Get(2), "FAIL", "test(c([], z, 1))");
        AssertPrologEvent(listener.Get(3), "REDO", "test(_)");
    }

    private void AssertPrologEvent(string @event, string expectedType, string expectedMessage)
    {
        Assert.AreEqual(expectedType + expectedMessage, @event);
    }

    private PredicateKey CreateKey(string name, int numArgs)
    {
        return new PredicateKey(name, numArgs);
    }
}
