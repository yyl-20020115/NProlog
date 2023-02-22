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
using Org.NProlog.Core.Math;
using Org.NProlog.Core.Parser;
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Predicate.Builtin.Kb;

/* TEST
squared(X,Y) :- Y is X * X.

%?- squared(3,X)
% X=9

%?- X is squared(3)
%ERROR Cannot find arithmetic operator: squared/1

%TRUE arithmetic_function(squared/1)

%?- X is squared(3)
% X=9
*/
/**
 * <code>arithmetic_function(X)</code> - defines a predicate as an arithmetic function.
 * <p>
 * Allows the predicate defined by <code>X</code> to be used as an arithmetic function.
 */
public class AddUserDefinedArithmeticOperator : AbstractSingleResultPredicate
{

    protected override bool Evaluate(Term arg)
    {
        var key = PredicateKey.CreateFromNameAndArity(arg);
        var arithmeticOperator = new UserDefinedArithmeticOperator(Predicates, key);
        ArithmeticOperators.AddArithmeticOperator(key, arithmeticOperator);
        return true;
    }

    public class UserDefinedArithmeticOperator : ArithmeticOperator
    {
        readonly int numArgs;
        readonly PredicateKey key;
        readonly PredicateFactory pf;

        public UserDefinedArithmeticOperator(Predicates p, PredicateKey originalKey)
        {
            this.numArgs = originalKey.NumArgs;
            this.key = new PredicateKey(originalKey.Name, numArgs + 1);
            this.pf = p.GetPredicateFactory(key);
        }


        public Numeric Calculate(Term[] args)
        {
            var result = new Variable("result");
            var argsPlusResult = CreateArgumentsIncludingResult(args, result);

            return pf.GetPredicate(argsPlusResult).Evaluate()
                ? TermUtils.CastToNumeric(result)
                : throw new PrologException("Could not evaluate: " + key + " with arguments: " + Arrays.ToString(args));
        }

        private Term[] CreateArgumentsIncludingResult(Term[] args, Variable result)
        {
            var argsPlusResult = new Term[numArgs + 1];
            for (int i = 0; i < numArgs; i++)
                argsPlusResult[i] = args[i].Term;
            argsPlusResult[numArgs] = result;
            return argsPlusResult;
        }
    }
}
