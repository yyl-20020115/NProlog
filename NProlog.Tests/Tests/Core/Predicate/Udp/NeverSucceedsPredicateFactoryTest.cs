/*
 * Copyright 2021 S. Webber
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
using Org.NProlog.Core.Event;
using Org.NProlog.Core.Kb;
using Org.NProlog.Core.Terms;
using static Org.NProlog.Core.Event.SpyPoints;

namespace Org.NProlog.Core.Predicate.Udp;

[TestClass]
public class NeverSucceedsPredicateFactoryTest : TestUtils
{
    private SpyPoints spyPoints;
    private NeverSucceedsPredicateFactory testObject;
    private Term[] queryArgs = Array(Atom("a"), Atom("b"), Atom("c"));
    private SimplePrologListener listener;

    [TestInitialize]
    public void Before()
    {
        this.listener = new SimplePrologListener();
        var observable = new PrologListeners();
        observable.AddListener(listener);
        this.spyPoints = new SpyPoints(observable, TestUtils.CreateTermFormatter());
        var spyPoint = spyPoints.GetSpyPoint(new PredicateKey("test", 3));

        this.testObject = new NeverSucceedsPredicateFactory(spyPoint);
    }

    [TestMethod]
    public void TestGetPredicateSpyPointDisabled()
    {
        var predicate = testObject.GetPredicate(queryArgs);

        Assert.AreSame(PredicateUtils.FALSE, predicate);
        Assert.AreEqual("", listener.GetResult());
    }

    [TestMethod]
    public void TestGetPredicateSpyPointEnabled()
    {
        spyPoints.TraceEnabled = (true);

        var predicate = testObject.GetPredicate(queryArgs);

        Assert.AreSame(PredicateUtils.FALSE, predicate);
        Assert.AreEqual("CALLtest(a, b, c)FAILtest(a, b, c)", listener.GetResult());
    }

    [TestMethod]
    public void TestIsRetryable() => Assert.IsFalse(testObject.IsRetryable);
}
