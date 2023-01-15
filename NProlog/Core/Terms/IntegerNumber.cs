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
using Org.NProlog.Core.Math;

namespace Org.NProlog.Core.Terms;



/**
 * Represents a value of the primitive type {@code long} as a {@link Term}.
 * <p>
 * IntegerNumbers are constant; their values cannot be changed after they are created. IntegerNumbers have no arguments.
 */
public class IntegerNumber : Numeric
{
    private readonly long value;

    /**
     * @param value the value this term represents
     */
    public IntegerNumber(long value)
    {
        this.value = value;
    }

    /**
     * Returns a {@code string} representation of the {@code long} this term represents.
     *
     * @return a {@code string} representation of the {@code long} this term represents
     */

    public string Name => ToString();


    public Term[] Args => TermUtils.EMPTY_ARRAY;


    public int NumberOfArguments => 0;

    /**
     * @throws IndexOutOfRangeException as this implementation of {@link Term} has no arguments
     */

    public Term GetArgument(int index) => throw new IndexOutOfRangeException(nameof(index)+":"+index);

    /**
     * Returns {@link TermType#INTEGER}.
     *
     * @return {@link TermType#INTEGER}
     */

    public TermType Type => TermType.INTEGER;


    public bool IsImmutable => true;


    public IntegerNumber Term => this;


    public IntegerNumber Copy(Dictionary<Variable, Variable> sharedVariables) => this;


    public bool Unify(Term t)
    {
        var tType = t.Type;
        return tType == TermType.INTEGER
            ? value == ((Numeric)t.Term).Long
            : (tType.isVariable || tType == TermType.CLP_VARIABLE) && t.Unify(this);
    }


    public void Backtrack()
    {
        // do nothing
    }

    /**
     * @return the {@code long} value of this term
     */

    public long Long => value;

    /**
     * @return the {@code long} value of this term cast to a {@code double}
     */

    public double Double => value;


    public IntegerNumber Calculate(Term[] args) => this;


    public override bool Equals(object? o) => o == this || (o is Numeric n && n.IsImmutable && n.Type == TermType.INTEGER && value == n.Long);


    public override int GetHashCode() => value.GetHashCode();

    /**
     * @return a {@code string} representation of the {@code long} this term represents
     */

    public override string ToString() => value.ToString();

    Term Term.Copy(Dictionary<Variable, Variable> sharedVariables) => this.Copy(sharedVariables);

    Term Term.Term => this.Term;

    public Term Bound => this;

    Numeric ArithmeticOperator.Calculate(Term[] args) => this.Calculate(args);
}
