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
public class ConjunctionTest : TestUtils
{
    [TestMethod]
    public void TestPreprocessCannotOptimiseWhenBothArgumentsAreVariables()
    {
        KnowledgeBase kb = CreateKnowledgeBase();
        Term term = ParseTerm("X, Y.");
        Conjunction c = (Conjunction)kb.Predicates.GetPredicateFactory(term);
        PredicateFactory optimised = c.Preprocess(term);

        Assert.AreSame(c, optimised);
    }

    [TestMethod]
    public void TestPreprocessCannotOptimiseWhenFirstArgumentIsVariable()
    {
        KnowledgeBase kb = CreateKnowledgeBase();
        Term term = ParseTerm("X, true.");
        Conjunction c = (Conjunction)kb.Predicates.GetPredicateFactory(term);
        PredicateFactory optimised = c.Preprocess(term);

        Assert.AreSame(c, optimised);
    }

    [TestMethod]
    public void TestPreprocessCannotOptimiseWhenSecondArgumentIsVariable()
    {
        KnowledgeBase kb = CreateKnowledgeBase();
        Term term = ParseTerm("true, Y.");
        Conjunction c = (Conjunction)kb.Predicates.GetPredicateFactory(term);
        PredicateFactory optimised = c.Preprocess(term);

        Assert.AreSame(c, optimised);
    }

    [TestMethod]
    public void TestPreprocessOptimisedSingletonConjuction()
    {
        KnowledgeBase kb = CreateKnowledgeBase();
        Term term = ParseTerm("true, true.");
        Conjunction c = (Conjunction)kb.Predicates.GetPredicateFactory(term);
        PredicateFactory optimised = c.Preprocess(term);

        Assert.AreEqual("OptimisedSingletonConjuction", optimised.GetType().Name);
        Assert.IsFalse(optimised.IsRetryable);
        Assert.AreSame(PredicateUtils.TRUE, optimised.GetPredicate(term.Args));
    }

    [TestMethod]
    public void TestPreprocessOptimisedSingletonConjuctionWithVariables()
    {
        KnowledgeBase kb = CreateKnowledgeBase();
        Term term = ParseTerm("X=6, \\+ atom(X).");
        Conjunction c = (Conjunction)kb.Predicates.GetPredicateFactory(term);
        PredicateFactory optimised = c.Preprocess(term);

        Assert.AreEqual("OptimisedSingletonConjuction", optimised.GetType().Name);
        Assert.IsFalse(optimised.IsRetryable);
        Assert.IsFalse(optimised.IsAlwaysCutOnBacktrack);
        Dictionary<Variable, Variable> sharedVariables = new();
        Term Copy = term.Copy(sharedVariables);
        Assert.AreSame(PredicateUtils.TRUE, optimised.GetPredicate(Copy.Args));
        Variable variable = sharedVariables.Values.First();
        // confirm the Backtrack implemented by Not did not unassign X
        Assert.AreEqual("X", variable.Id);
        Assert.AreEqual(new IntegerNumber(6), variable.Term);
    }

    [TestMethod]
    public void TestPreprocessFirstArgumentRetryable()
    {
        KnowledgeBase kb = CreateKnowledgeBase();
        Term term = ParseTerm("repeat(2), true.");
        Conjunction c = (Conjunction)kb.Predicates.GetPredicateFactory(term);
        PredicateFactory optimised = c.Preprocess(term);
        Predicate predicate = optimised.GetPredicate(term.Args);

        Assert.AreEqual("OptimisedRetryableConjuction", optimised.GetType().Name);
        Assert.IsTrue(optimised.IsRetryable);
        Assert.IsFalse(optimised.IsAlwaysCutOnBacktrack);
        Assert.AreEqual("ConjunctionPredicate", predicate.GetType().Name);
        Assert.IsTrue(predicate.CouldReevaluationSucceed);
        Assert.IsTrue(predicate.Evaluate());
        Assert.IsTrue(predicate.CouldReevaluationSucceed);
        Assert.IsTrue(predicate.Evaluate());
        Assert.IsFalse(predicate.CouldReevaluationSucceed);
        Assert.IsFalse(predicate.Evaluate());
    }

    [TestMethod]
    public void TestPreprocessFirstArgumentRetryableWithVariable()
    {
        KnowledgeBase kb = CreateKnowledgeBase();
        TermFormatter tf = kb.TermFormatter;
        Term term = ParseTerm("member(X, [a,b]), Y=X.");
        Conjunction c = (Conjunction)kb.Predicates.GetPredicateFactory(term);
        PredicateFactory optimised = c.Preprocess(term);
        Predicate predicate = optimised.GetPredicate(term.Args);

        Assert.AreEqual("OptimisedRetryableConjuction", optimised.GetType().Name);
        Assert.IsTrue(optimised.IsRetryable);
        Assert.AreEqual("ConjunctionPredicate", predicate.GetType().Name);
        Assert.IsTrue(predicate.CouldReevaluationSucceed);
        Assert.IsTrue(predicate.Evaluate());
        Assert.AreEqual("member(a, [a,b]) , a = a", tf.FormatTerm(term));
        Assert.IsTrue(predicate.CouldReevaluationSucceed);
        Assert.IsTrue(predicate.Evaluate());
        Assert.AreEqual("member(b, [a,b]) , b = b", tf.FormatTerm(term));
        Assert.IsFalse(predicate.CouldReevaluationSucceed);
        Assert.IsFalse(predicate.Evaluate());
    }

    [TestMethod]
    public void TestPreprocessSecondArgumentRetryable()
    {
        KnowledgeBase kb = CreateKnowledgeBase();
        Term term = ParseTerm("true, repeat(2).");
        Conjunction c = (Conjunction)kb.Predicates.GetPredicateFactory(term);
        PredicateFactory optimised = c.Preprocess(term);
        Predicate predicate = optimised.GetPredicate(term.Args);

        Assert.AreEqual("OptimisedRetryableConjuction", optimised.GetType().Name);
        Assert.IsTrue(optimised.IsRetryable);
        Assert.AreEqual("ConjunctionPredicate", predicate.GetType().Name);
        Assert.IsTrue(predicate.CouldReevaluationSucceed);
        Assert.IsTrue(predicate.Evaluate());
        Assert.IsTrue(predicate.CouldReevaluationSucceed);
        Assert.IsTrue(predicate.Evaluate());
        Assert.IsFalse(predicate.CouldReevaluationSucceed);
        Assert.IsFalse(predicate.Evaluate());
    }

    [TestMethod]
    public void TestPreprocessBothArgumentsRetryable()
    {
        KnowledgeBase kb = CreateKnowledgeBase();
        Term term = ParseTerm("member(X, [2,3]), repeat(X).");
        Conjunction c = (Conjunction)kb.Predicates.GetPredicateFactory(term);
        PredicateFactory optimised = c.Preprocess(term);
        Predicate predicate = optimised.GetPredicate(term.Args);

        Assert.AreEqual("OptimisedRetryableConjuction", optimised.GetType().Name);
        Assert.IsTrue(optimised.IsRetryable);
        Assert.AreEqual("ConjunctionPredicate", predicate.GetType().Name);
        Assert.IsTrue(predicate.CouldReevaluationSucceed);
        Assert.IsTrue(predicate.Evaluate());
        Assert.IsTrue(predicate.CouldReevaluationSucceed);
        Assert.IsTrue(predicate.Evaluate());
        Assert.IsTrue(predicate.CouldReevaluationSucceed);
        Assert.IsTrue(predicate.Evaluate());
        Assert.IsTrue(predicate.CouldReevaluationSucceed);
        Assert.IsTrue(predicate.Evaluate());
        Assert.IsTrue(predicate.CouldReevaluationSucceed);
        Assert.IsTrue(predicate.Evaluate());
        Assert.IsFalse(predicate.CouldReevaluationSucceed);
        Assert.IsFalse(predicate.Evaluate());
    }

    string[] vs1 = {
               "!,true.",
               "true,!.",
               "!,true,!.",
               "repeat,!.",
               "repeat,!,true.",
               "!,repeat,!.",};
    [TestMethod]
    public void TestIsAlwaysCutOnBacktrackTrue()
    {
        foreach(var clause in vs1)
        {
            var kb = CreateKnowledgeBase();
            var term = ParseTerm(clause);
            var c = (Conjunction)kb.Predicates.GetPredicateFactory(term);
            var optimised = c.Preprocess(term);
            Assert.IsFalse(optimised.IsAlwaysCutOnBacktrack);
        }
    }

    string[] vs2 = {
               "true,true.",
               "repeat,repeat.",
               "true,repeat.",
               "repeat,true.",
               "!,repeat.",
               "true,!,repeat.",
               "!,true,!,repeat.",
               "repeat,!,repeat.",
               "!,repeat,!,repeat.",};
    [TestMethod]
    public void TestIsAlwaysCutOnBacktrackFalse()
    {
        foreach(var clause in vs2)
        {
            var kb = CreateKnowledgeBase();
            var term = ParseTerm(clause);
            var c = (Conjunction)kb.Predicates.GetPredicateFactory(term);
            var optimised = c.Preprocess(term);
            Assert.IsFalse(optimised.IsAlwaysCutOnBacktrack);
        }
    }
}
