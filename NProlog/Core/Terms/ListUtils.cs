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
using Org.NProlog.Core.Exceptions;

namespace Org.NProlog.Core.Terms;

/**
 * Helper methods for performing common tasks with Prolog list data structures.
 *
 * @see List
 * @see ListFactory
 * @see TermUtils
 */
public static class ListUtils
{
    /**
     * Returns a new {@code java.util.List} containing the contents of the specified {@code org.projog.core.term.List}.
     * <p>
     * Will return {@code null} if {@code list} is neither of type {@link TermType#LIST} or {@link TermType#EMPTY_LIST},
     * or if {@code list} represents a partial list (i.e. a list that does not have an empty list as its tail).
     * </p>
     *
     * @see #toSortedJavaUtilList(Term)
     */
    public static List<Term> ToList(Term list)
    {
        if (list.Type == TermType.LIST)
        {
            List<Term> result = new();
            do
            {
                result.Add(list.GetArgument(0));
                list = list.GetArgument(1);
            } while (list.Type == TermType.LIST);

            if (list.Type == TermType.EMPTY_LIST)
            {
                return result;
            }
            else
            {
                // partial list
                return null;
            }
        }
        else if (list.Type == TermType.EMPTY_LIST)
        {
            return new();// Collections.emptyList();
        }
        else
        {
            // not a list
            // TODO consider throwing exception here rather than returning null
            return null;
        }
    }

    /**
     * Returns a new {@code java.util.List} containing the sorted contents of the specified
     * {@code org.projog.core.term.List}.
     * <p>
     * The elements in the returned list will be ordered using the standard ordering of terms, as implemented by
     * {@link TermComparator}.
     * </p>
     * <p>
     * Will return {@code null} if {@code list} is neither of type {@link TermType#LIST} or {@link TermType#EMPTY_LIST},
     * or if {@code list} represents a partial list (i.e. a list that does not have an empty list as its tail).
     * </p>
     *
     * @see #ListUtils.toJavaUtilList(Term)
     */
    public static List<Term> ToSortedList(Term unsorted)
    {
        var elements = ToList(unsorted);
        elements?.Sort(TermComparator.TERM_COMPARATOR);
        return elements;
    }

    /**
     * Checks is a term can be unified with at least one element of a list.
     * <p>
     * Iterates through each element of {@code list} attempting to unify with {@code element}. Returns {@code true}
     * immediately after the first unifiable element is found. If {@code list} Contains no elements that can be unified
     * with {@code element} then {@code false} is returned.
     * </p>
     *
     * @throws ProjogException if {@code list} is not of type {@code TermType#LIST} or {@code TermType#EMPTY_LIST}
     */
    public static bool IsMember(Term element, Term list)
    {
        if (list.Type != TermType.LIST && list.Type != TermType.EMPTY_LIST)
        {
            // TODO have InvalidTermTypeException which : ProjogException
            throw new PrologException("Expected list or empty list but got: " + list.Type + " with value: " + list);
        }
        while (list.Type == TermType.LIST)
        {
            if (element.Unify(list.GetArgument(0)))
            {
                return true;
            }
            element.Backtrack();
            list.Backtrack();
            list = list.GetArgument(1);
        }
        return list.Type != TermType.EMPTY_LIST
            && (list.Type.IsVariable
                ? list.Unify(ListFactory.CreateList(element, new Variable()))
                : throw new PrologException("Expected empty list or variable but got: " + list.Type + " with value: " + list));
    }
}
