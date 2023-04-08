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
using Org.NProlog.Core.Kb;
using Org.NProlog.Core.Predicate;
using Org.NProlog.Core.Predicate.Udp;
using Org.NProlog.Core.Terms;
using System.Collections.Concurrent;

namespace Org.NProlog.Core.Event;

/**
 * Collection of spy points.
 * <p>
 * Spy points are useful in the debugging of Prolog programs. When a spy point is set on a predicate a
 * {@link SpyPointEvent} is generated every time the predicate is executed, fails or succeeds.
 * </p>
 * <p>
 * Each {@link org.projog.core.kb.KnowledgeBase} has a single unique {@code SpyPoints} instance.
 * </p>
 *
 * @see KnowledgeBase#getSpyPoints()
 */
public class SpyPoints
{
    private readonly object syncRoot = new();
    private readonly ConcurrentDictionary<PredicateKey, SpyPoint> spyPoints = new(); 
    private readonly KnowledgeBase? kb = null;
    private readonly PrologListeners prologListeners;
    private readonly TermFormatter termFormatter;

    public bool TraceEnabled = false;

    public SpyPoints(KnowledgeBase kb)
    {
        this.kb = kb;
        this.prologListeners = kb.PrologListeners;
        this.termFormatter = kb.TermFormatter;
    }

    public SpyPoints(PrologListeners observable, TermFormatter termFormatter)
    { // NOTICE only used by tests - Remove
        this.kb = null;
        this.prologListeners = observable;
        this.termFormatter = termFormatter;
    }


    public void SetSpyPoint(PredicateKey key, bool set)
    {
        lock (syncRoot)
        {
            var sp = GetSpyPoint(key);
            sp.Set = set;
        }
    }

    public SpyPoint GetSpyPoint(PredicateKey key)
    {
        if (!spyPoints.TryGetValue(key, out var spyPoint))
        {
            spyPoint = CreateNewSpyPoint(key);
        }
        return spyPoint;
    }

    private SpyPoint CreateNewSpyPoint(PredicateKey key)
    {
        lock (syncRoot)
        {
            if (!spyPoints.TryGetValue(key, out var spyPoint))
                spyPoints[key]=(spyPoint = new(key, this));
            return spyPoint;
        }
    }

    public Dictionary<PredicateKey, SpyPoint> GetSpyPoints() => new(spyPoints);//Collections.unmodifiableMap(spyPoints);

    public class SpyPoint
    {
        private readonly SpyPoints spypoints;
        private readonly PredicateKey key;
        
        public SpyPoint(PredicateKey key, SpyPoints spypoints)
        {
            this.key = key;
            this.spypoints = spypoints;
        }

        public PredicateKey PredicateKey => key;

        public bool Set = false;

        public bool IsEnabled => this.Set || this.spypoints.TraceEnabled;

        /** Notifies listeners of a first attempt to evaluate a goal. */
        public void LogCall(object source, Term[] args)
        {
            if (IsEnabled)
                this.spypoints.prologListeners.NotifyCall(new(this.spypoints, key, args, source));
        }

        /** Notifies listeners of an attempt to re-evaluate a goal. */
        public void LogRedo(object source, Term[] args)
        {
            if (IsEnabled)
                this.spypoints.prologListeners.NotifyRedo(new(this.spypoints, key, args, source));
        }

        /** Notifies listeners of that an attempt to evaluate a goal has succeeded. */

        public void LogExit(object source, Term[] args, int clauseNumber)
        {
            ClauseModel? clauseModel = null;
            if (clauseNumber != -1)
            {
                var userDefinedPredicates = spypoints?.kb?.Predicates.GetUserDefinedPredicates();
                if (userDefinedPredicates!=null 
                    && userDefinedPredicates.TryGetValue(PredicateKey, out var userDefinedPredicate))
                    // clauseNumber starts at 1 / getClauseModel starts at 0
                    clauseModel = userDefinedPredicate.GetClauseModel(clauseNumber - 1);
            }

            LogExit(source, args, clauseModel);
        }

        /** Notifies listeners of that an attempt to evaluate a goal has succeeded. */
        public void LogExit(object source, Term[] args, ClauseModel? clause)
        {
            if (IsEnabled)
                this.spypoints.prologListeners.NotifyExit(new(this.spypoints, key, args, source, clause));
        }

        /** Notifies listeners of that an attempt to evaluate a goal has failed. */
        public void LogFail(object source, Term[] args)
        {
            if (IsEnabled)
                this.spypoints.prologListeners.NotifyFail(new(this.spypoints, key, args, source));
        }
    }

    public class SpyPointEvent
    {
        protected readonly SpyPoints spypoints;
        protected readonly PredicateKey key;
        protected readonly Term[] args;
        protected readonly object source;

        public SpyPointEvent(SpyPoints spypoints, PredicateKey key, Term[] args, object source)
        {
            this.spypoints = spypoints;
            this.key = key;
            this.args = TermUtils.Copy(args);
            this.source = source;
        }

        public PredicateKey PredicateKey => key;

        public string GetFormattedTerm()
            => args.Length == 0 ? key.Name : this.spypoints.termFormatter.FormatTerm(Structure.CreateStructure(key.Name, args));
        public int GetSourceId() => source.GetHashCode();
        public override string ToString() => GetFormattedTerm();
    }

    public class SpyPointExitEvent : SpyPointEvent
    {
        protected readonly ClauseModel? clauseModel;

        public SpyPointExitEvent(SpyPoints spypoints, PredicateKey key, Term[] args, object source, ClauseModel? clauseModel)
          : base(spypoints, key, args, source) => this.clauseModel = clauseModel;

        public string GetFormattedClause()
            => this.spypoints.termFormatter.FormatTerm(clauseModel?.Original);

        public ClauseModel? ClauseModel => clauseModel;
    }
}
