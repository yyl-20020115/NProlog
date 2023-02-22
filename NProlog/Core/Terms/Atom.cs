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
namespace Org.NProlog.Core.Terms;


/**
 * Represents a specific object or relationship.
 * <p>
 * Atoms are constant; their values cannot be changed after they are created. Atoms have no arguments.
 */
public class Atom : Term
{
    private readonly string value;

    /**
     * @param value the value this {@code Atom} represents
     */
    public Atom(string value) => this.value = value;

    /**
     * Returns the value this {@code Atom} represents.
     *
     * @return the value this {@code Atom} represents
     */

    public string Name => value;


    public Term[] Args => TermUtils.EMPTY_ARRAY;


    public int NumberOfArguments => 0;

    /**
     * @throws IndexOutOfRangeException as this implementation of {@link Term} has no arguments
     */

    public Term GetArgument(int index) => throw new IndexOutOfRangeException(nameof(index)+":"+index);

    /**
     * Returns {@link TermType#ATOM}.
     *
     * @return {@link TermType#ATOM}
     */

    public TermType Type => TermType.ATOM;


    public bool IsImmutable => true;


    public Atom Copy(Dictionary<Variable, Variable> sharedVariables) => this;


    public Atom Term => this;


    public bool Unify(Term t)
    {
        var tType = t.Type;
        return tType == TermType.ATOM ? value.Equals(t.Name) : tType.IsVariable ? t.Unify(this) : false;
    }


    public void Backtrack()
    {
        // do nothing
    }


    public override bool Equals(object? o) => o == this || (o is Atom atom && value.Equals(atom.value));


    public override int GetHashCode() => value.GetHashCode();

    /**
     * @return {@link #Name}
     */

    public override string ToString() => Name;

    Term Term.Copy(Dictionary<Variable, Variable> sharedVariables) => this.Copy(sharedVariables);

    Term Term.Term => this.Term;

    public Term Bound => this;
}
