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
using Org.NProlog.Core.Exceptions;
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Predicate.Builtin.Construct;



/* TEST
%?- arg(2, a(b,c(d)), X)
% X=c(d)

%?- arg(1, a+(b+c), X )
% X=a

%FAIL arg(1, a+(b+c), b)

%?- arg(2, [a,b,c], X)
% X=[b,c]

%?- arg(3, [a,b,c], X)
%ERROR Cannot get argument at position: 3 from: .(a, .(b, .(c, [])))
*/
/**
 * <code>arg(N,T,A)</code> - allows access to an argument of a structure.
 * <p>
 * <code>arg(N,T,A)</code> provides a mechanism for accessing a specific argument of a structure.
 * <code>arg(N,T,A)</code> succeeds if the <code>N</code>th argument of the structure <code>T</code> is, or can be
 * assigned to, <code>A</code>.
 * </p>
 */
public class Arg : AbstractSingleResultPredicate
{

    protected override bool Evaluate(Term arg1, Term arg2, Term arg3)
    {
        var argIdx = TermUtils.ToInt(arg1);
        if (arg2.NumberOfArguments < argIdx)
        {
            throw new PrologException("Cannot get argument at position: " + argIdx + " from: " + arg2);
        }
        var t = arg2.GetArgument(argIdx - 1);
        return arg3.Unify(t);
    }
}
