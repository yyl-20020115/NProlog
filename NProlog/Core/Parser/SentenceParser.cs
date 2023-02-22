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
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Parser;

/**
 * Parses Prolog syntax representing rules including operators.
 * <p>
 * <b>Note:</b> not thread safe.
 * </p>
 *
 * @see Operands
 */
public class SentenceParser
{
    private const string MINUS_SIGN = "-";
    private readonly TokenParser parser;
    private readonly Operands operands;
    /**
     * A collection of {@code Variable}s this parser currently knows about (key = the variable name).
     * <p>
     * The reason this information needs to be stored is so that each instance of the same variable name, in a single
     * sentence, refers to the same {@code Variable} instance.
     * <p>
     * e.g. During the parsing of the sentence <code>Y is 2, X is Y * 2.</code> two {@code Variable} instances need to be
     * created - one for the variable name <code>X</code> and one that is shared between both references to the variable
     * name <code>Y</code>.
     */
    private readonly Dictionary<string, Variable> variables = new();
    /**
     * Tokens created, during the parsing of the current sentence, that were represented using infix notation.
     * <p>
     * Example of infix notation: <code>X = 1</code> where the predicate name <code>=</code> is positioned between its
     * two arguments <code>X</code> and <code>1</code>.
     * <p>
     * The reason these need to be kept a record of is, when the sentence has been fully read, the individual tokens can
     * be reordered to conform to operator precedence.
     * <p>
     * e.g. <code>1+2/3</code> will get ordered like: <code>+(1, /(2, 3))</code> while <code>1/2+3</code> will be ordered
     * like: <code>+(/(1, 2), 3)</code>.
     */
    private readonly HashSet<Token> parsedInfixTokens = new();
    /**
     * Tokens created, during the parsing of the current sentence, that were enclosed in brackets.
     * <p>
     * The reason this information needs to be stored is so that the parser knows to <i>not</i> reorder these tokens as
     * part of the reordering of infix tokens that occurs once the sentence is fully read. i.e. Using brackets to
     * explicitly define the ordering of tokens overrules the default operator precedence of infix tokens.
     * <p>
     * e.g. Although <code>1/2+3</code> will be ordered like: <code>+(/(1, 2), 3)</code>, <code>1/(2+3)</code> (i.e.
     * where <code>2+3</code> is enclosed in brackets) will be ordered like: <code>/(1, +(2, 3))</code>.
     */
    private readonly HashSet<Token> bracketedTokens = new();

    /**
     * Returns a new {@code SentenceParser} will parse the specified {@code string} using the specified {@code Operands}.
     *
     * @param prologSyntax the prolog syntax to be parsed
     * @param operands details of the operands to use during parsing
     * @return a new {@code SentenceParser}
     */
    public static SentenceParser GetInstance(string prologSyntax, Operands operands) 
        => GetInstance(new StringReader(prologSyntax), operands);

    /**
     * Returns a new {@code SentenceParser} that will parse Prolog syntax read from the specified {@code TextReader} using
     * the specified {@code Operands}.
     *
     * @param reader the source of the prolog syntax to be parsed
     * @param operands details of the operands to use during parsing
     * @return a new {@code SentenceParser}
     */
    public static SentenceParser GetInstance(TextReader reader, Operands operands)
        => new (reader, operands);

    private SentenceParser(TextReader reader, Operands operands)
    {
        this.parser = new (reader, operands);
        this.operands = operands;
    }

    /**
     * Creates a {@link Term} from Prolog syntax, terminated by a {@code .}, read from this object's
     * {@link CharacterParser}.
     *
     * @return a {@link Term} created from Prolog syntax read from this object's {@link CharacterParser} or {@code null}
     * if the end of the underlying stream being parsed has been reached
     * @th rows ParserException if an error parsing the Prolog syntax occurs
     */
    public Term ParseSentence()
    {
        var t = ParseTerm();
        if (t == null)
            return null;
        
        var trailingToken = PopValue();
        return Delimiters.IsSentenceTerminator(trailingToken)
            ? t
            : throw NewParserException("Expected . after: " + t + " but got: " + trailingToken);
    }

    /**
     * Creates a {@link Term} from Prolog syntax read from this object's {@link CharacterParser}.
     *
     * @return a {@link Term} created from Prolog syntax read from this object's {@link CharacterParser} or {@code null}
     * if the end of the underlying stream being parsed has been reached
     * @throws ParserException if an error parsing the Prolog syntax occurs
     * @see SentenceParser#parseSentence()
     */
    public Term ParseTerm()
    {
        if (parser.HasNext)
        {
            ResetState();
            return ToTerm(GetToken(int.MaxValue));
        }
        else
            return null;
    }

    private void ResetState()
    {
        parsedInfixTokens.Clear();
        variables.Clear();
        bracketedTokens.Clear();
    }

    /**
     * Returns collection of {@link Variable} instances created by this {@code SentenceParser}.
     * <p>
     * Returns all {@link Variable}s created by this {@code SentenceParser} either since it was created or since the last
     * execution of {@link #parseTerm()}.
     *
     * @return collection of {@link Variable} instances created by this {@code SentenceParser}
     */

    public Dictionary<string, Variable> GetParsedTermVariables() => new (variables);

    /**
     * Creates a {@link Token} from Prolog syntax read from this object's {@link CharacterParser}.
     */
    private Token GetToken(int maxLevel)
    {
        var firstArg = GetPossiblePrefixArgument(maxLevel);
        return parser.HasNext ? GetToken(firstArg, maxLevel, maxLevel, true) : firstArg;
    }

    /**
     * Recursively called to combine individual tokens into a composite token.
     * <p>
     * While the parsing of the individual tokens is performed, priority of the operands they represent needs to be
     * considered to make sure the tokens are ordered correctly (due to different operand precedence it is not always the
     * case that the tokens will be ordered in the resulting composite Token in the same order they were parsed from the
     * input stream).
     *
     * @param currentToken represents the current state of the process to parse a complete token
     * @param currentLevel the current priority/precedence/level of tokens being parsed - if an operand represented by a
     * Token retrieved by this method has a higher priority then reordering needs to take place to position the Token in
     * the right position in relation to the other tokens that exist within the {@code currentToken} structure (in order
     * to maintain the correct priority)
     * @param maxLevel the maximum priority/precedence/level of operands to parse - if an operand represented by the next
     * Token retrieved by this method has a higher priority then it is ignored for now ({@code currentToken} is returned
     * "as-is").
     * @param {@code true} if this method is being called by another method, {@code false} if it is being called
     * recursively by itself.
     */
    private Token GetToken(Token currentToken, int currentLevel, int maxLevel, bool isFirst)
    {
        var nextToken = PopValue();
        var next = nextToken.Name;
        if (operands.Postfix(next) && operands.GetPostfixPriority(next) <= currentLevel)
        {
            var postfixToken = AddPostfixOperand(next, currentToken);
            return GetToken(postfixToken, currentLevel, maxLevel, false);
        }
        else if (!operands.Infix(next))
        {
            // could be '.' if end of sentence
            // or ',', '|', ']' or ')' if parsing list or predicate
            // or could be an error
            parser.Rewind(nextToken);
            return currentToken;
        }

        int level = operands.GetInfixPriority(next);
        if (level > maxLevel)
        {
            parser.Rewind(nextToken);
            return currentToken;
        }

        var secondArg = GetPossiblePrefixArgument(level);

        if (isFirst)
        {
            var t = CreateStructure(next, new Token[] { currentToken, secondArg });
            return GetToken(t, level, maxLevel, false);
        }
        else if (level < currentLevel)
        {
            // compare previous.GetArgument(1) to level -
            // keep going until find right level to add this Token to
            var t = currentToken;
            while (IsParsedInfixToken(t.GetArgument(1)) && GetInfixLevel(t.GetArgument(1)) > level)
            {
                if (bracketedTokens.Contains(t.GetArgument(1)))
                    break;
                t = t.GetArgument(1);
            }
            var predicate = CreateStructure(next, new Token[] { t.GetArgument(1), secondArg });
            parsedInfixTokens.Add(predicate);
            t.SetArgument(1, predicate);
            return GetToken(currentToken, currentLevel, maxLevel, false);
        }
        else
        {
            if (level == currentLevel)
            {
                if (operands.Xfx(next))
                {
                    throw NewParserException("Operand " + next + " has same precedence level as preceding operand: " + currentToken);
                }
            }
            var predicate = CreateStructure(next, new Token[] { currentToken, secondArg });
            parsedInfixTokens.Add(predicate);
            return GetToken(predicate, level, maxLevel, false);
        }
    }

    private static Token CreateStructure(string name, Token[] tokens) 
        => new (name, TokenType.STRUCTURE, tokens);

    /**
     * Parses and returns a {@code Token}.
     * <p>
     * If the parsed {@code Token} represents a prefix operand, then the subsequent Token is also parsed so it can be
     * used as an argument in the returned structure.
     *
     * @param currentLevel the current priority level of tokens being parsed (if the parsed Token represents a prefix
     * operand, then the operand cannot have a higher priority than {@code currentLevel} (a {@code ParserException} will
     * be thrown if does).
     */
    private Token GetPossiblePrefixArgument(int currentLevel)
    {
        var token = PopValue();
        var value = token.Name;
        if (operands.Prefix(value) && parser.IsFollowedByTerm())
        {
            if (value.Equals(MINUS_SIGN) && IsFollowedByNumber())
                return GetNegativeNumber();

            int prefixLevel = operands.GetPrefixPriority(value);
            if (prefixLevel > currentLevel)
                throw NewParserException("Invalid prefix: " + value + " level: " + prefixLevel + " greater than current level: " + currentLevel);

            // The difference between "fy" and "fx" associativity is that a "y" means that the argument
            // can contain operators of <i>the same</i> or lower level of priority
            // while a "x" means that the argument can <i>only</i> contain operators of a lower priority.
            if (operands.Fx(value))
            {
                // -1 to only parse tokens of a lower priority than the current prefix operator.
                prefixLevel--;
            }

            var argument = GetToken(prefixLevel);
            return CreatePrefixToken(value, argument);
        }
        else
        {
            parser.Rewind(token);
            return GetDiscreteToken();
        }
    }

    private Token GetNegativeNumber()
    {
        var token = PopValue();
        var value = "-" + token.Name;
        return new(value, token.Type);
    }

    /**
     * Returns a new {@code Token} representing the specified prefix operand and argument.
     */
    private static Token CreatePrefixToken(string prefixOperandName, Token argument) => CreateStructure(prefixOperandName, new Token[] { argument });

    /**
     * Add a token, representing a post-fix operand, in the appropriate point of a composite token.
     * <p>
     * The correct position of the post-fix operand within the composite Token (and so what the post-fix operands actual
     * argument will be) is determined by operand priority.
     *
     * @param original a composite Token representing the current state of parsing the current sentence
     * @param postfixOperand a Token which represents a post-fix operand
     */
    private Token AddPostfixOperand(string postfixOperand, Token original)
    {
        int level = operands.GetPostfixPriority(postfixOperand);
        if (original.NumberOfArguments == 2)
        {
            bool higherLevelInfixOperand = operands.Infix(original.Name) && GetInfixLevel(original) > level;
            if (higherLevelInfixOperand)
            {
                var name = original.Name;
                var firstArg = original.GetArgument(0);
                var newSecondArg = AddPostfixOperand(postfixOperand, original.GetArgument(1));
                return CreateStructure(name, new Token[] { firstArg, newSecondArg });
            }
        }
        else if (original.NumberOfArguments == 1)
        {
            if (operands.Prefix(original.Name))
            {
                if (GetPrefixLevel(original) > level)
                {
                    var name = original.Name;
                    var newFirstArg = AddPostfixOperand(postfixOperand, original.GetArgument(0));
                    return CreateStructure(name, new Token[] { newFirstArg });
                }
            }
            else if (operands.Postfix(original.Name))
            {
                int levelToCompareTo = GetPostfixLevel(original);
                // "x" in "xf" means that the argument can <i>only</i> contain operators of a lower priority.
                if (levelToCompareTo > level || (operands.Xf(postfixOperand) && levelToCompareTo == level))
                {
                    throw NewParserException("Invalid postfix: " + postfixOperand + " " + level + " and term: " + original + " " + levelToCompareTo);
                }
            }
        }
        return CreateStructure(postfixOperand, new Token[] { original });
    }

    private Token GetDiscreteToken()
    {
        var token = PopValue();
        if (Delimiters.IsListOpenBracket(token))
        {
            return ParseList();
        }
        else if (Delimiters.IsPredicateOpenBracket(token))
        {
            return GetTokenInBrackets();
        }
        else if (token.Type == TokenType.SYMBOL || token.Type == TokenType.ATOM)
        {
            return GetAtomOrStructure(token.Name);
        }
        else
        {
            return token;
        }
    }

    private Term ToTerm(Token token)
    {
        switch (token.Type)
        {
            case TokenType.ATOM:
                return new Atom(token.Name);
            case TokenType.INTEGER:
                return IntegerNumberCache.ValueOf(long.TryParse(token.Name,out var v)?v:0);
            case TokenType.FLOAT:
                return new DecimalFraction(double.TryParse(token.Name,out var d)?d:0);
            case TokenType.VARIABLE:
                return GetVariable(token.Name);
            case TokenType.STRUCTURE:
                var args = new Term[token.NumberOfArguments];
                for (int i = 0; i < args.Length; i++)
                    args[i] = ToTerm(token.GetArgument(i));
                return Structure.CreateStructure(token.Name, args);
            case TokenType.EMPTY_LIST:
                return EmptyList.EMPTY_LIST;
            default:
                throw new SystemException("Unexpected token type: " + token.Type + " with value: " + token);
        }
    }

    /**
     * Returns either an {@code Atom} or {@code Structure} with the specified name.
     * <p>
     * If the next character read from the parser is a {@code (} then a newly created {@code Structure} is returned else
     * a newly created {@code Atom} is returned.
     */
    private Token GetAtomOrStructure(string name)
    {
        var token = parser.HasNext ? PeekValue() : null;
        if (Delimiters.IsPredicateOpenBracket(token))
        {
            PopValue(); //skip opening bracket
            if (Delimiters.IsPredicateCloseBracket(PeekValue()))
                throw NewParserException("No arguments specified for structure: " + name);

            List<Token> args = new();

            var t = GetCommaSeparatedArgument();
            args.Add(t);

            do
            {
                token = PopValue();
                if (Delimiters.IsPredicateCloseBracket(token))
                    return CreateStructure(name, ToArray(args));
                else if (Delimiters.IsArgumentSeperator(token))
                    args.Add(GetCommaSeparatedArgument());
                else
                    throw NewParserException("While parsing arguments of " + name + " expected ) or , but got: " + token);
            } while (true);
        }
        else
        {
            return new (name, TokenType.ATOM);
        }
    }

    /**
     * Returns a variable with the specified id.
     * <p>
     * If this object already has an instance of {@code Variable} with the specified id then it will be returned else a
     * new {@code Variable} will be created. The only exception to this behaviour is when the id Equals
     * {@link Variable#ANONYMOUS_VARIABLE_ID} - in which case a new {@code Variable} will be always be returned.
     */
    private Variable GetVariable(string id) => Variable.ANONYMOUS_VARIABLE_ID.Equals(id) ? new Variable() : GetNamedVariable(id);

    private Variable GetNamedVariable(string id)
    {
        if (!variables.TryGetValue(id,out var v))
            variables.Add(id, v = new Variable(id));
        return v;
    }

    /** Returns a newly created {@code Token} representing a Prolog list with elements read from the parser. */
    private Token ParseList()
    {
        List<Token> args = new();
        var tail = new Token(null, TokenType.EMPTY_LIST);

        while (true)
        {
            var token = PopValue();
            if (Delimiters.IsListCloseBracket(token))
            {
                break;
            }
            parser.Rewind(token);
            var arg = GetCommaSeparatedArgument();
            args.Add(arg);

            token = PopValue(); // | ] or ,
            if (Delimiters.IsListCloseBracket(token))
            {
                break;
            }
            else if (Delimiters.IsListTail(token))
            {
                tail = GetCommaSeparatedArgument();
                token = PopValue();
                if (!Delimiters.IsListCloseBracket(token))
                   throw NewParserException("Expected ] to mark end of list after tail but got: " + token);
                break;
            }
            else if (!Delimiters.IsArgumentSeperator(token))
                throw NewParserException("While parsing list expected ] | or , but got: " + token);
        }

        var list = tail;
        for (int i = args.Count - 1; i > -1; i--)
        {
            var element = args[(i)];
            list = CreateStructure(ListFactory.LIST_PREDICATE_NAME, new Token[] { element, list });
        }
        return list;
    }

    /**
     * Parses and returns the next argument of a list or structure.
     * <p>
     * As a comma would indicate a delimiter in a sequence of arguments, we only want to continue parsing up to the point
     * of any comma. i.e. Any parsed comma should not be considered as part of the argument currently being parsed.
     */
    private Token GetCommaSeparatedArgument() =>
        // Call getArgument with a priority/precedence/level of one less than the priority of a comma -
        // as we only want to continue parsing tokens that have a lower priority level than that.
        // The reason this is slightly complicated is because of the overloaded use of a comma in Prolog -
        // as well as acting as a delimiter in a sequence of arguments for a list or structure,
        // a comma is also a predicate in its own right (as a conjunction).
        operands.Infix(",") ? GetToken(operands.GetInfixPriority(",") - 1) : GetToken(int.MaxValue);

    private Token GetTokenInBrackets()
    {
        // As we are at the starting point for parsing a Token contained in brackets
        // (and as it being in brackets means we can parse it in isolation without
        // considering the priority of any surrounding tokens outside the brackets)
        // we call getArgument with the highest possible priority.
        var token = GetToken(int.MaxValue);
        var next = PopValue();
        if (!Delimiters.IsPredicateCloseBracket(next))
            throw NewParserException("Expected ) but got: " + next + " after " + token);
        bracketedTokens.Add(token);
        return token;
    }

    private Token PopValue() => parser.Next();

    private Token PeekValue()
    {
        var token = PopValue();
        parser.Rewind(token);
        return token;
    }

    private bool IsFollowedByNumber()
    {
        var token = PopValue();
        TokenType tt = token.Type;
        parser.Rewind(token);
        return tt == TokenType.INTEGER || tt == TokenType.FLOAT;
    }

    /**
     * Has the specified Token already been parsed, and included as an argument in an infix operand, as part of parsing
     * the current sentence?
     */
    private bool IsParsedInfixToken(Token t) 
        => parsedInfixTokens.Contains(t);

    private int GetPrefixLevel(Token t)
        => operands.GetPrefixPriority(t.Name);

    private int GetInfixLevel(Token t)
        => operands.GetInfixPriority(t.Name);

    private int GetPostfixLevel(Token t)
        => operands.GetPostfixPriority(t.Name);

    private static Token[] ToArray(List<Token> al)
        => al.ToArray();

    /** Returns a new {@link ParserException} with the specified message. */
    private ParserException NewParserException(string message)
        => parser.NewParserException(message);
}
