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
using Org.NProlog.Core.Exceptions;

namespace Org.NProlog.Core.Parser;
/**
 * Signals a failure to successfully parse Prolog syntax.
 */
public class ParserException : PrologException
{
    private readonly string message;
    private readonly string line;
    private readonly int lineNumber;
    private readonly int columnNumber;

    public ParserException(string message, CharacterParser parser) : this(message, parser, null) { }

    public ParserException(string message, CharacterParser parser, Exception t)
        : base(message + " Line: " + parser.Line, t)
    {
        this.message = message;
        this.line = parser.Line;
        this.lineNumber = parser.LineNumber;
        this.columnNumber = parser.ColumnNumber;
    }

    /**
     * Returns the contents of the line being parsed when the problem occurred.
     */
    public string Line => line;

    /**
     * Returns the line number of the line being parsed when the problem occurred.
     */
    public int LineNumber => lineNumber;

    /**
     * Returns the index in the line being parsed of the character being parsed when the problem occurred.
     */
    public int ColumnNumber => columnNumber;

    /**
     * Writes a description of this exception to the specified Write stream.
     * <p>
     * The description Contains the particular line being parsed when the exception was thrown.
     * 
     * @param _out {@code TextWriter} to use for output
     */
    public void GetDescription(TextWriter writer)
    {
        writer.WriteLine(message);
        writer.WriteLine(Line);
        for (var c = 0; c < ColumnNumber - 1; c++)
        {
            writer.Write(' ');
        }
        writer.WriteLine("^");
    }
}
