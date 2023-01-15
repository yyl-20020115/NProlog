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
        Assert.IsTrue(TermType.FRACTION.isNumeric);
        Assert.IsTrue(TermType.INTEGER.isNumeric);

        Assert.IsFalse(TermType.ATOM.isNumeric);
        Assert.IsFalse(TermType.EMPTY_LIST.isNumeric);
        Assert.IsFalse(TermType.LIST.isNumeric);
        Assert.IsFalse(TermType.STRUCTURE.isNumeric);
        Assert.IsFalse(TermType.VARIABLE.isNumeric);
        Assert.IsFalse(TermType.CLP_VARIABLE.isNumeric);
    }

    [TestMethod]
    public void TestIsStructure()
    {
        Assert.IsTrue(TermType.LIST.isStructure);
        Assert.IsTrue(TermType.STRUCTURE.isStructure);

        Assert.IsFalse(TermType.EMPTY_LIST.isStructure);
        Assert.IsFalse(TermType.FRACTION.isStructure);
        Assert.IsFalse(TermType.INTEGER.isStructure);
        Assert.IsFalse(TermType.ATOM.isStructure);
        Assert.IsFalse(TermType.VARIABLE.isStructure);
        Assert.IsFalse(TermType.CLP_VARIABLE.isStructure);
    }

    [TestMethod]
    public void TestIsVariable()
    {
        Assert.IsTrue(TermType.VARIABLE.isVariable);

        Assert.IsFalse(TermType.CLP_VARIABLE.isVariable);
        Assert.IsFalse(TermType.INTEGER.isVariable);
        Assert.IsFalse(TermType.ATOM.isVariable);
        Assert.IsFalse(TermType.EMPTY_LIST.isVariable);
        Assert.IsFalse(TermType.LIST.isVariable);
        Assert.IsFalse(TermType.STRUCTURE.isVariable);
    }

    [TestMethod]
    public void TestGetPrecedence()
    {
        Assert.AreEqual(1, TermType.VARIABLE.precedence);
        Assert.AreEqual(2, TermType.CLP_VARIABLE.precedence);
        Assert.AreEqual(3, TermType.FRACTION.precedence);
        Assert.AreEqual(4, TermType.INTEGER.precedence);
        Assert.AreEqual(5, TermType.EMPTY_LIST.precedence);
        Assert.AreEqual(6, TermType.ATOM.precedence);
        // all compound structures share the same precedence
        Assert.AreEqual(7, TermType.STRUCTURE.precedence);
        Assert.AreEqual(8, TermType.LIST.precedence);
    }
}
