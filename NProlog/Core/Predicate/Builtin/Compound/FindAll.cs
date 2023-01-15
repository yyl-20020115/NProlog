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
using Org.NProlog.Core.Predicate.Builtin.List;
using Org.NProlog.Core.Predicate.Udp;
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Predicate.Builtin.Compound;



/* TEST
z(r).
z(t).
z(y).

x(a,b,c).
x(q,X,e) :- z(X).
x(1,2,3).
x(w,b,c).
x(d,b,c).
x(a,b,c).

%?- findall(X,x(X,Y,Z),L)
% L=[a,q,q,q,1,w,d,a]
% X=UNINSTANTIATED VARIABLE
% Y=UNINSTANTIATED VARIABLE
% Z=UNINSTANTIATED VARIABLE

%?- findall(X,x(X,y,z),L)
% L=[]
% X=UNINSTANTIATED VARIABLE

q(a(W)).
q(C).
q(1).
y(X) :- X = o(T,R), q(T), q(R).

%?- findall(X,y(X),L)
% L=[o(a(W), a(W)),o(a(W), R),o(a(W), 1),o(T, a(W)),o(T, R),o(T, 1),o(1, a(W)),o(1, R),o(1, 1)]
% X=UNINSTANTIATED VARIABLE

%?- findall(X,y(X),L), L=[H|_], H=o(a(q),a(z))
% L=[o(a(q), a(z)),o(a(W), R),o(a(W), 1),o(T, a(W)),o(T, R),o(T, 1),o(1, a(W)),o(1, R),o(1, 1)]
% H=o(a(q), a(z))
% X=UNINSTANTIATED VARIABLE

%?- findall(Y, (member(X,[6,3,7,2,5,4,3]), X<4, Y is X*X), L)
% L=[9,4,9]
% X=UNINSTANTIATED VARIABLE
% Y=UNINSTANTIATED VARIABLE

%?- findall(X,P,L)
%ERROR Expected an atom or a predicate but got a VARIABLE with value: P
*/
/**
 * <code>findall(X,P,L)</code> - find all solutions that satisfy the goal.
 * <p>
 * <code>findall(X,P,L)</code> produces a list (<code>L</code>) of <code>X</code> for each possible solution of the goal
 * <code>P</code>. Succeeds with <code>L</code> unified to an empty list if <code>P</code> has no solutions.
 */
public class FindAll : AbstractSingleResultPredicate, PreprocessablePredicateFactory
{

    protected override bool Evaluate(Term template, Term goal, Term output) 
        => EvaluateFindAll(Predicates.GetPredicateFactory(goal), template, goal, output);

    private static bool EvaluateFindAll(PredicateFactory pf, Term template, Term goal, Term output)
    {
        var predicate = pf.GetPredicate(goal.Args);
        var solutions = predicate.Evaluate() ? CreateListOfAllSolutions(template, predicate) : EmptyList.EMPTY_LIST;
        template.Backtrack();
        goal.Backtrack();
        return output.Unify(solutions);
    }

    private static Term CreateListOfAllSolutions(Term template, Predicate predicate)
    {
        List<Term> solutions = new();
        do
        {
            solutions.Add(template.Copy(new Dictionary<Variable, Variable>()));
        } while (HasFoundAnotherSolution(predicate));
        var output = ListFactory.CreateList(solutions);
        output.Backtrack();
        return output;
    }

    private static bool HasFoundAnotherSolution(Predicate predicate) => predicate.CouldReevaluationSucceed && predicate.Evaluate();


    public virtual PredicateFactory Preprocess(Term term)
    {
        var goal = term.GetArgument(1);
        return PartialApplicationUtils.IsAtomOrStructure(goal)
            ? new PreprocessedFindAll(Predicates.GetPreprocessedPredicateFactory(goal))
            : this;
    }

    private class PreprocessedFindAll : PredicateFactory
    {
        private readonly PredicateFactory pf;

        public PreprocessedFindAll(PredicateFactory pf)
        {
            this.pf = pf;
        }


        public virtual Predicate GetPredicate(Term[] args)
            => PredicateUtils.ToPredicate(EvaluateFindAll(pf, args[0], args[1], args[2]));


        public bool IsRetryable => false;
    }
}
