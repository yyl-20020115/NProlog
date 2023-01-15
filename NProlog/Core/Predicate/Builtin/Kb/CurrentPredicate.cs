/*
 * Copyright 2018 S. Webber
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
using Org.NProlog.Core.Predicate.Builtin.Db;
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Predicate.Builtin.Kb;



/* TEST
%TRUE_NO current_predicate(!/0)

%?- current_predicate(!/X)
% X=0
%NO

%FAIL current_predicate(!/1)

%FAIL current_predicate(doesnt_exist/1)

%?- current_predicate(call/X)
% X=1
% X=2
% X=3
% X=4
% X=5
% X=6
% X=7
% X=8
% X=9
% X=10
%NO
*/
/**
 * <code>current_predicate(X)</code> - unifies with defined predicates.
 * <p>
 * <code>current_predicate(X)</code> attempts to unify <code>X</code> against all currently defined predicates.
 */
public class CurrentPredicate : AbstractPredicateFactory
{

    protected override Predicate GetPredicate(Term arg)
    {
        var keys = Predicates.GetAllDefinedPredicateKeys();
        return new Retryable(arg, keys);
    }

    public class Retryable : Predicate
    {
        private readonly Term arg;
        private readonly ICheckedEnumerator<PredicateKey> iterator;

        public Retryable(Term arg, HashSet<PredicateKey> keys)
        {
            this.arg = arg;
            this.iterator = ListCheckedEnumerator<PredicateKey>.Of(keys.ToList());
        }


        public virtual bool Evaluate()
        {
            while (iterator.MoveNext())
            {
                var next = iterator.Current.ToTerm();
                arg.Backtrack();
                if (arg.Unify(next)) return true;
            }
            return false;
        }


        public virtual bool CouldReevaluationSucceed => iterator.CanMoveNext;
    }
}
