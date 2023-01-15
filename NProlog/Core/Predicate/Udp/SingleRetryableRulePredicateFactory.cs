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


public class SingleRetryableRulePredicateFactory : PreprocessablePredicateFactory
{
    private readonly ClauseAction clause;
    private readonly SpyPoints.SpyPoint spyPoint;

    public SingleRetryableRulePredicateFactory(ClauseAction clause, SpyPoints.SpyPoint spyPoint)
    {
        this.clause = clause;
        this.spyPoint = spyPoint;
    }


    public RetryableRulePredicate GetPredicate(Term[] args)
        => new RetryableRulePredicate(clause, spyPoint, args);


    public bool IsRetryable => true;

    public class RetryableRulePredicate : Predicate
    {
        private readonly Term[] args;
        private readonly ClauseAction clause;
        private readonly SpyPoints.SpyPoint spyPoint;
        private readonly bool isSpyPointEnabled;
        private Predicate? p;

        public RetryableRulePredicate(ClauseAction clause, SpyPoints.SpyPoint spyPoint, Term[] queryArgs)
        {
            this.clause = clause;
            this.spyPoint = spyPoint;
            this.args = queryArgs;
            this.isSpyPointEnabled = spyPoint.IsEnabled;
        }


        public virtual bool Evaluate()
        {
            try
            {
                if (p == null)
                {
                    if (isSpyPointEnabled)
                    {
                        spyPoint.LogCall(this, args);
                    }
                    p = clause.GetPredicate(args);
                }
                else if (isSpyPointEnabled)
                {
                    spyPoint.LogRedo(this, args);
                }

                if (p.Evaluate())
                { // TODO p.couldReevaluationSucceed() &&
                    if (isSpyPointEnabled)
                    {
                        spyPoint.LogExit(this, args, clause.Model);
                    }
                    return true;
                }
                else
                {
                    if (isSpyPointEnabled)
                    {
                        spyPoint.LogFail(this, args);
                    }
                    return false;
                }
            }
            catch (CutException)
            {
                if (isSpyPointEnabled)
                {
                    spyPoint.LogFail(typeof(SingleNonRetryableRulePredicateFactory), args);
                }
                return false;
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


        public virtual bool CouldReevaluationSucceed => p == null || p.CouldReevaluationSucceed;
    }


    public PredicateFactory Preprocess(Term arg)
         => ClauseActionFactory.IsMatch(clause, arg.Args) ? this : new NeverSucceedsPredicateFactory(spyPoint);

    Predicate PredicateFactory.GetPredicate(Term[] args) => this.GetPredicate(args);
}
