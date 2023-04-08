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


/**
 * Reads characters from a {@code BufferedReader}.
 * <p>
 * Provides details of current line and column number being parsed.
 * </p>
 * 
 * @see SentenceParser#GetInstance(java.io.TextReader, org.prolog.core.Operands)
 */
public class CharacterParser
{
    private const int END_OF_STREAM = -1;

    private readonly TextReader reader;
    private string? currentLine = null;
    /**
     * The line number of the current line being parsed.
     * <p>
     * Required in order to provide useful information if a {@link ParserException} is thrown.
     */
    private int lineNumber = 0;
    /**
     * The position, within the current line, of the character being parsed.
     * <p>
     * Required in order to provide useful information if a {@link ParserException} is thrown.
     */
    private int columnNumber = 0;

    public CharacterParser(TextReader reader) 
        => this.reader = reader;

    /**
     * Reads a single character.
     * <p>
     * Every call to {@code getNext()} causes the parser to move forward one character - meaning that by making repeated
     * calls to {@code getNext()} all characters in the the underlying stream represented by this object will be
     * returned.
     * 
     * @return The character read, as an integer in the range 0 to 65535 (<tt>0x00-0xffff</tt>), or -1 if the end of the
     * stream has been reached
     * @exception ParserException if an I/O error occurs
     * @see #peek()
     */
    public int GetNext()
    {
        try
        {
            // proceed to next line
            if (currentLine == null || columnNumber > currentLine.Length)
            {
                var nextLine = reader.ReadLine();
                if (nextLine == null) return END_OF_STREAM;
                currentLine = nextLine;
                lineNumber++;
                columnNumber = 0;
            }

            // if reached end of a line return the new line character
            if (columnNumber == currentLine.Length)
            {
                columnNumber++;
                return '\n';
            }
            else
            {
                return currentLine[columnNumber++];
            }
        }
        catch (IOException e)
        {
            throw new ParserException("Unexpected exception getting next character", this, e);
        }
    }

    /**
     * Reads a single character but does not consume it.
     * 
     * @return The character read, as an integer in the range 0 to 65535 (<tt>0x00-0xffff</tt>), or -1 if the end of the
     * stream has been reached
     * @exception ParserException if an I/O error occurs
     * @see #getNext()
     */
    public int Peek()
    {
        var i = GetNext();
        if (i != END_OF_STREAM) Rewind();
        return i;
    }

    /**
     * Moves the parser back one character.
     * 
     * @throws ParserException if attempting to rewind back past the start of the current line
     */
    //public void Rewind()
    //{
    //    Rewind(1);
    //}

    /**
     * Moves the parser back by the specified number of characters.
     * 
     * @throws ParserException if attempting to rewind back past the start of the current line
     */
    public void Rewind(int numberOfCharacters = 1)
    {
        if (numberOfCharacters > columnNumber)
            throw new ParserException("Cannot rewind past start of current line", this);
        columnNumber -= numberOfCharacters;
    }

    /**
     * Skips the remainder of the line currently being parsed.
     */
    public void SkipLine() => 
        columnNumber = currentLine!=null ? currentLine.Length + 1 : 1;

    /**
     * Returns the entire contents of the line currently being parsed.
     */
    public string? Line => currentLine;

    /**
     * Returns the line number of the line currently being parsed.
     */
    public int LineNumber => lineNumber;

    /**
     * Returns the index, in the line currently being parsed, of the character that will be returned by the next call to
     * {@link #getNext()}.
     */
    public int ColumnNumber => columnNumber;
}
