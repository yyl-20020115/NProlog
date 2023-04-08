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
using Org.NProlog.Core.Event;
using Org.NProlog.Core.Exceptions;
using Org.NProlog.Core.Kb;
using Org.NProlog.Core.Math;
using Org.NProlog.Core.Parser;
using Org.NProlog.Core.Predicate;
using Org.NProlog.Core.Predicate.Udp;
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Api;
/**
 * Provides an entry point for other Java code to interact with Prolog.
 * <p>
 * Contains a single instance of {@link org.prolog.core.kb.KnowledgeBase}.
 * </p>
 * <img src="doc-files/Prolog.png">
 */
public class Prolog
{
    private readonly KnowledgeBase knowledgeBase;

    /**
     * Constructs a new {@code Prolog} object using {@link PrologDefaultProperties} and the specified
     * {@code PrologListener}s.
     */
    public Prolog(params PrologListener[] listeners) :
         this(new PrologDefaultProperties(), listeners) { }

    /**
     * Constructs a new {@code Prolog} object with the specified {@code PrologProperties} and {@code PrologListener}s.
     */
    public Prolog(PrologProperties prologProperties, params PrologListener[] listeners)
    {
        this.knowledgeBase = KnowledgeBaseUtils.CreateKnowledgeBase(prologProperties);
        foreach (var listener in listeners) AddListener(listener);
        KnowledgeBaseUtils.Bootstrap(knowledgeBase);
    }

    /**
     * Populates this objects {@code KnowledgeBase} with clauses read from the specified file.
     *
     * @param prologScript source of the prolog syntax defining the clauses to add
     * @throws PrologException if there is any problem parsing the syntax or adding the new clauses
     */
    public void ConsultFile(string prologScript) 
        => PrologSourceReader.ParseFile(knowledgeBase, prologScript);

    /**
     * Populates this objects {@code KnowledgeBase} with clauses read from the specified {@code TextReader}.
     *
     * @param reader source of the prolog syntax defining the clauses to add
     * @throws PrologException if there is any problem parsing the syntax or adding the new clauses
     */
    public void ConsultReader(TextReader reader) 
        => PrologSourceReader.ParseReader(knowledgeBase, reader);

    /**
     * Populates this objects {@code KnowledgeBase} with clauses read from the specified resource.
     * <p>
     * If {@code prologSourceResourceName} refers to an existing file on the file system then that file is used as the
     * source of the prolog syntax else {@code prologSourceResourceName} is read from the classpath.
     *
     * @param resourceName source of the prolog syntax defining clauses to add to the KnowledgeBase
     * @throws PrologException if there is any problem parsing the syntax or adding the new clauses to the KnowledgeBase
     */
    public void ConsultResource(string resourceName)
        => PrologSourceReader.ParseResource(knowledgeBase, resourceName);

    /**
     * Reassigns the "standard" input stream.
     * <p>
     * By default the "standard" input stream will be {@code System.in}.
     */
    public void SetUserInput(TextReader reader)
        => knowledgeBase.FileHandles.SetUserInput(reader);

    /**
     * Reassigns the "standard" output stream.
     * <p>
     * By default the "standard" output stream will be {@code System._out}.
     */
    public void SetUserOutput(TextWriter writer) 
        => knowledgeBase.FileHandles.SetUserOutput(writer);

    /**
     * Associates a {@link PredicateFactory} with the {@code KnowledgeBase} of this {@code Prolog}.
     * <p>
     * This method provides a mechanism for "plugging in" or "injecting" implementations of {@link PredicateFactory} at
     * runtime. This mechanism provides an easy way to configure and extend the functionality of Prolog - including
     * adding functionality not possible to define in pure Prolog syntax.
     * </p>
     *
     * @param key The name and arity to associate the {@link PredicateFactory} with.
     * @param predicateFactory The {@link PredicateFactory} to be added.
     * @throws PrologException if there is already a {@link PredicateFactory} associated with the {@code PredicateKey}
     */
    public void AddPredicateFactory(PredicateKey key, PredicateFactory predicateFactory)
        => knowledgeBase.Predicates.AddPredicateFactory(key, predicateFactory);

    /**
     * Associates a {@link ArithmeticOperator} with this {@code KnowledgeBase} of this {@code Prolog}.
     * <p>
     * This method provides a mechanism for "plugging in" or "injecting" implementations of {@link ArithmeticOperator} at
     * runtime. This mechanism provides an easy way to configure and extend the functionality of Prolog - including
     * adding functionality not possible to define in pure Prolog syntax.
     * </p>
     *
     * @param key The name and arity to associate the {@link ArithmeticOperator} with.
     * @param operator The instance of {@code ArithmeticOperator} to be associated with {@code key}.
     * @throws PrologException if there is already a {@link ArithmeticOperator} associated with the {@code PredicateKey}
     */
    public void AddArithmeticOperator(PredicateKey key, ArithmeticOperator @operator) 
        => knowledgeBase.ArithmeticOperators.AddArithmeticOperator(key, @operator);

    /**
     * Creates a {@link QueryPlan} for querying the Prolog environment.
     * <p>
     * The newly created object represents the query parsed from the specified syntax. A single {@link QueryPlan} can be
     * used to create multiple {@link QueryStatement} objects.
     *
     * @param prologQuery prolog syntax representing a query
     * @return representation of the query parsed from the specified syntax
     * @throws PrologException if an error occurs parsing {@code prologQuery}
     * @see #createStatement(string)
     * @see #executeQuery(string)
     * @see #executeOnce(string)
     */
    public QueryPlan CreatePlan(string prologQuery)
        => new(knowledgeBase, prologQuery);

    /**
     * Creates a {@link QueryStatement} for querying the Prolog environment.
     * <p>
     * The newly created object represents the query parsed from the specified syntax. Before the query is executed,
     * values can be assigned to variables in the query by using {@link QueryStatement#setTerm(string, Term)}. The query
     * can be executed by calling {@link QueryStatement#executeQuery()}. The resulting {@link QueryResult} can be used to
     * access the result.
     * <p>
     * Note: If you do not intend to assign terms to variables then {@link #executeQuery(string)} can be called instead.
     * </p>
     *
     * @param prologQuery prolog syntax representing a query
     * @return representation of the query parsed from the specified syntax
     * @throws PrologException if an error occurs parsing {@code prologQuery}
     * @see #createPlan(string)
     * @see #executeQuery(string)
     * @see #executeOnce(string)
     */
    public QueryStatement CreateStatement(string prologQuery)
        => new(knowledgeBase, prologQuery);

    /**
     * Creates a {@link QueryResult} for querying the Prolog environment.
     * <p>
     * The newly created object represents the query parsed from the specified syntax. The {@link QueryResult#next()} and
     * {@link QueryResult#getTerm(string)} methods can be used to evaluate the query and access values unified to the
     * variables of the query.
     * </p>
     *
     * @param prologQuery prolog syntax representing a query
     * @return representation of the query parsed from the specified syntax
     * @throws PrologException if an error occurs parsing {@code prologQuery}
     * @see #createPlan(string)
     * @see #createStatement(string)
     * @see #executeOnce(string)
     */
    public QueryResult ExecuteQuery(string prologQuery) 
        => CreateStatement(prologQuery).ExecuteQuery();

    /**
     * Evaluate once the given query.
     * <p>
     * The query will only be evaluated once, even if further solutions could of been found on backtracking.
     *
     * @param prologQuery prolog syntax representing a query
     * @throws PrologException if an error occurs parsing {@code prologQuery} or no solution can be found for it
     * @see #createPlan(string)
     * @see #createStatement(string)
     * @see #executeQuery(string)
     */
    public void ExecuteOnce(string prologQuery)
        => CreateStatement(prologQuery).ExecuteOnce();

    /**
     * Registers an {@code PrologListener} to receive notifications of events generated during the evaluation of Prolog
     * goals.
     *
     * @param listener an listener to be added
     */
    public void AddListener(PrologListener listener)
        => knowledgeBase.PrologListeners.AddListener(listener);

    /**
     * Returns a string representation of the specified {@code Term}.
     *
     * @param t the {@code Term} to represent as a string
     * @return a string representation of the specified {@code Term}
     * @see org.prolog.core.term.TermFormatter#formatTerm(Term)
     */
    public string FormatTerm(Term term) 
        => knowledgeBase.TermFormatter.FormatTerm(term);

    /**
     * Returns the {@link KnowledgeBase} associated with this object.
     * <p>
     * Each {@code Prolog} object is associated with its own {@link KnowledgeBase}. In normal usage it should not be
     * necessary to call this method - as the other methods of {@code Prolog} provide a more convenient mechanism for
     * updating and querying the "core" inference engine.
     *
     * @return the {@link KnowledgeBase} associated with this object.
     * @see org.prolog.core.kb.KnowledgeBaseUtils
     */
    public KnowledgeBase KnowledgeBase => knowledgeBase;

    /**
     * Writes the all clauses contained in the specified throwable's stack trace to the standard error stream.
     */
    public void PrintPrologStackTrace(Exception exception) 
        => PrintPrologStackTrace(exception, Console.Error);

    /**
     * Writes the all clauses contained in the specified throwable's stack trace to the specified Write stream.
     */
    public void PrintPrologStackTrace(Exception exception, TextWriter writer)
    {
        var stackTrace = GetStackTrace(exception);
        foreach (var e in stackTrace)
            writer.WriteLine($"{e.PredicateKey} clause: {FormatTerm(e.Term)}");
    }

    /**
     * Provides programmatic access to the stack trace information Writeed by {@link #WritePrologStackTrace(Exception)}.
     */
    public static PrologStackTraceElement[] GetStackTrace(Exception exception)
    {
        List<PrologStackTraceElement> result = new();
        var clauses = GetClauses(exception);
        foreach (var clause in clauses)
            result.Add(new (clause.PredicateKey, clause.Original));
        return result.ToArray();
    }

    private static List<ClauseModel> GetClauses(Exception e) 
        => e is PrologException pe ? pe.GetClauses() : new();
}
