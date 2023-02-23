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
        var arg1 = ParseTerm("[a]");
        var arg2 = ParseTerm("[a,b,c]");
        var singleResultPredicate = FACTORY.GetPredicate(new Term[] { arg1, arg2 });

        Assert.IsFalse(singleResultPredicate.CouldReevaluationSucceed);
        Assert.IsTrue(singleResultPredicate.Evaluate());
    }

    [TestMethod]
    public void TestMultiResultQuery()
    {
        var arg1 = ParseTerm("X");
        var arg2 = ParseTerm("[a,b,c]");
        var multiResultPredicate = FACTORY.GetPredicate(new Term[] { arg1, arg2 });

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
        var kb = CreateKnowledgeBase();
        var clauses = CreateClauseModels(firstClauseSyntax, secondClauseSyntax);
        var metaData = TailRecursivePredicateMetaData.Create(kb, clauses);
        return new InterpretedTailRecursivePredicateFactory(kb, metaData);
    }

    private static List<ClauseModel> CreateClauseModels(string firstClauseSyntax, string secondClauseSyntax) => new()
        {
            CreateClauseModel(firstClauseSyntax),
            CreateClauseModel(secondClauseSyntax)
        };
}
