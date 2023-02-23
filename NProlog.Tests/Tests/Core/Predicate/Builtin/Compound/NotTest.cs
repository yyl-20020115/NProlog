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
using Org.NProlog.Core.Predicate.Udp;
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Predicate.Builtin.Compound;

[TestClass]
public class NotTest : TestUtils
{
    [TestMethod]
    public void TestBacktrackOnSuccess()
    {
        var kb = CreateKnowledgeBase();
        var term = ParseTerm("not((X=4, X<3)).");
        var n = (Not)kb.Predicates.GetPredicateFactory(term);

        Dictionary<Variable, Variable> sharedVariables = new();
        var Copy = term.Copy(sharedVariables);
        Assert.AreSame(PredicateUtils.TRUE, n.GetPredicate(Copy.Args));
        var variable = sharedVariables.Values.First();
        // confirm the Backtrack implemented by Not did not unassign X
        Assert.AreEqual("X", variable.Id);
        Assert.AreEqual(TermType.VARIABLE, variable.Type);
    }

    [TestMethod]
    public void TestPreprocessCannotOptimiseVariable()
    {
        var kb = CreateKnowledgeBase();
        var term = ParseTerm("not(X).");
        var n = (Not)kb.Predicates.GetPredicateFactory(term);

        var optimised = n.Preprocess(term);

        Assert.AreSame(n, optimised);
    }

    [TestMethod]
    public void TestPreprocessNotPreprocessablePredicateFactory()
    {
        var kb = CreateKnowledgeBase();
        var notTerm = ParseTerm("not(test(a, b)).");
        var queryArg = notTerm.GetArgument(0);
        // note not a PreprocessablePredicateFactory
        var mockPredicateFactory = new MockPredicateFactory();
        var mockPredicate = new MockPredicate();
        var key = PredicateKey.CreateForTerm(queryArg);
        kb.Predicates.AddPredicateFactory(key, mockPredicateFactory);
        When(mockPredicateFactory?.GetPredicate(queryArg.Args)).ThenReturn(mockPredicate);
        When(mockPredicate?.Evaluate()).ThenReturn(true, false, true);

        var n = (Not)kb.Predicates.GetPredicateFactory(notTerm);
        var optimised = n.Preprocess(notTerm);

        Assert.AreEqual("OptimisedNot", optimised.GetType().Name);
        Assert.AreSame(PredicateUtils.FALSE, optimised.GetPredicate(new Term[] { queryArg }));
        Assert.AreSame(PredicateUtils.FALSE, optimised.GetPredicate(new Term[] { queryArg }));
        Assert.AreSame(PredicateUtils.FALSE, optimised.GetPredicate(new Term[] { queryArg }));

        Verify(mockPredicateFactory, Times(3))?.GetPredicate(queryArg.Args);
        Verify(mockPredicate, Times(3))?.Evaluate();
        VerifyNoMoreInteractions(mockPredicateFactory, mockPredicate);
    }

    [TestMethod]
    public void TestPreprocessPreprocessablePredicateFactory()
    {
        var kb = CreateKnowledgeBase();
        var notTerm = ParseTerm("not(test(a, b)).");
        var queryArg = notTerm.GetArgument(0);
        var mockPreprocessablePredicateFactory = new MockPreprocessablePredicateFactory();
        var mockPredicateFactory = new MockPredicateFactory();
        var mockPredicate = new MockPredicate();
        var key = PredicateKey.CreateForTerm(queryArg);
        kb.Predicates.AddPredicateFactory(key, mockPreprocessablePredicateFactory);
        When(mockPreprocessablePredicateFactory.Preprocess(queryArg)).ThenReturn(mockPredicateFactory);
        When(mockPredicateFactory.GetPredicate(queryArg.Args)).ThenReturn(mockPredicate);
        When(mockPredicate.Evaluate()).ThenReturn(true, false, true);

        var n = (Not)kb.Predicates.GetPredicateFactory(notTerm);
        var optimised = n.Preprocess(notTerm);

        Assert.AreEqual("OptimisedNot", optimised.GetType().Name);
        Assert.AreSame(PredicateUtils.FALSE, optimised.GetPredicate(new Term[] { queryArg }));
        Assert.AreSame(PredicateUtils.FALSE, optimised.GetPredicate(new Term[] { queryArg }));
        Assert.AreSame(PredicateUtils.FALSE, optimised.GetPredicate(new Term[] { queryArg }));

        Verify(mockPreprocessablePredicateFactory)?.Preprocess(queryArg);
        Verify(mockPredicateFactory, Times(3))?.GetPredicate(queryArg.Args);
        Verify(mockPredicate, Times(3))?.Evaluate();
        VerifyNoMoreInteractions(mockPreprocessablePredicateFactory, mockPredicateFactory, mockPredicate);
    }
}
