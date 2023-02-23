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
public class DynamicUserDefinedPredicateFactoryTest : TestUtils
{
    private const string TEST_PREDICATE_NAME = "test";

    [TestMethod]
    public void TestSimpleAdditionAndIteration1()
    {
        var dp = CreateDynamicPredicate();
        AssertIterator(dp);

        AddLast(dp, "a");
        AssertIterator(dp, "a");

        AddLast(dp, "b");
        AssertIterator(dp, "a", "b");

        AddFirst(dp, "c");
        AssertIterator(dp, "c", "a", "b");
    }

    [TestMethod]
    public void TestSimpleAdditionAndIteration2()
    {
        var dp = CreateDynamicPredicate();
        AssertIterator(dp);

        AddFirst(dp, "a");
        AssertIterator(dp, "a");

        AddFirst(dp, "b");
        AssertIterator(dp, "b", "a");

        AddLast(dp, "c");
        AssertIterator(dp, "b", "a", "c");
    }

    [TestMethod]
    public void TestRemoval()
    {
        var dp = CreateDynamicPredicate();
        AddLast(dp, "a");
        AddLast(dp, "b");
        AddLast(dp, "c");
        AssertIterator(dp, "a", "b", "c");

        var itr1 = dp.GetImplications();
        var itr2 = dp.GetImplications();
        var itr3 = dp.GetImplications();
        var itr4 = dp.GetImplications();
        var itr5 = dp.GetImplications();

        // iterate to a
        itr2.MoveNext();

        // iterate to b
        itr3.MoveNext();
        itr3.MoveNext();

        // iterate to c
        itr4.MoveNext();
        itr4.MoveNext();
        itr4.MoveNext();

        // Add d
        AddLast(dp, "d");

        // delete b and c
        itr5.MoveNext();
        itr5.MoveNext();

        itr5.Remove();
        itr5.MoveNext();
        itr5.Remove();

        AssertIterator(dp, "a", "d");
        AssertIterator(itr1, "a", "d");
        AssertIterator(itr2, "d");
        AssertIterator(itr3, "c", "d");
        AssertIterator(itr4, "d");
    }

    [TestMethod]
    public void TestRemoveFirstAndLast()
    {
        var dp = CreateDynamicPredicate();
        AddLast(dp, "a");
        AddLast(dp, "b");
        AddLast(dp, "c");
        AddLast(dp, "d");
        AddLast(dp, "e");
        AssertIterator(dp, "a", "b", "c", "d", "e");

        var itr = dp.GetImplications();
        itr.MoveNext();
        itr.Remove(); // remove a
        itr.MoveNext();
        itr.MoveNext();
        itr.MoveNext();
        itr.MoveNext();
        itr.Remove(); // remove e

        AssertIterator(dp, "b", "c", "d");
    }

    [TestMethod]
    public void TestRemoveAll()
    {
        var dp = CreateDynamicPredicate();
        AddLast(dp, "a");
        AddLast(dp, "b");
        AddLast(dp, "c");

        Assert.IsTrue(dp.GetPredicate(new Term[] { Atom("a") }).Evaluate());
        Assert.IsTrue(dp.GetPredicate(new Term[] { Atom("b") }).Evaluate());
        Assert.IsTrue(dp.GetPredicate(new Term[] { Atom("c") }).Evaluate());
        Assert.IsFalse(dp.GetPredicate(new Term[] { Atom("z") }).Evaluate());

        var itr = dp.GetImplications();
        itr.MoveNext();
        itr.Remove();

        Assert.IsFalse(dp.GetPredicate(new Term[] { Atom("a") }).Evaluate());
        Assert.IsTrue(dp.GetPredicate(new Term[] { Atom("b") }).Evaluate());
        Assert.IsTrue(dp.GetPredicate(new Term[] { Atom("c") }).Evaluate());
        Assert.IsFalse(dp.GetPredicate(new Term[] { Atom("z") }).Evaluate());

        itr.MoveNext();
        itr.Remove();

        Assert.IsFalse(dp.GetPredicate(new Term[] { Atom("a") }).Evaluate());
        Assert.IsFalse(dp.GetPredicate(new Term[] { Atom("b") }).Evaluate());
        Assert.IsTrue(dp.GetPredicate(new Term[] { Atom("c") }).Evaluate());
        Assert.IsFalse(dp.GetPredicate(new Term[] { Atom("z") }).Evaluate());

        itr.MoveNext();
        itr.Remove();

        Assert.IsFalse(dp.GetPredicate(new Term[] { Atom("a") }).Evaluate());
        Assert.IsFalse(dp.GetPredicate(new Term[] { Atom("b") }).Evaluate());
        Assert.IsFalse(dp.GetPredicate(new Term[] { Atom("c") }).Evaluate());
        Assert.IsFalse(dp.GetPredicate(new Term[] { Atom("z") }).Evaluate());

        Assert.IsFalse(dp.GetImplications().CanMoveNext);
    }

    [TestMethod]
    public void TestGetClauseModel()
    {
        var dp = CreateDynamicPredicate();
        var ci1 = CreateClauseModel2("a");
        dp.AddLast(ci1);
        var ci2 = CreateClauseModel2("b");
        dp.AddLast(ci2);
        var ci3 = CreateClauseModel2("c");
        dp.AddLast(ci3);

        AssertClauseModel(dp, 0, ci1);
        AssertClauseModel(dp, 1, ci2);
        AssertClauseModel(dp, 2, ci3);
        Assert.IsNull(dp.GetClauseModel(3));
        Assert.IsNull(dp.GetClauseModel(7));
    }

    private static void AssertClauseModel(DynamicUserDefinedPredicateFactory dp, int index, ClauseModel original)
    {
        var actual = dp.GetClauseModel(index);
        Assert.AreNotSame(original, actual);
        Assert.AreSame(original.Original, actual.Original);
    }

    [TestMethod]
    public void TestGetPredicate()
    {
        string[] data = { "a", "b", "c" };
        var dp = CreateDynamicPredicate();
        foreach (string d in data)
        {
            AddLast(dp, d);
        }

        // test evaluate with atom as argument - will use index
        foreach (string d in data)
        {
            var _inputArg = Atom(d);
            var _args = new Term[] { _inputArg };
            var _e = dp.GetPredicate(_args);
            Assert.IsFalse(_e.CouldReevaluationSucceed);
            Assert.IsTrue(_e.Evaluate());
            Assert.AreSame(_inputArg, _args[0]);
        }

        // test evaluate with variable as argument
        var inputArg = Variable();
        var args = new Term[] { inputArg };
        var e = dp.GetPredicate(args);
        Assert.IsTrue(e.CouldReevaluationSucceed);
        foreach (string d in data)
        {
            Assert.IsTrue(e.Evaluate());
            Assert.AreSame(TermType.ATOM, args[0].Type);
            Assert.AreEqual(d, args[0].Name);
        }
        Assert.IsFalse(e.Evaluate());
        Assert.AreSame(inputArg, args[0]);
        Assert.AreSame(inputArg, args[0].Term);
    }

    private static DynamicUserDefinedPredicateFactory CreateDynamicPredicate()
    {
        var kb = CreateKnowledgeBase();
        var key = new PredicateKey(TEST_PREDICATE_NAME, 1);
        var dp = new DynamicUserDefinedPredicateFactory(kb, key);
        Assert.AreEqual(key, dp.PredicateKey);
        Assert.IsTrue(dp.IsDynamic);
        return dp;
    }

    private static void AddFirst(DynamicUserDefinedPredicateFactory dp, string argumentSyntax)
    {
        var ci = CreateClauseModel2(argumentSyntax);
        dp.AddFirst(ci);
    }

    private static void AddLast(DynamicUserDefinedPredicateFactory dp, string argumentSyntax)
    {
        var ci = CreateClauseModel2(argumentSyntax);
        dp.AddLast(ci);
    }

    private static ClauseModel CreateClauseModel2(string argumentSyntax)
    {
        var inputSyntax = CreateStructureSyntax(argumentSyntax);
        return CreateClauseModel(inputSyntax + ".");
    }

    private static void AssertIterator(DynamicUserDefinedPredicateFactory dp, params string[] expectedOrder)
    {
        var itr = dp.GetImplications();
        AssertIterator(itr, expectedOrder);
    }

    private static void AssertIterator(IEnumerator<ClauseModel> itr, params string[] expectedOrder)
    {
        foreach (var expected in expectedOrder)
        {
            Assert.IsTrue(itr.MoveNext());

            var ci = itr.Current;
            var predicateSyntax = CreateStructureSyntax(expected);
            Assert.AreEqual(predicateSyntax, ci.Original.ToString());
        }
        Assert.IsFalse(itr.MoveNext());
    }

    private static string CreateStructureSyntax(string argumentSyntax)
        => TEST_PREDICATE_NAME + "(" + argumentSyntax + ")";
}
