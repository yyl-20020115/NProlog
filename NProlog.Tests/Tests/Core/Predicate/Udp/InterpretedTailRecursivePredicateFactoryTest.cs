/*
 * Copyright 2013-2014 S. Webber
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

namespace Org.NProlog.Core.Predicate.Udp;

[TestClass]
public class InterpretedTailRecursivePredicateFactoryTest : TestUtils
{
    private readonly InterpretedTailRecursivePredicateFactory FACTORY = CreateFactory("prefix([],Ys).", "prefix([X|Xs],[X|Ys]) :- prefix(Xs,Ys).");

    [TestMethod]
    public void TestSingleResultQuery()
    {
        Term arg1 = ParseTerm("[a]");
        Term arg2 = ParseTerm("[a,b,c]");
        InterpretedTailRecursivePredicate singleResultPredicate = FACTORY.GetPredicate(new Term[] { arg1, arg2 });

        Assert.IsFalse(singleResultPredicate.CouldReevaluationSucceed);
        Assert.IsTrue(singleResultPredicate.Evaluate());
    }

    [TestMethod]
    public void TestMultiResultQuery()
    {
        Term arg1 = ParseTerm("X");
        Term arg2 = ParseTerm("[a,b,c]");
        InterpretedTailRecursivePredicate multiResultPredicate = FACTORY.GetPredicate(new Term[] { arg1, arg2 });

        Assert.IsTrue(multiResultPredicate.CouldReevaluationSucceed);
        Assert.IsTrue(multiResultPredicate.Evaluate());
        Assert.AreEqual("[]", Write(arg1));
        Assert.IsTrue(multiResultPredicate.Evaluate());
        Assert.AreEqual("[a]", Write(arg1));
        Assert.IsTrue(multiResultPredicate.Evaluate());
        Assert.AreEqual("[a,b]", Write(arg1));
        Assert.IsTrue(multiResultPredicate.Evaluate());
        Assert.AreEqual("[a,b,c]", Write(arg1));
        Assert.IsFalse(multiResultPredicate.Evaluate());
    }

    private static InterpretedTailRecursivePredicateFactory CreateFactory(string firstClauseSyntax, string secondClauseSyntax)
    {
        KnowledgeBase kb = CreateKnowledgeBase();
        List<ClauseModel> clauses = CreateClauseModels(firstClauseSyntax, secondClauseSyntax);
        TailRecursivePredicateMetaData metaData = TailRecursivePredicateMetaData.Create(kb, clauses);
        return new InterpretedTailRecursivePredicateFactory(kb, metaData);
    }

    private static List<ClauseModel> CreateClauseModels(string firstClauseSyntax, string secondClauseSyntax)
    {
        List<ClauseModel> clauses = new();
        clauses.Add(CreateClauseModel(firstClauseSyntax));
        clauses.Add(CreateClauseModel(secondClauseSyntax));
        return clauses;
    }
}
