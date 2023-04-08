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

/** @see TokenParser#next() */
public class Token
{
    public static readonly Token Default = new ("", TokenType.ATOM);
    private static readonly Token[] EMPTY_ARGS = Array.Empty<Token>();

    private readonly string? value;
    private readonly TokenType type;
    private readonly Token[] args;

    public Token(string? value, TokenType type) : this(value, type, EMPTY_ARGS) { }

    public Token(string? value, TokenType type, Token[] args)
    {
        this.value = value;
        this.type = type;
        this.args = args;
    }

    public string? Name => value;

    public TokenType Type => type;

    public Token GetArgument(int i) => args[i];

    public void SetArgument(int i, Token t) => args[i] = t;

    public int NumberOfArguments => args.Length;


    public override string ToString() => value??"";
}
