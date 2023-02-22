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

namespace Org.NProlog.Core.Terms;

/**
 * Represents a data structure with two {@link Term}s - a head and a tail.
 * <p>
 * The head and tail can be any {@link Term}s - including other {@code List}s. By having a {@code List} with a
 * {@code List} as its tail it is possible to represent an ordered sequence of {@link Term}s of any Length. The end of
 * an ordered sequence of {@link Term}s is normally represented as a tail having the value of an {@link EmptyList}.
 *
 * @see EmptyList
 * @see ListFactory
 * @see ListUtils
 */
public class List : Term
{
    private readonly Term head;
    private readonly Term tail;
    private readonly bool immutable;
    private readonly int hashCode;

    /**
     * Creates a new list with the specified head and tail.
     * <p>
     * Consider using {@link ListFactory} rather than calling directly.
     *
     * @param head the head of the new list
     * @param tail the tail of the new list
     */
    public List(Term head, Term tail)
    {
        this.head = head;
        this.tail = tail;
        this.immutable = head.IsImmutable && tail.IsImmutable;
        this.hashCode = head.GetHashCode() + (tail.GetHashCode() * 7);
    }

    /**
     * Returns {@link ListFactory#LIST_PREDICATE_NAME}.
     *
     * @return {@link ListFactory#LIST_PREDICATE_NAME}
     */

    public string Name => ListFactory.LIST_PREDICATE_NAME;


    public Term[] Args => new Term[] { head, tail };


    public int NumberOfArguments => 2;


    public Term GetArgument(int index) => index switch
    {
        0 => head,
        1 => tail,
        _ => throw new IndexOutOfRangeException(nameof(index)+":"+index),
    };

    /**
     * Returns {@link TermType#LIST}.
     *
     * @return {@link TermType#LIST}
     */

    public TermType Type => TermType.LIST;


    public bool IsImmutable => immutable;


    public List Term 
        => Traverse(t=>t.Term);// Traverse(Term.Term);


    public List Copy(Dictionary<Variable, Variable> sharedVariables) =>
        Traverse(t => t.Copy(sharedVariables));

    /**
     * Used by {@link #getTerm()} and {@link #copy(Dictionary)} to traverse a list without using recursion.
     *
     * @param f the operation to apply to each mutable element of the list
     * @return the resulting list produced as a result of applying {@link f} to each of the mutable elements
     */
    private List Traverse(Func<Term,Term> f)
    {
        if (immutable)
        {
            return this;
        }

        List list = this;

        List<List> elements = new();
        while (!list.immutable && list.tail.Type == TermType.LIST)
        {
            elements.Add(list);
            list = (List)list.tail.Bound;
        }

        if (!list.immutable)
        {
            var newHead = f(list.head);
            var newTail = f(list.tail);
            if (newHead != list.head || newTail != list.tail)
            {
                list = new List(newHead, newTail);
            }
        }

        for (int i = elements.Count - 1; i > -1; i--)
        {
            var next = elements[(i)];
            var newHead = f(next.head);
            if (newHead != next.head || list != next.tail)
            {
                list = new List(newHead, list);
            }
            else
            {
                list = next;
            }
        }

        return list;
    }


    public bool Unify(Term t1)
    {
        // used to be implemented using recursion but caused stack overflow problems with long lists
        Term t2 = this;
        do
        {
            var tType = t1.Type;
            if (tType == TermType.LIST)
            {
                if (t2.GetArgument(0).Unify(t1.GetArgument(0)) == false)
                {
                    return false;
                }
                t1 = t1.GetArgument(1);
                t2 = t2.GetArgument(1);
            }
            else if (tType.IsVariable)
            {
                return t1.Unify(t2);
            }
            else
            {
                return false;
            }
        } while (t2.Type == TermType.LIST);
        return t2.Unify(t1);
    }


    public void Backtrack()
    {
        // used to be implemented using recursion but caused stack overflow problems with long lists
        List list = this;
        while (!list.immutable)
        {
            list.head.Backtrack();
            if (list.tail is List list1)
            {
                list = list1;
            }
            else
            {
                list.tail.Backtrack();
                return;
            }
        }
    }


    public override bool Equals(object? o)
    {
        if (o == this)
        {
            return true;
        }

        if (o is List list && hashCode == o.GetHashCode())
        {
            // used to be implemented using recursion but caused stack overflow problems with long lists
            Term a = this;
            Term b = list;

            do
            {
                if (!a.GetArgument(0).Equals(b.GetArgument(0)))
                {
                    return false;
                }

                a = a.GetArgument(1);
                b = b.GetArgument(1);
            } while (a is List && b is List);

            return a.Equals(b);
        }

        return false;
    }


    public override int GetHashCode() => hashCode;


    public override string ToString()
    {
        // used to be implemented using recursion but caused stack overflow problems with long lists
        var builder = new StringBuilder();
        int listCtr = 0;
        Term t = this;
        do
        {
            builder.Append(ListFactory.LIST_PREDICATE_NAME);
            builder.Append('(');
            builder.Append(t.GetArgument(0));
            builder.Append(", ");
            t = t.GetArgument(1);
            listCtr++;
        } while (t.Type == TermType.LIST);
        builder.Append(t);
        for (int i = 0; i < listCtr; i++)
        {
            builder.Append(')');
        }
        return builder.ToString();
    }

    Term Term.Copy(Dictionary<Variable, Variable> sharedVariables) => this.Copy(sharedVariables);

    Term Term.Term => this.Term;

    public Term Bound => this;
}
