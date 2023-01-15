/*
 * Copyright 2021 S. Webber
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
namespace Org.NProlog.Core.Predicate.Builtin.Flow;

[TestClass]

public class CutTest : TestUtils
{
    [TestMethod]
    public void TestIsAlwaysCutOnBacktrack()
    {
        Cut c = new Cut();
        Assert.IsTrue(c.IsAlwaysCutOnBacktrack);
    }

    [TestMethod]
    public void TestIsRetryable()
    {
        Cut c = new Cut();
        Assert.IsTrue(c.IsRetryable);
    }

    [TestMethod]
    public void TestGetPredicate()
    {
        Cut c = new Cut();

        Predicate p1 = c.GetPredicate();
        Assert.IsTrue(p1.Evaluate());
        AssertThrows(typeof(CutException), () => p1.Evaluate());

        Predicate p2 = c.GetPredicate();
        Assert.IsTrue(p2.Evaluate());
        AssertThrows(typeof(CutException), () => p2.Evaluate());
    }
}
