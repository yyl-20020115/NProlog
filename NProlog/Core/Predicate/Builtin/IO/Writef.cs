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
using System.Text;

namespace Org.NProlog.Core.Predicate.Builtin.IO;




/* TEST
%?- writef('%s%n %t%r', [[h,e,l,l,o], 44, world, !, 3])
%OUTPUT hello, world!!!
%YES

%?- writef('.%7l.\\n.%7l.\\n.%7l.\\n.%7l.\\n.%7l.', [a, abc, abcd, abcdefg, abcdefgh])
%OUTPUT
%.a      .
%.abc    .
%.abcd   .
%.abcdefg.
%.abcdefgh.
%OUTPUT
%YES

%?- writef('.%7r.\\n.%7r.\\n.%7r.\\n.%7r.\\n.%7r.', [a, abc, abcd, abcdefg, abcdefgh])
%OUTPUT
%.      a.
%.    abc.
%.   abcd.
%.abcdefg.
%.abcdefgh.
%OUTPUT
%YES

%?- writef('.%7c.\\n.%7c.\\n.%7c.\\n.%7c.\\n.%7c.', [a, abc, abcd, abcdefg, abcdefgh])
%OUTPUT
%.   a   .
%.  abc  .
%. abcd  .
%.abcdefg.
%.abcdefgh.
%OUTPUT
%YES

%?- writef('%w %d', [1+1, 1+1])
%OUTPUT 1 + 1 +(1, 1)
%YES

%?- writef('\\%\\%%q\\\\\\\\\\r\\n\\u0048',[abc])
%OUTPUT
%%%abc\\
%H
%OUTPUT
%YES

% Note: calling writef with only 1 argument is the same as calling it with an empty list for the second argument:
%?- writef('\\u0048\\u0065\\u006C\\u006c\\u006F', [])
%OUTPUT Hello
%YES
%?- writef('\\u0048\\u0065\\u006C\\u006c\\u006F')
%OUTPUT Hello
%YES
*/
/**
 * <code>writef(X,Y)</code> - writes formatted text to the output stream.
 * <p>
 * The first argument is an atom representing the text to be output. The text can contain special character sequences
 * which specify formatting and substitution rules.
 * </p>
 * <p>
 * Supported special character sequences are:
 * </p>
 * <table>
 * <tr>
 * <th>Sequence</th>
 * <th>Action</th>
 * </tr>
 * <tr>
 * <td>\n</td>
 * <td>Output a 'new line' character (ASCII code 10).</td>
 * </tr>
 * <tr>
 * <td>\l</td>
 * <td>Same as <code>\n</code>.</td>
 * </tr>
 * <tr>
 * <td>\r</td>
 * <td>Output a 'carriage return' character (ASCII code 13).</td>
 * </tr>
 * <tr>
 * <td>\t</td>
 * <td>Output a tab character (ASCII code 9).</td>
 * </tr>
 * <tr>
 * <td>\\</td>
 * <td>Output the <code>\</code> character.</td>
 * </tr>
 * <tr>
 * <td>\%</td>
 * <td>Output the <code>%</code> character.</td>
 * </tr>
 * <tr>
 * <td>\\u<i>NNNN</i></td>
 * <td>Output the unicode character represented by the hex digits <i>NNNN</i>.</td>
 * </tr>
 * <tr>
 * <td>%t</td>
 * <td>Output the next term - in same format as <code>write/1</code>.</td>
 * </tr>
 * <tr>
 * <td>%w</td>
 * <td>Same as <code>\t</code>.</td>
 * </tr>
 * <tr>
 * <td>%q</td>
 * <td>Same as <code>\t</code>.</td>
 * </tr>
 * <tr>
 * <td>%p</td>
 * <td>Same as <code>\t</code>.</td>
 * </tr>
 * <tr>
 * <td>%d</td>
 * <td>Output the next term - in same format as <code>write_canonical/1</code>.</td>
 * </tr>
 * <tr>
 * <td>%f</td>
 * <td>Ignored (only included to support compatibility with other Prologs).</td>
 * </tr>
 * <tr>
 * <td>%s</td>
 * <td>Output the elements contained in the list represented by the next term.</td>
 * </tr>
 * <tr>
 * <td>%n</td>
 * <td>Output the character code of the next term.</td>
 * </tr>
 * <tr>
 * <td>%r</td>
 * <td>Write the next term <i>N</i> times, where <i>N</i> is the value of the second term.</td>
 * </tr>
 * <tr>
 * <td>%<i>N</i>c</td>
 * <td>Write the next term centered in <i>N</i> columns.</td>
 * </tr>
 * <tr>
 * <td>%<i>N</i>l</td>
 * <td>Write the next term left-aligned in <i>N</i> columns.</td>
 * </tr>
 * <tr>
 * <td>%<i>N</i>r</td>
 * <td>Write the next term right-aligned in <i>N</i> columns.</td>
 * </tr>
 * </table>
 * <p>
 * <code>writef(X)</code> produces the same results as <code>writef(X, [])</code>.
 * </p>
 */
public class Writef : AbstractSingleResultPredicate
{

    protected override bool Evaluate(Term atom) => Evaluate(atom, EmptyList.EMPTY_LIST);


    protected override bool Evaluate(Term atom, Term list)
    {
        var text = TermUtils.GetAtomName(atom);
        var args = ListUtils.ToList(list);
        if (args == null) return false;

        Write(Format(text, args));

        return true;
    }

    private StringBuilder Format(string text, List<Term> args)
    {
        var f = new Formatter(text, args, TermFormatter);
        while (f.HasMore())
        {
            int c = f.Pop();
            if (c == '%')
            {
                ParsePercentEscapeSequence(f);
            }
            else if (c == '\\')
            {
                ParseSlashEscapeSequence(f);
            }
            else
            {
                f.WriteChar(c);
            }
        }
        return f.output;
    }

    private void ParsePercentEscapeSequence(Formatter f)
    {
        int next = f.Pop();
        if (next == 'f')
        {
            // flush - not supported, so ignore
            return;
        }

        var arg = f.NextArg();
        var output = string.Empty;
        switch (next)
        {
            case 't':
            case 'w':
            case 'q':
            case 'p':
                output = f.Format(arg);
                break;
            case 'n':
                long charCode =TermUtils.ToLong(ArithmeticOperators, arg);
                output = ((char)charCode).ToString();
                break;
            case 'r':
                long timesToRepeat = TermUtils.ToLong(ArithmeticOperators, f.NextArg());
                output = Repeat(f.Format(arg), timesToRepeat);
                break;
            case 's':
                output = Concat(f, arg);
                break;
            case 'd':
                // Write the term, ignoring operators.
                output = arg?.ToString()??string.Empty;
                break;
            default:
                f.Rewind();
                output = Align(f, arg);
                break;
        }
        f.WriteString(output??string.Empty);
    }

    private static string Repeat(string text, long timesToRepeat)
    {
        var builder = new StringBuilder();
        for (long i = 0; i < timesToRepeat; i++)
            builder.Append(text);
        return builder.ToString();
    }

    private string Concat(Formatter f, Term t)
    {
        var l = ListUtils.ToList(t);
        if (l == null)
            throw new ArgumentException("Expected list but got: " + t);
        var builder = new StringBuilder();
        foreach (var e in l)
            builder.Append(f.Format(e));
        return builder.ToString();
    }

    private string Align(Formatter f, Term t)
    {
        var s = f.Format(t);
        int actualWidth = s.Length;
        int requiredWidth = ParseNumber(f);
        int diff = System.Math.Max(0, requiredWidth - actualWidth);
        int alignmentChar = f.Pop();
        switch (alignmentChar)
        {
            case 'l':
                return s + GetWhitespace(diff);
            case 'r':
                return GetWhitespace(diff) + s;
            case 'c':
                var prefix = GetWhitespace(diff / 2);
                var suffix = diff % 2 == 0 ? prefix : prefix + " ";
                return prefix + s + suffix;
            default:
                throw new ArgumentException("? " + alignmentChar);
        }
    }

    private static string GetWhitespace(int diff) => new (' ', diff);

    private static int ParseNumber(Formatter f)
    {
        int next = 0;
        while (IsNumber(f.Peek()))
        {
            next = (next * 10) + (f.Pop() - '0');
        }
        return next;
    }

    private static void ParseSlashEscapeSequence(Formatter f)
    {
        int next = f.Pop();
        var output = next switch
        {
            'l' or 'n' => '\n',
            'r' => '\r',
            't' => '\t',
            '\\' => '\\',
            '%' => '%',
            'u' => ParseUnicode(f),
            _ => throw new ArgumentException("? " + next),
        };
        f.WriteChar(output);
    }

    private static int ParseUnicode(Formatter f)
    {
        var builder = new StringBuilder();
        builder.Append((char)f.Pop());
        builder.Append((char)f.Pop());
        builder.Append((char)f.Pop());
        builder.Append((char)f.Pop());
        return int.TryParse(builder.ToString(),System.Globalization.NumberStyles.HexNumber,null,out var v)?v:0;
    }

    private static bool IsNumber(int c)
    {
        return c >= '0' && c <= '9';
    }

    private void Write(StringBuilder sb)
    {
        FileHandles.CurrentWriter.Write(sb);
    }

    public class Formatter
    {
        public readonly StringBuilder output = new StringBuilder();
        readonly char[] chars;
        readonly List<Term> args;
        readonly TermFormatter termFormatter;
        int charIdx;
        int argIdx;

        public Formatter(string text, List<Term> args, TermFormatter termFormatter)
        {
            this.chars = text.ToCharArray();
            this.args = args;
            this.termFormatter = termFormatter;
        }

        public void Rewind() => charIdx--;

        public Term NextArg() => args[(argIdx++)];

        public string Format(Term t)
            => termFormatter.FormatTerm(t);

        public int Peek()
            => HasMore() ? chars[charIdx] : -1;

        public int Pop()
        {
            int c = Peek();
            charIdx++;
            return c;
        }

        public bool HasMore() => charIdx < chars.Length;

        public void WriteChar(int c)
        {
            output.Append((char)c);
        }

        public void WriteString(string s)
        {
            output.Append(s);
        }
    }
}
