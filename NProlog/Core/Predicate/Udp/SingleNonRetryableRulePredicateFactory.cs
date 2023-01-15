/*
 * Copyright 2020 S. Webber
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


public class SingleNonRetryableRulePredicateFactory : PreprocessablePredicateFactory
{
    private readonly ClauseAction clause;
    private readonly SpyPoints.SpyPoint spyPoint;

    public SingleNonRetryableRulePredicateFactory(ClauseAction clause, SpyPoints.SpyPoint spyPoint)
    {
        this.clause = clause;
        this.spyPoint = spyPoint;
    }


    public Predicate GetPredicate(Term[] args)
        => EvaluateClause(clause, spyPoint, args);

    public static Predicate EvaluateClause(ClauseAction clause, SpyPoints.SpyPoint spyPoint, Term[] args)
    {
        try
        {
            if (spyPoint.IsEnabled)
            {
                spyPoint.LogCall(typeof(SingleNonRetryableRulePredicateFactory), args);

                bool result = clause.GetPredicate(args).Evaluate();

                if (result)
                {
                    spyPoint.LogExit(typeof(SingleNonRetryableRulePredicateFactory), args, clause.Model);
                }
                else
                {
                    spyPoint.LogFail(typeof(SingleNonRetryableRulePredicateFactory), args);
                }

                return PredicateUtils.ToPredicate(result);
            }
            else
            {
                return PredicateUtils.ToPredicate(clause.GetPredicate(args).Evaluate());
            }
        }
        catch (CutException)
        {
            if (spyPoint.IsEnabled)
            {
                spyPoint.LogFail(typeof(SingleNonRetryableRulePredicateFactory), args);
            }
            return PredicateUtils.FALSE;
        }
        catch (PrologException pe)
        {
            pe.AddClause(clause.Model);
            throw pe;
        }
        catch (Exception t)
        {
            var pe = new PrologException("Exception processing: " + spyPoint.PredicateKey, t);
            pe.AddClause(clause.Model);
            throw pe;
        }
    }


    public bool IsRetryable => false;


    public PredicateFactory Preprocess(Term arg) 
        => ClauseActionFactory.IsMatch(clause, arg.Args) ? this : new NeverSucceedsPredicateFactory(spyPoint);
}
