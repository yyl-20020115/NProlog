/*
 * Copyright 2020 S. Webber
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
 * Represents a plan for executing a Prolog query.
 * <p>
 * A single {@code QueryPlan} can be used to create multiple {@link QueryStatement} objects. If you are intending to
 * execute the same query multiple times then, for performance reasons, it is recommended to use a {@code QueryPlan}
 * rather than create multiple {@link QueryStatement} directly. When using a {@code QueryPlan} the Prolog syntax will
 * only be parsed once and the plan for executing the query will be optimised for performance.
 */
public class QueryPlan
{
    private readonly PredicateFactory predicateFactory;
    private readonly Term parsedInput;

    public QueryPlan(KnowledgeBase kb, string prologQuery)
    {
        try
        {
            var parser = SentenceParser.GetInstance(prologQuery, kb.Operands);

            this.parsedInput = parser.ParseSentence();
            this.predicateFactory = kb.Predicates.GetPreprocessedPredicateFactory(parsedInput);

            if (parser.ParseSentence() != null)
                throw new PrologException($"More input found after . in {prologQuery}");
        }
        catch (ParserException pe)
        {
            throw pe;
        }
        catch (Exception ex)
        {
            throw new PrologException($"{ex.GetType().Name} caught parsing: {prologQuery}", ex);
        }
    }

    /**
     * Return a newly created {@link QueryStatement} for the query represented by this plan.
     * <p>
     * Before the query is executed, values can be assigned to variables in the query by using
     * {@link QueryStatement#setTerm(string, Term)}. The query can be executed by calling
     * {@link QueryStatement#executeQuery()}.
     * </p>
     * <p>
     * Note: If you do not intend to assign terms to variables then {@link #executeQuery()} can be called instead.
     * </p>
     *
     * @see #executeQuery()
     * @see #executeOnce()
     */
    public QueryStatement CreateStatement() 
        => new(predicateFactory, parsedInput);

    /**
     * Return a newly created {@link QueryResult} for the query represented by this plan.
     * <p>
     * The {@link QueryResult#next()} and {@link QueryResult#getTerm(string)} methods can be used to evaluate the query
     * and access values unified to the variables of the query.
     *
     * @see #createStatement()
     * @see #executeOnce()
     */
    public QueryResult ExecuteQuery() 
        => CreateStatement().ExecuteQuery();

    /**
     * Evaluate once the query represented by this statement.
     * <p>
     * The query will only be evaluated once, even if further solutions could of been found on backtracking.
     *
     * @throws PrologException if no solution can be found
     * @see #createStatement()
     * @see #executeQuery()
     */
    public void ExecuteOnce() 
        => CreateStatement().ExecuteOnce();

    public string FindFirstAsAtomName() 
        => CreateStatement().FindFirstAsAtomName();

    public double FindFirstAsDouble() 
        => CreateStatement().FindFirstAsDouble();

    public long FindFirstAsLong() 
        => CreateStatement().FindFirstAsLong();

    public Term FindFirstAsTerm() 
        => CreateStatement().FindFirstAsTerm();

    public Optional<string> FindFirstAsOptionalAtomName() 
        => CreateStatement().FindFirstAsOptionalAtomName();

    public Optional<double> FindFirstAsOptionalDouble()
        => CreateStatement().FindFirstAsOptionalDouble();

    public Optional<long> FindFirstAsOptionalLong() 
        => CreateStatement().FindFirstAsOptionalLong();

    public Optional<Term> FindFirstAsOptionalTerm() 
        => CreateStatement().FindFirstAsOptionalTerm();

    public List<string> FindAllAsAtomName() 
        => CreateStatement().FindAllAsAtomName();

    public List<double> FindAllAsDouble() 
        => CreateStatement().FindAllAsDouble();

    public List<long> FindAllAsLong() 
        => CreateStatement().FindAllAsLong();

    public List<Term> FindAllAsTerm() 
        => CreateStatement().FindAllAsTerm();
}
