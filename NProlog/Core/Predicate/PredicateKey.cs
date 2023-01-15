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
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Predicate;



/**
 * Represents the structure of a {@link Term}.
 * <p>
 * Defines {@link Term}s by their name (functor) and number of arguments (arity). This "metadata" or "descriptor
 * information" allows rules whose heads (consequences) share the same structure to be grouped together.
 * <p>
 * As {@link org.projog.core.term.Atom} and {@link org.projog.core.term.Structure} are the only subclasses of
 * {@link org.projog.core.term.Term} that can be the head (consequent) of a rule they are the only subclasses of
 * {@link Term} that {@code PredicateKey} is intended to describe.
 * <p>
 * PredicateKeys are constant; their values cannot be changed after they are created.
 */
public class PredicateKey : IComparable<PredicateKey>
{
    private static readonly string PREDICATE_KEY_FUNCTOR = "/";

    private readonly string name;
    private readonly int numArgs;

    /**
     * Returns a {@code PredicateKey} for the specified term.
     *
     * @param t a term the returned {@code PredicateKey} should represent (needs to have a {@link Term#getType()} value
     * of {@link TermType#ATOM} or {@link TermType#STRUCTURE})
     * @return a {@code PredicateKey} for the specified term.
     * @throws ProjogException if {@code t} is not of type {@link TermType#ATOM} or {@link TermType#STRUCTURE}
     */
    public static PredicateKey CreateForTerm(Term t)
    {
        var type = t.Type;
        if (type != TermType.STRUCTURE && type != TermType.ATOM && type != TermType.LIST)
        {
            throw new PrologException(GetInvalidTypeExceptionMessage(t));
        }
        return new PredicateKey(t.Name, t.NumberOfArguments);
    }

    /**
     * @param t must be a structure named {@code /} where the first argument is the name of the predicate to represent
     * and the second (and readonly) argument is the arity.
     */
    public static PredicateKey CreateFromNameAndArity(Term t)
    {
        if (t.Type != TermType.STRUCTURE)
        {
            throw new PrologException("Expected a predicate with two arguments and the name: '/' but got: " + t);
        }

        if (!PREDICATE_KEY_FUNCTOR.Equals(t.Name) || t.Args.Length != 2)
        {
            throw new PrologException("Expected a predicate with two arguments and the name: '/' but got: " + t);
        }

        var name = TermUtils.GetAtomName(t.Args[0]);
        int arity = TermUtils.ToInt(t.Args[1]);
        return new PredicateKey(name, arity);
    }

    private static string GetInvalidTypeExceptionMessage(Term t) 
        => "Expected an atom or a predicate but got a " + t.Type + " with value: " + t;

    public PredicateKey(string name, int numArgs)
    {
        if (numArgs < 0)
        {
            throw new ArgumentException("Number of arguments: " + numArgs + " is less than 0");
        }
        this.name = name;
        this.numArgs = numArgs;
    }

    public string Name => name;

    public int NumArgs => numArgs;

    public Term ToTerm() => Structure.CreateStructure(PREDICATE_KEY_FUNCTOR, new Term[] { new Atom(name), IntegerNumberCache.ValueOf(numArgs) });

    /**
     * @param o the reference object with which to compare.
     * @return {@code true} if {@code o} is an is {@code PredicateKey} and has the same name and number of
     * arguments as this instance
     */

    public override bool Equals(object? o) => o is PredicateKey k && name.Equals(k.name) && numArgs == k.numArgs;


    public override int GetHashCode() => ToString().GetHashCode();// + numArgs;

    /**
     * @return {@code name+"/"+numArgs}
     */

    public override string ToString() => name + PREDICATE_KEY_FUNCTOR + numArgs;

    /**
     * Ordered on name or, if names identical, number of arguments.
     */

    public int CompareTo(PredicateKey? o)
    {
        int c = name.CompareTo(o?.name);
        return c == 0 ? numArgs.CompareTo(o?.numArgs) : c;
    }
}
