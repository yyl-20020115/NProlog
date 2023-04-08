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
using Org.NProlog.Core.Math;

namespace Org.NProlog.Core.Terms;


/**
 * Helper methods for performing common tasks on {@link Term} instances.
 */
public class TermUtils
{
    /**
     * A {@link Term} array of Length 0.
     * <p>
     * Should be used wherever a zero-Length {@link Term} array is required in order to minimise object creation.
     */
    public static readonly Term[] EMPTY_ARRAY = Array.Empty<Term>();

    /**
     * Private constructor as all methods are static.
     */

    /**
     * Returns copies of the specified {link Term}s
     *
     * @param input {@link Term}s to copy
     * @return copies of the specified {link Term}s
     */
    public static Term[] Copy(params Term[] input)
    {
        var numTerms = input.Length;
        var output = new Term[numTerms];
        Dictionary<Variable, Variable> vars = new();
        for (int i = 0; i < numTerms; i++)
            output[i] = input[i].Copy(vars);
        return output;
    }

    /**
     * Backtracks all {@link Term}s in the specified array.
     *
     * @param terms {@link Term}s to Backtrack
     * @see Term#Backtrack()
     */
    public static void Backtrack(Term[] terms)
    {
        foreach (var t in terms)
            t.Backtrack();
    }

    /**
     * Attempts to unify all corresponding {@link Term}s in the specified arrays.
     * <p>
     * <b>Note: If the attempt to unify the corresponding terms is unsuccessful only the terms in {@code queryArgs} will
     * get backtracked.</b>
     *
     * @param queryArgs terms to unify with {@code consequentArgs}
     * @param consequentArgs terms to unify with {@code queryArgs}
     * @return {@code true} if the attempt to unify all corresponding terms was successful
     */
    public static bool Unify(Term[] queryArgs, Term[] consequentArgs)
    {
        for (int i = 0; i < queryArgs.Length; i++)
        {
            if (!consequentArgs[i].Unify(queryArgs[i]))
            {
                for (int j = 0; j < i; j++)
                    queryArgs[j].Backtrack();
                return false;
            }
        }
        return true;
    }

    /**
     * Returns all {@link Variable}s contained in the specified term.
     *
     * @param argument the term to find variables for
     * @return all {@link Variable}s contained in the specified term.
     */
    public static HashSet<Variable> GetAllVariablesInTerm(Term argument) 
        => GetAllVariablesInTerm(argument, new());

    private static HashSet<Variable> GetAllVariablesInTerm(Term argument, HashSet<Variable> variables)
    {
        if (argument.IsImmutable)
        {
            // ignore
        }
        else if (argument.Type == TermType.VARIABLE)
            variables.Add((Variable)argument);
        else
        {
            for (int i = 0; i < argument.NumberOfArguments; i++)
                GetAllVariablesInTerm(argument.GetArgument(i), variables);
        }
        return variables;
    }

    /**
     * Return the {@link Numeric} represented by the specified {@link Term}.
     *
     * @param t the term representing a {@link Numeric}
     * @return the {@link Numeric} represented by the specified {@link Term}
     * @throws PrologException if the specified {@link Term} does not represent a {@link Numeric}
     */
    public static Numeric CastToNumeric(Term t) 
        => t.Type.IsNumeric
        ? t.Term as Numeric
        : throw new PrologException($"Expected Numeric but got: {t.Type} with value: {t}");

    /**
     * Returns the integer value of the {@link Numeric} represented by the specified {@link Term}.
     *
     * @param t the term representing a {@link Numeric}
     * @return the {@code int} value represented by {@code t}
     * @throws PrologException if the specified {@link Term} cannot be represented as an {@code int}.
     */
    public static int ToInt(Term t)
    {
        var n = CastToNumeric(t);
        var l = n.Long;
        if (l < int.MinValue || l > int.MaxValue)
        {
            throw new PrologException($"Value cannot be cast to an int without losing precision: {l}");
        }
        return (int)l;
    }

    /**
     * Return the long value represented by the specified term.
     *
     * @param t the term representing a long value
     * @return the {@code long} value represented by {@code t}
     * @throws PrologException if the specified {@link Term} does not represent a term of type {@link TermType#INTEGER}
     */
    public static long ToLong(ArithmeticOperators operators, Term t)
    {
        var n = operators.GetNumeric(t);
        return n?.Type == TermType.INTEGER 
            ? n.Long 
            : throw new PrologException($"Expected integer but got: {n?.Type} with value: {n}");
    }

    /**
     * Return the name of the {@link Atom} represented by the specified {@link Atom}.
     *
     * @param t the term representing an {@link Atom}
     * @return the name of {@link Atom} represented by the specified {@link Term}
     * @throws PrologException if the specified {@link Term} does not represent an {@link Atom}
     */
    public static string GetAtomName(Term t) 
        => t.Type != TermType.ATOM ? throw new PrologException($"Expected an atom but got: {t.Type} with value: {t}") : t.Name;

    public static void AssertType(Term t, TermType type)
    {
        if (t.Type != type)
            throw new PrologException($"Expected {type} but got: {t.Type} with value: {t}");
    }

    public static bool TermsEqual(Term? a, Term? b)
        => (a?.Term?.Equals(b?.Term)).GetValueOrDefault();

}
