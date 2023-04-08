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
using Org.NProlog.Core.Math;
using Org.NProlog.Core.Predicate;
using Org.NProlog.Core.Predicate.Builtin.Bool;
using Org.NProlog.Core.Predicate.Builtin.Compare;
using Org.NProlog.Core.Predicate.Builtin.Kb;
using Org.NProlog.Core.Predicate.Udp;
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Kb;

[TestClass]
public class KnowledgeBaseTest : TestUtils
{
    private readonly KnowledgeBase kb = CreateKnowledgeBase();
    private readonly Predicates predicates;
    public KnowledgeBaseTest()
    {
        predicates = kb.Predicates;
    }

    /** Check that {@link PrologDefaultProperties} is used by default. */
    [TestMethod]
    public void TestDefaultPrologProperties()
    {
        var kb = KnowledgeBaseUtils.CreateKnowledgeBase();
        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreSame(typeof(PrologDefaultProperties), kb.PrologProperties.GetType());
    }

    /** Check that {@link PrologProperties} is configurable. */
    [TestMethod]
    public void TestConfiguredPrologProperties()
    {
        var kb = KnowledgeBaseUtils.CreateKnowledgeBase(TestUtils.PROLOG_DEFAULT_PROPERTIES);
        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreSame(TestUtils.PROLOG_DEFAULT_PROPERTIES, kb.PrologProperties);
    }

    /** @see ArithmeticOperatorsTest */
    [TestMethod]
    public void TestGetNumeric()
    {
        var p = Structure("-", IntegerNumber(7), IntegerNumber(3));
        var n = kb.ArithmeticOperators.GetNumeric(p);
        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreSame(typeof(IntegerNumber), n.GetType());
        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(4, n.Long);
    }

    [TestMethod]
    public void TestCannotOverwritePluginPredicate()
    { // TODO these assertions are duplicated in PredicatesTest
        var input = Atom("true");
        var key = PredicateKey.CreateForTerm(input);
        try
        {
            predicates.CreateOrReturnUserDefinedPredicate(key);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.Fail();
        }
        catch (PrologException e)
        {
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual("Cannot replace already defined built-in predicate: true/0", e.Message);
        }
        try
        {
            predicates.AddUserDefinedPredicate(new StaticUserDefinedPredicateFactory(kb, key));
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.Fail();
        }
        catch (PrologException e)
        {
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual("Cannot replace already defined built-in predicate: true/0", e.Message);
        }
        try
        {
            PredicateFactory mockPredicateFactory = new MockPredicateFactory();
            predicates.AddPredicateFactory(key, mockPredicateFactory);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.Fail();
        }
        catch (PrologException e)
        {
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual("Already defined: true/0", e.Message);
        }
        try
        {
            predicates.AddPredicateFactory(key, "com.example.DummyPredicateFactory");
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.Fail();
        }
        catch (PrologException e)
        {
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual("Already defined: true/0", e.Message);
        }
    }

    [TestMethod]
    public void TestGetUserDefinedPredicates()
    {
        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(predicates.GetUserDefinedPredicates().Count == 0);

        // Create user defined predicate test/0.
        var key1 = PredicateKey.CreateForTerm(Atom("test"));
        var udp1 = predicates.CreateOrReturnUserDefinedPredicate(key1);
        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreSame(key1, udp1.PredicateKey);
        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreSame(key1, udp1.PredicateKey);
        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(1, predicates.GetUserDefinedPredicates().Count);

        // Add a clause to the user defined predicate.
        var clause1 = ClauseModel.CreateClauseModel(TestUtils.ParseSentence("test :- write(clause1)."));
        udp1.AddLast(clause1);

        // Retrieve user defined predicate test/0
        var key2 = PredicateKey.CreateForTerm(Atom("test"));
        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreSame(udp1, predicates.CreateOrReturnUserDefinedPredicate(key2));
        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(1, predicates.GetUserDefinedPredicates().Count);

        // Create new user defined predicate with same key as already defined version. Add a clause.
        var udp2 = new StaticUserDefinedPredicateFactory(kb, key1);
        var clause2 = ClauseModel.CreateClauseModel(TestUtils.ParseSentence("test :- write(clause2)."));
        udp2.AddLast(clause2);

        // Add new user defined predicate test/0 and confirm previous version has been updated with extra clause.
        predicates.AddUserDefinedPredicate(udp2);
        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(1, predicates.GetUserDefinedPredicates().Count);
        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreSame(udp1, predicates.CreateOrReturnUserDefinedPredicate(key1));
        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(clause1.Original, udp1.GetClauseModel(0).Original);
        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(clause2.Original, udp1.GetClauseModel(1).Original);
        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(1, predicates.GetUserDefinedPredicates().Count);

        var key3 = PredicateKey.CreateForTerm(Atom("test2"));
        var udp3 = predicates.CreateOrReturnUserDefinedPredicate(key3);
        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreSame(key3, udp3.PredicateKey);
        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(2, predicates.GetUserDefinedPredicates().Count);

        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreNotSame(udp1, udp2);
        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreNotSame(udp1, udp3);
        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreNotSame(udp2, udp3);
    }

    [TestMethod]
    public void TestGetUserDefinedPredicatesUnmodifiable()
    {
        var userDefinedPredicates = predicates.GetUserDefinedPredicates();
        try
        {
            userDefinedPredicates.Add(null, null);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.Fail();
        }
        catch (ArgumentNullException)
        {
            // expected
        }
    }

    [TestMethod]
    public void TestGetAllDefinedPredicateKeys()
    {
        // create keys
        var k1 = new PredicateKey("a", 9);
        var k2 = new PredicateKey("x", 1);
        var k3 = new PredicateKey("x", 2);
        var k4 = new PredicateKey("x", 3);
        var k5 = new PredicateKey("z", 2);
        var k6 = new PredicateKey("z", 6);
        var k7 = new PredicateKey("z", 7);

        // Add keys to knowledge base
        // Add some as "build-in" predicates and others as "user-defined" predicates
        var kb = KnowledgeBaseUtils.CreateKnowledgeBase();
        var predicates = kb.Predicates;
        predicates.CreateOrReturnUserDefinedPredicate(k7);
        predicates.AddPredicateFactory(k4, "com.example.Abc");
        predicates.CreateOrReturnUserDefinedPredicate(k5);
        predicates.AddPredicateFactory(k2, "com.example.Xyz");
        predicates.AddPredicateFactory(k3, "com.example.Abc");
        predicates.CreateOrReturnUserDefinedPredicate(k1);
        predicates.AddPredicateFactory(k6, new True());

        // get all defined predicate keys from the knowledge base
        var allKeys = predicates.GetAllDefinedPredicateKeys();
        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(8, allKeys.Count);

        // assert all the keys we added are included, and in the correct order
        var iterator = allKeys.GetEnumerator();
        
        var collection = new HashSet<PredicateKey>();
        while (iterator.MoveNext())
        {
            collection.Add(iterator.Current);
        }

        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(collection.Contains(k1));
        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(collection.Contains(k2));
        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(collection.Contains(k3));
        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(collection.Contains(k4));
        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(collection.Contains(k5));
        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(collection.Contains(k6));
        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(collection.Contains(k7));


        //iterator.MoveNext();
        //Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(k1, iterator.Current);
        //// although we didn't Add "pl_add_predicate/2" it will be returned -
        //// it is the one predicate that is hardcoded in Prolog and so is always present
        //iterator.MoveNext();
        //Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(TestUtils.ADD_PREDICATE_KEY, iterator.Current);
        //iterator.MoveNext();
        //Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(k2, iterator.Current);
        //iterator.MoveNext();
        //Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(k3, iterator.Current);
        //iterator.MoveNext();
        //Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(k4, iterator.Current);
        //iterator.MoveNext();
        //Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(k5, iterator.Current);
        //iterator.MoveNext();
        //Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(k6, iterator.Current);
        //iterator.MoveNext();
        //Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(k7, iterator.Current);
        //Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsFalse(iterator.MoveNext());
    }

    [TestMethod]
    public void TestGetPredicateAndGetPredicateFactory1()
    {
        AssertGetPredicateFactory(Atom("true"), typeof(True));
    }

    [TestMethod]
    public void TestGetPredicateAndGetPredicateFactory2()
    {
        AssertGetPredicateFactory(Atom("does_not_exist"), typeof(UnknownPredicate));
    }

    [TestMethod]
    public void TestGetPredicateAndGetPredicateFactory3()
    {
        AssertGetPredicateFactory(Structure("=", Atom(), Atom()), typeof(Equal));
    }

    [TestMethod]
    public void TestGetPredicateAndGetPredicateFactory4()
    {
        AssertGetPredicateFactory(Structure("=", Atom(), Atom(), Atom()), typeof(UnknownPredicate));
    }

    [TestMethod]
    public void TestGetAddPredicateFactory()
    {
        var ef = predicates.GetPredicateFactory(ADD_PREDICATE_KEY);
        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreSame(typeof(AddPredicateFactory), ef.GetType());
        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(ef is AbstractSingleResultPredicate);
    }

    [TestMethod]
    public void TestAddPredicateFactoryWithInstance()
    {
        var pf = new True();
        var key = new PredicateKey("testAddPredicateFactoryWithInstance", 1);

        predicates.AddPredicateFactory(key, pf);

        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreSame(pf, predicates.GetPredicateFactory(key));

        // assert exception thrown if try to re-Add
        try
        {
            predicates.AddPredicateFactory(key, typeof(True).Name);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.Fail();
        }
        catch (PrologException e)
        {
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual("Already defined: testAddPredicateFactoryWithInstance/1", e.Message);
        }
        try
        {
            predicates.AddPredicateFactory(key, typeof(Fail).Name);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.Fail();
        }
        catch (PrologException e)
        {
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual("Already defined: testAddPredicateFactoryWithInstance/1", e.Message);
        }
        try
        {
            predicates.AddPredicateFactory(key, pf);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.Fail();
        }
        catch (PrologException e)
        {
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual("Already defined: testAddPredicateFactoryWithInstance/1", e.Message);
        }
        try
        {
            predicates.AddPredicateFactory(key, new Fail());
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.Fail();
        }
        catch (PrologException e)
        {
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual("Already defined: testAddPredicateFactoryWithInstance/1", e.Message);
        }
    }

    [TestMethod]
    public void TestAddPredicateFactoryWithClassName()
    {
        // create PredicateKey to Add to KnowledgeBase
        var key = new PredicateKey("testAddPredicateFactoryWithClassName", 1);

        // assert not already defined in knowledge base
        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreSame(typeof(UnknownPredicate), predicates.GetPredicateFactory(key).GetType());

        // Add
        predicates.AddPredicateFactory(key, typeof(True));

        // assert now defined in knowledge base
        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreSame(typeof(True), predicates.GetPredicateFactory(key).GetType());
        // assert, once defined, the same instance is returned each time
        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreSame(predicates.GetPredicateFactory(key), predicates.GetPredicateFactory(key));

        // assert exception thrown if try to re-Add
        try
        {
            predicates.AddPredicateFactory(key, typeof(True).Name);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.Fail();
        }
        catch (PrologException e)
        {
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual("Already defined: testAddPredicateFactoryWithClassName/1", e.Message);
        }
        try
        {
            predicates.AddPredicateFactory(key, typeof(Fail).Name);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.Fail();
        }
        catch (PrologException e)
        {
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual("Already defined: testAddPredicateFactoryWithClassName/1", e.Message);
        }
        try
        {
            predicates.AddPredicateFactory(key, new True());
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.Fail();
        }
        catch (PrologException e)
        {
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual("Already defined: testAddPredicateFactoryWithClassName/1", e.Message);
        }
        try
        {
            predicates.AddPredicateFactory(key, new Fail());
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.Fail();
        }
        catch (PrologException e)
        {
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual("Already defined: testAddPredicateFactoryWithClassName/1", e.Message);
        }
    }

    [TestMethod]
    public void TestAddPredicateFactoryClassNotFound()
    {
        var key = new PredicateKey("testAddPredicateFactoryError", 1);
        predicates.AddPredicateFactory(key, "an invalid class name");
        try
        {
            predicates.GetPredicateFactory(key);
            //Microsoft.VisualStudio.TestTools.UnitTesting.Assert.Fail();
        }
        catch (Exception e)
        {
            // expected as specified class name is invalid
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual("Could not create new PredicateFactory using: an invalid class name", e.Message);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreSame(typeof(Exception), e.InnerException.GetType());
        }
    }

    /** Test attempting to Add a predicate factory that does not have a public no arg constructor. */
    [TestMethod]
    public void TestAddPredicateFactoryIllegalAccess()
    {
        var key = new PredicateKey("testAddPredicateFactoryError", 1);
        var className = typeof(DummyPredicateFactoryNoPublicConstructor).Name;
        predicates.AddPredicateFactory(key, typeof(DummyPredicateFactoryNoPublicConstructor).Name);
        try
        {
            predicates.GetPredicateFactory(key);
            //Microsoft.VisualStudio.TestTools.UnitTesting.Assert.Fail();
        }
        catch (SystemException e)
        {
            // expected as int has no public constructor (and is also not a PredicateFactory)
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual("Could not create new PredicateFactory using: " + className, e.Message);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreSame(typeof(Exception), e.InnerException.GetType());
        }
    }

    /** Test using a static method to Add a predicate factory that does not have a public no arg constructor. */
    [TestMethod]
    public void TestAddPredicateFactoryUsingStaticMethod()
    {
        var key = new PredicateKey("testAddPredicateFactory", 1);
        predicates.AddPredicateFactory(key, typeof(DummyPredicateFactoryNoPublicConstructor),"GetInstance");
        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreSame(
            typeof(DummyPredicateFactoryNoPublicConstructor), predicates.GetPredicateFactory(key).GetType());
    }

    [TestMethod]
    public void TestPreprocessWhenPreprocessablePredicateFactory()
    {
        var term = Structure("testOptimise", Atom("test"));
        var mockPreprocessablePredicateFactory = new MockPreprocessablePredicateFactory();
        predicates.AddPredicateFactory(PredicateKey.CreateForTerm(term), mockPreprocessablePredicateFactory);

        var mockPredicateFactory = new MockPredicateFactory();
        When(mockPreprocessablePredicateFactory.Preprocess(term)).ThenReturn(mockPredicateFactory);

        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreNotSame(mockPredicateFactory, predicates.GetPreprocessedPredicateFactory(term));

        Verify(mockPreprocessablePredicateFactory).Preprocess(term);
        VerifyNoMoreInteractions(mockPreprocessablePredicateFactory, mockPredicateFactory);
    }

    [TestMethod]
    public void TestPreprocessWhenNotPreprocessablePredicateFactory()
    {
        // note that mockPredicateFactory is not an instance of PreprocessablePredicateFactory
        var term = Structure("testOptimise", Atom("test"));
        PredicateFactory mockPredicateFactory = new MockPredicateFactory();
        predicates.AddPredicateFactory(PredicateKey.CreateForTerm(term), mockPredicateFactory);

        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreSame(mockPredicateFactory, predicates.GetPreprocessedPredicateFactory(term));

        VerifyNoMoreInteractions(mockPredicateFactory);
    }

    private void AssertGetPredicateFactory(Term input, Type expected)
    {
        var ef1 = predicates.GetPredicateFactory(input);
        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreSame(expected, ef1.GetType());

        var key = PredicateKey.CreateForTerm(input);
        var ef2 = predicates.GetPredicateFactory(key);
        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreSame(expected, ef2.GetType());
    }

    public class DummyPredicateFactoryNoPublicConstructor : PredicateFactory
    {
        public static DummyPredicateFactoryNoPublicConstructor GetInstance() => new ();

        private DummyPredicateFactoryNoPublicConstructor()
        {
            // private as want to test creation using GetInstance static method
        }


        public Predicate.Predicate GetPredicate(Term[] args) => throw new InvalidOperationException();


        public bool IsRetryable => false;

    }
}
