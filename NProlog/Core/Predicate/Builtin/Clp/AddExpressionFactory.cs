/*
 * Copyright 2022 S. Webber
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
using Org.NProlog.Core.Kb;
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Predicate.Builtin.Clp;



/* TEST
%?- 7 #= sum(X, 4)
%ERROR Cannot find CLP expression: sum/2

%TRUE pj_add_clp_expression(sum/2, 'org.projog.core.predicate.builtin.clp.CommonExpression/add')

%?- 7 #= sum(X, 4)
% X=3
*/
/**
 * <code>pj_add_clp_expression(X,Y)</code> - defines a Java class as an CLP expression.
 * <p>
 * <code>X</code> represents the name and arity of the expression. <code>Y</code> represents the full class name of an
 * implementation of <code>org.projog.core.predicate.builtin.clp.ExpressionFactory</code>.
 */
public class AddExpressionFactory : AbstractSingleResultPredicate
{
    protected ExpressionFactories expressions;


    protected override bool Evaluate(Term functionNameAndArity, Term javaClass)
    {
        var key = PredicateKey.CreateFromNameAndArity(functionNameAndArity);
        var className = TermUtils.GetAtomName(javaClass);
        expressions.addExpressionFactory(key, className);
        return true;
    }


    protected override void Init()
    {
        this.expressions = KnowledgeBaseServiceLocator.GetServiceLocator(knowledgeBase).GetInstance<ExpressionFactories>(
            typeof(ExpressionFactories));
    }
}
