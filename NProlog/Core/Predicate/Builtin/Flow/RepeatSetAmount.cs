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

namespace Org.NProlog.Core.Predicate.Builtin.Flow;



/* TEST
%?- repeat(3), write('hello, world'), nl
%OUTPUT
%hello, world
%
%OUTPUT
%YES
%OUTPUT
%hello, world
%
%OUTPUT
%YES
%OUTPUT
%hello, world
%
%OUTPUT
%YES

%?- repeat(1)
%YES
%?- repeat(2)
%YES
%YES
%?- repeat(3)
%YES
%YES
%YES
%FAIL repeat(0)
%FAIL repeat(-1)

%?- repeat(X)
%ERROR Expected Numeric but got: VARIABLE with value: X
*/
/**
 * <code>repeat(N)</code> - succeeds <code>N</code> times.
 */
public class RepeatSetAmount : AbstractPredicateFactory
{

    protected override Predicate GetPredicate(Term arg) => new RepeatSetAmountPredicate(TermUtils.CastToNumeric(arg).Long);

    public class RepeatSetAmountPredicate : Predicate
    {
        private readonly long limit;
        private long ctr;

        /** @param limit the number of times to successfully evaluate */
        public RepeatSetAmountPredicate(long limit) => this.limit = limit;

        public virtual bool Evaluate()
        {
            if (ctr < limit)
            {
                ctr++;
                return true;
            }
            else
                return false;
        }


        public virtual bool CouldReevaluationSucceed => ctr < limit;
    }
}
