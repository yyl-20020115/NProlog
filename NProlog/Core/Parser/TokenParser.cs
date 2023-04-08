/*
 * Copyright 2013 S. Webber
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System.Text;

namespace Org.NProlog.Core.Parser;



/**
 * Parses an input stream into discrete 'tokens' that are used to represent Prolog queries and rules.
 *
 * @see SentenceParser
 */
public class TokenParser
{
    private readonly CharacterParser parser;
    private readonly Operands operands;
    private Token lastParsedToken;
    private bool rewound = false;

    public TokenParser(TextReader reader, Operands operands)
    {
        this.parser = new CharacterParser(reader);
        this.operands = operands;
        this.lastParsedToken = Token.Default;
    }

    /** @return {@code true} if there are more tokens to be parsed, else {@code false} */
    public bool HasNext
    {
        get
        {
            if (rewound)
            {
                return true;
            }
            else
            {
                SkipWhitespaceAndComments();
                return !IsEndOfStream(parser.Peek());
            }
        }
    }

    /**
     * Parse and return the next {@code Token}.
     *
     * @return the token that was parsed as a result of this call
     * @throws ParserException if there are no more tokens to parse (i.e. parser has reached the end of the underlying
     * input stream)
     */
    public Token Next()
    {
        if (rewound)
            this.rewound = false;
        else
            this.lastParsedToken = ParseToken();
        return this.lastParsedToken;
    }

    private Token ParseToken()
    {
        SkipWhitespaceAndComments();
        int c = parser.GetNext();
        if (IsEndOfStream(c))
            throw NewParserException("Unexpected end of stream");
        else if (IsVariable(c))
            return ParseText(c, TokenType.VARIABLE);
        else if (char.IsLower((char)c))
            return ParseText(c, TokenType.ATOM);
        else if (IsQuote(c))
            return ParseQuotedText();
        else if (IsZero(c))
            return ParseLeadingZero(c);
        else if (char.IsDigit((char)c))
            return ParseNumber(c);
        else
            return ParseSymbol(c);
    }

    /**
     * Rewinds the parser (i.e. "pushes-back" the last parsed token).
     * <p>
     * The last parsed value will remain after the next call to {@link #next()}
     *
     * @param value the value to rewind
     * @throws ArgumentException if already in a rewound state (i.e. have already called
     * {@link TokenParser#rewind(string)} since the last call to {@link #next()}), or {@code value} is not equal to
     * {@link #getValue()}
     */
    public void Rewind(Token? value)
    {
        if (this.lastParsedToken != value)
            throw new ArgumentException("invalid argument",nameof(value));
        this.rewound = true;
    }

    /** Does the next value to be parsed represent a term (rather than a delimiter) */
    public bool IsFollowedByTerm()
    {
        SkipWhitespaceAndComments();
        int nextChar = parser.Peek();
        return Delimiters.IsListOpenBracket(nextChar) || !Delimiters.IsDelimiter(nextChar);
    }

    /** Returns a new {@link ParserException} with the specified message. */
    public ParserException NewParserException(string message)
        => new (message, parser);

    private void SkipWhitespaceAndComments()
    {
        while (true)
        {
            int c = parser.GetNext();
            if (IsEndOfStream(c))
                return;
            else if (char.IsWhiteSpace((char)c))
                SkipWhitespace();
            else if (IsSingleLineComment(c))
                parser.SkipLine();
            else if (IsMultiLineCommentStart(c, parser.Peek()))
                SkipMultiLineComment();
            else
            {
                parser.Rewind();
                return;
            }
        }
    }

    /** @param c the first, already parsed, character of the token. */
    private Token ParseText(int c, TokenType t)
    {
        var sb = new StringBuilder();

        do
        {
            sb.Append((char)c);
            c = parser.GetNext();
        } while (IsValidForAtom(c));
        parser.Rewind();

        return CreateToken(sb, t);
    }

    /**
     * Reads a {@code string} consisting of all characters read from the parser up to the next {@code '}.
     * <p>
     * If an atom's name is enclosed in quotes (i.e. {@code '}) then it may contain any character.
     * </p>
     */
    private Token ParseQuotedText()
    {
        var sb = new StringBuilder();
        do
        {
            int c = parser.GetNext();
            if (IsQuote(c))
            {
                c = parser.GetNext();
                // If we reach a ' that is not immediately followed by another '
                // we assume we have reached the end of the string.
                // If we find a ' that is immediately followed by another ' (i.e. '')
                // we treat it as a single ' - this is so the ' character can be included in strings.
                // e.g. 'abc''def' will be treated as  a single string with the value abc'def
                if (!IsQuote(c))
                {
                    // found closing '
                    parser.Rewind();
                    return CreateToken(sb,TokenType.ATOM);
                }
            }
            else if (IsEscapeSequencePrefix(c))
                c = ParseEscapeSequence();
            else if (IsEndOfStream(c))
                throw NewParserException("No closing ' on quoted string");
            sb.Append((char)c);
        } while (true);
    }

    /**
     * Parses a character code and represents it as an integer.
     * <p>
     * e.g. the text {@code 0'a} results in a token with the value {@code 97} (the ascii value for {@code a}) being
     * returned.
     */
    private Token ParseLeadingZero(int zero)
    {
        if (zero != '0')
        // sanity check - should never get here, as have already checked that the next character is a single quote
            throw new InvalidOperationException();

        if (!IsQuote(parser.Peek()))
            return ParseNumber(zero);

        parser.GetNext(); // skip single quote

        int next = parser.GetNext();
        if (next == -1)
            throw NewParserException("unexpected end of file after '");

        int code = IsEscapeSequencePrefix(next) ? ParseEscapeSequence() : next;

        return CreateToken(code.ToString(), TokenType.INTEGER);
    }

    private int ParseEscapeSequence()
    {
        int next = parser.GetNext();
        // e.g. 0'\u00a5
        // e.g. 0'\n
        return next == 'u' ? ParseUnicode() : Escape(next);
    }

    private int ParseUnicode()
    {
        var hex = new StringBuilder(4);
        hex.Append(ParseHex());
        hex.Append(ParseHex());
        hex.Append(ParseHex());
        hex.Append(ParseHex());
        return int.TryParse(hex.ToString(),
            System.Globalization.NumberStyles.HexNumber,
            null, out var v)?v:0;
    }

    private char ParseHex()
    {
        int h = parser.GetNext();
        if (char.IsDigit((char)h))
            return (char)h;
        else if (h >= 'a' && h <= 'f')
            return (char)h;
        else if (h >= 'A' && h <= 'F')
            return (char)h;
        else
            throw NewParserException("invalid unicode value");
    }

    /**
     * Parses a number, starting with the specified character, read from the parser.
     * <p>
     * Deals with numbers of the form {@code 3.4028235E38}.
     */
    private Token ParseNumber(int startChar)
    {
        var sb = new StringBuilder();

        bool keepGoing = true;
        bool readDecimalPoint = false;
        bool readExponent = false;
        bool wasLastCharExponent = false;
        int c = startChar;
        do
        {
            sb.Append((char)c);
            c = parser.GetNext();
            if (c == '.')
            {
                if (readDecimalPoint)
                    // can't have more than one decimal point per number
                    keepGoing = false;
                else if (char.IsDigit((char)parser.Peek()))
                    readDecimalPoint = true;
                else
                    // must be a digit after . for it to be a decimal number
                    keepGoing = false;
            }
            else if (c == 'e' || c == 'E')
            {
                if (readExponent)
                    throw NewParserException($"unexpected: {(char)c}");
                readExponent = true;
                wasLastCharExponent = true;
            }
            else if (!char.IsDigit((char)c))
                keepGoing = false;
            else
                wasLastCharExponent = false;
        } while (keepGoing);
        parser.Rewind();
        if (wasLastCharExponent)
        {
            throw NewParserException("expected digit after e");
        }

        return CreateToken(sb, readDecimalPoint ?TokenType.FLOAT : TokenType.INTEGER);
    }

    private int Escape(int escape) =>
        // https://docs.oracle.com/javase/tutorial/java/data/characters.html
        escape switch
        {
            // tab
            't' => '\t',
            // backspace
            'b' => '\b',
            // newline
            'n' => '\n',
            // carriage return
            'r' => '\r',
            // formfeed
            'f' => '\f',
            // single quote
            '\'' => '\'',
            // double quote
            '\"' => '\"',
            // backslash
            '\\' => (int)'\\',
            _ => throw NewParserException("invalid character escape sequence"),
        };

    private Token ParseSymbol(int c)
    {
        var sb = new StringBuilder();
        do
        {
            sb.Append((char)c);
            c = parser.GetNext();
        } while (!char.IsLetter((char)c) && !char.IsDigit((char)c) 
        && !char.IsWhiteSpace((char)c) && !IsEndOfStream(c));
        parser.Rewind();

        if (IsValidParseableElement(sb.ToString()))
        {
            return CreateToken(sb, TokenType.SYMBOL);
        }

        int Length = sb.Length;
        int idx = Length;
        while (--idx > 0)
        {
            var substring = sb.ToString()[..idx];
            if (IsValidParseableElement(substring))
            {
                parser.Rewind(Length - idx);
                return CreateToken(substring, TokenType.SYMBOL);
            }
        }

        for (int i = 1; i < Length; i++)
        {
            var substring = sb.ToString()[i..];
            if (IsValidParseableElement(substring) ||Delimiters.IsDelimiter(sb[i]))
            {
                parser.Rewind(Length - i);
                return CreateToken(sb.ToString()[..i], TokenType.SYMBOL);
            }
        }

        return CreateToken(sb, TokenType.SYMBOL);
    }

    private void SkipWhitespace()
    {
        while (char.IsWhiteSpace((char)parser.Peek()))
            parser.GetNext();
    }

    private void SkipMultiLineComment()
    {
        parser.GetNext(); // skip * after /
        int previous = parser.GetNext();
        while (true)
        {
            int current = parser.GetNext();
            if (IsEndOfStream(current))
                throw NewParserException("Missing */ to close multi-line comment");
            else if (IsMultiLineCommentEnd(previous, current))
                return;
            else
                previous = current;
        }
    }

    private bool IsValidParseableElement(string commandName) => Delimiters.IsDelimiter(commandName) || operands.IsDefined(commandName);

    private static bool IsEndOfStream(int c)
        => c == -1;

    private static bool IsSingleLineComment(int c) 
        => c == '%';

    private static bool IsMultiLineCommentStart(int c1, int c2)
        => c1 == '/' && c2 == '*';

    private static bool IsMultiLineCommentEnd(int c1, int c2)
        => c1 == '*' && c2 == '/';

    private static bool IsValidForAtom(int c)
        => char.IsLetterOrDigit((char)c) || IsUnderscore(c);

    //NOTICE: by Yilin, add '@' prefix to ensure Unicode variables which are not upper case letters.
    private static bool IsVariable(int c)
    => char.IsUpper((char)c) || IsUnderscore(c) || c is '@';

    private static bool IsUnderscore(int c)
        => c == '_';

    private static bool IsQuote(int c)
        => c == '\'';

    private static bool IsZero(int c) 
        => c == '0';

    private static bool IsEscapeSequencePrefix(int c) 
        => c == '\\';

    private static Token CreateToken(StringBuilder value, TokenType type)
        => CreateToken(value.ToString(), type);

    private static Token CreateToken(string value, TokenType type)
        => new(value, type);
}
