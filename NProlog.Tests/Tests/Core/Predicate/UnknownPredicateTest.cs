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
using Org.NProlog.Core.Predicate.Udp;
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Predicate;

[TestClass]
public class UnknownPredicateTest : TestUtils
{
    private const string FUNCTOR = "UnknownPredicateTest";

    [TestMethod]
    public void TestUnknownPredicate()
    {
        var kb = CreateKnowledgeBase();
        var key = new PredicateKey(FUNCTOR, 1);

        // create UnknownPredicate for a not-yet-defined UnknownPredicateTest/1 predicate
        var e = new UnknownPredicate(kb, key);
        Assert.IsTrue(e.IsRetryable);

        // assert that FAIL returned when UnknownPredicateTest/1 not yet defined
        Assert.AreSame(PredicateUtils.FALSE, e.GetPredicate(new Term[] { Variable() }));

        // define UnknownPredicateTest/1
        kb.Predicates.CreateOrReturnUserDefinedPredicate(key);

        // assert that new InterpretedUserDefinedPredicate is returned once UnknownPredicateTest/1 defined
        Assert.AreSame(typeof(InterpretedUserDefinedPredicate), e.GetPredicate(new Term[] { Variable() }).GetType());
        Assert.AreNotSame(e.GetPredicate(new Term[] { Variable() }), e.GetPredicate(new Term[] { Variable() }));
    }

    [TestMethod]
    public void TestPreprocessStillUnknown()
    {
        var kb = CreateKnowledgeBase();
        var key = new PredicateKey(FUNCTOR, 1);

        // create UnknownPredicate for a not-yet-defined predicate
        var original = new UnknownPredicate(kb, key);

        var result = original.Preprocess(Terms.Structure.CreateStructure(FUNCTOR, new Term[] { new Atom("a") }));

        Assert.AreSame(original, result);
    }

    [TestMethod]
    public void TestPreprocessNotPreprocessablePredicateFactory()
    {
        var kb = CreateKnowledgeBase();
        var key = new PredicateKey(FUNCTOR, 1);

        // create UnknownPredicate for a predicate represented by a mock PredicateFactory (note not a PreprocessablePredicateFactory)
        var original = new UnknownPredicate(kb, key);
        var mockPredicateFactory = new MockPredicateFactory();
        kb.Predicates.AddPredicateFactory(key, mockPredicateFactory);

        var result = original.Preprocess(Terms.Structure.CreateStructure(FUNCTOR, new Term[] { new Atom("a") }));

        Assert.AreSame(mockPredicateFactory, result);
        VerifyNoInteractions(mockPredicateFactory);
    }

    [TestMethod]
    public void TestPreprocessPreprocessablePredicateFactory()
    {
        var kb = CreateKnowledgeBase();
        var key = new PredicateKey(FUNCTOR, 1);

        // create UnknownPredicate for a predicate represented by a mock PreprocessablePredicateFactory
        var original = new UnknownPredicate(kb, key);
        var mockPreprocessablePredicateFactory = new MockPreprocessablePredicateFactory();
        kb.Predicates.AddPredicateFactory(key, mockPreprocessablePredicateFactory);
        var mockPredicateFactory = new MockPredicateFactory();
        var arg = Terms.Structure.CreateStructure(FUNCTOR, new Term[] { new Atom("a") });
        When(mockPreprocessablePredicateFactory.Preprocess(arg)).ThenReturn(mockPredicateFactory);

        var result = original.Preprocess(Terms.Structure.CreateStructure(FUNCTOR, new Term[] { new Atom("a") }));

        Assert.AreEqual(mockPreprocessablePredicateFactory, result);
        Verify(mockPreprocessablePredicateFactory).Preprocess(arg);
        VerifyNoMoreInteractions(mockPreprocessablePredicateFactory, mockPredicateFactory);
    }
}
