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
namespace Org.NProlog.Core.Terms;


/**
 * Static factory methods for creating new instances of {@link List}.
 *
 * @see List
 * @see ListUtils
 */
public static class ListFactory
{
    /**
     * A "{@code .}" is the functor name for all lists in Prolog.
     */
    public const char LIST_PREDICATE_NAME = '.';

    /**
     * Returns a new {@link List} with specified head and tail.
     *
     * @param head the first argument in the list
     * @param tail the second argument in the list
     * @return a new {@link List} with specified head and tail
     */
    public static LinkedTermList CreateList(Term head, Term tail) => new (head, tail);

    /**
     * Returns a new {@link List} with the specified terms and a empty list as the readonly tail element.
     *
     * @param terms contents of the list
     * @return a new {@link List} with the specified terms and a empty list as the readonly tail element
     */
    public static Term CreateList(ICollection<Term> terms) => CreateList(terms.ToArray());

    /**
     * Returns a new {@link List} with the specified terms and a empty list as the readonly tail element.
     * <p>
     * By having a {@code List} with a {@code List} as its tail it is possible to represent an ordered sequence of the
     * specified terms.
     *
     * @param terms contents of the list
     * @return a new {@link List} with the specified terms and a empty list as the readonly tail element
     */
    public static Term CreateList(Term[] terms) => CreateList(terms, EmptyList.EMPTY_LIST);

    /**
     * Returns a new {@link List} with the specified terms and the second parameter as the tail element.
     * <p>
     * By having a {@code List} with a {@code List} as its tail it is possible to represent an ordered sequence of the
     * specified terms.
     *
     * @param terms contents of the list
     * @return a new {@link List} with the specified terms and the second parameter as the tail element
     */
    public static Term CreateList(Term[] terms, Term tail)
    {
        int numberOfElements = terms.Length;
        if (numberOfElements == 0)
            return tail;
        var list = tail;
        for (int i = numberOfElements - 1; i > -1; i--)
        {
            var element = terms[i];
            list = new LinkedTermList(element, list);
        }
        return list;
    }

    /** Returns a new list of the specified Length where is each element is a variable. */
    public static Term CreateListOfLength(int Length)
    {
        if (Length == 0)
            return EmptyList.EMPTY_LIST;
        else if (Length < 0)
            throw new ArgumentException($"Cannot create list of Length: {Length}");
        else
        {
            Term t = EmptyList.EMPTY_LIST;
            for (int i = Length - 1; i > -1; i--)
            {
                t = new LinkedTermList(new Variable($"E{i}"), t);
            }
            return t;
        }
    }
}
