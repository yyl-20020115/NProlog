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

namespace Org.NProlog.Core.Predicate.Builtin.IO;

/* TEST
%?- write( 1+1 )
%OUTPUT 1 + 1
%YES

%?- write( '+'(1,1) )
%OUTPUT 1 + 1
%YES

%?- write(hello), nl, write(world), nl
%OUTPUT
%hello
%world
%
%OUTPUT
%YES

%?- writeln(hello), writeln(world)
%OUTPUT
%hello
%world
%
%OUTPUT
%YES
*/
/**
 * <code>write(X)</code> - writes a term to the output stream.
 * <p>
 * Writes the term <code>X</code> to the current output stream. <code>write</code> takes account of current operator
 * declarations - thus an infix operator will be Writeed _out between its arguments. <code>write</code> represents lists
 * as a comma separated sequence of elements enclosed in square brackets.
 * </p>
 * <p>
 * Succeeds only once.
 * </p>
 * <p>
 * <code>writeln(X)</code> writes the term <code>X</code> to the current output stream, followed by a new line
 * character. <code>writeln(X)</code> can be used as an alternative to <code>write(X), nl</code>.
 * </p>
 *
 * @see #toString(Term)
 */
public class Write : AbstractSingleResultPredicate
{
    private readonly bool addNewLine;

    public static Write DoWrite() => new(false);

    public static Write DoWriteLn() => new(true);

    private Write(bool addNewLine) => this.addNewLine = addNewLine;


    protected override bool Evaluate(Term arg)
    {
        WriteString(ToString(arg));
        return true;
    }

    private void WriteString(string s)
    {
        var writer = FileHandles.CurrentWriter;
        if (addNewLine)
        {
            writer.WriteLine(s);
        }
        else
        {
            writer.Write(s);
        }
    }

    private string ToString(Term t) => TermFormatter.FormatTerm(t);
}
