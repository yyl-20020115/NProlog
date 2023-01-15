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
public class KeyFactoriesTest : TestUtils
{
    private static readonly Atom A = Atom("a");
    private static readonly Atom B = Atom("b");
    private static readonly Atom C = Atom("c");
    private static readonly Atom D = Atom("d");
    private static readonly Atom E = Atom("e");

    [TestMethod]
    public void TestLimit()
    {
        Assert.AreEqual(3, KeyFactories.MAX_ARGUMENTS_PER_INDEX);
    }

    [TestMethod]
    public void TestOne()
    {
        Term[] args = new Term[] { A, B, C };
        KeyFactory kf = KeyFactories.GetKeyFactory(1);

        Assert.AreSame(A, kf.CreateKey(new int[] { 0 }, args));
        Assert.AreSame(B, kf.CreateKey(new int[] { 1 }, args));
        Assert.AreSame(C, kf.CreateKey(new int[] { 2 }, args));
    }

    [TestMethod]
    public void TestTwo()
    {
        Term[] args = new Term[] { A, B, C };
        KeyFactory kf = KeyFactories.GetKeyFactory(2);

        Object k = kf.CreateKey(new int[] { 0, 1 }, args);

        AssertNotEqualsHashCode(k, kf.CreateKey(new int[] { 0, 2 }, args));
        AssertNotEqualsHashCode(k, kf.CreateKey(new int[] { 1, 2 }, args));
        AssertNotEqualsHashCode(k, kf.CreateKey(new int[] { 0, 1 }, new Term[] { B, A, C }));

        AssertEqualsHashCode(k, kf.CreateKey(new int[] { 0, 1 }, args));
        AssertEqualsHashCode(k, kf.CreateKey(new int[] { 0, 1 }, new Term[] { A, B, D }));
    }

    [TestMethod]
    public void TestThree()
    {
        Term[] args = new Term[] { A, B, C, D };
        KeyFactory kf = KeyFactories.GetKeyFactory(3);

        Object k = kf.CreateKey(new int[] { 0, 1, 2 }, args);

        AssertNotEqualsHashCode(k, kf.CreateKey(new int[] { 0, 2, 3 }, args));
        AssertNotEqualsHashCode(k, kf.CreateKey(new int[] { 1, 2, 3 }, args));
        AssertNotEqualsHashCode(k, kf.CreateKey(new int[] { 0, 1, 2 }, new Term[] { A, C, B, D }));

        AssertEqualsHashCode(k, kf.CreateKey(new int[] { 0, 1, 2 }, args));
        AssertEqualsHashCode(k, kf.CreateKey(new int[] { 0, 1, 2 }, new Term[] { A, B, C, E }));
    }

    private static void AssertEqualsHashCode(Object o1, Object o2)
    { // TODO move to TestUtils
        Assert.IsTrue(o1.Equals(o2));
        Assert.IsTrue(o2.Equals(o1));
        Assert.AreEqual(o1.GetHashCode(), o2.GetHashCode());
    }
}
