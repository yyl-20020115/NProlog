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
namespace Org.NProlog.Core.Terms;

/**
 * @see TermTest
 */
//@RunWith(DataProviderRunner)
[TestClass]
public class EmptyListTest
{
    [TestMethod]
    public void TestGetName() => Assert.AreEqual(".", EmptyList.EMPTY_LIST.Name);

    [TestMethod]
    public void TestToString() => Assert.AreEqual("[]", EmptyList.EMPTY_LIST.ToString());

    [TestMethod]
    public void TestGetTerm()
    {
        var e = EmptyList.EMPTY_LIST.Term;
        Assert.AreSame(EmptyList.EMPTY_LIST, e);
    }

    [TestMethod]
    public void TestGetBound()
    {
        var e = EmptyList.EMPTY_LIST.Bound;
        Assert.AreSame(EmptyList.EMPTY_LIST, e);
    }

    [TestMethod]
    public void TestGetType()
    {
        Assert.AreSame(TermType.EMPTY_LIST, EmptyList.EMPTY_LIST.Type);
    }

    [TestMethod]
    public void TestGetNumberOfArguments()
    {
        Assert.AreEqual(0, EmptyList.EMPTY_LIST.NumberOfArguments);
    }

    [TestMethod]
    public void TestGetArgument()
    {
        for (int index = -1; index <= 1; index++)
            try
            {
                EmptyList.EMPTY_LIST.GetArgument(index);
                Assert.Fail();
            }
            catch (IndexOutOfRangeException e)
            {
                Assert.AreEqual("index:" + index, e.Message);
            }
    }

    [TestMethod]
    public void TestGetArgs() 
        => Assert.AreSame(TermUtils.EMPTY_ARRAY, EmptyList.EMPTY_LIST.Args);
}
