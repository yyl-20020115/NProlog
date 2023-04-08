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
using Org.NProlog.Core.Terms;
using static Org.NProlog.Core.Event.SpyPoints;

namespace Org.NProlog.Core.Predicate.Udp;

/**
 * Creates "tail recursion optimised" versions of user defined predicates.
 * <p>
 * Each instance of {@code InterpretedTailRecursivePredicateFactory} creates new instances of
 * {@link InterpretedTailRecursivePredicate} for a specific user defined predicate. The user defined predicate must be
 * judged as eligible for <i>tail recursion optimisation</i> using the criteria used by
 * {@link TailRecursivePredicateMetaData}.
 * </p>
 *
 * @see TailRecursivePredicate
 * @see TailRecursivePredicateMetaData
 */
public class InterpretedTailRecursivePredicateFactory : PredicateFactory
{
    private readonly SpyPoint spyPoint;
    private readonly TailRecursivePredicateMetaData metaData;
    private readonly PredicateFactory[] firstClausePredicateFactories;
    private readonly Term[] firstClauseConsequentArgs;
    private readonly Term[] firstClauseOriginalTerms;
    private readonly PredicateFactory[] secondClausePredicateFactories;
    private readonly Term[] secondClauseConsequentArgs;
    private readonly Term[] secondClauseOriginalTerms;

    public InterpretedTailRecursivePredicateFactory(KnowledgeBase kb, TailRecursivePredicateMetaData metaData)
    {
        this.spyPoint = GetSpyPoint(kb, metaData);
        this.metaData = metaData;
        var firstClause = metaData.FirstClause;
        var secondClause = metaData.SecondClause;

        this.firstClauseConsequentArgs = firstClause.Consequent.Args;
        this.secondClauseConsequentArgs = secondClause.Consequent.Args;

        this.firstClauseOriginalTerms = KnowledgeBaseUtils.ToArrayOfConjunctions(firstClause.Antecedent);
        this.secondClauseOriginalTerms = KnowledgeBaseUtils.ToArrayOfConjunctions(secondClause.Antecedent);

        this.firstClausePredicateFactories = new PredicateFactory[firstClauseOriginalTerms.Length];
        for (int i = 0; i < firstClauseOriginalTerms.Length; i++)
        {
            firstClausePredicateFactories[i] = kb.Predicates.GetPredicateFactory(firstClauseOriginalTerms[i]);
        }

        this.secondClausePredicateFactories = new PredicateFactory[secondClauseOriginalTerms.Length - 1];
        for (int i = 0; i < secondClausePredicateFactories.Length; i++)
        {
            secondClausePredicateFactories[i] = kb.Predicates.GetPredicateFactory(secondClauseOriginalTerms[i]);
        }
    }


    public InterpretedTailRecursivePredicate GetPredicate(Term[] args) => new InterpretedTailRecursivePredicate(spyPoint, args, firstClausePredicateFactories, firstClauseConsequentArgs, firstClauseOriginalTerms,
                    secondClausePredicateFactories, secondClauseConsequentArgs, secondClauseOriginalTerms, IsRetryableWith(args));

    private bool IsRetryableWith(Term[] args)
    {
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].IsImmutable && metaData.isSingleResultIfArgumentImmutable[(i)])
                return false;
        }
        return true;
    }

    private static SpyPoints.SpyPoint GetSpyPoint(KnowledgeBase kb, TailRecursivePredicateMetaData metaData)
    {
        var key = PredicateKey.CreateForTerm(metaData.FirstClause.Consequent);
        return kb.SpyPoints.GetSpyPoint(key);
    }

    Predicate? PredicateFactory.GetPredicate(Term[]? args) => this.GetPredicate(args);

    public bool IsRetryable => true; // TODO
}
