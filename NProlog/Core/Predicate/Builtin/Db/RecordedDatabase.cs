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
using Org.NProlog.Core.Terms;
using System.Collections;

namespace Org.NProlog.Core.Predicate.Builtin.Db;

/**
 * Provides a mechanism to associate a term with a key.
 * <p>
 * Multiple terms can be associated with the same key.
 */
public class RecordedDatabase
{
    private readonly AtomicLong refCount = new ();
    private readonly SortedDictionary<long, Link> references = new();
    private readonly List<PredicateKey> keys = new();
    private readonly SortedDictionary<PredicateKey, Chain> chains = new();

    /**
     * Associates a value with a key.
     *
     * @param key the key to associate the value with
     * @param value the value to store
     * @return reference for the newly added value
     */
    public IntegerNumber Add(PredicateKey key, Term value, bool addLast)
    {
        var chain = GetOrCreateChain(key);
        var link = CreateLink(chain, value, addLast);
        return link.reference;
    }

    public ICheckedEnumerator<Record> GetAll() 
        => new DatabaseIterator(this);

    public ICheckedEnumerator<Record> GetChain(PredicateKey key)
        => !chains.TryGetValue(key, out var chain) 
        ? new ChainIterator(null) : (ICheckedEnumerator<Record>)new ChainIterator(chain);

    /**
     * @param reference the reference of the term to Remove
     * @return {@code true} if a term was removed else {@code false} (i.e. if there was no term associated with the
     * specified {@code reference})
     */
    public bool Erase(long reference)
        => RemoveReference(reference);

    private Chain GetOrCreateChain(PredicateKey key)
    {
        if (!chains.TryGetValue(key,out var chain))
            chain = CreateChain(key);
        return chain;
    }

    private Chain CreateChain(PredicateKey key)
    {
        lock (chains)
        {
            if (!chains.TryGetValue(key, out var chain))
            {
                chains.Add(key, chain = new Chain(key));
                keys.Add(key);
            }
            return chain;
        }
    }

    private Link CreateLink(Chain chain, Term value, bool addLast)
    {
        var reference = CreateReference();
        var link = new Link(chain, reference, value);
        AddReference(reference, link, addLast);
        return link;
    }

    private IntegerNumber CreateReference() 
        => IntegerNumberCache.ValueOf(refCount.GetAndIncrement());

    private void AddReference(IntegerNumber reference, Link link, bool addLast)
    {
        lock (references)
        {
            references.Add(reference.Long, link);

            var c = link.chain;
            if (c.last == null)
            {
                c.first = c.last = link;
            }
            else if (addLast)
            {
                c.last.next = link;
                link.previous = c.last;
                c.last = link;
            }
            else
            {
                c.first.previous = link;
                link.next = c.first;
                c.first = link;
            }
        }
    }

    private bool RemoveReference(long reference)
    {
        lock (references)
        {
            if(references.TryGetValue(reference,out var link))
            {
                references.Remove(reference);

                var c = link.chain;
                var next = link.next;
                var previous = link.previous;
                if (next != null)
                {
                    next.previous = previous;
                }
                if (previous != null)
                {
                    previous.next = next;
                }
                if (link == c.last)
                {
                    c.last = previous;
                }
                if (link == c.first)
                {
                    c.first = next;
                }
                link.deleted = true;

                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public class DatabaseIterator:ICheckedEnumerator<Record>
    {
        private int keyIdx;
        private ChainIterator chainIterator;
        private readonly RecordedDatabase recordedDatabase;
        private bool disposed=false;
        public Record Current => this.disposed 
            ? throw new ObjectDisposedException(nameof(DatabaseIterator))
            : this.chainIterator == null ? throw new NoSuchElementException() : this.chainIterator.Current
            ;

        object IEnumerator.Current => this.Current;

        public bool CanMoveNext => this.TryCanMoveNext(0);
        protected bool TryCanMoveNext(int increase = 0)
        {
            while (chainIterator == null || !chainIterator.CanMoveNext)
            {
                if (HasIteratedOverAllChains)
                    return false;
                else 
                {
                    UpdateChainIterator(increase);
                    if (increase <= 0) return chainIterator.CanMoveNext;
                }
            }
            return true;
        }

        public DatabaseIterator(RecordedDatabase recordedDatabase)
        {
            this.recordedDatabase = recordedDatabase;
            this.UpdateChainIterator();
        }


        private bool HasIteratedOverAllChains => keyIdx >= recordedDatabase.keys.Count;

        private void UpdateChainIterator(int increase = 0)
        {
            keyIdx += increase;
            if (keyIdx < recordedDatabase.keys.Count)
            {
                var key = recordedDatabase.keys[keyIdx];
                if (recordedDatabase.chains.TryGetValue(key, out var chain))
                    chainIterator = new (chain);
            }
        }

        public bool MoveNext()
        {
            var can = this.TryCanMoveNext(0) && chainIterator.MoveNext();
            if (!can)
                can = this.TryCanMoveNext(1) && this.chainIterator.MoveNext();
            return can;
        } 

        public void Reset()
        {
            if (!this.disposed)
                this.chainIterator.Reset();
        }

        public void Dispose()
        {
            if (!this.disposed)
            {
                this.disposed = true;
                chainIterator.Dispose();
            }
        }

        public Record Remove() => this.chainIterator.Remove();
    }

    public class ChainIterator : ICheckedEnumerator<Record>
    {
        private readonly Link? first;
        private Link? current = null;
        private bool started = false;
        private bool disposed = false;
        public ChainIterator(Chain chain)
            => this.current = this.first = chain?.first;

        object IEnumerator.Current => this.Current;

        public Record Current => this.current==null
            ? throw new NoSuchElementException("Enumeration has not started, call MoveNext() first")
            : CreateRecord(current)
            ;

        public bool CanMoveNext
        {
            get
            {
                SkipDeleted();
                return this.current != null;
            }
        }
        public bool MoveNext()
        {
            if (!this.CanMoveNext) return false;
            return !this.started 
                ? (this.started = true) 
                : (this.current = this.current.next)!=null;
        }


        public void Dispose()
        {
            if (!this.disposed)
            {
                this.disposed = true;
                this.current = null;
            }
        }


        public void Reset()
        {
            if (!this.disposed)
                this.current = this.first;
        }

        private static Record CreateRecord(Link link)
            => new (link.chain.key, link.reference, link.value);

        private void SkipDeleted()
        {
            while (current != null && current.deleted)
                current = current.next;
        }

        public Record Remove() 
            => throw new InvalidOperationException("Unable to remove");
    }

    public class Chain
    {
        public readonly PredicateKey key;
        public Link? first;
        public Link? last;

        public Chain(PredicateKey key) => this.key = key;
    }

    public class Link
    {
        public readonly Chain chain;
        public readonly IntegerNumber reference;
        public readonly Term value;
        public Link? previous;
        public Link? next;
        public bool deleted = false;

        public Link(Chain chain, IntegerNumber reference, Term value)
        {
            this.chain = chain;
            this.reference = reference;
            this.value = value;
        }
    }
}
