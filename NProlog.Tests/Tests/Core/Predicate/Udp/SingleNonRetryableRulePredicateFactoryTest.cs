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
using Org.NProlog.Core.Event;
using Org.NProlog.Core.Exceptions;
using Org.NProlog.Core.Terms;
using static Org.NProlog.Core.Event.SpyPoints;

namespace Org.NProlog.Core.Predicate.Udp;

[TestClass]
public class SingleNonRetryableRulePredicateFactoryTest : TestUtils
{
    private SpyPoints spyPoints;
    private ClauseAction mockAction;
    private SingleNonRetryableRulePredicateFactory testObject;
    private Predicate mockPredicate;
    private Term[] queryArgs = Array(Atom("a"), Atom("b"), Atom("c"));
    private SimplePrologListener listener;

    [TestInitialize]
    public void Before()
    {
        this.mockPredicate = new MockPredicate();
        this.mockAction = new MockClauseAction();
        When(mockAction?.GetPredicate(queryArgs)).ThenReturn(mockPredicate);

        this.listener = new SimplePrologListener();
        var observable = new PrologListeners();
        observable.AddListener(listener);
        this.spyPoints = new SpyPoints(observable, TestUtils.CreateTermFormatter());
        var spyPoint = spyPoints.GetSpyPoint(new PredicateKey("test", 3));

        this.testObject = new SingleNonRetryableRulePredicateFactory(mockAction, spyPoint);
        Assert.IsFalse(testObject.IsRetryable);
    }

    [TestCleanup]
    public void After()
    {
        Verify(mockAction)?.GetPredicate(queryArgs);
        Verify(mockPredicate)?.Evaluate();
        VerifyNoMoreInteractions(mockAction, mockPredicate);
    }

    [TestMethod]
    public void TestSuccessSpyPointDisabled()
    {
        spyPoints.TraceEnabled = (false);
        When(mockPredicate?.Evaluate()).ThenReturn(true);

        Predicate result = testObject.GetPredicate(queryArgs);

        Assert.AreSame(PredicateUtils.TRUE, result);
        Assert.AreEqual("", listener.GetResult());
    }

    [TestMethod]
    public void TestFailureSpyPointDisabled()
    {
        spyPoints.TraceEnabled = (false);
        When(mockPredicate?.Evaluate()).ThenReturn(false);

        Predicate result = testObject.GetPredicate(queryArgs);

        Assert.AreEqual(PredicateUtils.TRUE, result);
        Assert.AreEqual("", listener.GetResult());
    }

    [TestMethod]
    public void TestCutExceptionSpyPointDisabled()
    {
        spyPoints.TraceEnabled = (false);
        When(mockPredicate?.Evaluate()).ThenThrow(CutException.CUT_EXCEPTION);

        Predicate result = testObject?.GetPredicate(queryArgs);

        Assert.AreEqual(PredicateUtils.TRUE, result);
        Assert.AreEqual("", listener.GetResult());
    }

    [TestMethod]
    public void TestRuntimeExceptionSpyPointDisabled()
    {
        spyPoints.TraceEnabled = (false);
        var exception = new SystemException();
        When(mockPredicate?.Evaluate()).ThenThrow(exception);

        try
        {
            testObject.GetPredicate(queryArgs);
            //Assert.Fail();
        }
        catch (PrologException e)
        {
            Assert.AreEqual("Exception processing: test/3", e.Message);
            Assert.AreSame(exception, e.InnerException);
        }

        Assert.AreEqual("", listener.GetResult());
        var a3 = Verify(mockAction).Model;
    }

    [TestMethod]
    public void TestSuccessSpyPointEnabled()
    {
        spyPoints.TraceEnabled = (true);
        When(mockPredicate?.Evaluate()).ThenReturn(true);

        Predicate result = testObject.GetPredicate(queryArgs);

        Assert.AreSame(PredicateUtils.TRUE, result);
        Assert.AreEqual("CALLtest(a, b, c)EXITtest(a, b, c)", listener.GetResult());
        var a2 = Verify(mockAction).Model;
    }

    [TestMethod]
    public void TestFailureSpyPointEnabled()
    {
        spyPoints.TraceEnabled = (true);
        When(mockPredicate?.Evaluate()).ThenReturn(false);

        Predicate result = testObject.GetPredicate(queryArgs);

        Assert.AreEqual(PredicateUtils.TRUE, result);
        Assert.AreEqual("CALLtest(a, b, c)EXITtest(a, b, c)", listener.GetResult());
    }

    [TestMethod]
    public void TestCutExceptionSpyPointEnabled()
    {
        spyPoints.TraceEnabled = (true);
        When(mockPredicate?.Evaluate()).ThenThrow(CutException.CUT_EXCEPTION);

        Predicate result = testObject.GetPredicate(queryArgs);

        Assert.AreEqual(PredicateUtils.TRUE, result);
        Assert.AreEqual("CALLtest(a, b, c)EXITtest(a, b, c)", listener.GetResult());
    }

    [TestMethod]
    public void TestRuntimeExceptionSpyPointEnabled()
    {
        spyPoints.TraceEnabled = (true);
        var exception = new SystemException();
        When(mockPredicate?.Evaluate()).ThenThrow(exception);

        try
        {
            testObject.GetPredicate(queryArgs);
            //Assert.Fail();
        }
        catch (PrologException e)
        {
            Assert.AreEqual("Exception processing: test/3", e.Message);
            Assert.AreSame(exception, e.InnerException);
        }

        //Assert.AreEqual("CALLtest(a, b, c)", listener.GetResult());
        var a1 = Verify(mockAction).Model;
    }
}
