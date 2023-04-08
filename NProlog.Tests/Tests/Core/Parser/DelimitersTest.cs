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
namespace Org.NProlog.Core.Parser;

[TestClass]
public class DelimitersTest
{
    [TestMethod]
    public void TestArgumentSeperator()
    {
        Assert.IsTrue(Delimiters.IsArgumentSeperator(Symbol(",")));
        Assert.IsFalse(Delimiters.IsArgumentSeperator(Atom(",")));
        Assert.IsFalse(Delimiters.IsArgumentSeperator(Symbol(";")));
        Assert.IsFalse(Delimiters.IsArgumentSeperator(Symbol(" ")));
        Assert.IsFalse(Delimiters.IsArgumentSeperator(null));
    }

    [TestMethod]
    public void TestListOpenBracket()
    {
        Assert.IsTrue(Delimiters.IsListOpenBracket(Symbol("[")));
        Assert.IsFalse(Delimiters.IsListOpenBracket(Atom("[")));
        Assert.IsFalse(Delimiters.IsListOpenBracket(Symbol("]")));
        Assert.IsFalse(Delimiters.IsListOpenBracket(Symbol("(")));
        Assert.IsFalse(Delimiters.IsListOpenBracket(null));
    }

    [TestMethod]
    public void TestListCloseBracket()
    {
        Assert.IsTrue(Delimiters.IsListCloseBracket(Symbol("]")));
        Assert.IsFalse(Delimiters.IsListCloseBracket(Atom("]")));
        Assert.IsFalse(Delimiters.IsListCloseBracket(Symbol("[")));
        Assert.IsFalse(Delimiters.IsListCloseBracket(Symbol(")")));
        Assert.IsFalse(Delimiters.IsListCloseBracket(null));
    }

    [TestMethod]
    public void TestPredicateOpenBracket()
    {
        Assert.IsTrue(Delimiters.IsPredicateOpenBracket(Symbol("(")));
        Assert.IsFalse(Delimiters.IsPredicateOpenBracket(Atom("(")));
        Assert.IsFalse(Delimiters.IsPredicateOpenBracket(Symbol(")")));
        Assert.IsFalse(Delimiters.IsPredicateOpenBracket(Symbol("[")));
        Assert.IsFalse(Delimiters.IsPredicateOpenBracket(null));
    }

    [TestMethod]
    public void TestPredicateCloseBracket()
    {
        Assert.IsTrue(Delimiters.IsPredicateCloseBracket(Symbol(")")));
        Assert.IsFalse(Delimiters.IsPredicateCloseBracket(Atom(")")));
        Assert.IsFalse(Delimiters.IsPredicateCloseBracket(Symbol("(")));
        Assert.IsFalse(Delimiters.IsPredicateCloseBracket(Symbol("]")));
        Assert.IsFalse(Delimiters.IsPredicateCloseBracket(null));
    }

    [TestMethod]
    public void TestListTail()
    {
        Assert.IsTrue(Delimiters.IsListTail(Symbol("|")));
        Assert.IsFalse(Delimiters.IsListTail(Atom("|")));
        Assert.IsFalse(Delimiters.IsListTail(Symbol("[")));
        Assert.IsFalse(Delimiters.IsListTail(Symbol("]")));
        Assert.IsFalse(Delimiters.IsListTail(null));
    }

    [TestMethod]
    public void TestSentenceTerminator()
    {
        Assert.IsTrue(Delimiters.IsSentenceTerminator(Symbol(".")));
        Assert.IsFalse(Delimiters.IsSentenceTerminator(Atom(".")));
        Assert.IsFalse(Delimiters.IsSentenceTerminator(Symbol("..=")));
        Assert.IsFalse(Delimiters.IsSentenceTerminator(Symbol(",")));
        Assert.IsFalse(Delimiters.IsSentenceTerminator(null));
    }

    [TestMethod]
    public void TestDelimiter()
    {
        AssertDelimiter(true, '[', ']', '(', ')', '|', ',', '.');
        AssertDelimiter(false, '!', '?', '{', '}', ':', ';', '-', 'a', 'A', '1');
        Assert.IsFalse(Delimiters.IsDelimiter("..="));
        Assert.IsFalse(Delimiters.IsDelimiter(""));
    }

    private static void AssertDelimiter(bool expectedResult, params char[] chars)
    {
        foreach (var c in chars)
        {
            Assert.AreEqual(expectedResult, Delimiters.IsDelimiter(c));
            Assert.AreEqual(expectedResult, Delimiters.IsDelimiter(c.ToString()));
        }
    }

    private static Token Symbol(string value) => new (value, TokenType.SYMBOL);

    private static Token Atom(string value) => new (value, TokenType.ATOM);
}
