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
using Org.NProlog.Core.Kb;
using Org.NProlog.Core.Predicate.Builtin.Db;
using Org.NProlog.Core.Terms;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Org.NProlog.Core.Predicate.Udp;



/**
 * Maintains a record of the clauses that represents a "dynamic" user defined predicate.
 * <p>
 * A "dynamic" user defined predicate is one that can have clauses added and removed <i>after</i> it has been first
 * defined. This is normally done using the {@code asserta/1}, {@code assertz/1} and {@code retract/1} predicates.
 *
 * @see org.projog.core.predicate.udp.InterpretedUserDefinedPredicate
 */
public class DynamicUserDefinedPredicateFactory : UserDefinedPredicateFactory
{
    // use array rather than two instances so that references not lost between
    // copies when heads or tails alter
    private static readonly int FIRST = 0;
    private static readonly int LAST = 1;

    private readonly object syncRoot = new();
    private readonly KnowledgeBase kb;
    private readonly SpyPoints.SpyPoint spyPoint;
    private readonly ClauseActionMetaData[] ends = new ClauseActionMetaData[2];
    private ConcurrentDictionary<Term, ClauseActionMetaData> index;
    private bool hasPrimaryKey;

    public DynamicUserDefinedPredicateFactory(KnowledgeBase kb, PredicateKey predicateKey)
    {
        this.kb = kb;
        if (predicateKey.NumArgs == 0)
        {
            this.hasPrimaryKey = false;
            this.index = null;
        }
        else
        {
            this.hasPrimaryKey = true;
            index = new();
        }
        this.spyPoint = kb.SpyPoints.GetSpyPoint(predicateKey);
    }


    public Predicate GetPredicate(Term[] args)
    {
        if (hasPrimaryKey)
        {
            var firstArg = args[0];
            if (firstArg.IsImmutable)
            {
                return !index.TryGetValue(firstArg,out var match)
                    ? PredicateUtils.CreateFailurePredicate(spyPoint, args)
                    : PredicateUtils.CreateSingleClausePredicate(match.clause, spyPoint, args);
            }
        }

        var itr = new ClauseActionIterator(ends[FIRST]);
        return new InterpretedUserDefinedPredicate(itr, spyPoint, args);
    }


    public PredicateKey PredicateKey => spyPoint.PredicateKey;


    public bool IsDynamic => true;

    /**
     * Returns an iterator over the clauses of this user defined predicate.
     * <p>
     * The iterator returned will have the following characteristics:
     * <ul>
     * <li>Calls to {@link java.util.Iterator#next()} return a <i>new copy</i> of the {@link ClauseModel} to avoid the
     * original being altered.</li>
     * <li>Calls to {@link java.util.Iterator#Remove()} <i>do</i> alter the underlying structure of this user defined
     * predicate.</li>
     * <li></li>
     * </ul>
     */

    public ICheckedEnumerator<ClauseModel> GetImplications() => new ImplicationsIterator(this);


    public void AddFirst(ClauseModel clauseModel)
    {
        lock (syncRoot)
        {
            var newClause = CreateClauseActionMetaData(clauseModel);
            AddToIndex(clauseModel, newClause);

            // if first used in a implication antecedent before being used as a consequent,
            // it will originally been created with first and last both null
            var first = ends[FIRST];
            if (first == null)
            {
                ends[FIRST] = newClause;
                ends[LAST] = newClause;
                return;
            }
            newClause.next = first;
            first.previous = newClause;
            ends[FIRST] = newClause;
        }
    }


    public void AddLast(ClauseModel clauseModel)
    {
        lock (syncRoot)
        {
            var newClause = CreateClauseActionMetaData(clauseModel);
            AddToIndex(clauseModel, newClause);

            // if first used in a implication antecedent before being used as a consequent,
            // it will originally been created with first and last both null
            var last = ends[LAST];
            if (last == null)
            {
                ends[FIRST] = newClause;
                ends[LAST] = newClause;
                return;
            }
            last.next = newClause;
            newClause.previous = last;
            ends[LAST] = newClause;
        }
    }

    private void AddToIndex(ClauseModel clauseModel, ClauseActionMetaData metaData)
    {
        if (hasPrimaryKey)
        {
            Term firstArg = clauseModel.Consequent.GetArgument(0);
            if (!firstArg.IsImmutable || !index.TryAdd(firstArg, metaData))
            {
                hasPrimaryKey = false;
                index.Clear();
            }
        }
    }


    public ClauseModel GetClauseModel(int index)
    {
        var next = ends[FIRST];
        for (int i = 0; i < index; i++)
        {
            if (next == null)
            {
                return null;
            }
            next = next.next;
        }
        if (next == null)
        {
            return null;
        }
        return next.clause.Model.Copy();
    }

    private ClauseActionMetaData CreateClauseActionMetaData(ClauseModel clauseModel) => new (kb, clauseModel);

    public class ClauseActionIterator: ICheckedEnumerator<ClauseAction>
    {
        private readonly ClauseActionMetaData first;

        private ClauseActionMetaData? current = null;
        private bool disposed = false;
        
        public ClauseActionIterator(ClauseActionMetaData first)
        {
            this.first = first;
        }
        public ClauseAction Current 
            => this.current == null 
            ? throw new InvalidOperationException("Call MoveNext first")
            : this.current.clause
            ;

        public bool CanMoveNext => !this.disposed;

        object IEnumerator.Current => this.Current;

        public void Dispose()
        {
            if (!this.disposed)
            {
                this.disposed = true;
                this.current = null;
            }
        }

        public bool MoveNext()
            => (this.current = (this.current == null ? this.first : this.current.next)) != null;

        public ClauseAction Remove()
        {
            //NOTICE: break link and relink
            var current = this.Current;
            if (this.current?.previous != null)
            {
                this.current.previous.next = this.current.next;
                this.current = this.current.next;
            }

            return current;
        }

        /** need to call getFree on result */

        public void Reset()
        {
            if (!this.disposed)
            {
                this.current = null;
            }
        }
    }

    public class ImplicationsIterator: ICheckedEnumerator<ClauseModel>
    {
        private readonly DynamicUserDefinedPredicateFactory factory;
        private ClauseActionMetaData? current;
        private bool disposed = false;
        private bool started = false;
        public ClauseModel Current => this.current == null
            ? throw new InvalidOperationException("Call MoveNext() first")
            : this.current.clause.Model.Copy()
            ;

        object IEnumerator.Current => this.Current;

        public ImplicationsIterator(DynamicUserDefinedPredicateFactory dynamicUserDefinedPredicateFactory)
        {
            this.factory = dynamicUserDefinedPredicateFactory;
        }

        public bool CanMoveNext => this.started ? this.current != null : this.factory.ends[FIRST]!=null;

        /**
        * Returns a <i>new copy</i> to avoid the original being altered.
        */

        public bool MoveNext()
        {
            if (!this.started)
            {
                this.current = factory.ends[FIRST];
                this.started = true;
                return current != null;
            }
            else
            {
                return (this.current = this.current?.next)!=null;
            }
        }

        public void Reset()
        {
            if (!this.disposed)
            {
                this.started = false;
                this.current = null;
            }
        }

        public void Dispose()
        {
            if (!this.disposed)
            {
                this.disposed = true;
            }
        }


        public void Remove()
        { // TODO find way to use index when retracting
            lock (factory.syncRoot)
            {
                if (factory.hasPrimaryKey)
                {
                    var firstArg = current.clause.Model.Consequent.GetArgument(0);

                    if (!factory.index.Remove(firstArg, out var r))
                    {
                        throw new InvalidOperationException();
                    }
                }
                if (current.previous != null)
                {
                    current.previous.next = current.next;
                }
                else
                {
                    var newHead = current.next;
                    if (newHead != null)
                    {
                        newHead.previous = null;
                    }
                    factory.ends[FIRST] = newHead;
                }
                if (current.next != null)
                {
                    current.next.previous = current.previous;
                }
                else
                {
                    var newTail = current.previous;
                    if (newTail != null)
                    {
                        newTail.next = null;
                    }
                    factory.ends[LAST] = newTail;
                }
                if (factory.ends[FIRST] == null && factory.ends[LAST] == null)
                {
                    factory.hasPrimaryKey = factory.index != null;
                }
            }
        }

        ClauseModel ICheckedEnumerator<ClauseModel>.Remove()
        {
            var current = this.Current;
            this.Remove();
            return current;
        }
    }

    public class ClauseActionMetaData
    {
        public readonly ClauseAction clause;
        public ClauseActionMetaData previous;
        public ClauseActionMetaData next;

        public ClauseActionMetaData(KnowledgeBase kb, ClauseModel clauseModel)
        {
            this.clause = ClauseActionFactory.CreateClauseAction(kb, clauseModel);
        }
    }


    public bool IsRetryable => true;
}
