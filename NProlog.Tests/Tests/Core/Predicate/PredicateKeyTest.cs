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
using Org.NProlog.Core.Exceptions;
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Predicate;

[TestClass]
public class PredicateKeyTest : TestUtils
{
    private static readonly string PREDICATE_KEY_FUNCTOR = "/";

    [TestMethod]
    public void TestCanCreateForAtom()
    {
        string name = "abc";
        TestCanCreate(Atom(name), name, 0);
    }

    [TestMethod]
    public void TestCanCreateForStructure()
    {
        string name = "abc";
        TestCanCreate(Structure(name, Atom()), name, 1);
        TestCanCreate(Structure(name, Atom(), IntegerNumber(), DecimalFraction()), name, 3);
    }

    [TestMethod]
    public void TestCanCreateForList()
    {
        TestCanCreate(List(Atom(), Atom()), ".", 2);
    }

    private void TestCanCreate(Term t, string name, int numArgs)
    {
        // test both static factory methods
        PredicateKey k1 = PredicateKey.CreateForTerm(t);
        Term arity = CreateArity(name, numArgs);
        PredicateKey k2 = PredicateKey.CreateFromNameAndArity(arity);

        TestCreatedKey(k1, name, numArgs);
        TestCreatedKey(k2, name, numArgs);

        // test that two keys instances with same name and number of arguments are considered equal
        TestEquals(k1, k2);
    }

    private static void TestCreatedKey(PredicateKey k, string name, int numArgs)
    {
        Assert.AreEqual(name, k.Name);
        Assert.AreEqual(numArgs, k.NumArgs);
        Assert.AreEqual(name + PREDICATE_KEY_FUNCTOR + numArgs, k.ToString());
    }

    [TestMethod]
    public void TestNotEquals()
    {
        // different named atoms
        TestNotEquals(Atom("abc"), Atom("abC"));
        TestNotEquals(Atom("abc"), Atom("abcd"));

        // atom versus structure
        TestNotEquals(Atom("abc"), Structure("abc", Atom("abc")));

        // structures with different names and/or number of arguments
        TestNotEquals(Structure("abc", Atom("a")), Structure("xyz", Atom("a")));
        TestNotEquals(Structure("abc", Atom("a")), Structure("abc", Atom("a"), Atom("b")));
    }

    private static void TestNotEquals(Term t1, Term t2)
    {
        PredicateKey k1 = PredicateKey.CreateForTerm(t1);
        PredicateKey k2 = PredicateKey.CreateForTerm(t2);
        Assert.IsFalse(k1.Equals(k2));
    }

    [TestMethod]
    public void TestEquals()
    {
        // structures with same name and number of arguments
        Term t1 = Structure("abc", Atom("a"), Atom("b"), Atom("c"));
        Term t2 = Structure("abc", IntegerNumber(), DecimalFraction(), Variable());
        PredicateKey k1 = PredicateKey.CreateForTerm(t1);
        PredicateKey k2 = PredicateKey.CreateForTerm(t2);
        TestEquals(k1, k2);
    }

    private static void TestEquals(PredicateKey k1, PredicateKey k2)
    {
        Assert.IsTrue(k1.Equals(k2));
        Assert.IsTrue(k2.Equals(k1));
        Assert.IsTrue(k1.Equals(k1));
        Assert.IsTrue(k2.Equals(k2));
        Assert.AreEqual(k1.GetHashCode(), k2.GetHashCode());
    }

    [TestMethod]
    public void TestNotEqualsNonPredicateKey()
    {
        PredicateKey k1 = PredicateKey.CreateForTerm(Structure());
        string s = "Not an is PredicateKey";
        Assert.IsFalse(k1.Equals(s));
    }

    [TestMethod]
    public void TestCannotCreateForTerm()
    {
        TestCannotCreateForTerm(IntegerNumber());
        TestCannotCreateForTerm(DecimalFraction());
        TestCannotCreateForTerm(Variable());
        TestCannotCreateForTerm(EmptyList.EMPTY_LIST);
    }

    private static void TestCannotCreateForTerm(Term t)
    {
        try
        {
            PredicateKey k = PredicateKey.CreateForTerm(t);
            Assert.Fail("Managed to create: " + k + " for: " + t);
        }
        catch (PrologException e)
        {
            // expected
        }
    }

    [TestMethod]
    public void TestCannotCreateFromNameAndArity()
    {
        TestCannotCreateFromNameAndArity(IntegerNumber());
        TestCannotCreateFromNameAndArity(Atom());
        TestCannotCreateFromNameAndArity(Variable());
        TestCannotCreateFromNameAndArity(Structure("\\", Atom(), Atom()));
        TestCannotCreateFromNameAndArity(Structure(PREDICATE_KEY_FUNCTOR, Atom(), Atom(), Atom()));
    }

    private static void TestCannotCreateFromNameAndArity(Term t)
    {
        try
        {
            PredicateKey k = PredicateKey.CreateFromNameAndArity(t);
            Assert.Fail("Managed to create: " + k + " for: " + t);
        }
        catch (PrologException e)
        {
            // expected
        }
    }

    [TestMethod]
    public void TestCompareTo()
    {
        PredicateKey k = CreateKey("bcde", 2);

        // equal to self
        Assert.IsTrue(k.CompareTo(k) == 0);

        // test, if exact same name, ordered on number of arguments
        Assert.IsTrue(k.CompareTo(CreateKey("bcde", 1)) > 0);
        Assert.IsTrue(k.CompareTo(CreateKey("bcde", 3)) < 0);

        // test greater based on name
        Assert.IsTrue(k.CompareTo(CreateKey("bcazzz", 1)) > 0);
        Assert.IsTrue(k.CompareTo(CreateKey("a", 1)) > 0);
        Assert.IsTrue(k.CompareTo(CreateKey("a", 2)) > 0);
        Assert.IsTrue(k.CompareTo(CreateKey("a", 3)) > 0);

        // test lower based on name
        Assert.IsTrue(k.CompareTo(CreateKey("bczaaa", 1)) < 0);
        Assert.IsTrue(k.CompareTo(CreateKey("z", 1)) < 0);
        Assert.IsTrue(k.CompareTo(CreateKey("z", 2)) < 0);
        Assert.IsTrue(k.CompareTo(CreateKey("z", 3)) < 0);
    }

    [TestMethod]
    public void TestIllegalArgumentException()
    {
        try
        {
            CreateKey("x", -1);
            Assert.Fail();
        }
        catch (ArgumentException e)
        {
            // expected
        }
        try
        {
            CreateKey("x", int.MinValue);
            Assert.Fail();
        }
        catch (ArgumentException e)
        {
            // expected
        }
    }

    [TestMethod]
    public void TestToTerm()
    {
        // create predicate key
        string name = "test";
        int numArgs = 7;
        PredicateKey key = CreateKey(name, numArgs);

        // create term from key
        Term term = key.ToTerm();

        // assert term matches details of key it was created from
        Assert.AreEqual(key, PredicateKey.CreateFromNameAndArity(term));
        Assert.AreSame(TermType.STRUCTURE, term.Type);
        Assert.AreEqual(PREDICATE_KEY_FUNCTOR, term.Name);
        Assert.AreEqual(2, term.NumberOfArguments);
        Assert.AreEqual(name, ((Atom)term.GetArgument(0)).Name);
        Assert.AreEqual(numArgs, ((IntegerNumber)term.GetArgument(1)).Long);
    }

    private static PredicateKey CreateKey(string name, int numArgs)
    {
        return PredicateKey.CreateFromNameAndArity(CreateArity(name, numArgs));
    }

    private static Term CreateArity(string name, int numArgs)
    {
        return Structure(PREDICATE_KEY_FUNCTOR, Atom(name), IntegerNumber(numArgs));
    }
}
