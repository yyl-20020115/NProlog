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
 * Represents an unspecified {@link Term}.
 * <p>
 * A {@code Variable} can be either instantiated (representing another single {@link Term}) or uninstantiated (not
 * representing any other {@link Term}). {@code Variable}s are not constants. What {@link Term}, if any, a
 * {@code Variable} is instantiated with can vary during its life time. A {@code Variable} becomes instantiated by calls
 * to {@link #unify(Term)} and becomes uninstantiated again by calls to {@link #Backtrack()}.
 */
public class Variable : Term
{
    public const string ANONYMOUS_VARIABLE_ID = "_";
    public const string UNICODE_VARIABLE_PREFIX = "@";
    /**
     * The value by which the variable can be identified
     */
    private readonly string id;

    /**
     * The {@link Term} this object is currently instantiated with (or {@code null} if it is currently uninstantiated)
     */
    private Term? value = null;

    /**
     * Creates an anonymous variable. The ID of the variable will be an underscore.
     */
    public Variable() : this(ANONYMOUS_VARIABLE_ID) { }
    /**
     * @param id value by which this variable can be identified
     */
    public Variable(string id) => this.id = id;

    /**
     * Calls {@link Term#Name} on the {@link Term} this variable is instantiated with.
     *
     * @throws NullReferenceException if the {@code Variable} is currently uninstantiated
     */

    public string Name => value == null ? throw new NullReferenceException() : Value?.Name??"";

    /**
     * @return value provided in constructor by which this variable can be identified
     */
    public string Id => id;

    public bool IsAnonymous => ANONYMOUS_VARIABLE_ID.Equals(id);

    /**
     * Calls {@link Term#getArgs()} on the {@link Term} this variable is instantiated with.
     *
     * @throws NullReferenceException if the {@code Variable} is currently uninstantiated
     */

    public Term[] Args => value == null ? throw new NullReferenceException() : Value.Args;

    /**
     * Calls {@link Term#getNumberOfArguments()} on the {@link Term} this variable is instantiated with.
     *
     * @throws NullReferenceException if the {@code Variable} is currently uninstantiated
     */

    public int NumberOfArguments => value == null ? throw new NullReferenceException() : Value.NumberOfArguments;

    /**
     * Calls {@link Term#getArgument(int)} on the {@link Term} this variable is instantiated with.
     *
     * @throws NullReferenceException if the {@code Variable} is currently uninstantiated
     */

    public Term GetArgument(int index) => value == null ? throw new NullReferenceException() : Value.GetArgument(index);


    public bool Unify(Term t)
    {
        t = t.Bound;
        if (t == this || t == value)
        {
            return true;
        }
        else if (value == null)
        {
            value = t;
            return true;
        }
        else
        {
            return Value.Unify(t.Term);
        }
    }

    /**
     * Returns {@link TermType#VARIABLE} if uninstantiated else {@link TermType} of instantiated {@link Term}.
     *
     * @return {@link TermType#VARIABLE} if this variable is uninstantiated else calls {@link Term#getType()} on the
     * {@link Term} this variable is instantiated with.
     */

    public TermType Type => value == null ? TermType.VARIABLE : Value.Type;

    /**
     * Always returns {@code false} even if instantiated with an immutable {@link Term}.
     *
     * @return {@code false}
     */

    public bool IsImmutable => false;


    public Term Copy(Dictionary<Variable, Variable> sharedVariables)
    {
        if (value == null)
        {
            if (!sharedVariables.TryGetValue(this, out var result))
                sharedVariables.Add(this, result = new Variable(id));
            return result.Term;
        }
        else
        {
            return Value.Copy(sharedVariables);
        }
    }


    public Term Bound => value == null ? this : Value;

    /**
     * @return itself if this variable is uninstantiated else calls {@link Term#getType()} on the {@link Term} this
     * variable is instantiated with.
     */

    public Term Term => value == null ? this : Value.Term;

    private Term? Value
    {
        get
        {
            if (value is Variable)
            {
                // if variable assigned to another variable use while loop
                // rather than value.getTerm() to avoid StackOverflowError
                Term t = value;
                do
                {
                    var v = (Variable)t;
                    if (v.value == null)
                    {
                        return v;
                    }
                    if (v.value is not Variable)
                    {
                        return v.value;
                    }
                    t = v.value;
                } while (t != value);

                throw new InvalidOperationException();
            }
            else
            {
                return value;
            }
        }
    }

    /**
     * Reverts this variable to an uninstantiated state.
     */

    public void Backtrack() => this.value = null;

    /**
     * @return if this variable is uninstantiated then returns this variable's id else calls {@code ToString()} on the
     * {@link Term} this variable is instantiated with.
     */

    public override string ToString() => value == null ? id : Value?.ToString()??"";
}
