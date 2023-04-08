/*
 * Copyright 2021 S. Webber
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


public class NeverSucceedsPredicateFactory : PredicateFactory
{
    private readonly SpyPoint spyPoint;

    public NeverSucceedsPredicateFactory(SpyPoint spyPoint) => this.spyPoint = spyPoint;


    public Predicate GetPredicate(Term[] args)
        => PredicateUtils.CreateFailurePredicate(spyPoint, args);


    public bool IsRetryable => false;
}