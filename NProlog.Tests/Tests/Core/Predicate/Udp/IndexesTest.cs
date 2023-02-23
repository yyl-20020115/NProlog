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
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Predicate.Udp;

[TestClass]
public class IndexesTest : TestUtils
{
    [TestMethod]
    public void TestAllPermutationsOfThreeArgs()
    {
        var a = Atom("a");
        var b = Atom("b");
        var c = Atom("c");
        var d = Atom("d");
        var clauses = CreateClauses("p(a,b,c).", "p(a,c,b).", "p(b,c,a).", "p(a,d,c).");
        var first = clauses.ClauseActions[0];
        var second = clauses.ClauseActions[1];
        var third = clauses.ClauseActions[2];
        var fourth = clauses.ClauseActions[3];

        var indexes = new Indexes(clauses);
        Assert.AreEqual(0, indexes.CountReferences());

        // 1 arg indexes
        AssertMatches(indexes, Array(a, CreateVariable(), CreateVariable()), first, second, fourth);
        AssertMatches(indexes, Array(CreateVariable(), c, CreateVariable()), second, third);
        AssertMatches(indexes, Array(CreateVariable(), CreateVariable(), a), third);
        AssertNoMatches(indexes, Array(CreateVariable(), CreateVariable(), d));

        // 2 arg indexes
        AssertMatches(indexes, Array(a, b, CreateVariable()), first);
        AssertMatches(indexes, Array(a, CreateVariable(), c), first, fourth);
        AssertMatches(indexes, Array(CreateVariable(), c, a), third);
        AssertNoMatches(indexes, Array(b, a, CreateVariable()));

        // 3 arg indexes
        AssertMatches(indexes, Array(a, b, c), first);
        AssertMatches(indexes, Array(a, c, b), second);
        AssertMatches(indexes, Array(b, c, a), third);
        AssertMatches(indexes, Array(a, d, c), fourth);
        AssertNoMatches(indexes, Array(c, a, b));

        Assert.AreEqual(7, indexes.CountReferences());
    }

    [TestMethod]
    public void TestMaxThreeArgsPerIndex()
    {
        var a = Atom("a");
        var b = Atom("b");
        var c = Atom("c");
        var d = Atom("d");
        var e = Atom("e");
        var clauses = CreateClauses("p(a,b,c,d,e).", "p(a,b,c,d,f).", "p(a,b,c,f,g).", "p(a,b,f,g,h).");
        var first = clauses.ClauseActions[0];
        var second = clauses.ClauseActions[1];
        var third = clauses.ClauseActions[2];

        var indexes = new Indexes(clauses);

        // a maximum of the first 3 immutable args will be used in the index
        AssertMatches(indexes, Array(a, b, c, d, e), first, second, third);
        AssertMatches(indexes, Array(CreateVariable(), b, c, d, e), first, second);
        AssertMatches(indexes, Array(CreateVariable(), CreateVariable(), c, d, e), first);
    }

    [TestMethod]
    public void TestMaxNineArgsIndexable()
    {
        var a = Atom("a");
        var b = Atom("b");
        var c = Atom("c");
        var d = Atom("d");
        var e = Atom("e");
        var f = Atom("f");
        var g = Atom("g");
        var h = Atom("h");
        var i = Atom("i");
        var j = Atom("j");
        var clauses = CreateClauses("p(a,b,c,d,e,f,g,h,i,j).", "p(q,w,e,r,t,y,u,i,o,p).", "p(z,x,k,v,b,n,m,a,s,d).");
        var first = clauses.ClauseActions[0];
        var second = clauses.ClauseActions[1];
        var third = clauses.ClauseActions[2];

        var indexes = new Indexes(clauses);

        // a maximum of the first 9 immutable args will be considered for use in the index
        AssertMatches(indexes, Array(a, CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable()), first);
        AssertMatches(indexes, Array(CreateVariable(), b, CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable()), first);
        AssertMatches(indexes, Array(CreateVariable(), CreateVariable(), c, CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable()), first);
        AssertMatches(indexes, Array(CreateVariable(), CreateVariable(), CreateVariable(), d, CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable()), first);
        AssertMatches(indexes, Array(CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), e, CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable()), first);
        AssertMatches(indexes, Array(CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), f, CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable()), first);
        AssertMatches(indexes, Array(CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), g, CreateVariable(), CreateVariable(), CreateVariable()), first);
        AssertMatches(indexes, Array(CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), h, CreateVariable(), CreateVariable()), first);
        AssertMatches(indexes, Array(CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), i, CreateVariable()), first);
        // expect all three clauses to be returned as readonly argument will not be used in the index
        AssertMatches(indexes, Array(CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), j), first, second, third);
        AssertMatches(indexes, Array(CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), IntegerNumber(9)), first, second, third);
    }

    [TestMethod]
    public void TestMuttableArgsNotIncludeInMaxLimits()
    {
        var a = Atom("a");
        var b = Atom("b");
        var c = Atom("c");
        var d = Atom("d");
        var q = Atom("q");
        var k = Atom("k");
        var j = Atom("j");
        var x = Atom("x");
        var z = Atom("z");
        // 3rd and 8th args not considered indexable as sometimes mutable
        var clauses = CreateClauses("p(z,x,X,v,b,n,m,h,s,d,e,q).", "p(a,b,c,d,e,f,g,p(Y),i,j,k,l).", "p(a,b,c,z,e,f,g,h,i,j,x,y).");
        var first = clauses.ClauseActions[0];
        var second = clauses.ClauseActions[1];
        var third = clauses.ClauseActions[2];

        var indexes = new Indexes(clauses);
        // 4th argument ("d") will be included in composite index.
        // This is dispite there being a 3 arg max limit in the number of args per index.
        // This is because the third argument is not considered for indexing as one of the clauses has
        // a mutable term (a variable named "X") in that position.
        AssertMatches(indexes, Array(a, b, c, d, CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable()), second);
        AssertMatches(indexes, Array(a, b, q, d, CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable()), second);
        AssertMatches(indexes, Array(a, b, c, z, CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable()), third);
        AssertMatches(indexes, Array(a, b, q, z, CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable()), third);
        // 3rd and 8th args not indexed as a clause has a mutable term in that position.
        AssertMatches(indexes, Array(CreateVariable(), CreateVariable(), c, CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable()), first, second, third);
        AssertMatches(indexes, Array(CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), z, CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable()), first, second, third);
        // 10th and 11th arg considered for indexing as within max of 9 mutable terms considered for indexing.
        AssertMatches(indexes, Array(CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), j, CreateVariable(), CreateVariable()), second, third);
        AssertMatches(indexes, Array(CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), k, CreateVariable()), second);
        AssertMatches(indexes, Array(CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), x, CreateVariable()), third);
        // 12th arg not used in index as only max of 9 mutable terms considered for indexing.
        AssertMatches(indexes, Array(CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), z), first, second, third);
    }

    [TestMethod]
    public void TestAllPermutationsOfLargeData()
    {
        int numClauses = 1000;
        int numArgs = 9;
        List<ClauseModel> models = new(numClauses);
        for (int i = 0; i < numClauses; i++)
        {
            Term[] args = new Term[numArgs];
            for (int i2 = 0; i2 < args.Length; i2++)
            {
                args[i2] = IntegerNumber(i + i2);
            }
            models.Add(ClauseModel.CreateClauseModel(Structure("p", args)));
        }
        var clauses = Clauses.CreateFromModels(TestUtils.CreateKnowledgeBase(), models);

        var indexes = new Indexes(clauses);

        //TODO:
        //Collections.shuffle(models);

        var itr = models.GetEnumerator();
        for (int i = 0; i < numArgs; i++)
        {
            itr.MoveNext();
            var expected = itr.Current;
            var args = Array(CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable());
            args[i] = expected.Consequent.GetArgument(i);
            var matches = indexes.Index(args);
            Assert.AreEqual(1, matches.Length);
            Assert.AreSame(expected, matches[0].Model);
        }

        for (int i1 = 0; i1 < numArgs; i1++)
        {
            for (int i2 = i1 + 1; i2 < numArgs; i2++)
            {
                itr.MoveNext();
                var expected = itr.Current;
                var args = Array(CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable());
                args[i1] = expected.Consequent.GetArgument(i1);
                args[i2] = expected.Consequent.GetArgument(i2);
                var matches = indexes.Index(args);
                Assert.AreEqual(1, matches.Length);
                Assert.AreSame(expected, matches[0].Model);
            }
        }

        for (int i1 = 0; i1 < numArgs; i1++)
        {
            for (int i2 = i1 + 1; i2 < numArgs; i2++)
            {
                for (int i3 = i2 + 1; i3 < numArgs; i3++)
                {
                    itr.MoveNext();
                    var expected = itr.Current;
                    var args = Array(CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable(), CreateVariable());
                    args[i1] = expected.Consequent.GetArgument(i1);
                    args[i2] = expected.Consequent.GetArgument(i2);
                    args[i3] = expected.Consequent.GetArgument(i3);
                    var matches = indexes.Index(args);
                    Assert.AreEqual(1, matches.Length);
                    Assert.AreSame(expected, matches[0].Model);
                }
            }
        }

        Assert.AreEqual(129, indexes.CountReferences());
        // TODO set numClauses to a larger number (e.g. 100,000) to verify that some indexes have been garbage collected
        // TODO Assert.IsTrue(indexes.countClearedReferences() > 0);
    }

    private static void AssertMatches(Indexes indexes, Term[] input, params ClauseAction[] expected)
    {
        Assert.IsTrue(expected.Length > 0);
        var actual = indexes.Index(input);
        Assert.AreSame(actual, indexes.Index(input)); // assert same object gets returned for multiple calls
        Assert.AreEqual(expected.Length, actual.Length);
        for (int i = 0; i < actual.Length; i++)
        {
            Assert.AreSame(expected[i], actual[i]);
        }
    }

    private static void AssertNoMatches(Indexes indexes, Term[] input)
    {
        var actual = indexes.Index(input);
        Assert.AreSame(actual, indexes.Index(input)); // assert same object gets returned for multiple calls
        Assert.AreEqual(0, actual.Length);
    }

    private static Variable CreateVariable() => Variable();
}
