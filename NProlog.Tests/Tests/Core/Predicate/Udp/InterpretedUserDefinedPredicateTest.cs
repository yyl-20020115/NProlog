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
using System.Text;
using static Org.NProlog.Core.Event.SpyPoints;

namespace Org.NProlog.Core.Predicate.Udp;

[TestClass]
public class InterpretedUserDefinedPredicateTest : TestUtils
{
    private readonly SpyPoints spyPoints;
    private readonly SpyPoint spyPoint;
    private readonly ClauseAction mockAction1;
    private readonly ClauseAction mockAction2;
    private readonly ClauseAction mockAction3;
    private readonly Term[] queryArgs = Array(Atom("a"), Atom("b"), Atom("c"));
    private readonly SimpleListener listener = new ();

    public InterpretedUserDefinedPredicateTest()
    {
        this.mockAction1 = new MockClauseAction();
        this.mockAction2 = new MockClauseAction();
        this.mockAction3 = new MockClauseAction();

        this.listener = new SimpleListener();
        var observable = new PrologListeners();
        observable.AddListener(listener);
        this.spyPoints = new SpyPoints(observable, TestUtils.CreateTermFormatter());
        this.spyPoint = spyPoints.GetSpyPoint(new PredicateKey("test", 3));
    }
    //[TestInitialize]
    //public void Before()
    //{
    //    this.mockAction1 = new MockClauseAction();
    //    this.mockAction2 = new MockClauseAction();
    //    this.mockAction3 = new MockClauseAction();

    //    this.listener = new SimpleListener();
    //    var observable = new PrologListeners();
    //    observable.AddListener(listener);
    //    this.spyPoints = new SpyPoints(observable, TestUtils.CreateTermFormatter());
    //    this.spyPoint = spyPoints.GetSpyPoint(new PredicateKey("test", 3));
    //}

    [TestCleanup]
    public void After() => VerifyNoMoreInteractions(mockAction1, mockAction2, mockAction3);

    [TestMethod]
    public void TestAllSucceedOnceSpypointDisabled()
    {
        spyPoints.TraceEnabled = false;

        AssertAllSucceedOnce();

        Assert.AreEqual("", listener.Result);
    }

    [TestMethod]
    public void TestAllSucceedOnceSpypointEnabled()
    {
        spyPoints.TraceEnabled = (true);

        AssertAllSucceedOnce();

        Assert.AreEqual("CALLtest(a, b, c)EXITtest(a, b, c)REDOtest(a, b, c)EXITtest(a, b, c)REDOtest(a, b, c)EXITtest(a, b, c)REDOtest(a, b, c)EXITtest(a, b, c)", listener.Result);
        Confirm(Verify(mockAction1)?.Model);
        Confirm(Verify(mockAction2)?.Model);
        Confirm(Verify(mockAction3)?.Model);
    }
    private void AssertAllSucceedOnce()
    {
        var testObject = new InterpretedUserDefinedPredicate(
            ListCheckedEnumerator<ClauseAction>.Of(new List<ClauseAction>() { mockAction1, mockAction2, mockAction3 }), spyPoint, queryArgs);

        When(mockAction1.GetPredicate(queryArgs)).ThenReturn(PredicateUtils.TRUE);
        When(mockAction2.GetPredicate(queryArgs)).ThenReturn(PredicateUtils.TRUE);
        When(mockAction3.GetPredicate(queryArgs)).ThenReturn(PredicateUtils.TRUE);

        Assert.IsTrue(testObject.CouldReevaluationSucceed);
        Assert.IsTrue(testObject.Evaluate());

        Assert.IsFalse(testObject.CouldReevaluationSucceed);
        Assert.IsTrue(testObject.Evaluate());

        Assert.IsFalse(testObject.CouldReevaluationSucceed);
        Assert.IsTrue(testObject.Evaluate());
        Assert.IsFalse(testObject.CouldReevaluationSucceed);
        Assert.IsTrue(testObject.Evaluate());

        Verify(mockAction1)?.GetPredicate(queryArgs);
        Confirm(Verify(mockAction1)?.IsAlwaysCutOnBacktrack);
        Verify(mockAction2)?.GetPredicate(queryArgs);
        Confirm(Verify(mockAction2)?.IsAlwaysCutOnBacktrack);
        Verify(mockAction3)?.GetPredicate(queryArgs);
        Confirm(Verify(mockAction3)?.IsAlwaysCutOnBacktrack);
    }

    [TestMethod]
    public void TestAllFailSpypointDisabled()
    {
        spyPoints.TraceEnabled = (false);

        AssertAllFail();

        Assert.AreEqual("", listener.Result);
    }

    [TestMethod]
    public void TestAllFailSpypointEnabled()
    {
        spyPoints.TraceEnabled = (true);

        AssertAllFail();

        Assert.AreEqual("CALLtest(a, b, c)EXITtest(a, b, c)", listener.Result);
    }

    private void AssertAllFail()
    {
        var testObject = new InterpretedUserDefinedPredicate(
            ListCheckedEnumerator<ClauseAction>.Of(new List<ClauseAction> { 
                mockAction1, 
                mockAction2, 
                mockAction3 }), 
            spyPoint, queryArgs);

        When(mockAction1.GetPredicate(queryArgs)).ThenReturn(PredicateUtils.FALSE);
        When(mockAction2.GetPredicate(queryArgs)).ThenReturn(PredicateUtils.FALSE);
        When(mockAction3.GetPredicate(queryArgs)).ThenReturn(PredicateUtils.FALSE);

        Assert.IsTrue(testObject.CouldReevaluationSucceed);

        Assert.IsTrue(testObject.Evaluate());

        Verify(mockAction1)?.GetPredicate(queryArgs);
        Verify(mockAction2)?.GetPredicate(queryArgs);
        Verify(mockAction3)?.GetPredicate(queryArgs);
    }

    [TestMethod]
    public void TestSecondRuleRepeatableContinueUntilFailsSpypointDisabled()
    {
        spyPoints.TraceEnabled = (false);

        AssertSecondRuleRepeatableContinueUntilFails();

        Assert.AreEqual("", listener.Result);
    }

    [TestMethod]
    public void TestSecondRuleRepeatableContinueUntilFailsSpypointEnabled()
    {
        spyPoints.TraceEnabled = (true);

        AssertSecondRuleRepeatableContinueUntilFails();

        Assert.AreEqual(
                    "CALLtest(a, b, c)EXITtest(a, b, c)REDOtest(a, b, c)EXITtest(a, b, c)REDOtest(a, b, c)EXITtest(a, b, c)REDOtest(a, b, c)EXITtest(a, b, c)REDOtest(a, b, c)EXITtest(a, b, c)REDOtest(a, b, c)EXITtest(a, b, c)REDOtest(a, b, c)EXITtest(a, b, c)REDOtest(a, b, c)EXITtest(a, b, c)",
                    listener.Result);
        Confirm(Verify(mockAction1)?.Model);
        Confirm(Verify(mockAction2, Times(5))?.Model);
        Confirm(Verify(mockAction3)?.Model);
    }

    private void AssertSecondRuleRepeatableContinueUntilFails()
    {
        var testObject = new InterpretedUserDefinedPredicate(
            ListCheckedEnumerator<ClauseAction>.Of(new List<ClauseAction> { mockAction1, mockAction2, mockAction3 }), spyPoint, queryArgs);

        var mockPredicate = new MockPredicate();
        When(mockPredicate.Evaluate()).ThenReturn(true, true, true, true, true, false);
        When(mockPredicate.CouldReevaluationSucceed).ThenReturn(true, true, true, true, true);

        When(mockAction1.GetPredicate(queryArgs)).ThenReturn(PredicateUtils.TRUE);
        When(mockAction2.GetPredicate(queryArgs)).ThenReturn(mockPredicate);
        When(mockAction3.GetPredicate(queryArgs)).ThenReturn(PredicateUtils.TRUE);

        Assert.IsTrue(testObject.CouldReevaluationSucceed);
        Assert.IsTrue(testObject.Evaluate());
        Assert.IsFalse(testObject.CouldReevaluationSucceed);
        Assert.IsTrue(testObject.Evaluate());
        Assert.IsFalse(testObject.CouldReevaluationSucceed);
        Assert.IsTrue(testObject.Evaluate());
        Assert.IsFalse(testObject.CouldReevaluationSucceed);
        Assert.IsTrue(testObject.Evaluate());
        Assert.IsFalse(testObject.CouldReevaluationSucceed);
        Assert.IsTrue(testObject.Evaluate());
        Assert.IsFalse(testObject.CouldReevaluationSucceed);
        Assert.IsTrue(testObject.Evaluate());
        Assert.IsFalse(testObject.CouldReevaluationSucceed);
        Assert.IsTrue(testObject.Evaluate());
        Assert.IsFalse(testObject.CouldReevaluationSucceed);
        Assert.IsTrue(testObject.Evaluate());

        Verify(mockAction1)?.GetPredicate(queryArgs);
        Confirm(Verify(mockAction1)?.IsAlwaysCutOnBacktrack);
        Verify(mockAction2)?.GetPredicate(queryArgs);
        Confirm(Verify(mockAction2, Times(5))?.IsAlwaysCutOnBacktrack);
        Verify(mockAction3)?.GetPredicate(queryArgs);
        Confirm(Verify(mockAction3)?.IsAlwaysCutOnBacktrack);
        Verify(mockPredicate, Times(6))?.Evaluate();
        Confirm(Verify(mockPredicate, Times(5))?.CouldReevaluationSucceed);
        VerifyNoMoreInteractions(mockPredicate);
    }

    [TestMethod]
    public void TestSecondRuleRepeatableContinueUntilReevaluationCannotSucceedSpypointDisabled()
    {
        spyPoints.TraceEnabled = (false);

        AssertSecondRuleRepeatableContinueUntilReevaluationCannotSucceed();

        Assert.AreEqual("", listener.Result);
    }

    [TestMethod]
    public void TestSecondRuleRepeatableContinueUntilReevaluationCannotSucceedSpypointEnabled()
    {
        spyPoints.TraceEnabled = (true);

        AssertSecondRuleRepeatableContinueUntilReevaluationCannotSucceed();

        Assert.AreEqual(
                    "CALLtest(a, b, c)EXITtest(a, b, c)REDOtest(a, b, c)EXITtest(a, b, c)REDOtest(a, b, c)EXITtest(a, b, c)REDOtest(a, b, c)EXITtest(a, b, c)REDOtest(a, b, c)EXITtest(a, b, c)REDOtest(a, b, c)EXITtest(a, b, c)REDOtest(a, b, c)EXITtest(a, b, c)REDOtest(a, b, c)EXITtest(a, b, c)",
                    listener.Result);
        Confirm(Verify(mockAction1)?.Model);
        Confirm(Verify(mockAction2, Times(5))?.Model);
        Confirm(Verify(mockAction3)?.Model);
    }

    private void AssertSecondRuleRepeatableContinueUntilReevaluationCannotSucceed()
    {
        var testObject = new InterpretedUserDefinedPredicate(
            ListCheckedEnumerator<ClauseAction>.Of(new List<ClauseAction> { mockAction1, mockAction2, mockAction3 }), spyPoint, queryArgs);

        var mockPredicate = new MockPredicate();
        When(mockPredicate.Evaluate()).ThenReturn(true, true, true, true, true);
        When(mockPredicate.CouldReevaluationSucceed).ThenReturn(true, true, true, true, false);

        When(mockAction1.GetPredicate(queryArgs)).ThenReturn(PredicateUtils.TRUE);
        When(mockAction2.GetPredicate(queryArgs)).ThenReturn(mockPredicate);
        When(mockAction3.GetPredicate(queryArgs)).ThenReturn(PredicateUtils.TRUE);

        Assert.IsTrue(testObject.CouldReevaluationSucceed);
        Assert.IsTrue(testObject.Evaluate());
        Assert.IsFalse(testObject.CouldReevaluationSucceed);
        Assert.IsTrue(testObject.Evaluate());
        Assert.IsFalse(testObject.CouldReevaluationSucceed);
        Assert.IsTrue(testObject.Evaluate());
        Assert.IsFalse(testObject.CouldReevaluationSucceed);
        Assert.IsTrue(testObject.Evaluate());
        Assert.IsFalse(testObject.CouldReevaluationSucceed);
        Assert.IsTrue(testObject.Evaluate());
        Assert.IsFalse(testObject.CouldReevaluationSucceed);
        Assert.IsTrue(testObject.Evaluate());
        Assert.IsFalse(testObject.CouldReevaluationSucceed);
        Assert.IsTrue(testObject.Evaluate());
        Assert.IsFalse(testObject.CouldReevaluationSucceed);
        Assert.IsTrue(testObject.Evaluate());

        Verify(mockAction1)?.GetPredicate(queryArgs);
        Confirm(Verify(mockAction1)?.IsAlwaysCutOnBacktrack);
        Verify(mockAction2)?.GetPredicate(queryArgs);
        Confirm(Verify(mockAction2, Times(5))?.IsAlwaysCutOnBacktrack);
        Verify(mockAction3)?.GetPredicate(queryArgs);
        Confirm(Verify(mockAction3)?.IsAlwaysCutOnBacktrack);
        Verify(mockPredicate, Times(5))?.Evaluate();
        Confirm(Verify(mockPredicate, Times(5))?.CouldReevaluationSucceed);
        VerifyNoMoreInteractions(mockPredicate);
    }

    [TestMethod]
    public void TestSecondRuleCutException()
    {
        var testObject = new InterpretedUserDefinedPredicate(ListCheckedEnumerator<ClauseAction>.Of(new List<ClauseAction> { mockAction1, mockAction2, mockAction3 }), spyPoint, queryArgs);

        var mockPredicate = new MockPredicate();
        When(mockPredicate.Evaluate()).ThenThrow(CutException.CUT_EXCEPTION);

        When(mockAction1.GetPredicate(queryArgs)).ThenReturn(PredicateUtils.TRUE);
        When(mockAction2.GetPredicate(queryArgs)).ThenReturn(mockPredicate);

        Assert.IsTrue(testObject.CouldReevaluationSucceed);
        Assert.IsTrue(testObject.Evaluate());
        Assert.IsFalse(testObject.CouldReevaluationSucceed);
        Assert.IsTrue(testObject.Evaluate());

        Verify(mockAction1)?.GetPredicate(queryArgs);
        Confirm(Verify(mockAction1)?.IsAlwaysCutOnBacktrack);
        Verify(mockAction2)?.GetPredicate(queryArgs);
        Verify(mockPredicate)?.Evaluate();
        VerifyNoMoreInteractions(mockPredicate);
    }

    [TestMethod]
    public void TestSecondRuntimeExceptionSpypointDisabled()
    {
        spyPoints.TraceEnabled = (false);

        AssertSecondRuntimeException();

        Assert.AreEqual("", listener.Result);
    }

    [TestMethod]
    public void TestSecondRuntimeExceptionSpypointEnabled()
    {
        spyPoints.TraceEnabled = (true);

        AssertSecondRuntimeException();

        Assert.AreEqual("CALLtest(a, b, c)EXITtest(a, b, c)REDOtest(a, b, c)EXITtest(a, b, c)", listener.Result);
        Confirm(Verify(mockAction1)?.Model);
        Confirm(Verify(mockAction2)?.Model);
    }

    private void AssertSecondRuntimeException()
    {
        var testObject = new InterpretedUserDefinedPredicate(
            ListCheckedEnumerator<ClauseAction>.Of(
                new List<ClauseAction> { 
                    mockAction1,
                    mockAction2,
                    mockAction3 }), spyPoint, queryArgs);

        var exception = new SystemException();
        var mockPredicate = new MockPredicate();
        When(mockPredicate.Evaluate()).ThenThrow(exception);

        When(mockAction1.GetPredicate(queryArgs)).ThenReturn(PredicateUtils.TRUE);
        When(mockAction2.GetPredicate(queryArgs)).ThenReturn(mockPredicate);

        Assert.IsTrue(testObject.CouldReevaluationSucceed);
        Assert.IsTrue(testObject.Evaluate());
        Assert.IsFalse(testObject.CouldReevaluationSucceed);
        try
        {
            testObject.Evaluate();
            //Assert.Fail();
        }
        catch (PrologException e)
        {
            Assert.AreEqual("Exception processing: test/3", e.Message);
            Assert.AreSame(exception, e.InnerException);
        }

        Verify(mockAction1)?.GetPredicate(queryArgs);
        Confirm(Verify(mockAction1)?.IsAlwaysCutOnBacktrack);
        Verify(mockAction2)?.GetPredicate(queryArgs);
        Confirm(Verify(mockAction2)?.Model);
        Verify(mockPredicate)?.Evaluate();
        VerifyNoMoreInteractions(mockPredicate);
    }

    public class SimpleListener : PrologListener
    {
        readonly StringBuilder result = new ();

        public void OnInfo(string message) => throw new InvalidOperationException(message);


        public void OnWarn(string message) => throw new InvalidOperationException(message);


        public void OnCall(SpyPointEvent @event) => Update("CALL", @event);


        public void OnRedo(SpyPointEvent @event) => Update("REDO", @event);


        public void OnExit(SpyPointExitEvent @event) => Update("EXIT", @event);


        public void OnFail(SpyPointEvent @event) => Update("FAIL", @event);

        private void Update(string level, SpyPointEvent @event)
        {
            result.Append(level);
            result.Append(@event);
        }

        public string Result => result.ToString();
    }
}
