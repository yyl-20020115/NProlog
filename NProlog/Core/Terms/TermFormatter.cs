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
using Org.NProlog.Core.Parser;
using System.Text;

namespace Org.NProlog.Core.Terms;


/**
 * Produces {@code string} representations of {@link Term} instances.
 * <p>
 * Does take account of operator precedence.
 *
 * @see #formatTerm(Term)
 */
public class TermFormatter
{
    private readonly Operands operands;

    public TermFormatter(Operands operands) => this.operands = operands;

    /**
     * Returns a string representation of the specified {@code Term}.
     * <p>
     * This method does take account of current operator declarations - thus an infix operator will be Writeed _out
     * between its arguments. This method represents lists as a comma separated sequence of elements enclosed in square
     * brackets.
     * <p>
     * For example: <pre>
     * Term structure = Structure.createStructure("+", new IntegerNumber(1), new IntegerNumber(2));
     * Term list = ListFactory.create(new Term[]{new Atom("a"), Atom("b"), Atom("c")});
     * System._out.WriteLine("Structure.ToString():      "+structure.ToString());
     * System._out.WriteLine("Write.toString(structure): "+write.toString(structure));
     * System._out.WriteLine("List.ToString():           "+list.ToString());
     * System._out.WriteLine("Write.toString(list):      "+write.toString(list));
     * </pre> would Write _out: <pre>
     * Structure.ToString():      +(1, 2)
     * Write.toString(structure): 1 + 2
     * List.ToString():           .(a, .(b, .(c, [])))
     * Write.toString(list):      [a,b,c]
     * </pre>
     *
     * @param t the {@code Term} to represent as a string
     * @return a string representation of the specified {@code Term}
     */
    public string FormatTerm(Term t)
    {
        var builder = new StringBuilder();
        Write(t, builder);
        return builder.ToString();
    }

    private StringBuilder Write(Term t, StringBuilder sb) => t.Type switch
    {
        var tt when tt == TermType.STRUCTURE => this.WritePredicate(t, sb),
        var tt when tt == TermType.LIST => WriteList(t, sb),
        var tt when tt == TermType.EMPTY_LIST => sb.Append("[]"),
        var tt when tt == TermType.VARIABLE => sb.Append(((Variable)t).Id),
        _ => sb.Append(t.ToString())
    };

    private StringBuilder WriteList(Term p, StringBuilder builder)
    {
        builder.Append('[');
        var head = p.GetArgument(0);
        var tail = p.GetArgument(1);
        Write(head, builder);
        Term list;
        while ((list = GetList(tail)) != null)
        {
            builder.Append(',');
            Write(list.GetArgument(0), builder);
            tail = list.GetArgument(1);
        }

        if (tail.Type != TermType.EMPTY_LIST)
        {
            builder.Append('|');
            Write(tail, builder);
        }
        builder.Append(']');
        return builder;
    }

    private static Term GetList(Term term) => term.Type == TermType.LIST ? term : null;

    private StringBuilder WritePredicate(Term @operator, StringBuilder builder)
    {
        if (IsInfixOperator(@operator))
        {
            WriteInfixOperator(@operator, builder);
        }
        else if (IsPrefixOperator(@operator))
        {
            WritePrefixOperator(@operator, builder);
        }
        else if (IsPostfixOperator(@operator))
        {
            WritePostfixOperator(@operator, builder);
        }
        else
        {
            WriteNonOperatorPredicate(@operator, builder);
        }
        return builder;
    }

    private bool IsInfixOperator(Term term)
        => term.Type == TermType.STRUCTURE && term.Args.Length == 2 && operands.Infix(term.Name);

    private int WriteInfixOperator(Term term, StringBuilder builder)
    {
        var args = term.Args;
        Write(args[0], builder);
        builder.Append(' ').Append(term.Name).Append(' ');
        // if second argument is an infix operand then add brackets around it so:
        //  ?-(,(fail, ;(fail, true)))
        // appears as:
        //  ?- fail , (fail ; true)
        // not:
        //  ?- fail , fail ; true
        if (IsInfixOperator(args[1]) && IsEqualOrLowerPriority(term, args[1]))
        {
            builder.Append('(');
            WriteInfixOperator(args[1], builder);
            builder.Append(')');
        }
        else
        {
            Write(args[1], builder);
        }
        return 0;
    }

    private bool IsEqualOrLowerPriority(Term p1, Term p2)
        => operands.GetInfixPriority(p1.Name) <= operands.GetInfixPriority(p2.Name);

    private bool IsPrefixOperator(Term term)
        => term.Type == TermType.STRUCTURE && term.Args.Length == 1 && operands.Prefix(term.Name);

    private void WritePrefixOperator(Term term, StringBuilder builder)
    {
        builder.Append(term.Name).Append(' ');
        Write(term.Args[0], builder);
    }

    private bool IsPostfixOperator(Term t)
        => t.Type == TermType.STRUCTURE && t.Args.Length == 1 && operands.Postfix(t.Name);

    private void WritePostfixOperator(Term term, StringBuilder builder)
    {
        Write(term.Args[0], builder);
        builder.Append(' ').Append(term.Name);
    }

    private void WriteNonOperatorPredicate(Term term, StringBuilder builder)
    {
        var name = term.Name;
        var args = term.Args;
        builder.Append(name);
        builder.Append('(');
        for (int i = 0; i < args.Length; i++)
        {
            if (i != 0)
                builder.Append(", ");
            Write(args[i], builder);
        }
        builder.Append(')');
    }
}
