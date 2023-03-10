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
using Org.NProlog.Core.Event;
using Org.NProlog.Core.IO;
using Org.NProlog.Core.Math;
using Org.NProlog.Core.Parser;
using Org.NProlog.Core.Predicate;
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Kb;

[TestClass]
public class KnowledgeBaseUtilsTest : TestUtils
{
    private readonly KnowledgeBase kb = CreateKnowledgeBase();

    [TestMethod]
    public void TestConjunctionPredicateName()
    {
        Assert.AreEqual(",", KnowledgeBaseUtils.CONJUNCTION_PREDICATE_NAME);
    }

    [TestMethod]
    public void TestImplicationPredicateName()
    {
        Assert.AreEqual(":-", KnowledgeBaseUtils.IMPLICATION_PREDICATE_NAME);
    }

    [TestMethod]
    public void TestQuestionPredicateName()
    {
        Assert.AreEqual("?-", KnowledgeBaseUtils.QUESTION_PREDICATE_NAME);
    }

    [TestMethod]
    public void TestGetPredicateKeysByName()
    {
        var predicateName = "testGetPredicateKeysByName";

        Assert.IsTrue(KnowledgeBaseUtils.GetPredicateKeysByName(kb, predicateName).Count == 0);

        PredicateKey[] input = { new PredicateKey(predicateName, 0), new PredicateKey(predicateName, 1), new PredicateKey(predicateName, 2), new PredicateKey(predicateName, 3) };

        foreach (var key in input)
        {
            kb.Predicates.CreateOrReturnUserDefinedPredicate(key);
            // Add entries with a different name to the name we are calling getPredicateKeysByName with
            // to check that the method isn't just returning ALL keys
            var keyWithDifferentName = new PredicateKey(predicateName + "X", key.NumArgs);
            kb.Predicates.CreateOrReturnUserDefinedPredicate(keyWithDifferentName);
        }

        var output = KnowledgeBaseUtils.GetPredicateKeysByName(kb, predicateName);
        Assert.AreEqual(input.Length, output.Count);
        foreach (var key in input)
        {
            Assert.IsTrue(output.Contains(key));
        }
    }

    [TestMethod]
    public void TestIsQuestionOrDirectiveFunctionCall()
    {
        Assert.IsTrue(KnowledgeBaseUtils.IsQuestionOrDirectiveFunctionCall(Structure("?-", Atom())));
        Assert.IsTrue(KnowledgeBaseUtils.IsQuestionOrDirectiveFunctionCall(Structure("?-", Structure("=", Atom(), Atom()))));
        Assert.IsTrue(KnowledgeBaseUtils.IsQuestionOrDirectiveFunctionCall(Structure(":-", Atom())));
        Assert.IsTrue(KnowledgeBaseUtils.IsQuestionOrDirectiveFunctionCall(Structure(":-", Structure("=", Atom(), Atom()))));

        Assert.IsFalse(KnowledgeBaseUtils.IsQuestionOrDirectiveFunctionCall(Atom("?-")));
        Assert.IsFalse(KnowledgeBaseUtils.IsQuestionOrDirectiveFunctionCall(Structure("?-", Atom(), Atom())));
        Assert.IsFalse(KnowledgeBaseUtils.IsQuestionOrDirectiveFunctionCall(Atom(":-")));
        Assert.IsFalse(KnowledgeBaseUtils.IsQuestionOrDirectiveFunctionCall(Structure(":-", Atom(), Atom())));
        Assert.IsFalse(KnowledgeBaseUtils.IsQuestionOrDirectiveFunctionCall(Structure(">=", Atom())));
    }

    [TestMethod]
    public void TestIsConjuction()
    {
        Assert.IsFalse(KnowledgeBaseUtils.IsConjunction(TestUtils.ParseSentence("true.")));
        Assert.IsTrue(KnowledgeBaseUtils.IsConjunction(TestUtils.ParseSentence("true, true.")));
        Assert.IsTrue(KnowledgeBaseUtils.IsConjunction(TestUtils.ParseSentence("true, true, true.")));
        Assert.IsTrue(KnowledgeBaseUtils.IsConjunction(TestUtils.ParseSentence("repeat(3), X<1, write(V), nl, true, !, fail.")));
    }

    [TestMethod]
    public void TestIsSingleAnswerNonRetryablePredicate()
    {
        // test single term representing a predicate that is not repeatable
        Assert.IsTrue(KnowledgeBaseUtils.IsSingleAnswer(kb, Atom("true")));
    }

    [TestMethod]
    public void TestIsSingleAnswerRetryablePredicate()
    {
        // test single term representing a predicate that *is* repeatable
        Assert.IsFalse(KnowledgeBaseUtils.IsSingleAnswer(kb, Atom("repeat")));
    }

    [TestMethod]
    public void TestIsSingleAnswerNonRetryableConjuction()
    {
        // test conjunction of terms that are all not repeatable
        var conjuctionOfNonRepeatableTerms = TestUtils.ParseSentence("write(X), nl, true, X<1.");
        Assert.IsTrue(KnowledgeBaseUtils.IsSingleAnswer(kb, conjuctionOfNonRepeatableTerms));
    }

    [TestMethod]
    public void TestIsSingleAnswerRetryableConjuction()
    {
        // test conjunction of terms where one is repeatable
        var conjuctionIncludingRepeatableTerm = TestUtils.ParseSentence("write(X), nl, repeat, true, X<1.");
        Assert.IsFalse(KnowledgeBaseUtils.IsSingleAnswer(kb, conjuctionIncludingRepeatableTerm));
    }

    [TestMethod]
    public void TestIsSingleAnswerDisjunction()
    {
        // test disjunction
        // (Note that the disjunction used in the test *would* only give a single answer
        // but KnowledgeBaseUtils.isSingleAnswer is not currently smart enough to spot this)
        var disjunctionOfTerms = TestUtils.ParseSentence("true ; fail.");
        Assert.IsFalse(KnowledgeBaseUtils.IsSingleAnswer(kb, disjunctionOfTerms));
    }

    [TestMethod]
    public void TestIsSingleAnswerVariable()
    {
        Assert.IsFalse(KnowledgeBaseUtils.IsSingleAnswer(kb, Variable()));
    }

    [TestMethod]
    public void TestToArrayOfConjunctions()
    {
        var t = ParseSentence("a, b(1,2,3), c.");
        var conjunctions = KnowledgeBaseUtils.ToArrayOfConjunctions(t);
        Assert.AreEqual(3, conjunctions.Length);
        Assert.AreSame(t.GetArgument(0).GetArgument(0), conjunctions[0]);
        Assert.AreSame(t.GetArgument(0).GetArgument(1), conjunctions[1]);
        Assert.AreSame(t.GetArgument(1), conjunctions[2]);
    }

    [TestMethod]
    public void TestToArrayOfConjunctionsNoneConjunctionArgument()
    {
        var t = ParseSentence("a(b(1,2,3), c).");
        var conjunctions = KnowledgeBaseUtils.ToArrayOfConjunctions(t);
        Assert.AreEqual(1, conjunctions.Length);
        Assert.AreSame(t, conjunctions[0]);
    }

    [TestMethod]
    public void TestGetPrologListeners()
    {
        var kb1 = CreateKnowledgeBase();
        var kb2 = CreateKnowledgeBase();
        var o1 = kb1.PrologListeners;
        var o2 = kb2.PrologListeners;
        Assert.IsNotNull(o1);
        Assert.IsNotNull(o2);
        Assert.AreNotSame(o1, o2);
        Assert.AreSame(o1, kb1.PrologListeners);
        Assert.AreSame(o2, kb2.PrologListeners);
    }

    [TestMethod]
    public void TestGetPrologProperties()
    {
        var kb1 = CreateKnowledgeBase();
        var kb2 = CreateKnowledgeBase();
        var o1 = kb1.PrologProperties;
        var o2 = kb2.PrologProperties;
        Assert.IsNotNull(o1);
        Assert.IsNotNull(o2);
        Assert.AreNotSame(o1, o2);
        Assert.AreSame(o1, kb1.PrologProperties);
        Assert.AreSame(o2, kb2.PrologProperties);
    }

    [TestMethod]
    public void TestGetOperands()
    {
        var kb1 = CreateKnowledgeBase();
        var kb2 = CreateKnowledgeBase();
        var o1 = kb1.Operands;
        var o2 = kb2.Operands;
        Assert.IsNotNull(o1);
        Assert.IsNotNull(o2);
        Assert.AreNotSame(o1, o2);
        Assert.AreSame(o1, kb1.Operands);
        Assert.AreSame(o2, kb2.Operands);
    }

    [TestMethod]
    public void TestGetTermFormatter()
    {
        var kb1 = CreateKnowledgeBase();
        var kb2 = CreateKnowledgeBase();
        var o1 = kb1.TermFormatter;
        var o2 = kb2.TermFormatter;
        Assert.IsNotNull(o1);
        Assert.IsNotNull(o2);
        Assert.AreNotSame(o1, o2);
        Assert.AreSame(o1, kb1.TermFormatter);
        Assert.AreSame(o2, kb2.TermFormatter);
    }

    [TestMethod]
    public void TestGetSpyPoints()
    {
        var kb1 = CreateKnowledgeBase();
        var kb2 = CreateKnowledgeBase();
        var o1 = kb1.SpyPoints;
        var o2 = kb2.SpyPoints;
        Assert.IsNotNull(o1);
        Assert.IsNotNull(o2);
        Assert.AreNotSame(o1, o2);
        Assert.AreSame(o1, kb1.SpyPoints);
        Assert.AreSame(o2, kb2.SpyPoints);
    }

    [TestMethod]
    public void TestGetFileHandles()
    {
        var kb1 = CreateKnowledgeBase();
        var kb2 = CreateKnowledgeBase();
        var o1 = kb1.FileHandles;
        var o2 = kb2.FileHandles;
        Assert.IsNotNull(o1);
        Assert.IsNotNull(o2);
        Assert.AreNotSame(o1, o2);
        Assert.AreSame(o1, kb1.FileHandles);
        Assert.AreSame(o2, kb2.FileHandles);
    }

    [TestMethod]
    public void TestArithmeticOperators()
    {
        var kb1 = CreateKnowledgeBase();
        var kb2 = CreateKnowledgeBase();
        var o1 = kb1.ArithmeticOperators;
        var o2 = kb2.ArithmeticOperators;
        Assert.IsNotNull(o1);
        Assert.IsNotNull(o2);
        Assert.AreNotSame(o1, o2);
        Assert.AreSame(o1, kb1.ArithmeticOperators);
        Assert.AreSame(o2, kb2.ArithmeticOperators);
    }
}
