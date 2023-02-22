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

namespace Org.NProlog.Core.Predicate.Builtin.Compare;

/* TEST
%TRUE abc \= def

%FAIL X \= Y

%FAIL p(X,b) \= p(a,Y)

%?- p(X,b,c) \= p(a,Y,z)
% X=UNINSTANTIATED VARIABLE
% Y=UNINSTANTIATED VARIABLE
*/
/**
 * <code>X \= Y</code> - checks whether two terms cannot be unified.
 * <p>
 * If <code>X</code> can be NOT unified with <code>Y</code> the goal succeeds else the goal fails.
 * </p>
 */
public class NotUnifiable : AbstractSingleResultPredicate
{

    protected override bool Evaluate(Term arg1, Term arg2)
    {
        var unifiable = arg1.Unify(arg2);
        arg1.Backtrack();
        arg2.Backtrack();
        return !unifiable;
    }
}
