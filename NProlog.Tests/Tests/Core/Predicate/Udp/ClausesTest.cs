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

namespace Org.NProlog.Core.Predicate.Udp;

[TestClass]
public class ClausesTest : TestUtils
{
    [TestMethod]
    public void TestEmpty()
    {
        var c = Clauses.CreateFromModels(TestUtils.CreateKnowledgeBase(), new());
        Assert.AreEqual(0, c.ClauseActions.Length); // TODO use assertEmpty
        Assert.AreEqual(0, c.ImmutableColumns.Length); // TODO use assertEmpty
    }

    [TestMethod]
    public void TestSingleNoArgClause()
    {
        var c = CreateClauses("p.");
        AssertEmpty(c.ImmutableColumns);
    }

    [TestMethod]
    public void TestSingleOneArgImmutableClause()
    {
        var c = CreateClauses("p(x).");
        AssertArrayEquals(new int[] { 0 }, c.ImmutableColumns);
    }

    [TestMethod]
    public void TestSingleTwoArgImmutableClause()
    {
        var c = CreateClauses("p(x,y).");
        AssertArrayEquals(new int[] { 0, 1 }, c.ImmutableColumns);
    }

    [TestMethod]
    public void TestSingleNineArgImmutableClause()
    {
        var c = CreateClauses("p(a,b,c,d,e,f,g,h,i).");
        AssertArrayEquals(new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }, c.ImmutableColumns);
    }

    [TestMethod]
    public void TestSingleOneArgMutableClause()
    {
        var c = CreateClauses("p(X).");
        AssertEmpty(c.ImmutableColumns);
    }

    [TestMethod]
    public void TestSingleTwoArgMutableClause()
    {
        var c = CreateClauses("p(X,y).");
        AssertArrayEquals(new int[] { 1 }, c.ImmutableColumns);
    }


    [TestMethod]
    public void TestSingleManyArgsMutableClause()
    {
        var c = CreateClauses("p(a,b,X,Y,e,f,g,h,i,Z,k,l).");
        AssertArrayEquals(new int[] { 0, 1, 4, 5, 6, 7, 8, 10, 11 }, c.ImmutableColumns);
    }

    [TestMethod]
    public void TestManyImmutableClauses()
    {
        var c = CreateClauses("p(a,b,c).", "p(d,e,f).", "p(g,h,i).");
        AssertArrayEquals(new int[] { 0, 1, 2 }, c.ImmutableColumns);
    }

    [TestMethod]
    public void TestManyImmutableClausesWithAntecedant()
    {
        var c = CreateClauses("p(a,b,c).", "p(d,e,f) :- x(z).", "p(g,h,i).");
        AssertArrayEquals(new int[] { 0, 1, 2 }, c.ImmutableColumns);
    }

    [TestMethod]
    public void TestManyMutableClausesWithoutIndexableArgs()
    {
        var c = CreateClauses("p(a,X,c).", "p(Y,e,f).", "p(g,h,Z).");
        AssertEmpty(c.ImmutableColumns);
    }

    [TestMethod]
    public void TestManyMutableClausesWithIndexableArgs()
    {
        var c = CreateClauses("p(a,X,c,d,e,f).", "p(Y,o,e,f,Q,r).", "p(g,h,e,u,p,Z).");
        AssertArrayEquals(new int[] { 2, 3 }, c.ImmutableColumns);
    }

    [TestMethod]
    public void TestManyMutableClausesWithoutAntecedantsWithIndexableArgs()
    {
        var c = CreateClauses("p(a,X,c,d,e,f).", "p(Y,o,e,f,Q,r).", "p(g,h,e,u,p,Z).");
        AssertArrayEquals(new int[] { 2, 3 }, c.ImmutableColumns);
    }

    [TestMethod]
    public void TestSingleImmutableRule()
    {
        var c = CreateClauses("p(x) :- z(a,b,c).");
        AssertArrayEquals(new int[] { 0 }, c.ImmutableColumns);
    }

    [TestMethod]
    public void TestSingleAlwaysMatchedRule()
    {
        var c = CreateClauses("p(X) :- z(a,b,c).");
        AssertEmpty(c.ImmutableColumns);
    }

    [TestMethod]
    public void TestClauseActions()
    {
        var c = CreateClauses("p(x,y).", "p(X,Y) :- a(X), b(Y).");
        var actions = c.ClauseActions;
        Assert.AreEqual(2, actions.Length);
        Assert.AreEqual("p(x, y)", actions[0].Model.Original.ToString());
        Assert.AreEqual(":-(p(X, Y), ,(a(X), b(Y)))", actions[1].Model.Original.ToString());
    }

    private static Clauses CreateClauses(params string[] clauses)
    { // TODO move to TestUtils
        var kb = CreateKnowledgeBase();
        List<ClauseModel> models = new();
        foreach (var clause in clauses)
        {
            models.Add(CreateClauseModel(clause));
        }
        return Clauses.CreateFromModels(kb, models);
    }

    private static void AssertEmpty(int[] array) => Assert.AreEqual(0, array.Length);
}
