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

[TestClass]
public class TermTypeTest
{
    [TestMethod]
    public void TestIsNumeric()
    {
        Assert.IsTrue(TermType.FRACTION.IsNumeric);
        Assert.IsTrue(TermType.INTEGER.IsNumeric);

        Assert.IsFalse(TermType.ATOM.IsNumeric);
        Assert.IsFalse(TermType.EMPTY_LIST.IsNumeric);
        Assert.IsFalse(TermType.LIST.IsNumeric);
        Assert.IsFalse(TermType.STRUCTURE.IsNumeric);
        Assert.IsFalse(TermType.VARIABLE.IsNumeric);
        Assert.IsFalse(TermType.CLP_VARIABLE.IsNumeric);
    }

    [TestMethod]
    public void TestIsStructure()
    {
        Assert.IsTrue(TermType.LIST.IsStructure);
        Assert.IsTrue(TermType.STRUCTURE.IsStructure);

        Assert.IsFalse(TermType.EMPTY_LIST.IsStructure);
        Assert.IsFalse(TermType.FRACTION.IsStructure);
        Assert.IsFalse(TermType.INTEGER.IsStructure);
        Assert.IsFalse(TermType.ATOM.IsStructure);
        Assert.IsFalse(TermType.VARIABLE.IsStructure);
        Assert.IsFalse(TermType.CLP_VARIABLE.IsStructure);
    }

    [TestMethod]
    public void TestIsVariable()
    {
        Assert.IsTrue(TermType.VARIABLE.IsVariable);

        Assert.IsFalse(TermType.CLP_VARIABLE.IsVariable);
        Assert.IsFalse(TermType.INTEGER.IsVariable);
        Assert.IsFalse(TermType.ATOM.IsVariable);
        Assert.IsFalse(TermType.EMPTY_LIST.IsVariable);
        Assert.IsFalse(TermType.LIST.IsVariable);
        Assert.IsFalse(TermType.STRUCTURE.IsVariable);
    }

    [TestMethod]
    public void TestGetPrecedence()
    {
        Assert.AreEqual(1, TermType.VARIABLE.Precedence);
        Assert.AreEqual(2, TermType.CLP_VARIABLE.Precedence);
        Assert.AreEqual(3, TermType.FRACTION.Precedence);
        Assert.AreEqual(4, TermType.INTEGER.Precedence);
        Assert.AreEqual(5, TermType.EMPTY_LIST.Precedence);
        Assert.AreEqual(6, TermType.ATOM.Precedence);
        // all compound structures share the same precedence
        Assert.AreEqual(7, TermType.STRUCTURE.Precedence);
        Assert.AreEqual(8, TermType.LIST.Precedence);
    }
}
