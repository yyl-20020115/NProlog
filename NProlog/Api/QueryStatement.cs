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
using Org.NProlog.Core.Kb;
using Org.NProlog.Core.Parser;
using Org.NProlog.Core.Predicate;
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Api;

/**
 * Represents a query.
 * <p>
 * single use, not multi-threaded
 *
 * @see Prolog#createStatement(string)
 * @see Prolog#createPlan(string)
 */
public class QueryStatement
{
    private static readonly Dictionary<string, Variable> EMPTY_VARIABLES = new();

    private readonly PredicateFactory predicateFactory;
    private readonly Term parsedInput;
    private readonly Dictionary<string, Variable> variables;
    private bool invoked;

    /**
     * Creates a new {@code QueryStatement} representing a query specified by {@code prologQuery}.
     *
     * @param kb the {@link KnowledgeBase} to query against
     * @param prologQuery prolog syntax representing a query (do not prefix with a {@code ?-})
     * @throws PrologException if an error occurs parsing {@code prologQuery}
     */
    public QueryStatement(KnowledgeBase kb, string prologQuery)
    {
        try
        {
            var parser = SentenceParser.GetInstance(prologQuery, kb.Operands);

            this.parsedInput = parser.ParseSentence();
            this.predicateFactory = kb.Predicates.GetPredicateFactory(parsedInput);
            this.variables = parser.GetParsedTermVariables();

            if (parser.ParseSentence() != null)
                throw new PrologException($"More input found after . in {prologQuery}");
        }
        catch (ParserException pe)
        {
            throw pe;
        }
        catch (Exception e)
        {
            throw new PrologException($"{e.GetType().Name} caught parsing: {prologQuery}", e);
        }
    }

    /**
     * Creates a new {@code QueryStatement} representing a query specified by {@code prologQuery}.
     *
     * @param PredicateFactory the {@link PredicateFactory} that will be used to execute the query
     * @param prologQuery prolog syntax representing a query (do not prefix with a {@code ?-})
     * @throws PrologException if an error occurs parsing {@code prologQuery}
     */
    public QueryStatement(PredicateFactory predicateFactory, Term prologQuery)
    {
        this.predicateFactory = predicateFactory;
        if (prologQuery.IsImmutable)
        {
            this.parsedInput = prologQuery;
            this.variables = EMPTY_VARIABLES;
        }
        else
        {
            Dictionary<Variable, Variable> sharedVariables = new();
            this.parsedInput = prologQuery.Copy(sharedVariables);
            this.variables = new(sharedVariables.Count);
            foreach (var variable in sharedVariables.Values)
                if (!variable.IsAnonymous && this.variables.ContainsKey(variable.Id))
                    throw new InvalidOperationException($"Duplicate variable id: {variable.Id}");
                else
                    this.variables.Add(variable.Id, variable);
        }
    }

    /**
     * Attempts to unify the specified term to the variable with the specified id.
     *
     * @param variableId the id of the variable
     * @param term the term to unify to the variable
     * @throws PrologException if no variable with the specified id exists in the query this object represents, or the
     * given term cannot be unified with the variable
     * @see #setAtomName(string, string)
     * @see #setDouble(string, double)
     * @see #setLong(string, long)
     * @see #setListOfAtomNames(string, List)
     * @see #setListOfAtomNames(string, string...)
     * @see #setListOfDoubles(string, List)
     * @see #setListOfDoubles(string, double...)
     * @see #setListOfLongs(string, List)
     * @see #setListOfLongs(string, long...)
     * @see #setListOfTerms(string, List)
     * @see #setListOfTerms(string, Term...)
     */
    public void SetTerm(string variableId, Term term)
    {
        if (!variables.TryGetValue(variableId, out var v))
            throw new PrologException($"Do not know about variable named: {variableId} in query: {parsedInput}");
        if (!v.Type.IsVariable)
            throw new PrologException($"Cannot set: {variableId} to: {term} as has already been set to: {v}");
        var unified = v.Unify(term);
        // should never get here, just checking result of unify(Term) as a sanity check
        if (!unified)
            throw new InvalidOperationException();
    }

    /**
     * Attempts to unify the specified {@code string} value as an {@link Atom} to the variable with the specified id.
     *
     * @param variableId the id of the variable
     * @param atomName the value to use as the name of the {@code Atom} that the variable will be unified with
     * @throws PrologException if no variable with the specified id exists in the query this object represents, or the
     * given term cannot be unified with the variable
     * @see #setTerm(string, Term)
     */
    public void SetAtomName(string variableId, string atomName) 
        => SetTerm(variableId, new Atom(atomName));

    /**
     * Attempts to unify the specified {@code double} as a {@link DecimalFraction} to the variable with the specified id.
     *
     * @param variableId the id of the variable
     * @param value the value to use as the name of the {@code DecimalFraction} that the variable will be unified with
     * @throws PrologException if no variable with the specified id exists in the query this object represents, or the
     * given term cannot be unified with the variable
     * @see #setTerm(string, Term)
     */
    public void SetDouble(string variableId, double value) 
        => SetTerm(variableId, new DecimalFraction(value));

    /**
     * Attempts to unify the specified {@code long} as a {@link IntegerNumber} to the variable with the specified id.
     *
     * @param variableId the id of the variable
     * @param value the value to use as the name of the {@code IntegerNumber} that the variable will be unified with
     * @throws PrologException if no variable with the specified id exists in the query this object represents, or the
     * given term cannot be unified with the variable
     * @see #setTerm(string, Term)
     */
    public void SetLong(string variableId, long value) 
        => SetTerm(variableId, new IntegerNumber(value));

    /**
     * Attempts to unify the specified {@code string} values as a Prolog list of atoms to the variable with the specified
     * id.
     *
     * @param variableId the id of the variable
     * @param atomNames the values to use as atom elements in the list that the variable will be unified with
     * @throws PrologException if no variable with the specified id exists in the query this object represents, or the
     * given term cannot be unified with the variable
     * @see #setTerm(string, Term)
     */
    public void SetListOfAtomNames(string variableId, params string[] atomNames)
    {
        var terms = new Term[atomNames.Length];
        for (int i = 0; i < atomNames.Length; i++)
            terms[i] = new Atom(atomNames[i]);
        SetTerm(variableId, ListFactory.CreateList(terms));
    }

    /**
     * Attempts to unify the specified {@code string} values as a Prolog list of atoms to the variable with the specified
     * id.
     *
     * @param variableId the id of the variable
     * @param atomNames the values to use as atom elements in the list that the variable will be unified with
     * @throws PrologException if no variable with the specified id exists in the query this object represents, or the
     * given term cannot be unified with the variable
     * @see #setTerm(string, Term)
     */
    public void SetListOfAtomNames(string variableId, List<string> atomNames)
    {
        var terms = new Term[atomNames.Count];
        for (int i = 0; i < atomNames.Count; i++)
            terms[i] = new Atom(atomNames[(i)]);
        SetTerm(variableId, ListFactory.CreateList(terms));
    }

    /**
     * Attempts to unify the specified {@code double} values as a Prolog list to the variable with the specified id.
     *
     * @param variableId the id of the variable
     * @param doubles the values to use as elements in the list that the variable will be unified with
     * @throws PrologException if no variable with the specified id exists in the query this object represents, or the
     * given term cannot be unified with the variable
     * @see #setTerm(string, Term)
     */
    public void SetListOfDoubles(string variableId, params double[] doubles)
    {
        var terms = new Term[doubles.Length];
        for (int i = 0; i < doubles.Length; i++)
            terms[i] = new DecimalFraction(doubles[i]);
        SetTerm(variableId, ListFactory.CreateList(terms));
    }

    /**
     * Attempts to unify the specified {@code Double} values as a Prolog list to the variable with the specified id.
     *
     * @param variableId the id of the variable
     * @param doubles the values to use as elements in the list that the variable will be unified with
     * @throws PrologException if no variable with the specified id exists in the query this object represents, or the
     * given term cannot be unified with the variable
     * @see #setTerm(string, Term)
     */
    public void SetListOfDoubles(string variableId, List<double> doubles)
    {
        var terms = new Term[doubles.Count];
        for (int i = 0; i < doubles.Count; i++)
            terms[i] = new DecimalFraction(doubles[(i)]);
        SetTerm(variableId, ListFactory.CreateList(terms));
    }

    /**
     * Attempts to unify the specified {@code long} values as a Prolog list to the variable with the specified id.
     *
     * @param variableId the id of the variable
     * @param longs the values to use as elements in the list that the variable will be unified with
     * @throws PrologException if no variable with the specified id exists in the query this object represents, or the
     * given term cannot be unified with the variable
     * @see #setTerm(string, Term)
     */
    public void SetListOfLongs(string variableId, params long[] longs)
    {
        var terms = new Term[longs.Length];
        for (int i = 0; i < longs.Length; i++)
            terms[i] = new IntegerNumber(longs[i]);
        SetTerm(variableId, ListFactory.CreateList(terms));
    }

    /**
     * Attempts to unify the specified {@code long} values as a Prolog list to the variable with the specified id.
     *
     * @param variableId the id of the variable
     * @param longs the values to use as elements in the list that the variable will be unified with
     * @throws PrologException if no variable with the specified id exists in the query this object represents, or the
     * given term cannot be unified with the variable
     * @see #setTerm(string, Term)
     */
    public void SetListOfLongs(string variableId, List<long> longs)
    {
        var terms = new Term[longs.Count];
        for (int i = 0; i < longs.Count; i++)
            terms[i] = new IntegerNumber(longs[(i)]);
        SetTerm(variableId, ListFactory.CreateList(terms));
    }

    /**
     * Attempts to unify the specified {@code Term} values as a Prolog list to the variable with the specified id.
     *
     * @param variableId the id of the variable
     * @param terms the values to use as elements in the list that the variable will be unified with
     * @throws PrologException if no variable with the specified id exists in the query this object represents, or the
     * given term cannot be unified with the variable
     * @see #setTerm(string, Term)
     */
    public void SetListOfTerms(string variableId, params Term[] terms) 
        => SetTerm(variableId, ListFactory.CreateList(terms));

    /**
     * Attempts to unify the specified {@code Term} values as a Prolog list to the variable with the specified id.
     *
     * @param variableId the id of the variable
     * @param terms the values to use as elements in the list that the variable will be unified with
     * @throws PrologException if no variable with the specified id exists in the query this object represents, or the
     * given term cannot be unified with the variable
     * @see #setTerm(string, Term)
     */
    public void SetListOfTerms(string variableId, List<Term> terms) 
        => SetTerm(variableId, ListFactory.CreateList(terms));

    /**
     * Returns a new {@link QueryResult} for the query represented by this object.
     * <p>
     * Note that the query is not evaluated as part of a call to {@code executeQuery()}. It is on the first call of
     * {@link QueryResult#next()} that the first attempt to evaluate the query will be made.
     *
     * @return a new {@link QueryResult} for the query represented by this object.
     */
    public QueryResult ExecuteQuery()
    {
        if (invoked)
            throw new PrologException("This QueryStatement has already been evaluated. "
                        + "If you want to reuse the same query then consider using a QueryPlan. See: Prolog.CreatePlan(string)");
        invoked = true;
        return new(predicateFactory, parsedInput, variables);
    }

    /**
     * Evaluate once the query represented by this statement.
     * <p>
     * The query will only be evaluated once, even if further solutions could of been found on backtracking.
     *
     * @throws PrologException if no solution can be found
     * @see #executeQuery()
     */
    public void ExecuteOnce()
    {
        if (!ExecuteQuery().Next())
            throw new PrologException($"Failed to find a solution for: {parsedInput}");
    }

    /**
     * Execute the query once and return a string representation of the atom the single query variable was unified with.
     *
     * @return the name of the atom the query variable has been unified with as a result of executing the query
     * @throws PrologException if the query could not be evaluated successfully
     * @throws PrologException of there is not exactly one named variable in the query this statement represents
     */
    public string FindFirstAsAtomName()
    {
        var variableId = GetSingleVariableId();
        var result = ExecuteQuery();
        return result.Next() ? result.GetAtomName(variableId) : throw NoSolutionFound();
    }

    /**
     * Execute the query once and return a {@code double} representation of the term the single query variable was
     * unified with.
     *
     * @return the value the query variable has been unified with as a result of executing the query
     * @throws PrologException if the query could not be evaluated successfully
     * @throws PrologException of there is not exactly one named variable in the query this statement represents
     */
    public double FindFirstAsDouble()
    {
        var variableId = GetSingleVariableId();
        var result = ExecuteQuery();
        return result.Next() ? result.GetDouble(variableId) : throw NoSolutionFound();
    }

    /**
     * Execute the query once and return a {@code long} representation of the term the single query variable was unified
     * with.
     *
     * @return the value query variable has been unified with as a result of executing the query
     * @throws PrologException if the query could not be evaluated successfully
     * @throws PrologException of there is not exactly one named variable in the query this statement represents
     */
    public long FindFirstAsLong()
    {
        var variableId = GetSingleVariableId();
        var result = ExecuteQuery();
        return result.Next() ? result.GetLong(variableId) : throw NoSolutionFound();
    }

    /**
     * Execute the query once and return the {@code Term} the single query variable was unified with.
     *
     * @return the value query variable has been unified with as a result of executing the query
     * @throws PrologException if the query could not be evaluated successfully
     * @throws PrologException of there is not exactly one named variable in the query this statement represents
     */
    public Term FindFirstAsTerm()
    {
        var variableId = GetSingleVariableId();
        var result = ExecuteQuery();
        return result.Next() ? result.GetTerm(variableId) : throw NoSolutionFound();
    }

    private static PrologException NoSolutionFound() 
        => new ("No solution found.");

    /**
     * Attempt to execute the query once and return a string representation of the atom the single query variable was
     * unified with.
     *
     * @return the name of the atom the query variable has been unified with, or an empty optional if the query was not
     * successfully evaluated
     * @throws PrologException if the query could not be evaluated successfully
     * @throws PrologException of there is not exactly one named variable in the query this statement represents
     */
    public Optional<string> FindFirstAsOptionalAtomName()
    {
        var variableId = GetSingleVariableId();
        var result = ExecuteQuery();
        return result.Next() 
            ? Optional<string>.Of(result.GetAtomName(variableId)) 
            : Optional<string>.Empty;
    }

    /**
     * Attempt to execute the query once and return a {@code Double} representation of the term the single query variable
     * was unified with.
     *
     * @return the value the query variable has been unified with, or an empty optional if the query was not successfully
     * evaluated
     * @throws PrologException if the query could not be evaluated successfully
     * @throws PrologException of there is not exactly one named variable in the query this statement represents
     */
    public Optional<double> FindFirstAsOptionalDouble()
    {
        var variableId = GetSingleVariableId();
        var result = ExecuteQuery();
        return result.Next() ? Optional<double>.Of(result.GetDouble(variableId)) : Optional<double>.Empty;
    }

    /**
     * Attempt to execute the query once and return a {@code long} representation of the term the single query variable
     * was unified with.
     *
     * @return the value the query variable has been unified with, or an empty optional if the query was not successfully
     * evaluated
     * @throws PrologException if the query could not be evaluated successfully
     * @throws PrologException of there is not exactly one named variable in the query this statement represents
     */
    public Optional<long> FindFirstAsOptionalLong()
    {
        var variableId = GetSingleVariableId();
        var result = ExecuteQuery();
        return result.Next() ? Optional<long>.Of(result.GetLong(variableId)) : Optional<long>.Empty;
    }

    /**
     * Attempt to execute the query once and return a {@code Term} representation of the term the single query variable
     * was unified with.
     *
     * @return the value the query variable has been unified with, or an empty optional if the query was not successfully
     * evaluated
     * @throws PrologException if the query could not be evaluated successfully
     * @throws PrologException of there is not exactly one named variable in the query this statement represents
     */
    public Optional<Term> FindFirstAsOptionalTerm()
    {
        var variableId = GetSingleVariableId();
        var result = ExecuteQuery();
        return result.Next() ? Optional<Term>.Of(result.GetTerm(variableId)) : Optional<Term>.Empty;
    }

    /**
     * Find all solutions generated by the query and return string representations of the atoms the single query variable
     * was unified with.
     *
     * @return list of atom names the query variable was been unified with as a result of executing the query until no
     * more solutions were found
     * @throws PrologException if the query could not be evaluated successfully
     * @throws PrologException of there is not exactly one named variable in the query this statement represents
     */
    public List<string> FindAllAsAtomName()
    {
        var variableId = GetSingleVariableId();
        var result = ExecuteQuery();
        List<string> values = new();
        while (result.Next())
            values.Add(result.GetAtomName(variableId));
        return values;
    }

    /**
     * Find all solutions generated by the query and return the {@code double} values the single query variable was
     * unified with.
     *
     * @return list of values the query variable was been unified with as a result of executing the query until no more
     * solutions were found
     * @throws PrologException if the query could not be evaluated successfully
     * @throws PrologException of there is not exactly one named variable in the query this statement represents
     */
    public List<double> FindAllAsDouble()
    {
        var variableId = GetSingleVariableId();
        var result = ExecuteQuery();
        List<double> values = new();
        while (result.Next())
            values.Add(result.GetDouble(variableId));
        return values;
    }

    /**
     * Find all solutions generated by the query and return the {@code long} values the single query variable was unified
     * with.
     *
     * @return list of values the query variable was been unified with as a result of executing the query until no more
     * solutions were found
     * @throws PrologException if the query could not be evaluated successfully
     * @throws PrologException of there is not exactly one named variable in the query this statement represents
     */
    public List<long> FindAllAsLong()
    {
        var variableId = GetSingleVariableId();
        var result = ExecuteQuery();
        List<long> atomNames = new();
        while (result.Next())
            atomNames.Add(result.GetLong(variableId));
        return atomNames;
    }

    /**
     * Find all solutions generated by the query and return the {@code Term} values the single query variable was unified
     * with.
     *
     * @return list of values the query variable was been unified with as a result of executing the query until no more
     * solutions were found
     * @throws PrologException if the query could not be evaluated successfully
     * @throws PrologException of there is not exactly one named variable in the query this statement represents
     */
    public List<Term> FindAllAsTerm()
    {
        var variableId = GetSingleVariableId();
        var result = ExecuteQuery();
        List<Term> terms = new();
        while (result.Next())
            terms.Add(result.GetTerm(variableId));
        return terms;
    }

    /**
     * Returns the ID of the single variable contained in the query this statement represents.
     *
     * @return variable ID
     * @throws PrologException of there is not exactly one named variable in the query this statement represents
     */
    private string GetSingleVariableId()
    {
        string? id = null;

        foreach (var e in variables)
            if (e.Value.Type.IsVariable)
                {
                    if (id != null)
                        throw new PrologException($"Expected exactly one uninstantiated variable but found {id} and {e.Key}");
                    id = e.Key;
                }

        if (id == null)
            throw new PrologException($"Expected exactly one uninstantiated variable but found none in: {parsedInput} {variables}");

        return id;
    }
}
