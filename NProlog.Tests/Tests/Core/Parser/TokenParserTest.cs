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
namespace Org.NProlog.Core.Parser;

[TestClass]
public class TokenParserTest : TestUtils
{
    private readonly Operands operands = CreateKnowledgeBase().Operands;

    [TestMethod]
    public void TestAtom()
    {
        AssertTokenType("a", TokenType.ATOM);
        AssertTokenType("ab", TokenType.ATOM);
        AssertTokenType("aB", TokenType.ATOM);
        AssertTokenType("a1", TokenType.ATOM);
        AssertTokenType("a_", TokenType.ATOM);
        AssertTokenType("a_2bY", TokenType.ATOM);
    }

    [TestMethod]
    public void TestQuotedAtom()
    {
        AssertTokenType("'abcdefg'", "abcdefg", TokenType.ATOM);
        AssertTokenType("'@'", "@", TokenType.ATOM);
        AssertTokenType("','", ",", TokenType.ATOM);
        AssertTokenType("''", "", TokenType.ATOM);
        AssertTokenType("''''", "'", TokenType.ATOM);
        AssertTokenType("''''''''''", "''''", TokenType.ATOM);
        AssertTokenType("'q 1 \" 0.5 | '' @#~'", "q 1 \" 0.5 | ' @#~", TokenType.ATOM);
    }

    [TestMethod]
    public void TestSymbol()
    {
        AssertTokenType("@", "@", TokenType.SYMBOL);
        AssertTokenType("@$%", "@$%", TokenType.SYMBOL);
        AssertTokenType(",", ",", TokenType.SYMBOL);
        AssertTokenType(".", ".", TokenType.SYMBOL);
    }

    [TestMethod]
    public void TestVariable()
    {
        AssertTokenType("X", TokenType.VARIABLE);
        AssertTokenType("XY", TokenType.VARIABLE);
        AssertTokenType("Xy", TokenType.VARIABLE);
        AssertTokenType("X1", TokenType.VARIABLE);
        AssertTokenType("X_", TokenType.VARIABLE);
        AssertTokenType("X_7hU", TokenType.VARIABLE);
        AssertTokenType("__", TokenType.VARIABLE);
        AssertTokenType("_X", TokenType.VARIABLE);
        AssertTokenType("_x", TokenType.VARIABLE);
        AssertTokenType("_2", TokenType.VARIABLE);
        AssertTokenType("_X_2a", TokenType.VARIABLE);
    }

    [TestMethod]
    public void TestAnonymousVariable()
    {
        AssertTokenType("_", TokenType.VARIABLE);
    }

    [TestMethod]
    public void TestInteger()
    {
        AssertTokenType("0", TokenType.INTEGER);
        AssertTokenType("1", TokenType.INTEGER);
        AssertTokenType("6465456456", TokenType.INTEGER);
    }

    [TestMethod]
    public void TestFloat()
    {
        AssertTokenType("0.0", TokenType.FLOAT);
        AssertTokenType("0.1", TokenType.FLOAT);
        AssertTokenType("768.567567", TokenType.FLOAT);
        AssertTokenType("3.4028235E38", TokenType.FLOAT);
        AssertTokenType("3.4028235e38", TokenType.FLOAT);
    }

    [TestMethod]
    public void TestUnescapedCharCode()
    {
        for (char c = '!'; c <= '~'; c++)
        {
            // ignore escape character - that is tested in testEscapedCharCode instead
            if (c != '\\')
            {
                AssertCharCode("0'" + c, c);
            }
        }
    }

    [TestMethod]
    public void TestEscapedCharCode()
    {
        AssertCharCode("0'\\t", '\t');
        AssertCharCode("0'\\b", '\b');
        AssertCharCode("0'\\n", '\n');
        AssertCharCode("0'\\r", '\r');
        AssertCharCode("0'\\f", '\f');
        AssertCharCode("0'\\'", '\'');
        AssertCharCode("0'\\\"", '\"');
        AssertCharCode("0'\\\\", '\\');
    }

    [TestMethod]
    public void TestInvalidEscapedCharCode()
    {
        AssertParserException("0'\\a", "invalid character escape sequence Line: 0'\\a");
        AssertParserException("0'\\A", "invalid character escape sequence Line: 0'\\A");
        AssertParserException("0'\\1", "invalid character escape sequence Line: 0'\\1");
        AssertParserException("0'\\ ", "invalid character escape sequence Line: 0'\\ ");
        AssertParserException("0'\\.", "invalid character escape sequence Line: 0'\\.");
    }

    [TestMethod]
    public void TestUnicodeCharCode()
    {
        AssertCharCode("0'\\u0020", ' ');
        AssertCharCode("0'\\u0061", 'a');
        AssertCharCode("0'\\u0059", 'Y');
        AssertCharCode("0'\\u00A5", 165);
        AssertCharCode("0'\\u017F", 383);
        AssertCharCode("0'\\u1E6A", '\u1E6A');
        AssertCharCode("0'\\u1EF3", '\u1EF3');
        AssertCharCode("0'\\u00a5", 165);
        AssertCharCode("0'\\u1ef3", '\u1EF3');
        AssertCharCode("0'\\uabcd", '\uabcd');
        AssertCharCode("0'\\u1eF3", '\u1EF3');
    }

    [TestMethod]
    public void TestInvalidUnicodeCharCode()
    {
        // not letters or numbers
        AssertParserException("0'\\u12-4", "invalid unicode value Line: 0'\\u12-4");
        AssertParserException("0'\\u12/4", "invalid unicode value Line: 0'\\u12/4");
        AssertParserException("0'\\u12:4", "invalid unicode value Line: 0'\\u12:4");
        AssertParserException("0'\\u12@4", "invalid unicode value Line: 0'\\u12@4");

        // not hex letter
        AssertParserException("0'\\u12G4", "invalid unicode value Line: 0'\\u12G4");
        AssertParserException("0'\\u12g4", "invalid unicode value Line: 0'\\u12g4");
        AssertParserException("0'\\u12Z4", "invalid unicode value Line: 0'\\u12Z4");
        AssertParserException("0'\\u12z4", "invalid unicode value Line: 0'\\u12z4");

        // too short
        AssertParserException("0'\\u12", "invalid unicode value Line: 0'\\u12");
        AssertParserException("0'\\u12\n4", "invalid unicode value Line: 0'\\u12");
        AssertParserException("0'\\u12.", "invalid unicode value Line: 0'\\u12.");
        AssertParserException("0'\\u.", "invalid unicode value Line: 0'\\u.");
        AssertParserException("0'\\u", "invalid unicode value Line: 0'\\u");
    }

    [TestMethod]
    public void TestEmptyInput()
    {
        Assert.IsFalse(Create("").HasNext);
        Assert.IsFalse(Create("\t \r\n   ").HasNext);
        Assert.IsFalse(Create("%abcde").HasNext); // single line comment
        Assert.IsFalse(Create("/* hgjh\nghj*/").HasNext); // multi line comment
    }

    [TestMethod]
    public void TestSequence()
    {
        AssertParse("Abc12.5@>=-0_2_jgkj a-2hUY_ty\nu\n% kghjgkj\na/*b*/c 0'zyz 0' 0'\u00610'\u0062345", "Abc12", ".", "5", "@>=", "-", "0", "_2_jgkj", "a", "-", "2", "hUY_ty", "u",
                    "a", "c", "122", "yz", "32", "97", "98", "345");
    }

    [TestMethod]
    public void TestSentence()
    {
        AssertParse("X is ~( 'Y', 1 ,a).", "X", "is", "~", "(", "Y", ",", "1", ",", "a", ")", ".");
    }

    [TestMethod]
    public void TestNonAlphanumericCharacterFollowedByPeriod()
    {
        AssertParse("!.", "!", ".");
    }

    /** Test that "!" and ";" get parsed separately, rather than as single combined "!;" element. */
    [TestMethod]
    public void TestCutFollowedByDisjunction()
    {
        AssertParse("!;true", "!", ";", "true");
    }

    /** Test that "(", "!", ")" and "." get parsed separately, rather than as single combined "(!)." element. */
    [TestMethod]
    public void TestCutInBrackets()
    {
        AssertParse("(!).", "(", "!", ")", ".");
    }

    [TestMethod]
    public void TestWhitespaceAndComments()
    {
        TokenParser p = Create("/* comment */\t % comment\n % comment\r\n\n");
        Assert.IsFalse(p.HasNext);
    }

    [TestMethod]
    public void TestMultiLineComments()
    {
        AssertParse("/*\n\n*\n/\n*/a/*/b*c/d/*e*/f", "a", "f");
    }

    [TestMethod]
    public void TestFollowedByTerm()
    {
        TokenParser tp = Create("?- , [ abc )");
        tp.Next();
        Assert.IsFalse(tp.IsFollowedByTerm());
        tp.Next();
        Assert.IsTrue(tp.IsFollowedByTerm());
        tp.Next();
        Assert.IsTrue(tp.IsFollowedByTerm());
        tp.Next();
        Assert.IsFalse(tp.IsFollowedByTerm());
    }

    /** @see {@link TokenParser#rewind(string)} */
    [TestMethod]
    public void TestRewindException()
    {
        TokenParser tp = Create("a b c");
        Assert.AreEqual("a", tp.Next().Name);
        Token b = tp.Next();
        Assert.AreEqual("b", b.Name);
        tp.Rewind(b);
        Assert.AreSame(b, tp.Next());
        tp.Rewind(b);

        // check that can only rewind one token
        AssertRewindException(tp, "b");
        AssertRewindException(tp, "a");

        Assert.AreEqual("b", tp.Next().Name);
        Token c = tp.Next();
        Assert.AreEqual("c", c.Name);

        // check that the value specified in call to rewind has to be the last value parsed
        AssertRewindException(tp, "b");
        AssertRewindException(tp, null);
        AssertRewindException(tp, "z");

        tp.Rewind(c);
        Assert.AreSame(c, tp.Next());
        Assert.IsFalse(tp.HasNext);
        tp.Rewind(c);
        Assert.IsTrue(tp.HasNext);

        // check that can only rewind one token
        AssertRewindException(tp, "c");
    }

    private void AssertRewindException(TokenParser tp, string value)
    {
        try
        {
            tp.Rewind(new Token(value, TokenType.ATOM));
            Assert.Fail();
        }
        catch (ArgumentException e)
        {
            // expected
        }
    }

    private void AssertCharCode(string input, int expectedOutput)
    {
        AssertTokenType(input, expectedOutput.ToString(), TokenType.INTEGER);
    }

    private void AssertTokenType(string syntax, TokenType type)
    {
        AssertTokenType(syntax, syntax, type);
    }

    private void AssertTokenType(string syntax, string value, TokenType type)
    {
        TokenParser p = Create(syntax);
        Assert.IsTrue(p.HasNext);
        Token token = p.Next();
        Assert.AreEqual(value, token.Name);
        Assert.AreEqual(type, token.Type);
        Assert.IsFalse(p.HasNext);
    }

    private void AssertParse(string sentence, params string[] tokens)
    {
        TokenParser tp = Create(sentence);
        foreach (string w in tokens)
        {
            Token next = tp.Next();
            Assert.AreEqual(w, next.Name);
            tp.Rewind(next);
            Assert.AreSame(next, tp.Next());
        }
        Assert.IsFalse(tp.HasNext);
        try
        {
            tp.Next();
            Assert.Fail();
        }
        catch (ParserException e)
        {
            Assert.AreEqual("Unexpected end of stream Line: " + e.Line, e.Message);
        }
    }

    private void AssertParserException(string input, string expectedExceptionMessage)
    {
        try
        {
            TokenParser p = Create(input);
            p.Next();
            Assert.Fail();
        }
        catch (ParserException e)
        {
            Assert.AreEqual(expectedExceptionMessage, e.Message);
        }
    }

    private TokenParser Create(string syntax)
    {
        StringReader sr = new StringReader(syntax);
        return new TokenParser(sr, operands);
    }
}
