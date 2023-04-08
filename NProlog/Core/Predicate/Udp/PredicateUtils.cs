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
using Org.NProlog.Core.Terms;
using static Org.NProlog.Core.Event.SpyPoints;

namespace Org.NProlog.Core.Predicate.Udp;


public static class PredicateUtils
{
    public static readonly SucceedsOncePredicate TRUE = SucceedsOncePredicate.SINGLETON;

    public static readonly SucceedsNeverPredicate FALSE = SucceedsNeverPredicate.SINGLETON;

    public static Predicate ToPredicate(bool result) => result ? TRUE : FALSE;

    public static Predicate CreateSingleClausePredicate(ClauseAction clause, SpyPoint spyPoint, Term[] args)
        => clause.IsRetryable
            ? new SingleRetryableRulePredicateFactory.RetryableRulePredicate(clause, spyPoint, args)
          : SingleNonRetryableRulePredicateFactory.EvaluateClause(clause, spyPoint, args);

    public static Predicate CreateFailurePredicate(SpyPoint spyPoint, Term[] args)
    {
        if (spyPoint.IsEnabled)
        {
            spyPoint.LogCall(typeof(PredicateUtils), args);
            spyPoint.LogFail(typeof(PredicateUtils), args);
        }
        return FALSE;
    }
}
