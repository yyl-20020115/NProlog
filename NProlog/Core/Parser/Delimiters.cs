/*
 * Copyright 2013-2014 S. Webber
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
namespace Org.NProlog.Core.Parser;


public static class Delimiters
{
    private const char ARGUMENT_SEPARATOR = ',';
    private const char PREDICATE_OPENING_BRACKET = '(';
    private const char PREDICATE_CLOSING_BRACKET = ')';
    private const char LIST_OPENING_BRACKET = '[';
    private const char LIST_CLOSING_BRACKET = ']';
    private const char LIST_TAIL = '|';
    private const char PERIOD = '.';

    public static bool IsDelimiter(string s) => s != null && s.Length == 1 && IsDelimiter(s[(0)]);

    public static bool IsDelimiter(int c) => c switch
    {
        ARGUMENT_SEPARATOR or PREDICATE_OPENING_BRACKET or PREDICATE_CLOSING_BRACKET or LIST_OPENING_BRACKET or LIST_CLOSING_BRACKET or LIST_TAIL or PERIOD => true,
        _ => false,
    };

    public static bool IsListOpenBracket(int c) => c == LIST_OPENING_BRACKET;

    public static bool IsPredicateOpenBracket(Token token) => IsMatch(token, PREDICATE_OPENING_BRACKET);

    public static bool IsPredicateCloseBracket(Token token) => IsMatch(token, PREDICATE_CLOSING_BRACKET);

    public static bool IsListOpenBracket(Token token) => IsMatch(token, LIST_OPENING_BRACKET);

    public static bool IsListCloseBracket(Token token) => IsMatch(token, LIST_CLOSING_BRACKET);

    public static bool IsListTail(Token token) => IsMatch(token, LIST_TAIL);

    public static bool IsArgumentSeperator(Token token) => IsMatch(token, ARGUMENT_SEPARATOR);

    public static bool IsSentenceTerminator(Token token) => IsMatch(token, PERIOD);

    public static bool IsMatch(Token token, char expected) => token != null && token.Type == TokenType.SYMBOL && token.Name.Length == 1 && token.Name[0] == expected;
}
