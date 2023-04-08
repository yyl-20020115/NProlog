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
 * Defines the characteristics of a tail recursive user defined predicate.
 * <p>
 * Projog uses the following rules to determine if a user defined predicate is "tail recursive" (and therefore suitable
 * for <i>tail recursion optimisation</i> using a {@link TailRecursivePredicate}):
 * </p>
 * <ul>
 * <li>The user defined predicate must consist of exactly 2 rules.</li>
 * <li>It must be possible to detect, at the point that the user defined predicate is defined, that the antecedent of
 * the first rule will never generate multiple solutions per-query.</li>
 * <li>If the antecedent of the second rule is not a conjunction, it must be a call to itself (i.e. the user defined
 * predicate being defined) - this is what makes the predicate recursive.</li>
 * <li>If the antecedent of the second rule is a conjunction, the readonly element (i.e. the tail) of the conjunction must
 * be a call to itself (i.e. the user defined predicate being defined) - this is what makes the predicate recursive. It
 * must be possible to detect, at the point that the user defined predicate is defined, that all elements prior to the
 * readonly element of the conjunction will never generate multiple solutions per-query.</li>
 * </ul>
 * <p>
 * Examples of tail recursive predicates suitable for <i>tail recursion optimisation</i>:
 * </p>
 * <pre>
 * :- list([]).
 * list([X|Xs]) :- list(Xs).
 * </pre> <pre>
 * r(N).
 * r(N) :- N &gt; 1, N1 is N-1, r(N1).
 * </pre> <pre>
 * writeAndRepeat(N) :- write(N), nl.
 * writeAndRepeat(N) :- N &gt; 1, N1 is N-1, writeAndRepeat(N1).
 * </pre>
 *
 * @see TailRecursivePredicate
 */
public class TailRecursivePredicateMetaData
{
    public readonly ClauseModel firstClause;
    public readonly ClauseModel secondClause;
    public readonly bool isPotentialSingleResult;
    public readonly bool[] isTailRecursiveArgument;
    public readonly bool[] isSingleResultIfArgumentImmutable;

    /**
     * Returns a new {@code TailRecursivePredicateMetaData} representing the user defined predicate defined by the
     * specified clauses or {@code null} if the predicate is not tail recursive.
     *
     * @param clauses the clauses that the user defined predicate consists of
     * @return a new {@code TailRecursivePredicateMetaData} or {@code null} if the predicate is not tail recursive
     */
    public static TailRecursivePredicateMetaData? Create(KnowledgeBase kb, List<ClauseModel> clauses)
        => IsTailRecursive(kb, clauses) ? new TailRecursivePredicateMetaData(clauses) : null;

    private static bool IsTailRecursive(KnowledgeBase kb, List<ClauseModel> terms)
    {
        if (terms.Count != 2) return false;

        var firstTerm = terms[0];
        if (!KnowledgeBaseUtils.IsSingleAnswer(kb, firstTerm.Antecedent)) return false;

        var secondTerm = terms[1];
        return IsAntecedentRecursive(kb, secondTerm);
    }

    public static bool IsAntecedentRecursive(KnowledgeBase kb, ClauseModel secondTerm)
    {
        var consequent = secondTerm.Consequent;
        var antecedent = secondTerm.Antecedent;
        var functions = KnowledgeBaseUtils.ToArrayOfConjunctions(antecedent);
        var lastFunction = functions[^1];
        if (lastFunction.Type == TermType.STRUCTURE
            && lastFunction.Name.Equals(consequent.Name)
            && lastFunction.NumberOfArguments == consequent.NumberOfArguments)
        {
            for (int i = 0; i < functions.Length - 1; i++)
            {
                if (!KnowledgeBaseUtils.IsSingleAnswer(kb, functions[i]))
                    return false;
            }
            return true;
        }
        else
        {
            return false;
        }
    }

    private static bool IsTail(Term list, Term term)
        => list.Type == TermType.LIST && TermUtils.TermsEqual(list.GetArgument(1), term);

    /**
     * @see TailRecursivePredicateMetaData#create(KnowledgeBase, List)
     */
    public TailRecursivePredicateMetaData(List<ClauseModel> clauses)
    {
        this.firstClause = clauses[0];
        this.secondClause = clauses[1];

        int numberOfArguments = firstClause.Consequent.NumberOfArguments;

        this.isTailRecursiveArgument = new bool[numberOfArguments];
        this.isSingleResultIfArgumentImmutable = new bool[numberOfArguments];

        var firstRuleConsequent = firstClause.Consequent;
        var secondRuleConsequent = secondClause.Consequent;
        var secondRuleAntecedentFinalFunction = GetFinalFunction(secondClause.Antecedent);
        bool firstRuleConsequentHasEmptyListAsAnArgument = false;
        for (int i = 0; i < numberOfArguments; i++)
        {
            var secondRuleConsequentArgument = secondRuleConsequent.GetArgument(i);
            var secondRuleAntecedentArgument = secondRuleAntecedentFinalFunction.GetArgument(i);
            if (IsTail(secondRuleConsequentArgument, secondRuleAntecedentArgument))
            {
                isTailRecursiveArgument[i] = true;
                if (firstRuleConsequent.GetArgument(i).Type == TermType.EMPTY_LIST)
                {
                    isSingleResultIfArgumentImmutable[i] = true;
                    firstRuleConsequentHasEmptyListAsAnArgument = true;
                }
            }
        }

        this.isPotentialSingleResult = firstRuleConsequentHasEmptyListAsAnArgument;
    }

    private static Term GetFinalFunction(Term t)
    {
        var functions = KnowledgeBaseUtils.ToArrayOfConjunctions(t);
        return functions[^1];
    }

    public ClauseModel FirstClause => firstClause;

    public ClauseModel SecondClause => secondClause;

    //public bool isPotentialSingleResult() {
    //   return isPotentialSingleResult;
    //}

    //public bool isTailRecursiveArgument(int idx) {
    //   return isTailRecursiveArgument[idx];
    //}

    //public bool isSingleResultIfArgumentImmutable(int idx) {
    //   return isSingleResultIfArgumentImmutable[idx];
    //}
}
