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
using Org.NProlog.Core.Terms;
using Org.NProlog.Core.Kb;
using Org.NProlog.Core.Exceptions;
using Org.NProlog.Core.Predicate.Builtin.Db;
using Org.NProlog.Core.Parser;
using static Org.NProlog.Core.Predicate.Udp.DynamicUserDefinedPredicateFactory;
using System.Collections;

namespace Org.NProlog.Core.Predicate.Udp;



/**
 * Maintains a record of the clauses that represents a "static" user defined predicate.
 * <p>
 * A "static" user defined predicate is one that can not have clauses added or removed after it is first defined.
 */
public class StaticUserDefinedPredicateFactory : UserDefinedPredicateFactory, PreprocessablePredicateFactory
{
    private readonly object _lock = new object();
    private readonly PredicateKey predicateKey;
    private readonly KnowledgeBase kb;
    private readonly SpyPoints.SpyPoint spyPoint;
    private readonly List<ClauseModel> implications;
    private PredicateFactory compiledPredicateFactory;
    private int setCompiledPredicateFactoryInvocationCtr;

    public StaticUserDefinedPredicateFactory(KnowledgeBase kb, PredicateKey predicateKey)
    {
        this.predicateKey = predicateKey;
        this.kb = kb;
        this.spyPoint = kb.SpyPoints.GetSpyPoint(predicateKey);
        this.implications = new();
    }

    /**
     * Not supported.
     * <p>
     * It is not possible to add a clause to the beginning of a <i>static</i> user defined predicate.
     *
     * @throws ProjogException
     */

    public void AddFirst(ClauseModel clauseModel)
        => throw new PrologException("Cannot add clause to already defined user defined predicate as it is not dynamic: " + predicateKey + " clause: " + clauseModel.Original);

    /**
     * Adds new clause to list of clauses for this predicate.
     * <p>
     * Note: it is not possible to add clauses to a <i>static</i> user defined predicate once it has been compiled.
     *
     * @throws InvalidOperationException if the predicate has already been compiled.
     */

    public void AddLast(ClauseModel clauseModel)
    {
        if (compiledPredicateFactory == null)
        {
            implications.Add(clauseModel);
        }
        else
        {
            throw new PrologException("Cannot add clause to already defined user defined predicate as it is not dynamic: " + predicateKey + " clause: " + clauseModel.Original);
        }
    }

    public void Compile()
    {
        // make sure we only call setCompiledPredicateFactory once per instance
        if (compiledPredicateFactory == null)
        {
            lock (_lock)
            {
                if (compiledPredicateFactory == null)
                {
                    SetCompiledPredicateFactory();
                }
            }
        }
    }

    private void SetCompiledPredicateFactory()
    {
        setCompiledPredicateFactoryInvocationCtr++;
        // TODO always create Clauses here - can we move creation until InterpretedUserDefinedPredicatePredicateFactory
        var clauses = Clauses.CreateFromModels(kb, implications);
        var clauseModels = GetCopyOfImplications(); // TODO do we need to copy here?
        compiledPredicateFactory = CreateInterpretedPredicateFactoryFromClauseActions(clauses, clauseModels);
    }

    private List<ClauseModel> GetCopyOfImplications()
    {
        List<ClauseModel> copyImplications = new(implications.Count);
        foreach (var clauseModel in implications)
        {
            copyImplications.Add(clauseModel.Copy());
        }
        return copyImplications;
    }

    /**
     * Return {@code true} if this predicate calls a predicate that in turns calls this predicate.
     * <p>
     * For example, in the following script both {@code a} and {@code b} are cyclic, but {@code c} is not. <pre>
     * a(Z) :- b(Z).
     *
     * b(Z) :- a(Z).
     *
     * c(Z) :- b(Z).
     * </pre>
     */
    private bool IsCyclic() => setCompiledPredicateFactoryInvocationCtr > 1;

    private PredicateFactory CreateInterpretedPredicateFactoryFromClauseActions(Clauses clauses, List<ClauseModel> clauseModels)
    {
        var tailRecursiveMetaData = TailRecursivePredicateMetaData.Create(kb, clauseModels);
        return tailRecursiveMetaData != null
            ? new InterpretedTailRecursivePredicateFactory(kb, tailRecursiveMetaData)
            : CreateInterpretedPredicateFactoryFromClauses(clauses);
    }

    private PredicateFactory CreateInterpretedPredicateFactoryFromClauses(Clauses clauses)
    {
        if (clauses.ClauseActions.Length == 1)
        {
            return CreateSingleClausePredicateFactory(clauses.ClauseActions[0]);
        }
        else if (clauses.ClauseActions.Length == 0)
        {
            return new NeverSucceedsPredicateFactory(spyPoint);
        }
        else if (clauses.ImmutableColumns.Length == 0)
        {
            return new NotIndexablePredicateFactory(this,clauses);
        }
        else if (clauses.ImmutableColumns.Length == 1)
        {
            var index = new Indexes(clauses).GetOrCreateIndex(1);
            var actions = clauses.ClauseActions;
            if (index.KeyCount == actions.Length)
            {
                return new LinkedHashMapPredicateFactory(clauses,this);
            }
            else
            {
                return new SingleIndexPredicateFactory(clauses,this);
            }
        }
        else
        {
            return new IndexablePredicateFactory(clauses,this);
        }
    }

    private PredicateFactory CreateSingleClausePredicateFactory(ClauseAction clause)
        => clause.IsRetryable && !clause.IsAlwaysCutOnBacktrack
            ? new SingleRetryableRulePredicateFactory(clause, spyPoint)
            : new SingleNonRetryableRulePredicateFactory(clause, spyPoint);

    private Predicate CreatePredicate(Term[] args, ClauseAction[] clauses) => clauses.Length switch
    {
        0 => PredicateUtils.CreateFailurePredicate(spyPoint, args),
        1 => PredicateUtils.CreateSingleClausePredicate(clauses[0], spyPoint, args),
        _ => new InterpretedUserDefinedPredicate(
                            new ActionIterator(clauses), spyPoint, args),
    };

    /**
     * Returns true if clauses could return more than one result.
     * <p>
     * Returns false if use of cut means once a result has been found a subsequent attempt at backtracking will
     * immediately fail. e.g.: <pre>
     * p(X) :- var(X), !.
     * p(1) :- !.
     * p(7). % OK for last rule not to contain a cut, as long as it is not retryable.
     * </pre>
     */
    private static bool IsClausesRetryable(ClauseAction[] clauses)
    {
        int lastIdx = clauses.Length - 1;
        var last = clauses[lastIdx];
        if (last.IsRetryable && !last.IsAlwaysCutOnBacktrack)
        {
            return true;
        }

        for (int i = lastIdx - 1; i > -1; i--)
        {
            if (!clauses[i].IsAlwaysCutOnBacktrack)
            {
                return true;
            }
        }

        return false;
    }


    public Predicate GetPredicate(Term[] args)
    {
        if (args.Length != predicateKey.NumArgs)
        {
            throw new PrologException("User defined predicate: " + predicateKey + " is being called with the wrong number of arguments: " + args.Length + " " + Arrays.ToString(args));
        }
        Compile();
        return compiledPredicateFactory.GetPredicate(args);
    }


    public PredicateKey PredicateKey => predicateKey;

    public PredicateFactory GetActualPredicateFactory()
    { // TODO make namespace level access
        Compile();
        return compiledPredicateFactory;
    }

    /**
     * Returns an iterator over the clauses of this user defined predicate.
     * <p>
     * The iterator returned will have the following characteristics which prevent the underlying structure of the user
     * defined predicate being altered:
     * <ul>
     * <li>Calls to {@link java.util.Iterator#next()} return a <i>new copy</i> of the {@link ClauseModel}.</li>
     * <li>Calls to {@link java.util.Iterator#Remove()} cause a {@code InvalidOperationException}</li>
     * <li>
     * </ul>
     */

    public ICheckedEnumerator<ClauseModel> GetImplications() => new ImplicationsIterator(this.implications);


    public bool IsDynamic => false;


    public ClauseModel GetClauseModel(int index)
        => index >= implications.Count ? null : implications[(index)].Copy();


    public bool IsRetryable
    {
        get
        {
            if (compiledPredicateFactory == null && !IsCyclic())
            {
                Compile();
            }

            return compiledPredicateFactory == null || compiledPredicateFactory.IsRetryable;
        }
    }

    public PredicateFactory Preprocess(Term arg)
    {
        if (compiledPredicateFactory == null && !IsCyclic())
        {
            Compile();
        }

        if (compiledPredicateFactory is PreprocessablePredicateFactory factory)
        {
            return factory.Preprocess(arg);
        }
        else if (compiledPredicateFactory != null)
        {
            return compiledPredicateFactory;
        }
        else
        {
            return this;
        }
    }

    /**
     * @see StaticUserDefinedPredicateFactory#getImplications
     */
    public class ImplicationsIterator: ICheckedEnumerator<ClauseModel>
    {
        private readonly ICheckedEnumerator<ClauseModel> iterator;
        public ImplicationsIterator(List<ClauseModel> implications)
        {
            iterator = ListCheckedEnumerator<ClauseModel>.Of(implications);
        }
        object IEnumerator.Current => this.Current;
        public ClauseModel Current => iterator.Current.Copy()
            ;

        public bool CanMoveNext => this.iterator.CanMoveNext;

        public void Dispose()
        {
            this.iterator.Dispose();
        }

        public bool MoveNext() => this.iterator.MoveNext();

        /**
         * Returns a <i>new copy</i> to avoid the original being altered.
         */
        public void Reset()
        {
            this.iterator.Reset();
        }

        public ClauseModel Remove()
        {
            throw new InvalidOperationException("unable to remove");
        }
    }

    public class LinkedHashMapPredicateFactory : PreprocessablePredicateFactory
    {
        private readonly int argIdx;
        private readonly ClauseAction[] actions;
        private readonly Dictionary<Term, ClauseAction> map;
        private readonly bool retryable;
        private readonly StaticUserDefinedPredicateFactory staticUserDefinedPredicateFactory;
        public LinkedHashMapPredicateFactory(Clauses clauses, StaticUserDefinedPredicateFactory staticUserDefinedPredicateFactory)
        {
            this.argIdx = clauses.ImmutableColumns[0];
            this.actions = clauses.ClauseActions;
            this.map = new(actions.Length);
            foreach (ClauseAction a in actions)
            {
                map.Add(a.Model.Consequent.GetArgument(argIdx), a);
            }
            this.retryable = IsClausesRetryable(actions);
            this.staticUserDefinedPredicateFactory = staticUserDefinedPredicateFactory;
        }


        public Predicate GetPredicate(Term[] args) 
            => args[argIdx].IsImmutable
                ? !map.TryGetValue(args[argIdx], out var action)
                    ? PredicateUtils.CreateFailurePredicate(staticUserDefinedPredicateFactory.spyPoint, args)
                    : PredicateUtils.CreateSingleClausePredicate(action, staticUserDefinedPredicateFactory.spyPoint, args)
                : staticUserDefinedPredicateFactory.CreatePredicate(args, actions);


        public bool IsRetryable => retryable;


        public PredicateFactory Preprocess(Term arg)
        {
            if (arg.GetArgument(argIdx).IsImmutable)
            {
                return !map.TryGetValue(arg.GetArgument(argIdx),out var action)
                    ? new NeverSucceedsPredicateFactory(staticUserDefinedPredicateFactory.spyPoint)
                    : staticUserDefinedPredicateFactory.CreateSingleClausePredicateFactory(action);
            }
            else
            {
                var result = OptimisePredicateFactory(staticUserDefinedPredicateFactory.kb, actions, arg);
                if (result.Count < actions.Length)
                {
                    var clauses = Clauses.CreateFromActions(staticUserDefinedPredicateFactory.kb, result, arg);
                    return staticUserDefinedPredicateFactory.CreateInterpretedPredicateFactoryFromClauses(clauses);
                }
                else
                {
                    return this;
                }
            }
        }
    }

    public class SingleIndexPredicateFactory : PreprocessablePredicateFactory
    {
        private readonly int argIdx;
        private readonly Index index;
        private readonly ClauseAction[] actions;
        private readonly bool retryable;
        private readonly StaticUserDefinedPredicateFactory staticUserDefinedPredicateFactory;
        public SingleIndexPredicateFactory(Clauses clauses, StaticUserDefinedPredicateFactory staticUserDefinedPredicateFactory)
        {
            this.argIdx = clauses.ImmutableColumns[0];
            this.index = new Indexes(clauses).GetOrCreateIndex(1);
            this.actions = clauses.ClauseActions;
            this.retryable = IsClausesRetryable(actions);
            this.staticUserDefinedPredicateFactory = staticUserDefinedPredicateFactory;
        }


        public Predicate GetPredicate(Term[] args)
        {
            var data = args[argIdx].IsImmutable ? index.GetMatches(args) : actions;
            return staticUserDefinedPredicateFactory.CreatePredicate(args, data);
        }


        public bool IsRetryable => retryable;


        public PredicateFactory Preprocess(Term arg)
        {
            var data = arg.GetArgument(argIdx).IsImmutable ? index.GetMatches(arg.Args) : actions;
            var result = OptimisePredicateFactory(staticUserDefinedPredicateFactory.kb, data, arg);
            if (result.Count < actions.Length)
            {
                var clauses = Clauses.CreateFromActions(staticUserDefinedPredicateFactory.kb, result, arg);
                return staticUserDefinedPredicateFactory.CreateInterpretedPredicateFactoryFromClauses(clauses);
            }
            else
            {
                return this;
            }
        }
    }

    public class IndexablePredicateFactory : PreprocessablePredicateFactory
    {
        private readonly Indexes index;
        private readonly bool retryable;

        private readonly StaticUserDefinedPredicateFactory staticUserDefinedPredicateFactory;
        public IndexablePredicateFactory(Clauses clauses, StaticUserDefinedPredicateFactory staticUserDefinedPredicateFactory)
        {
            this.index = new Indexes(clauses);
            this.retryable = IsClausesRetryable(clauses.ClauseActions);
            this.staticUserDefinedPredicateFactory = staticUserDefinedPredicateFactory;
        }


        public Predicate GetPredicate(Term[] args) 
            => staticUserDefinedPredicateFactory.CreatePredicate(args, index.Index(args));


        public bool IsRetryable => retryable;


        public PredicateFactory Preprocess(Term arg)
        {
            var data = index.Index(arg.Args);
            var result = OptimisePredicateFactory(staticUserDefinedPredicateFactory.kb, data, arg);
            if (result.Count < index.ClauseCount)
            {
                var clauses = Clauses.CreateFromActions(staticUserDefinedPredicateFactory.kb, result, arg);
                return staticUserDefinedPredicateFactory.CreateInterpretedPredicateFactoryFromClauses(clauses);
            }
            else
            {
                return this;
            }
        }
    }

    public class NotIndexablePredicateFactory : PreprocessablePredicateFactory
    {
        private readonly StaticUserDefinedPredicateFactory staticUserDefinedPredicateFactory;
        private readonly ClauseAction[] data;
        private readonly bool retryable;

        public NotIndexablePredicateFactory(StaticUserDefinedPredicateFactory staticUserDefinedPredicateFactory,Clauses clauses)
        {
            this.staticUserDefinedPredicateFactory = staticUserDefinedPredicateFactory;
            this.data = clauses.ClauseActions;
            this.retryable = IsClausesRetryable(data);
        }


        public Predicate GetPredicate(Term[] args) =>
            // TODO or do: return createPredicate(args, data);
            new InterpretedUserDefinedPredicate(
                new ActionIterator(data), staticUserDefinedPredicateFactory.spyPoint, args);


        public bool IsRetryable => retryable;


        public PredicateFactory Preprocess(Term arg)
        {
            var result = OptimisePredicateFactory(staticUserDefinedPredicateFactory.kb, data, arg);
            if (result.Count < data.Length)
            {
                var clauses = Clauses.CreateFromActions(staticUserDefinedPredicateFactory.kb, result, arg);
                return staticUserDefinedPredicateFactory.CreateInterpretedPredicateFactoryFromClauses(clauses);
            }
            else
            {
                return this;
            }
        }
    }

    private static List<ClauseAction> OptimisePredicateFactory(KnowledgeBase kb, ClauseAction[] data, Term arg)
    {
        List<ClauseAction> result = new();
        var queryArgs = TermUtils.Copy(arg.Args);
        foreach (ClauseAction action in data)
        {
            if (ClauseActionFactory.IsMatch(action, queryArgs))
            {
                result.Add(action);
            }
        }
        if (result.Count == 0)
        {
            kb.PrologListeners.NotifyWarn(arg + " will never succeed");
        }
        return result;
    }

    public class ActionIterator  : ICheckedEnumerator<ClauseAction>
    {
        private readonly ClauseAction[] clauses;
        private int pos = -1;
        private bool disposed = false;
        public ActionIterator(ClauseAction[] clauses)
        {
            this.clauses = clauses;
        }
        public ClauseAction Current 
            => this.pos < 0 
            ? throw new InvalidOperationException("Call MoveNext() first") 
            : clauses[this.pos]
            ;

        public bool CanMoveNext
            => this.pos < this.clauses.Length - 1;

        object IEnumerator.Current => this.Current;

        public void Dispose()
        {
            if (!this.disposed)
            {
                this.disposed = true;
                this.pos = -1;
            }
        }
        public bool MoveNext()
        {
            if (this.pos < this.clauses.Length - 1)
            {
                this.pos++;
                return true;
            }
            return false;
        }

        public ClauseAction Remove()
        {
            throw new InvalidOperationException("unable to remove");
        }

        public void Reset()
            => this.pos = -1;
    }
}
