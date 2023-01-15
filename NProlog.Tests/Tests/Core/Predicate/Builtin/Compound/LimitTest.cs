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
using Org.NProlog.Core.Kb;
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Predicate.Builtin.Compound;

[TestClass]



public class LimitTest : TestUtils
{
    [TestMethod]
    public void TestPreprocessCannotOptimiseVariable()
    {
        Limit o = new Limit();

        Term t = Terms.Structure.CreateStructure("limit", new Term[] { new IntegerNumber(3), new Variable("Y") });
        PredicateFactory optimised = o.Preprocess(t);

        Assert.AreSame(o, optimised);
    }

    [TestMethod]
    public void TestPreprocessNotPreprocessablePredicateFactory()
    {
        var kb = CreateKnowledgeBase();
        var limitTerm = ParseTerm("limit(3, test(a, b)).");
        var queryArg = limitTerm.GetArgument(1);
        // note not a PreprocessablePredicateFactory
        var mockPredicateFactory = new MockPredicateFactory();
        var mockPredicate = new MockPredicate();
        var key = PredicateKey.CreateForTerm(queryArg);
        kb.Predicates.AddPredicateFactory(key, mockPredicateFactory);
        When(mockPredicateFactory.GetPredicate(queryArg.Args)).ThenReturn(mockPredicate);
        When(mockPredicate.Evaluate()).ThenReturn(true);
        When(mockPredicate.CouldReevaluationSucceed).ThenReturn(true);

        var o = (Limit)kb.Predicates.GetPredicateFactory(limitTerm);
        var optimised = o.Preprocess(limitTerm);

        Assert.AreEqual("OptimisedLimit", optimised.GetType().Name);
        var queryArgs = new Term[] { limitTerm.GetArgument(0), limitTerm.GetArgument(1) };
        var p = optimised.GetPredicate(queryArgs);
        Assert.AreEqual("LimitPredicate", p.GetType().Name);
        Assert.AreNotSame(p, optimised.GetPredicate(queryArgs));

        Assert.IsTrue(p.CouldReevaluationSucceed);
        Assert.IsTrue(p.Evaluate());
        Assert.IsFalse(p.CouldReevaluationSucceed);
        Assert.IsFalse(p.Evaluate());
        Assert.IsFalse(p.CouldReevaluationSucceed);
        Assert.IsFalse(p.Evaluate());
        Assert.IsFalse(p.CouldReevaluationSucceed);
        Assert.IsFalse(p.Evaluate());

        Verify(mockPredicateFactory, Times(2)).GetPredicate(queryArg.Args);
        Verify(mockPredicate, Times(3)).Evaluate();
        var a1 = Verify(mockPredicate, Times(4)).CouldReevaluationSucceed;
        VerifyNoMoreInteractions(mockPredicateFactory, mockPredicate);
    }


    [TestMethod]
    public void TestPreprocessPreprocessablePredicateFactory()
    {
        var kb = CreateKnowledgeBase();
        var limitTerm = ParseTerm("limit(3, test(a, b)).");
        var queryArg = limitTerm.GetArgument(1);
        var mockPreprocessablePredicateFactory = new MockPreprocessablePredicateFactory();
        var mockPredicateFactory = new MockPredicateFactory();
        var mockPredicate = new MockPredicate();
        var key = PredicateKey.CreateForTerm(queryArg);
        kb.Predicates.AddPredicateFactory(key, mockPreprocessablePredicateFactory);
        When(mockPreprocessablePredicateFactory.Preprocess(queryArg)).ThenReturn(mockPredicateFactory);
        When(mockPredicateFactory.GetPredicate(queryArg.Args)).ThenReturn(mockPredicate);
        When(mockPredicate.Evaluate()).ThenReturn(true);
        When(mockPredicate.CouldReevaluationSucceed).ThenReturn(true);

        var o = (Limit)kb.Predicates.GetPredicateFactory(limitTerm);
        var optimised = o.Preprocess(limitTerm);

        Assert.AreEqual("OptimisedLimit", optimised.GetType().Name);
        var queryArgs = new Term[] { limitTerm.GetArgument(0), limitTerm.GetArgument(1) };
        var p = optimised.GetPredicate(queryArgs);
        Assert.AreEqual("LimitPredicate", p.GetType().Name);
        Assert.AreNotSame(p, optimised.GetPredicate(queryArgs));

        Assert.IsTrue(p.CouldReevaluationSucceed);
        Assert.IsTrue(p.Evaluate());
        Assert.IsFalse(p.CouldReevaluationSucceed);
        Assert.IsFalse(p.Evaluate());
        Assert.IsFalse(p.CouldReevaluationSucceed);
        Assert.IsFalse(p.Evaluate());
        Assert.IsFalse(p.CouldReevaluationSucceed);
        Assert.IsFalse(p.Evaluate());

        Verify(mockPreprocessablePredicateFactory).Preprocess(queryArg);
        Verify(mockPredicateFactory, Times(2)).GetPredicate(queryArg.Args);
        Verify(mockPredicate, Times(3)).Evaluate();
        var a1 = Verify(mockPredicate, Times(4)).CouldReevaluationSucceed;
        VerifyNoMoreInteractions(mockPreprocessablePredicateFactory, mockPredicateFactory, mockPredicate);
    }
}
