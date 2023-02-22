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
using Org.NProlog.Core.Math;
using Org.NProlog.Core.Predicate.Udp;
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Predicate.Builtin.Compare;

/* TEST
%TRUE between(1, 5, 1)
%TRUE between(1, 5, 2)
%TRUE between(1, 5, 3)
%TRUE between(1, 5, 4)
%TRUE between(1, 5, 5)

%FAIL between(1, 5, 0)
%FAIL between(1, 5, -1)
%FAIL between(1, 5, -9223372036854775808)

%FAIL between(1, 5, 6)
%FAIL between(1, 5, 7)
%FAIL between(1, 5, 9223372036854775807)

%TRUE between(-9223372036854775808, 9223372036854775807, -9223372036854775808)
%TRUE between(-9223372036854775808, 9223372036854775807, -1)
%TRUE between(-9223372036854775808, 9223372036854775807, 0)
%TRUE between(-9223372036854775808, 9223372036854775807, 1)
%TRUE between(-9223372036854775808, 9223372036854775807, 9223372036854775807)

%?- between(1, 1, X)
% X=1

%?- between(1, 2, X)
% X=1
% X=2

%?- between(1, 5, X)
% X=1
% X=2
% X=3
% X=4
% X=5

%FAIL between(5, 1, X)

%TRUE between(5-2, 2+3, 2*2)
%FAIL between(5-2, 2+3, 8-6)
*/
/**
 * <code>between(X,Y,Z)</code> - checks if a number is within a specified range.
 * <p>
 * <code>between(X,Y,Z)</code> succeeds if the integer numeric value represented by <code>Z</code> is greater than or
 * equal to the integer numeric value represented by <code>X</code> and is less than or equal to the integer numeric
 * value represented by <code>Y</code>.
 * </p>
 * <p>
 * If <code>Z</code> is an uninstantiated variable then <code>Z</code> will be successively unified with all integer
 * values in the range from <code>X</code> to <code>Y</code>.
 * </p>
 */
public class Between : AbstractPredicateFactory
{
    protected override Predicate GetPredicate(Term low, Term high, Term middle)
    {
        var operators = ArithmeticOperators;
        if (middle.Type.IsVariable)
            return new Retryable(middle, TermUtils.ToLong(operators, low), TermUtils.ToLong(operators, high));
        else
        {
            var result = NumericTermComparator.Compare(low, middle, operators) < 1
                && NumericTermComparator.Compare(middle, high, operators) < 1;
            return PredicateUtils.ToPredicate(result);
        }
    }

    public class Retryable : Predicate
    {
        readonly Term middle;
        readonly long max;
        long ctr;

        public Retryable(Term middle, long start, long max)
        {
            this.middle = middle;
            this.ctr = start;
            this.max = max;
        }

        public bool Evaluate()
        {
            while (CouldReevaluationSucceed)
            {
                middle.Backtrack();
                var n = IntegerNumberCache.ValueOf(ctr++);
                return middle.Unify(n);
            }
            return false;
        }

        public bool CouldReevaluationSucceed => ctr <= max;
    }
}
