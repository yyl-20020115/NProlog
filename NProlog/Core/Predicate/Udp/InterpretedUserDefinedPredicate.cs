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
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Predicate.Udp;



/**
 * Represents a user defined predicate.
 *
 * @see #evaluate()
 */
public class InterpretedUserDefinedPredicate : Predicate
{
    private readonly ICheckedEnumerator<ClauseAction> clauseActions;
    private readonly SpyPoints.SpyPoint spyPoint;
    private readonly Term[] queryArgs;
    private readonly bool debugEnabled;

    private ClauseAction currentClause;
    private Predicate currentPredicate;
    private bool retryCurrentClauseAction;

    public InterpretedUserDefinedPredicate(ICheckedEnumerator<ClauseAction> clauseActions, SpyPoints.SpyPoint spyPoint, Term[] queryArgs)
    {
        this.clauseActions = clauseActions;
        this.spyPoint = spyPoint;
        this.queryArgs = queryArgs;
        this.debugEnabled = spyPoint.IsEnabled;
    }

    /**
     * Evaluates a user defined predicate.
     * <p>
     * The process for evaluating a user defined predicate is as follows:
     * <ul>
     * <li>Iterates through every clause of the user defined predicate.</li>
     * <li>For each clause it attempts to unify the arguments in its head (consequent) with the arguments in the query (
     * {@code queryArgs}).</li>
     * <li>If the head of the clause can be unified with the query then an attempt is made to evaluate the body
     * (antecedent) of the clause.</li>
     * <li>If the body of the clause is successfully evaluated then {@code true} is returned.</li>
     * <li>If the body of the clause is not successfully evaluated then the arguments in the query are backtracked.</li>
     * <li>When there are no more clauses left to check then {@code false} is returned.</li>
     * </ul>
     * Once {@code evaluate()} has returned {@code true} subsequent invocations of {@code evaluate()} will attempt to
     * re-evaluate the antecedent of the previously successfully evaluated clause. If the body of the clause is
     * successfully re-evaluated then {@code true} is returned. If the body of the clause is not successfully
     * re-evaluated then the arguments in the query are backtracked and the method continues to iterate through the
     * clauses starting with the next clause in the sequence.
     */

    public virtual bool Evaluate()
    {
        try
        {
            if (retryCurrentClauseAction)
            {
                if (debugEnabled)
                {
                    spyPoint.LogRedo(this, queryArgs);
                }
                if (currentPredicate.Evaluate())
                {
                    retryCurrentClauseAction = currentPredicate.CouldReevaluationSucceed;
                    if (debugEnabled)
                    {
                        spyPoint.LogExit(this, queryArgs, currentClause.Model);
                    }
                    return true;
                }
                // attempt at retrying has failed so discard it
                retryCurrentClauseAction = false;
                TermUtils.Backtrack(queryArgs);
            }
            else if (currentClause == null)
            {
                if (debugEnabled)
                {
                    spyPoint.LogCall(this, queryArgs);
                }
            }
            else
            {
                if (debugEnabled)
                {
                    spyPoint.LogRedo(this, queryArgs);
                }
                TermUtils.Backtrack(queryArgs);
            }
            // cycle though all rules until none left
            while (clauseActions.MoveNext())
            {
                currentClause = clauseActions.Current;
                currentPredicate = currentClause.GetPredicate(queryArgs);
                if (currentPredicate != null && currentPredicate.Evaluate())
                {
                    retryCurrentClauseAction = currentPredicate.CouldReevaluationSucceed;
                    if (debugEnabled)
                    {
                        spyPoint.LogExit(this, queryArgs, currentClause.Model);
                    }
                    return true;
                }
                else
                {
                    retryCurrentClauseAction = false;
                    TermUtils.Backtrack(queryArgs);
                }
            }
            if (debugEnabled)
            {
                spyPoint.LogFail(this, queryArgs);
            }
            return false;
        }
        catch (CutException)
        {
            if (debugEnabled)
            {
                spyPoint.LogFail(this, queryArgs);
            }
            return false;
        }
        catch (PrologException pe)
        {
            pe.AddClause(currentClause.Model);
            throw pe;
        }
        catch (Exception t)
        {
            var pe = new PrologException("Exception processing: " + spyPoint.PredicateKey, t);
            pe.AddClause(currentClause.Model);
            throw pe;
        }
    }


    public virtual bool CouldReevaluationSucceed =>
        (currentClause == null || !currentClause.IsAlwaysCutOnBacktrack)
                && (retryCurrentClauseAction || clauseActions.CanMoveNext);
}
