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
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Predicate.Udp;

/**
 * Constructs new {@link ClauseAction} instances.
 */
public class ClauseActionFactory
{
    /**
     * Returns true if the arguments unify with the consequent of the clause.
     * <p>
     * TODO move to another class, e.g. ClauseAction
     */
    public static bool IsMatch(ClauseAction clause, Term[] queryArgs)
    {
        var clauseArgs = TermUtils.Copy(clause.Model.Consequent.Args);
        var match = TermUtils.Unify(queryArgs, clauseArgs);
        TermUtils.Backtrack(queryArgs);
        return match;
    }

    /**
     * Returns a new {@link ClauseAction} based on the specified {@link ClauseModel}.
     */
    public static ClauseAction CreateClauseAction(KnowledgeBase? kb, ClauseModel model)
    {
        var antecedent = model.Antecedent;
        if (antecedent.Type.IsVariable)
            return new VariableAntecedantClauseAction(model, kb);

        bool isFact = model.IsFact;

        var consequent = model.Consequent;
        if (consequent.NumberOfArguments == 0)
            // have zero arg rule
            return isFact ? new AlwaysMatchedFact(model) : new ZeroArgConsequentRule(model, kb.Predicates.GetPreprocessedPredicateFactory(antecedent));

        // if all non-shared variables then always true
        // if all concrete terms (no variables) then reusable
        bool hasVariables = false;
        bool hasConcreteTerms = false;
        bool hasSharedVariables = false;
        HashSet<Term> variables = new();
        foreach (var term in consequent.Args)
        {
            if (term.Type == TermType.VARIABLE)
            {
                hasVariables = true;
                if (!variables.Add(term))
                {
                    hasSharedVariables = true;
                }
            }
            else
            {
                hasConcreteTerms = true;
                if (term.IsImmutable == false)
                {
                    hasVariables = true;
                }
            }
        }

        var preprocessedPredicateFactory = kb.Predicates.GetPreprocessedPredicateFactory(antecedent);
        return !hasSharedVariables && !hasConcreteTerms
            ? isFact ? new AlwaysMatchedFact(model) : new MutableRule(model, preprocessedPredicateFactory)
            : hasConcreteTerms && !hasVariables
                ? isFact ? new ImmutableFact(model) : new ImmutableConsequentRule(model, preprocessedPredicateFactory)
                : isFact ? new MutableFact(model) : new MutableRule(model, preprocessedPredicateFactory);
    }

    /**
     * Clause where the antecedent is a variable.
     * <p>
     * When the antecedent is a variable then the associated predicate factory can only be determined at runtime.
     * </p>
     * <p>
     * e.g. "p(X) :- X."
     */
    public class VariableAntecedantClauseAction : ClauseAction
    {
        private readonly ClauseModel model;
        private readonly KnowledgeBase? kb;

        public VariableAntecedantClauseAction(ClauseModel model, KnowledgeBase? kb)
        {
            this.model = model;
            this.kb = kb;
        }


        public Predicate GetPredicate(Term[] input)
        {
            var consequentArgs = model.Consequent.Args;
            Dictionary<Variable, Variable> sharedVariables = new();
            for (int i = 0; i < input.Length; i++)
            {
                if (!input[i].Unify(consequentArgs[i].Copy(sharedVariables)))
                {
                    return PredicateUtils.FALSE;
                }
            }

            var antecedant = model.Antecedent.Copy(sharedVariables);
            return kb.Predicates.GetPredicateFactory(antecedant).GetPredicate(antecedant.Args);
        }


        public ClauseModel Model => model;


        public bool IsRetryable => true;


        public bool IsAlwaysCutOnBacktrack => false;
    }

    /**
     * Clause where all consequent args are distinctly different variables and antecedent is true.
     * <p>
     * e.g. "p." or "p(X,Y,Z)."
     */
    public class AlwaysMatchedFact : ClauseAction
    {
        private readonly ClauseModel model;

        public AlwaysMatchedFact(ClauseModel model) => this.model = model;

        public Predicate GetPredicate(Term[] input) => PredicateUtils.TRUE;

        public ClauseModel Model => model;

        public bool IsRetryable => false;

        public bool IsAlwaysCutOnBacktrack => false;
    }

    /**
     * Clause where the consequent has no args.
     * <p>
     * e.g. "p :- test." or "p :- test(_)."
     */
    public class ZeroArgConsequentRule : ClauseAction
    {
        private readonly ClauseModel model;
        private readonly PredicateFactory factory;

        public ZeroArgConsequentRule(ClauseModel model, PredicateFactory factory)
        {
            this.model = model;
            this.factory = factory;
        }


        public Predicate GetPredicate(Term[] input)
        {
            var antecedent = model.Antecedent;
            return antecedent.IsImmutable ? factory.GetPredicate(antecedent.Args) : factory.GetPredicate(TermUtils.Copy(antecedent.Args));
        }

        public ClauseModel Model => model;

        public bool IsRetryable => factory.IsRetryable;

        public bool IsAlwaysCutOnBacktrack => factory.IsAlwaysCutOnBacktrack;
    }

    /**
     * Clause where the consequent args are all immutable and the antecedent is true.
     * <p>
     * e.g. "p(a,b,c)."
     */
    public class ImmutableFact : ClauseAction
    {
        private readonly ClauseModel model;

        public ImmutableFact(ClauseModel model) => this.model = model;

        public Predicate GetPredicate(Term[] input)
        {
            var consequentArgs = model.Consequent.Args;
            for (int i = 0; i < input.Length; i++)
                if (!input[i].Unify(consequentArgs[i]))
                    return PredicateUtils.FALSE;

            return PredicateUtils.TRUE;
        }

        public ClauseModel Model => model;

        public bool IsRetryable => false;

        public bool IsAlwaysCutOnBacktrack => false;
    }

    /**
     * Clause where the consequent args are all immutable and the antecedent is not true.
     * <p>
     * e.g. "p(a,b,c) :- test." or "p(a,b,c) :- test(_)."
     */
    public class ImmutableConsequentRule : ClauseAction
    {
        private readonly ClauseModel model;
        private readonly PredicateFactory factory;

        public ImmutableConsequentRule(ClauseModel model, PredicateFactory factory)
        {
            this.model = model;
            this.factory = factory;
        }


        public Predicate GetPredicate(Term[] input)
        {
            var consequentArgs = model.Consequent.Args;
            for (int i = 0; i < input.Length; i++)
                if (!input[i].Unify(consequentArgs[i]))
                    return PredicateUtils.FALSE;

            var antecedent = model.Antecedent;
            return antecedent.IsImmutable ? factory.GetPredicate(antecedent.Args) : factory.GetPredicate(TermUtils.Copy(antecedent.Args));
        }


        public ClauseModel Model => model;

        public bool IsRetryable => factory.IsRetryable;

        public bool IsAlwaysCutOnBacktrack => factory.IsAlwaysCutOnBacktrack;
    }

    /**
     * Clause where at least one consequent arg is mutable and the antecedent is true.
     * <p>
     * e.g. "p(a,_,c)." or "p(X,X)."
     */
    public class MutableFact : ClauseAction
    {
        private readonly ClauseModel model;

        public MutableFact(ClauseModel model) => this.model = model;


        public Predicate GetPredicate(Term[] input)
        {
            // TODO would be a performance improvement if no clause variable is created unless is a shared variable
            var consequentArgs = model.Consequent.Args;
            Dictionary<Variable, Variable> sharedVariables = new();
            for (int i = 0; i < input.Length; i++)
                if (!input[i].Unify(consequentArgs[i].Copy(sharedVariables)))
                    return PredicateUtils.FALSE;

            return PredicateUtils.TRUE;
        }


        public ClauseModel Model => model;

        public bool IsRetryable => false;

        public bool IsAlwaysCutOnBacktrack => false;
    }

    /**
     * Clause where at least one consequent arg is mutable and the antecedent is not true.
     * <p>
     * e.g. "p(a,_,c) :- test." or ""p(X,X) :- test."
     */
    public class MutableRule : ClauseAction
    {
        private readonly ClauseModel model;
        private readonly PredicateFactory factory;

        public MutableRule(ClauseModel model, PredicateFactory factory)
        {
            this.model = model;
            this.factory = factory;
        }


        public Predicate GetPredicate(Term[] input)
        {
            var consequentArgs = model.Consequent.Args;
            Dictionary<Variable, Variable> sharedVariables = new();
            for (int i = 0; i < input.Length; i++)
                if (!input[i].Unify(consequentArgs[i].Copy(sharedVariables)))
                    return PredicateUtils.FALSE;

            var antecedent = model.Antecedent;
            if (antecedent.IsImmutable)
                return factory.GetPredicate(antecedent.Args);
            else
            {
                var originalAntecedentArgs = antecedent.Args;
                var copyAntecedentArgs = new Term[originalAntecedentArgs.Length];
                for (int i = 0; i < originalAntecedentArgs.Length; i++)
                    copyAntecedentArgs[i] = originalAntecedentArgs[i].Copy(sharedVariables);
                return factory.GetPredicate(copyAntecedentArgs);
            }
        }

        public ClauseModel Model => model;

        public bool IsRetryable => factory.IsRetryable;

        public bool IsAlwaysCutOnBacktrack => factory.IsAlwaysCutOnBacktrack;
    }

    // TODO add variation for where antecedent is conjuction of non-retryable predicates
}
