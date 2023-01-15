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
 * Represents a data structure with no {@link Term}s.
 *
 * @see List
 * @see ListFactory
 */
public class EmptyList : Term
{
    /**
     * Singleton instance
     * <p>
     * For performance reasons, use t.Type==TermType.EMPTY_LIST to check if a term is an empty list, rather than
     * using t.getTerm()==EmptyList.EMPTY_LIST. TODO confirm this is what the code is doing
     */
    public static readonly EmptyList EMPTY_LIST = new();

    /**
     * Private constructor to force use of {@link #EMPTY_LIST}
     */
    private EmptyList()
    {
        // do nothing
    }


    public void Backtrack()
    {
        // do nothing
    }


    public Term Copy(Dictionary<Variable, Variable> sharedVariables) => EMPTY_LIST;


    public EmptyList Term => EMPTY_LIST;


    public bool IsImmutable => true;


    public Term[] Args => TermUtils.EMPTY_ARRAY;


    public int NumberOfArguments => 0;

    /**
     * @throws IndexOutOfRangeException as this implementation of {@link Term} has no arguments
     */

    public Term GetArgument(int index) => throw new IndexOutOfRangeException(nameof(index) + ":" + index);

    /**
     * Returns {@link ListFactory#LIST_PREDICATE_NAME}.
     *
     * @return {@link ListFactory#LIST_PREDICATE_NAME}
     */

    public string Name => ListFactory.LIST_PREDICATE_NAME;

    /**
     * Returns {@link TermType#EMPTY_LIST}.
     *
     * @return {@link TermType#EMPTY_LIST}
     */

    public TermType Type => TermType.EMPTY_LIST;

    Term Term.Term => this.Term;

    public Term Bound => this;

    public bool Unify(Term t)
    {
        var tType = t.Type;
        return tType == TermType.EMPTY_LIST || (tType.isVariable && t.Unify(this));
    }

    /**
     * @return {@code []}
     */

    public override string ToString() => "[]";
}
