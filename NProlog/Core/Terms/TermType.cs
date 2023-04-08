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
 * Defines the type of terms supported by Prolog.
 *
 * @see Term#getType()
 */
public class TermType
{
    /** @see Variable */
    public static readonly TermType VARIABLE = new(false, false, true, 1,nameof(VARIABLE));
    /** Constraint Logic Programming variable */
    public static readonly TermType CLP_VARIABLE = new(false, false, false, 2, nameof(CLP_VARIABLE));
    /** @see DecimalFraction */
    public static readonly TermType FRACTION = new(false, true, false, 3, nameof(FRACTION));
    /** @see IntegerNumber */
    public static readonly TermType INTEGER = new(false, true, false, 4, nameof(INTEGER));
    /** @see EmptyList */
    public static readonly TermType EMPTY_LIST = new(false, false, false, 5, nameof(EMPTY_LIST));
    /** @see Atom */
    public static readonly TermType ATOM = new(false, false, false, 6, nameof(ATOM));
    /** @see Structure */
    public static readonly TermType STRUCTURE = new(true, false, false, 7, nameof(STRUCTURE));
    /** @see List */
    public static readonly TermType LIST = new(true, false, false, 8, nameof(LIST));

    public readonly string Name;
    public readonly bool IsStructure;
    public readonly bool IsNumeric;
    public readonly bool IsVariable;
    public readonly int Precedence;

    private TermType(bool isStructure, bool isNumeric, bool isVariable, int precedence, string name)
    {
        this.IsStructure = isStructure;
        this.IsNumeric = isNumeric;
        this.IsVariable = isVariable;
        this.Precedence = precedence;
        this.Name = name;
    }

    /**
     * Used to consistently order {@link Term}s of different types.
     *
     * @return precedence of this type
     * @see TermComparator#compare(Term, Term)
     */
    //public int Precedence => precedence;
    public override string ToString() => this.Name;
}
