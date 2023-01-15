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
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Predicate.Builtin.Compare;


/* TEST
%?- X is 3
% X=3
%?- X is 3+2
% X=5
%?- X is 3.5+2.25
% X=5.75
%TRUE 5 is 5
%FAIL 5 is 6
%TRUE 5 is 4+1
%FAIL 5 is 4+2

%?- X is Y
%ERROR Cannot get Numeric for term: Y of type: VARIABLE

%?- Z=1+1, Y=9-Z, X is Y
% X=7
% Y=9 - (1 + 1)
% Z=1 + 1

%?- X is _
%ERROR Cannot get Numeric for term: _ of type: VARIABLE

%?- X is sum(1,2)
%ERROR Cannot find arithmetic operator: sum/2

%?- X is ten
%ERROR Cannot find arithmetic operator: ten/0

%?- X is []
%ERROR Cannot get Numeric for term: [] of type: EMPTY_LIST

%?- X is [1,2,3]
%ERROR Cannot get Numeric for term: .(1, .(2, .(3, []))) of type: LIST
*/
/**
 * <code>X is Y</code> - evaluate arithmetic expression.
 * <p>
 * Firstly structure <code>Y</code> is evaluated as an arithmetic expression to give a number. Secondly an attempt is
 * made to match the number to <code>X</code>. The goal succeeds or fails based on the match.
 * </p>
 */
public class Is : AbstractSingleResultPredicate, PreprocessablePredicateFactory
{

    protected override bool Evaluate(Term arg1, Term arg2)
    {
        var n = ArithmeticOperators.GetNumeric(arg2);
        return arg1.Unify(n);
    }


    public PredicateFactory Preprocess(Term arg)
    {
        var o = ArithmeticOperators.GetPreprocessedArithmeticOperator(arg.GetArgument(1));
        return o == null ? this : (global::Org.NProlog.Core.Predicate.PredicateFactory)(o is Numeric numeric ? new Unify(numeric) : new PreprocessedIs(o));
    }

    public class Unify : AbstractSingleResultPredicate
    {
        readonly Numeric n;

        public Unify(Numeric n) => this.n = n;


        protected override bool Evaluate(Term arg1, Term arg2) => arg1.Unify(n);
    }

    public class PreprocessedIs : AbstractSingleResultPredicate
    {
        readonly ArithmeticOperator o;

        public PreprocessedIs(ArithmeticOperator o) => this.o = o;


        protected override bool Evaluate(Term arg1, Term arg2) => arg1.Unify(o.Calculate(arg2.Args));
    }
}
