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

using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Predicate.Builtin.Db;

[TestClass]
public class RecordedDatabaseTest
{
    [TestMethod]
    public void TestAdd()
    {
        var d = new RecordedDatabase();
        var key = new PredicateKey("a", 1);
        var value = new Atom("test");

        Assert.AreEqual(0L, d.Add(key, value, true).Long);
        Assert.AreEqual(1L, d.Add(key, value, true).Long);
        Assert.AreEqual(2L, d.Add(key, value, false).Long);
        Assert.AreEqual(3L, d.Add(key, value, false).Long);
        Assert.AreEqual(4L, d.Add(key, value, true).Long);
    }

    [TestMethod]
    public void TestGetAllEmpty()
    {
        var d = new RecordedDatabase();

        var itr = d.GetAll();

        Assert.IsFalse(itr.MoveNext());
        AssertNoMoreElements(itr);
    }

    [TestMethod]
    public void TestGetAllSingleElement()
    {
        var d = new RecordedDatabase();

        var key = new PredicateKey("a", 1);
        var value = new Atom("test");
        var reference = d.Add(key, value, true);

        var itr = d.GetAll();

        AssertNext(itr, key, reference, value);

        AssertNoMoreElements(itr);
    }

    [TestMethod]
    public void TestGetAllMultipleElementsAddLast()
    {
        var d = new RecordedDatabase();

        var key = new PredicateKey("a", 1);
        var value1 = new Atom("test1");
        var value2 = new Atom("test2");
        var value3 = new Atom("test3");
        var reference1 = d.Add(key, value1, true);
        var reference2 = d.Add(key, value2, true);
        var reference3 = d.Add(key, value3, true);

        var itr = d.GetAll();

        AssertNext(itr, key, reference1, value1);
        AssertNext(itr, key, reference2, value2);
        AssertNext(itr, key, reference3, value3);

        AssertNoMoreElements(itr);
    }

    [TestMethod]
    public void TestGetAllMultipleElementsAddFirst()
    {
        var d = new RecordedDatabase();

        var key = new PredicateKey("a", 1);
        var value1 = new Atom("test1");
        var value2 = new Atom("test2");
        var value3 = new Atom("test3");
        var reference1 = d.Add(key, value1, false);
        var reference2 = d.Add(key, value2, false);
        var reference3 = d.Add(key, value3, false);

        var itr = d.GetAll();

        AssertNext(itr, key, reference3, value3);
        AssertNext(itr, key, reference2, value2);
        AssertNext(itr, key, reference1, value1);

        AssertNoMoreElements(itr);
    }

    [TestMethod]
    public void TestGetAllMultipleElementsAddFirstAndLast()
    {
        var d = new RecordedDatabase();

        var key = new PredicateKey("a", 1);
        var value1 = new Atom("test1");
        var value2 = new Atom("test2");
        var value3 = new Atom("test3");
        var reference1 = d.Add(key, value1, true);
        var reference2 = d.Add(key, value2, false);
        var reference3 = d.Add(key, value3, true);

        var itr = d.GetAll();

        AssertNext(itr, key, reference2, value2);
        AssertNext(itr, key, reference1, value1);
        AssertNext(itr, key, reference3, value3);

        AssertNoMoreElements(itr);
    }

    [TestMethod]
    public void TestErase()
    {
        var d = new RecordedDatabase();

        var key = new PredicateKey("a", 1);
        var value1 = new Atom("test1");
        var value2 = new Atom("test2");
        var value3 = new Atom("test3");
        var reference1 = d.Add(key, value1, true);
        var reference2 = d.Add(key, value2, true);
        var reference3 = d.Add(key, value3, true);

        Assert.IsTrue(d.Erase(reference2.Long));
        var itr = d.GetAll();
        AssertNext(itr, key, reference1, value1);
        AssertNext(itr, key, reference3, value3);
        AssertNoMoreElements(itr);

        Assert.IsFalse(d.Erase(reference2.Long));
        itr = d.GetAll();
        AssertNext(itr, key, reference1, value1);
        AssertNext(itr, key, reference3, value3);
        AssertNoMoreElements(itr);

        Assert.IsTrue(d.Erase(reference1.Long));
        itr = d.GetAll();
        AssertNext(itr, key, reference3, value3);
        AssertNoMoreElements(itr);

        Assert.IsTrue(d.Erase(reference3.Long));
        itr = d.GetAll();
        AssertNoMoreElements(itr);

        Assert.IsFalse(d.Erase(reference3.Long));
    }

    [TestMethod]
    public void TestGetAllMultipleKeys()
    {
        var d = new RecordedDatabase();
        var key1 = new PredicateKey("a", 1);
        var key2 = new PredicateKey("b", 1);
        var value1 = new Atom("test1");
        var value2 = new Atom("test2");
        var value3 = new Atom("test3");
        var reference1 = d.Add(key1, value1, true);
        var reference2 = d.Add(key2, value2, true);
        var reference3 = d.Add(key1, value3, true);

        var itr = d.GetAll();

        AssertNext(itr, key1, reference1, value1);
        AssertNext(itr, key1, reference3, value3);
        AssertNext(itr, key2, reference2, value2);

        AssertNoMoreElements(itr);
    }

    [TestMethod]
    public void TestGetChainEmpty()
    {
        var d = new RecordedDatabase();
        var key = new PredicateKey("a", 1);

        var itr = d.GetChain(key);

        Assert.IsFalse(itr.MoveNext());
        AssertNoMoreElements(itr);
    }

    [TestMethod]
    public void TestGetChainSingleElement()
    {
        var d = new RecordedDatabase();
        var key = new PredicateKey("a", 1);
        var value = new Atom("test");
        var reference = d.Add(key, value, true);

        var itr = d.GetChain(key);

        AssertNext(itr, key, reference, value);

        AssertNoMoreElements(itr);
    }

    [TestMethod]
    public void TestGetChainMultipleKeys()
    {
        var d = new RecordedDatabase();
        var key1 = new PredicateKey("a", 1);
        var key2 = new PredicateKey("b", 1);
        var value1 = new Atom("test1");
        var value2 = new Atom("test2");
        var value3 = new Atom("test3");
        var reference1 = d.Add(key1, value1, true);
        var reference2 = d.Add(key2, value2, true);
        var reference3 = d.Add(key1, value3, true);

        var itr1 = d.GetChain(key1);
        var itr2 = d.GetChain(key2);

        AssertNext(itr1, key1, reference1, value1);
        AssertNext(itr1, key1, reference3, value3);
        AssertNoMoreElements(itr1);

        AssertNext(itr2, key2, reference2, value2);
        AssertNoMoreElements(itr2);
    }

    private void AssertNext(ICheckedEnumerator<Record> itr, PredicateKey key, Term reference, Term value)
    {
        Assert.IsTrue(itr.CanMoveNext);
        itr.MoveNext();
        Record r = itr.Current;
        AssertKey(key, r.Key);
        Assert.AreEqual(reference, r.Reference);
        Assert.AreEqual(value, r.Value);
    }

    private static void AssertKey(PredicateKey key, Term term)
    {
        int numberOfArguments = key.NumArgs;
        Assert.AreEqual(numberOfArguments, term.NumberOfArguments);
        HashSet<Term> uniqueTerms = new();
        for (int i = 0; i < numberOfArguments; i++)
        {
            Term a = term.GetArgument(i);
            Assert.IsTrue(uniqueTerms.Add(a));
            Assert.AreSame(TermType.VARIABLE, a.Type);
            Assert.AreEqual("_", a.ToString());
        }
    }

    private static void AssertNoMoreElements(IEnumerator<Record> itr)
    {
        Assert.IsFalse(itr.MoveNext());
        try
        {
            var m = itr.Current;
            Assert.Fail();
        }
        catch (NoSuchElementException )
        {
            // expected
        }
    }
}
