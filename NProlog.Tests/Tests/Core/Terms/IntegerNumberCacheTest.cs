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
namespace Org.NProlog.Core.Terms;

[TestClass]
public class IntegerNumberCacheTest
{
    private static readonly int MIN_CACHED_VALUE = -128;
    private static readonly int MAX_CACHED_VALUE = 127;

    [TestMethod]
    public void TestZero()
    {
        Assert.AreEqual(new IntegerNumber(0), IntegerNumberCache.ZERO);
        Assert.AreSame(IntegerNumberCache.ValueOf(0), IntegerNumberCache.ZERO);
    }

    [TestMethod]
    public void TestCached()
    {
        for (int i = MIN_CACHED_VALUE; i <= MAX_CACHED_VALUE; i++)
        {
            Assert.AreSame(IntegerNumberCache.ValueOf(i), IntegerNumberCache.ValueOf(i));
            Assert.AreEqual(new IntegerNumber(i), IntegerNumberCache.ValueOf(i));
        }
    }

    [TestMethod]
    public void TestOutsideCacheTooLow()
    {
        long value = MIN_CACHED_VALUE - 1;
        Assert.AreNotSame(IntegerNumberCache.ValueOf(value), IntegerNumberCache.ValueOf(value));
        Assert.AreEqual(new IntegerNumber(value), IntegerNumberCache.ValueOf(value));
    }

    [TestMethod]
    public void TestOutsideCacheTooHigh()
    {
        long value = MAX_CACHED_VALUE + 1;
        Assert.AreNotSame(IntegerNumberCache.ValueOf(value), IntegerNumberCache.ValueOf(value));
        Assert.AreEqual(new IntegerNumber(value), IntegerNumberCache.ValueOf(value));
    }

    [TestMethod]
    public void TestMinimumLongValue()
    {
        Assert.AreNotSame(IntegerNumberCache.ValueOf(long.MinValue), IntegerNumberCache.ValueOf(long.MinValue));
        Assert.AreEqual(new IntegerNumber(long.MinValue), IntegerNumberCache.ValueOf(long.MinValue));
    }

    [TestMethod]
    public void TestMaximumLongValue()
    {
        Assert.AreNotSame(IntegerNumberCache.ValueOf(long.MaxValue), IntegerNumberCache.ValueOf(long.MaxValue));
        Assert.AreEqual(new IntegerNumber(long.MaxValue), IntegerNumberCache.ValueOf(long.MaxValue));
    }
}
