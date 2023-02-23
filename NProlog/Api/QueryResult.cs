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
using Org.NProlog.Core.Predicate;
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Api;

/**
 * Represents an executing query.
 */
public class QueryResult
{
    private readonly Predicate predicate;
    private readonly Dictionary<string, Variable> variables;
    private bool hasBeenEvaluated;
    private bool hasFailed;

    /**
     * Evaluates a query.
     *
     * @param PredicateFactory the {@link PredicateFactory} that will be used to evaluate the query
     * @param query represents the query statement being evaluated
     * @param variables collection of variables contained in the query (keyed by variable id)
     * @see QueryStatement#executeQuery()
     */
    public QueryResult(PredicateFactory predicateFactory, Term query, Dictionary<string, Variable> variables)
    {
        var numArgs = query.NumberOfArguments;
        if (numArgs == 0)
        {
            this.predicate = predicateFactory.GetPredicate(TermUtils.EMPTY_ARRAY);
        }
        else
        {
            var args = new Term[numArgs];
            for (int i = 0; i < args.Length; i++)
                args[i] = query.GetArgument(i).Term;
            this.predicate = predicateFactory.GetPredicate(args);
        }

        this.variables = variables;
    }

    /**
     * Attempts to evaluate the query this object represents.
     * <p>
     * Subsequent calls of the {@code next()} method attempt to reevaluate the query, and because it returns
     * {@code false} when the are no more results, it can be used in a {@code while} loop to iterate through all the
     * results.
     *
     * @return {@code true} if the query was (re)evaluated successfully or {@code false} if there are no more results.
     * Once {@code false} has been returned by {@code next()} the {@code next()} method should no longer be called on
     * that object.
     * @throws ProjogException if an error occurs while evaluating the query
     */
    public bool Next()
    {
        if (hasFailed)
            throw new PrologException("Query has already been exhausted. Last call to QueryResult.next() returned false.");

        bool result;

        if (!hasBeenEvaluated)
        {
            hasBeenEvaluated = true;
            result = Evaluate();
        }
        else
        {
            result = predicate.CouldReevaluationSucceed && Evaluate();
        }

        hasFailed = !result;

        return result;
    }

    private bool Evaluate()
    {
        try
        {
            return predicate.Evaluate();
        }
        catch (CutException)
        {
            // e.g. for a query like: "?- true, !."
            return false;
        }
    }

    /**
     * Returns {@code true} if it is known that all possible solutions have been found, else {@code false}.
     *
     * @return {@code true} if it is known that all possible solutions have been found, else {@code false}.
     * @see org.projog.core.predicate.Predicate#couldReevaluationSucceed()
     */
    public bool IsExhausted => hasFailed || (hasBeenEvaluated && !predicate.CouldReevaluationSucceed);

    /**
     * Returns the name of the atom instantiated to the variable with the specified id.
     *
     * @param variableId the id of the variable from which to return the instantiated term
     * @return the name of the atom instantiated to the variable with the specified id
     * @throws ProjogException if no variable with the specified id exists in the query this object represents, or if the
     * term instantiated to the variable is not an atom
     * @see #getTerm(string)
     */
    public string GetAtomName(string variableId) => TermUtils.GetAtomName(GetTerm(variableId));

    /**
     * Returns the {@code double} value instantiated to the variable with the specified id.
     *
     * @param variableId the id of the variable from which to return the instantiated term
     * @return the name of the atom instantiated to the variable with the specified id
     * @throws ProjogException if no variable with the specified id exists in the query this object represents, or if the
     * term instantiated to the variable is not a number
     * @see #getTerm(string)
     */
    public double GetDouble(string variableId) => TermUtils.CastToNumeric(GetTerm(variableId)).Double;

    /**
     * Returns the {@code long} value instantiated to the variable with the specified id.
     *
     * @param variableId the id of the variable from which to return the instantiated term
     * @return the value instantiated to the variable with the specified id
     * @throws ProjogException if no variable with the specified id exists in the query this object represents, or if the
     * term instantiated to the variable is not a number
     * @see #getTerm(string)
     */
    public long GetLong(string variableId) => TermUtils.CastToNumeric(GetTerm(variableId)).Long;

    /**
     * Returns the term instantiated to the variable with the specified id.
     * <p>
     * {@link #next()} must be called before this method.
     *
     * @param variableId the id of the variable from which to return the instantiated term
     * @return the term instantiated to the variable with the specified id (or the {@link org.projog.core.term.Variable}
     * of representing the variable if it is uninstantiated)
     * @throws ProjogException if no variable with the specified id exists in the query this object represents
     * @see #getAtomName(string)
     * @see #getDouble(string)
     * @see #getLong(string)
     */
    public Term GetTerm(string variableId) => !hasBeenEvaluated
            ? throw new PrologException("Query not yet evaluated. Call QueryResult.next() before attempting to get value of variables.")
            : hasFailed
            ? throw new PrologException("No more solutions. Last call to QueryResult.next() returned false.")
            : !variables.TryGetValue(variableId, out var v)
            ? throw new PrologException($"Unknown variable ID: {variableId}. Query Contains the variables: {StringUtils.ToString(GetVariableIds())}")
            : v.Term;
    /**
     * Returns id's of all variables defined in the query this object represents.
     *
     * @return id's of all variables defined in the query this object represents
     */
    public HashSet<string> GetVariableIds() => new(variables.Keys);
}
