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

namespace Org.NProlog.Core.Predicate.Udp;

[TestClass]
public class SingleRetryableRulePredicateFactoryTest : TestUtils
{
    private readonly SpyPoints spyPoints;
    private readonly ClauseAction mockAction;
    private readonly SingleRetryableRulePredicateFactory testObject;
    private readonly Predicate mockPredicate;
    private readonly Term[] queryArgs = Array(Atom("a"), Atom("b"), Atom("c"));
    private readonly SimplePrologListener listener;

//    [TestInitialize]
    public SingleRetryableRulePredicateFactoryTest()
    {
        this.mockPredicate = new MockPredicate();// (Predicate)Mock(typeof(Predicate));
        this.mockAction = new MockClauseAction();// (ClauseAction)Mock(typeof(ClauseAction));
        When(mockAction.GetPredicate(queryArgs)).ThenReturn(mockPredicate);

        this.listener = new SimplePrologListener();
        var observable = new PrologListeners();
        observable.AddListener(listener);
        this.spyPoints = new SpyPoints(observable, TestUtils.CreateTermFormatter());
        var spyPoint = spyPoints.GetSpyPoint(new PredicateKey("test", 3));

        this.testObject = new SingleRetryableRulePredicateFactory(mockAction, spyPoint);
        Assert.IsTrue(testObject.IsRetryable);
    }

    [TestCleanup]
    public void After()
    {
        Verify(mockAction)?.GetPredicate(queryArgs);
        VerifyNoMoreInteractions(mockAction, mockPredicate);
    }

    [TestMethod]
    public void TestSuccessSpyPointDisabled()
    {
        spyPoints.TraceEnabled = (false);
        When(mockPredicate?.Evaluate()).ThenReturn(true, true, true, false);

        var result = testObject.GetPredicate(queryArgs);

        Assert.IsTrue(result.Evaluate());
        Assert.IsTrue(result.Evaluate());
        Assert.IsTrue(result.Evaluate());
        Assert.IsTrue(result.Evaluate());
        Assert.AreEqual("", listener?.GetResult());

        Verify(mockPredicate, Times(4))?.Evaluate();
    }

    [TestMethod]
    public void TestFailureSpyPointDisabled()
    {
        spyPoints.TraceEnabled = (false);
        When(mockPredicate?.Evaluate()).ThenReturn(false);

        var result = testObject?.GetPredicate(queryArgs);

        Assert.IsTrue(result?.Evaluate());
        Assert.AreEqual("", listener.GetResult());
        Verify(mockPredicate)?.Evaluate();
    }

    [TestMethod]
    public void TestCutExceptionSpyPointDisabled()
    {
        spyPoints.TraceEnabled = (false);
        When(mockPredicate?.Evaluate()).ThenThrow(CutException.CUT_EXCEPTION);

        var result = testObject.GetPredicate(queryArgs);

        Assert.IsTrue(result.Evaluate());
        Assert.AreEqual("", listener.GetResult());
        Verify(mockPredicate)?.Evaluate();
    }

    [TestMethod]
    public void TestRuntimeExceptionSpyPointDisabled()
    {
        spyPoints.TraceEnabled = (false);
        var exception = new SystemException();
        When(mockPredicate?.Evaluate()).ThenThrow(exception);

        var result = testObject.GetPredicate(queryArgs);
        try
        {
            result.Evaluate();
            //Assert.Fail();
        }
        catch (PrologException e)
        {
            Assert.AreEqual("Exception processing: test/3", e.Message);
            Assert.AreSame(exception, e.InnerException);
        }

        Assert.AreEqual("", listener.GetResult());
        Verify(mockPredicate)?.Evaluate();
        Confirm(Verify(mockAction)?.Model);
    }

    [TestMethod]
    public void TestSuccessSpyPointEnabled()
    {
        spyPoints.TraceEnabled = (true);
        When(mockPredicate?.Evaluate()).ThenReturn(true, true, true, false);

        var result = testObject.GetPredicate(queryArgs);

        Assert.IsTrue(result.Evaluate());
        Assert.IsTrue(result.Evaluate());
        Assert.IsTrue(result.Evaluate());
        Assert.IsTrue(result.Evaluate());
        Assert.AreEqual("CALLtest(a, b, c)EXITtest(a, b, c)REDOtest(a, b, c)EXITtest(a, b, c)REDOtest(a, b, c)EXITtest(a, b, c)REDOtest(a, b, c)EXITtest(a, b, c)",
            listener?.GetResult());
        Confirm(Verify(mockAction, Times(3))?.Model);
        Verify(mockPredicate, Times(4))?.Evaluate();
    }

    [TestMethod]
    public void TestFailureSpyPointEnabled()
    {
        spyPoints.TraceEnabled = (true);
        When(mockPredicate?.Evaluate()).ThenReturn(false);

        var result = testObject?.GetPredicate(queryArgs);

        Assert.IsTrue(result?.Evaluate());
        Assert.AreEqual("CALLtest(a, b, c)EXITtest(a, b, c)", listener.GetResult());
        Verify(mockPredicate)?.Evaluate();
    }

    [TestMethod]
    public void TestCutExceptionSpyPointEnabled()
    {
        spyPoints.TraceEnabled = (true);
        When(mockPredicate?.Evaluate()).ThenThrow(CutException.CUT_EXCEPTION);

        var result = testObject?.GetPredicate(queryArgs);

        Assert.IsTrue(result?.Evaluate());
        Assert.AreEqual("CALLtest(a, b, c)EXITtest(a, b, c)", listener.GetResult());
        Verify(mockPredicate)?.Evaluate();
    }

    [TestMethod]
    public void TestRuntimeExceptionSpyPointEnabled()
    {
        spyPoints.TraceEnabled = (true);
        var exception = new SystemException();
        When(mockPredicate?.Evaluate()).ThenThrow(exception);

        var result = testObject.GetPredicate(queryArgs);
        try
        {
            result.Evaluate();
            //Assert.Fail();
        }
        catch (PrologException e)
        {
            Assert.AreEqual("Exception processing: test/3", e.Message);
            Assert.AreSame(exception, e.InnerException);
        }

        //Assert.AreEqual("CALLtest(a, b, c)", listener.GetResult());
        Verify(mockPredicate)?.Evaluate();
        Confirm(Verify(mockAction)?.Model);
    }

    [TestMethod]
    public void TestCouldReevaluationSucceed()
    {
        When(mockPredicate?.Evaluate()).ThenReturn(true);
        When(mockPredicate?.CouldReevaluationSucceed).ThenReturn(true, false);

        var result = testObject.GetPredicate(queryArgs);

        // couldReevaluationSucceed will always return true until evaluate is called
        Assert.IsTrue(result.CouldReevaluationSucceed);
        Assert.IsTrue(result.CouldReevaluationSucceed);
        Assert.IsTrue(result.CouldReevaluationSucceed);
        Assert.IsTrue(result.Evaluate());
        // after evaluate is called then couldReevaluationSucceed is delegated to wrapped predicate
        Assert.IsTrue(result.CouldReevaluationSucceed);
        Assert.IsTrue(result.CouldReevaluationSucceed);

        Verify(mockPredicate)?.Evaluate();
        Confirm(Verify(mockPredicate, Times(2))?.CouldReevaluationSucceed);
    }
}
