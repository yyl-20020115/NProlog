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
using static Org.NProlog.Core.Event.SpyPoints;

namespace Org.NProlog.Core.Predicate.Udp;



/**
 * A implementation of {@link TailRecursivePredicate} for interpreted user defined predicates.
 * <p>
 * The user defined predicate must be judged as eligible for <i>tail recursion optimisation</i> using the criteria used
 * by {@link TailRecursivePredicateMetaData}.
 *
 * @see InterpretedTailRecursivePredicateFactory
 * @see TailRecursivePredicateMetaData
 */
public class InterpretedTailRecursivePredicate : TailRecursivePredicate
{
    // TODO add exception handling ProjogException and CutException
    private readonly bool isSpyPointEnabled;
    private readonly SpyPoint spyPoint;
    private readonly int numArgs;
    private readonly Term[] currentQueryArgs;
    private readonly bool isRetryable;
    private readonly PredicateFactory[] firstClausePredicateFactories;
    private readonly Term[] firstClauseConsequentArgs;
    private readonly Term[] firstClauseOriginalTerms;
    private readonly PredicateFactory[] secondClausePredicateFactories;
    private readonly Term[] secondClauseConsequentArgs;
    private readonly Term[] secondClauseOriginalTerms;

    public InterpretedTailRecursivePredicate(SpyPoint spyPoint, Term[] inputArgs, PredicateFactory[] firstClausePredicateFactories, Term[] firstClauseConsequentArgs,
                Term[] firstClauseOriginalTerms, PredicateFactory[] secondClausePredicateFactories, Term[] secondClauseConsequentArgs, Term[] secondClauseOriginalTerms,
                bool isRetryable)
    {
        this.isSpyPointEnabled = spyPoint.IsEnabled;
        this.spyPoint = spyPoint;
        this.numArgs = inputArgs.Length;
        this.currentQueryArgs = new Term[numArgs];
        for (int i = 0; i < numArgs; i++)
        {
            currentQueryArgs[i] = inputArgs[i].Term;
        }

        this.firstClausePredicateFactories = firstClausePredicateFactories;
        this.firstClauseConsequentArgs = firstClauseConsequentArgs;
        this.firstClauseOriginalTerms = firstClauseOriginalTerms;
        this.secondClausePredicateFactories = secondClausePredicateFactories;
        this.secondClauseConsequentArgs = secondClauseConsequentArgs;
        this.secondClauseOriginalTerms = secondClauseOriginalTerms;
        this.isRetryable = isRetryable;
    }


    protected override bool MatchFirstRule()
    {
        Dictionary<Variable, Variable> sharedVariables = new();
        var newConsequentArgs = new Term[numArgs];
        for (int i = 0; i < numArgs; i++)
        {
            newConsequentArgs[i] = firstClauseConsequentArgs[i].Copy(sharedVariables);
        }

        if (Unify(currentQueryArgs, newConsequentArgs) == false)
        {
            return false;
        }

        for (int i = 0; i < firstClauseOriginalTerms.Length; i++)
        {
            var t = firstClauseOriginalTerms[i].Copy(sharedVariables);
            if (!firstClausePredicateFactories[i].GetPredicate(t.Args).Evaluate())
            {
                return false;
            }
        }

        return true;
    }


    protected override bool MatchSecondRule()
    {
        Dictionary<Variable, Variable> sharedVariables = new();
        var newConsequentArgs = new Term[numArgs];
        for (int i = 0; i < numArgs; i++)
        {
            newConsequentArgs[i] = secondClauseConsequentArgs[i].Copy(sharedVariables);
        }

        if (Unify(currentQueryArgs, newConsequentArgs) == false)
        {
            return false;
        }

        for (int i = 0; i < secondClauseOriginalTerms.Length - 1; i++)
        {
            var t = secondClauseOriginalTerms[i].Copy(sharedVariables);
            if (!secondClausePredicateFactories[i].GetPredicate(t.Args).Evaluate())
            {
                return false;
            }
        }

        var finalTermArgs = secondClauseOriginalTerms[secondClauseOriginalTerms.Length - 1].Args;
        for (int i = 0; i < numArgs; i++)
        {
            currentQueryArgs[i] = finalTermArgs[i].Copy(sharedVariables);
        }

        return true;
    }

    /**
     * Unifies the arguments in the head (consequent) of a clause with a query.
     * <p>
     * When Prolog attempts to answer a query it searches its knowledge base for all rules with the same functor and
     * arity. For each rule founds it attempts to unify the arguments in the query with the arguments in the head
     * (consequent) of the rule. Only if the query and rule's head can be unified can it attempt to evaluate the body
     * (antecedent) of the rule to determine if the rule is true.
     *
     * @param inputArgs the arguments contained in the query
     * @param consequentArgs the arguments contained in the head (consequent) of the clause
     * @return {@code true} if the attempt to unify the arguments was successful
     * @see Term#unify(Term)
     */
    public static bool Unify(Term[] inputArgs, Term[] consequentArgs)
    {
        for (int i = 0; i < inputArgs.Length; i++)
        {
            if (!inputArgs[i].Unify(consequentArgs[i]))
            {
                return false;
            }
        }
        return true;
    }


    protected override void LogCall()
    {
        if (isSpyPointEnabled)
        {
            spyPoint.LogCall(this, currentQueryArgs);
        }
    }


    protected override void LogRedo()
    {
        if (isSpyPointEnabled)
        {
            spyPoint.LogCall(this, currentQueryArgs);
        }
    }


    protected override void LogExit()
    {
        if (isSpyPointEnabled)
        {
            spyPoint.LogExit(this, currentQueryArgs, 1);
        }
    }


    protected override void LogFail()
    {
        if (isSpyPointEnabled)
        {
            spyPoint.LogFail(this, currentQueryArgs);
        }
    }


    protected override void Backtrack()
    {
        for (int i = 0; i < numArgs; i++)
        {
            currentQueryArgs[i].Backtrack();
        }
    }

    public override bool CouldReevaluationSucceed => this.isRetryable;
}
