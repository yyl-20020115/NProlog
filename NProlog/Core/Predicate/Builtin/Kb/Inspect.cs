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
using Org.NProlog.Core.Predicate.Builtin.Db;
using Org.NProlog.Core.Predicate.Udp;
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Predicate.Builtin.Kb;



/* TEST
% Examples of using "clause":
test(a,b) :- true.
test(1,2) :- true.
test(A,B,C) :- true.
test([_|Y],p(1)) :- true.
%?- clause(test(X,Y), Z)
% X=a
% Y=b
% Z=true
% X=1
% Y=2
% Z=true
% X=[_|Y]
% Y=p(1)
% Z=true

%TRUE clause(test(a,b,c), true)
%TRUE clause(test(1,2,3), true)
%FAIL clause(tset(1,2,3), true)

% Examples of using "retract":

%TRUE assertz(p(a,b,c))
%TRUE assertz(p(1,2,3))
%TRUE assertz(p(a,c,e))

%FAIL retract(p(x,y,z))
%FAIL retract(p(a,b,e))

%?- p(X,Y,Z)
% X=a
% Y=b
% Z=c
% X=1
% Y=2
% Z=3
% X=a
% Y=c
% Z=e

%?- retract(p(a,Y,Z))
% Y=b
% Z=c
% Y=c
% Z=e

%?- p(X,Y,Z)
% X=1
% Y=2
% Z=3

%?- retract(p(X,Y,Z))
% X=1
% Y=2
% Z=3

%FAIL p(X,Y,Z)

% retract and clause will fail if predicate does not exist
%FAIL retract(unknown_predicate(1,2,3))
%FAIL clause(unknown_predicate(1,2,3),X)

% Argument must be suitably instantiated that the predicate of the clause can be determined.
%?- retract(X)
%ERROR Expected an atom or a predicate but got a VARIABLE with value: X
%?- clause(X,Y)
%ERROR Expected an atom or a predicate but got a VARIABLE with value: X

%?- retract(true)
%ERROR Cannot inspect clauses of built-in predicate: true/0
%?- clause(true,X)
%ERROR Cannot inspect clauses of built-in predicate: true/0
%?- retract(is(1,2))
%ERROR Cannot inspect clauses of built-in predicate: is/2
%?- clause(is(1,2),X)
%ERROR Cannot inspect clauses of built-in predicate: is/2

non_dynamic_predicate(1,2,3).
%?- retract(non_dynamic_predicate(1,2,3))
%ERROR Cannot retract clause from user defined predicate as it is not dynamic: non_dynamic_predicate/3
%?- retract(non_dynamic_predicate(_,_,_))
%ERROR Cannot retract clause from user defined predicate as it is not dynamic: non_dynamic_predicate/3
%FAIL retract(non_dynamic_predicate(4,5,6))
*/
/**
 * <code>clause(X,Y)</code> / <code>retract(X)</code> - matches terms to existing clauses.
 * <p>
 * <code>clause(X,Y)</code> causes <code>X</code> and <code>Y</code> to be matched to the head and body of an existing
 * clause. If no clauses are found for the predicate represented by <code>X</code> then the goal fails. If there are
 * more than one that matches, the clauses will be matched one at a time as the goal is re-satisfied. <code>X</code>
 * must be suitably instantiated that the predicate of the clause can be determined.
 * </p>
 * <p>
 * <code>retract(X)</code> - Remove clauses from the knowledge base. The first clause that <code>X</code> matches is
 * removed from the knowledge base. When an attempt is made to re-satisfy the goal, the next clause that <code>X</code>
 * matches is removed. <code>X</code> must be suitably instantiated that the predicate of the clause can be determined.
 * </p>
 */
public class Inspect : AbstractPredicateFactory
{
    public static readonly Inspect Default = new ();
    /**
     * {@code true} if matching rules should be removed (retracted) from the knowledge base as part of calls to
     * {@link #evaluate(Term, Term)} or {@code false} if the knowledge base should remain unaltered.
     */
    private readonly bool doRemoveMatches;
    public Inspect()
    {
        this.doRemoveMatches = false;
    }

    public static Inspect InspectClause() => new (false);

    public static Inspect Retract() => new (true);

    private Inspect(bool doRemoveMatches) => this.doRemoveMatches = doRemoveMatches;


    protected override Predicate GetPredicate(Term clauseHead) => GetPredicate(clauseHead, null);


    protected override Predicate GetPredicate(Term clauseHead, Term clauseBody)
    {
        var predicateFactory = Predicates.GetPredicateFactory(clauseHead);
        if (predicateFactory is UserDefinedPredicateFactory userDefinedPredicate)
        {
            return new InspectPredicate(clauseHead, clauseBody, userDefinedPredicate.GetImplications(), this);
        }
        else if (predicateFactory is UnknownPredicate)
        {
            return PredicateUtils.FALSE;
        }
        else
        {
            throw new PrologException("Cannot inspect clauses of built-in predicate: " + PredicateKey.CreateForTerm(clauseHead));
        }
    }

    public class InspectPredicate : Predicate
    {
        private readonly Term clauseHead;
        private readonly Term clauseBody;
        private readonly ICheckedEnumerator<ClauseModel> implications;
        private readonly Inspect inspect;
        public InspectPredicate(Term clauseHead, Term clauseBody, ICheckedEnumerator<ClauseModel> implications, Inspect inspect)
        {
            this.clauseHead = clauseHead;
            this.clauseBody = clauseBody;
            this.implications = implications;
            this.inspect = inspect;
        }

        /**
         * @return {@code true} if there is a rule in the knowledge base whose consequent can be unified with
         * {@code clauseHead} and, if {@code clauseBody} is not {@code null}, whose antecedent can be unified with
         * {@code clauseBody}.
         */

        public virtual bool Evaluate()
        {
            var it = implications;
            while(it.MoveNext())
            {
                Backtrack(clauseHead, clauseBody);
                if (Unifiable(clauseHead, clauseBody, it.Current))
                {
                    if (inspect.doRemoveMatches)
                        Remove();
                    return true;
                }
            }
            return false;
        }

        private void Remove()
        {
            try
            {
                //TODO:fixme
                //implications.Remove();
            }
            catch (InvalidOperationException)
            {
                throw new PrologException("Cannot retract clause from user defined predicate as it is not dynamic: " + PredicateKey.CreateForTerm(clauseHead));
            }
        }

        private static void Backtrack(Term clauseHead, Term clauseBody)
        {
            clauseHead?.Backtrack();
            clauseBody?.Backtrack();
        }

        private static bool Unifiable(Term clauseHead, Term clauseBody, ClauseModel clauseModel)
        {
            var consequent = clauseModel.Consequent;
            var antecedent = clauseModel.Antecedent;
            return clauseHead.Unify(consequent) && (clauseBody == null || clauseBody.Unify(antecedent));
        }


        public virtual bool CouldReevaluationSucceed => implications == null || implications.CanMoveNext;
    }
}
