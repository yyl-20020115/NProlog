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

namespace Org.NProlog.Core.Predicate.Builtin.Kb;

/* TEST
%?- X is sum(1, 1)
%ERROR Cannot find arithmetic operator: sum/2

%TRUE pl_add_arithmetic_operator(sum/2, 'org.prolog.core.math.builtin.Add')

%?- X is sum(1, 1)
% X=2
*/
/**
 * <code>pl_add_arithmetic_operator(X,Y)</code> - defines a Java class as an arithmetic operator.
 * <p>
 * <code>X</code> represents the name and arity of the predicate. <code>Y</code> represents the full class name of an
 * implementation of <code>org.prolog.core.ArithmeticOperator</code>.
 */
public class AddArithmeticOperator : AbstractSingleResultPredicate
{

    protected override bool Evaluate(Term functionNameAndArity, Term typeFullName)
    {
        var key = PredicateKey.CreateFromNameAndArity(functionNameAndArity);
        var typeName = TermUtils.GetAtomName(typeFullName);
        ArithmeticOperators.AddArithmeticOperator(key, typeName);
        return true;
    }
}
