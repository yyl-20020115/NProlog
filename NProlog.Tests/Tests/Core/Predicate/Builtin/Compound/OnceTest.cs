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
using Org.NProlog.Core.Predicate.Udp;
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Predicate.Builtin.Compound;

[TestClass]
public class OnceTest : TestUtils
{
    [TestMethod]
    public void TestPreprocessCannotOptimiseVariable()
    {
        var o = new Once();

        var t = Terms.Structure.CreateStructure("once", new Term[] { new Variable("X") });
        var optimised = o.Preprocess(t);

        Assert.AreSame(o, optimised);
    }

    [TestMethod]
    public void TestPreprocessNotPreprocessablePredicateFactory()
    {
        var kb = CreateKnowledgeBase();
        var onceTerm = ParseTerm("once(test(a, b)).");
        var queryArg = onceTerm.GetArgument(0);
        // note not a PreprocessablePredicateFactory
        var mockPredicateFactory = new MockPredicateFactory();
        var mockPredicate = new MockPredicate();
        var key = PredicateKey.CreateForTerm(queryArg);
        kb.Predicates.AddPredicateFactory(key, mockPredicateFactory);
        When(mockPredicateFactory.GetPredicate(queryArg.Args)).ThenReturn(mockPredicate);
        When(mockPredicate.Evaluate()).ThenReturn(true, false, true);

        var o = (Once)kb.Predicates.GetPredicateFactory(onceTerm);
        var optimised = o.Preprocess(onceTerm);

        Assert.AreEqual("OptimisedOnce", optimised.GetType().Name);
        Assert.AreSame(PredicateUtils.TRUE, optimised.GetPredicate(new Term[] { queryArg }));
        Assert.AreSame(PredicateUtils.TRUE, optimised.GetPredicate(new Term[] { queryArg }));
        Assert.AreSame(PredicateUtils.TRUE, optimised.GetPredicate(new Term[] { queryArg }));

        Verify(mockPredicateFactory, Times(3)).GetPredicate(queryArg.Args);
        Verify(mockPredicate, Times(3)).Evaluate();
        VerifyNoMoreInteractions(mockPredicateFactory, mockPredicate);
    }

    [TestMethod]
    public void TestPreprocessPreprocessablePredicateFactory()
    {
        var kb = CreateKnowledgeBase();
        var onceTerm = ParseTerm("once(test(a, b)).");
        var queryArg = onceTerm.GetArgument(0);
        var mockPreprocessablePredicateFactory = new MockPreprocessablePredicateFactory();
        var mockPredicateFactory = new MockPredicateFactory();
        var mockPredicate = new MockPredicate();
        var key = PredicateKey.CreateForTerm(queryArg);
        kb.Predicates.AddPredicateFactory(key, mockPreprocessablePredicateFactory);
        When(mockPreprocessablePredicateFactory?.Preprocess(queryArg)).ThenReturn(mockPredicateFactory);
        When(mockPredicateFactory?.GetPredicate(queryArg.Args)).ThenReturn(mockPredicate);
        When(mockPredicate?.Evaluate()).ThenReturn(true, false, true);

        var o = (Once)kb.Predicates.GetPredicateFactory(onceTerm);
        var optimised = o.Preprocess(onceTerm);

        Assert.AreEqual("OptimisedOnce", optimised.GetType().Name);
        Assert.AreSame(PredicateUtils.TRUE, optimised.GetPredicate(new Term[] { queryArg }));
        Assert.AreSame(PredicateUtils.TRUE, optimised.GetPredicate(new Term[] { queryArg }));
        Assert.AreSame(PredicateUtils.TRUE, optimised.GetPredicate(new Term[] { queryArg }));

        Verify(mockPreprocessablePredicateFactory)?.Preprocess(queryArg);
        Verify(mockPredicateFactory, Times(3))?.GetPredicate(queryArg.Args);
        Verify(mockPredicate, Times(3))?.Evaluate();
        VerifyNoMoreInteractions(mockPreprocessablePredicateFactory, mockPredicateFactory, mockPredicate);
    }
}
