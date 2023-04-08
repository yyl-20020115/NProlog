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
namespace Org.NProlog.Core.Terms;

/**
 * Test implementations of {@link Term}
 * <p>
 * As so much of the tests are about interactions between different classes of Terms it was decided to have a generic
 * TermTest class to test generic behaviour and have only specific behaviour tested in separate test classes specific to
 * a particular Term implementation.
 *
 * @see AtomTest
 * @see DecimalFractionTest
 * @see EmptyListTest
 * @see IntegerNumberTest
 * @see ListTest
 * @see StructureTest
 * @see VariableTest
 */
[TestClass]
public class TermTest : TestUtils
{
    private static readonly Term[] IMMUTABLE_TERMS = {
               Atom("a"),
               Atom("b"),
               Atom("c"),
               Atom("A"),
               Atom("B"),
               Atom("C"),
               Atom("abc"),
               Atom("ABC"),
               Atom("AbC"),
               Atom("0"),
               Atom("1"),
               Atom("-1"),
               Atom("[]"),

               IntegerNumber(0),
               IntegerNumber(1),
               IntegerNumber(-1),
               IntegerNumber(int.MinValue),
               IntegerNumber(int.MaxValue),

               DecimalFraction(0),
               DecimalFraction(1),
               DecimalFraction(-1),
               DecimalFraction(0.0001),
               DecimalFraction(-0.0001),
               DecimalFraction(double.MinValue),
               DecimalFraction(double.MaxValue),

               Structure("abc", Atom()),
               Structure("abc", Atom(), Atom()),
               Structure("ABC", Atom()),
               Structure("ABC", Atom(), Atom()),
               Structure("1", Atom()),
               Structure("1", Atom(), Atom()),

               List(Atom(), Atom()),
               List(Atom(), Atom(), Atom()),
               List(Atom("a"), IntegerNumber(1), DecimalFraction(1), Structure("abc", Atom())),

               EmptyList.EMPTY_LIST};

    /** check both Unify and strictEquality methods against various immutable Terms */
    [TestMethod]
    public void TestUnifyAndStrictEquality()
    {
        foreach (var t1 in IMMUTABLE_TERMS)
        {
            foreach (var t2 in IMMUTABLE_TERMS)
            {
                AssertUnify(t1, t2, t1 == t2);
                AssertStrictEquality(t1, t2, t1 == t2);
            }
        }
    }

    /** check calling Copy() on an immutable Term returns the Term */
    [TestMethod]
    public void TestCopy()
    {
        foreach (var t1 in IMMUTABLE_TERMS)
        {
            Dictionary<Variable, Variable> sharedVariables = new();
            var t2 = t1.Copy(sharedVariables);
            Assert.AreSame(t1, t2);
            Assert.IsTrue(sharedVariables.Count == 0);
        }
    }

    /** check calling getValue() on an immutable Term returns the Term */
    [TestMethod]
    public void TestGetValue()
    {
        foreach (var t1 in IMMUTABLE_TERMS)
        {
            var t2 = t1.Term;
            Assert.AreSame(t1, t2);
        }
    }

    [TestMethod]
    public void TestIsImmutable()
    {
        foreach (var element in IMMUTABLE_TERMS)
        {
            Assert.IsTrue(element.IsImmutable);
        }
    }

    /** check calling Backtrack() has no effect on an immutable Term */
    [TestMethod]
    public void TestBacktrack()
    {
        foreach (var t in IMMUTABLE_TERMS)
        {
            // keep track of the Term's current properties
            var originalType = t.Type;
            int originalNumberOfArguments = t.NumberOfArguments;
            var originalToString = t.ToString();

            // perform the Backtrack()
            t.Backtrack();

            // check properties are the same as prior to the Backtrack()
            Assert.AreSame(originalType, t.Type);
            Assert.AreEqual(originalNumberOfArguments, t.NumberOfArguments);
            Assert.AreEqual(originalToString, t.ToString());
        }
    }

    [TestMethod]
    public void TestUnifyAndStrictEqualityWithVariable()
    {
        foreach (var t in IMMUTABLE_TERMS)
        {
            var v = Variable("X");

            // check equal
            AssertStrictEquality(t, v, false);

            // check can Unify (with Unify called on t with v passed as a parameter)
            Assert.IsTrue(t.Unify(v));

            // check equal after unification
            AssertVariableIsUnifiedToTerm(v, t);

            // Backtrack
            v.Backtrack();

            // check Backtrack undid result of unification
            Assert.AreSame(TermType.VARIABLE, v.Type);
            AssertStrictEquality(t, v, false);

            // check can Unify again (but this time with Unify called on v with t passed as a parameter)
            Assert.IsTrue(t.Unify(v));

            // check equal after unification
            AssertVariableIsUnifiedToTerm(v, t);

            // Backtrack
            v.Backtrack();

            // check Backtrack undid result of unification
            Assert.AreSame(TermType.VARIABLE, v.Type);
            AssertStrictEquality(t, v, false);

            // Unify v to something else
            v.Unify(Atom("some atom"));

            // check v and t can no longer be unified
            AssertUnify(t, v, false);
        }
    }

    /** test {@link AnonymousVariable} unifies with everything and is strictly equal to nothing */
    [TestMethod]
    public void TestUnifyAndStrictEqualityWithAnonymousVariable()
    {
        foreach (var t in IMMUTABLE_TERMS)
        {
            AssertUnify(t, new Variable(), true);
            AssertStrictEquality(t, new Variable(), false);
        }
    }

    private void AssertVariableIsUnifiedToTerm(Variable v, Term t)
    {
        AssertStrictEquality(t, v, true);
        Assert.IsFalse(v.Equals(t));
        Assert.IsFalse(t.Equals(v));
        Assert.AreEqual(t.ToString(), v.ToString());
        Assert.AreSame(t.Type, v.Type);
        Assert.AreSame(t, v.Term);
        Assert.AreSame(t, v.Copy(null));
        AssertUnify(t, v, true);
    }

    private static void AssertUnify(Term t1, Term t2, bool expected)
    {
        Assert.AreEqual(expected, t1.Unify(t2));
        Assert.AreEqual(expected, t2.Unify(t1));
    }
}
