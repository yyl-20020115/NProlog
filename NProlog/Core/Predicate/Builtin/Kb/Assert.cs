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
using Org.NProlog.Core.Predicate.Udp;
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Predicate.Builtin.Kb;


/* TEST
%?- X=p(1), asserta(X), asserta(p(2)), asserta(p(3))
% X=p(1)
%?- p(X)
% X=3
% X=2
% X=1

%?- retract(p(X))
% X=3
% X=2
% X=1
%FAIL p(X)

%?- X=p(1), assertz(X), assertz(p(2)), assertz(p(3))
% X=p(1)
%?- p(X)
% X=1
% X=2
% X=3

%?- retract(p(X))
% X=1
% X=2
% X=3
%FAIL p(X)

% Note: "assert" is a synonym for "assertz".
%FAIL z(X)
%TRUE assert(z(a)), assert(z(b)), assert(z(c))
%?- z(X)
% X=a
% X=b
% X=c

% rules can be asserted, but have to be surrounded by brackets
%FAIL q(X,Y,Z)
%?- assert((q(X,Y,Z) :- Z is X+Y))
% X=UNINSTANTIATED VARIABLE
% Y=UNINSTANTIATED VARIABLE
% Z=UNINSTANTIATED VARIABLE
%?- assert((q(X,Y,Z) :- Z is X-Y))
% X=UNINSTANTIATED VARIABLE
% Y=UNINSTANTIATED VARIABLE
% Z=UNINSTANTIATED VARIABLE
%?- assert((q(X,Y,Z) :- Z is X*Y, repeat(3)))
% X=UNINSTANTIATED VARIABLE
% Y=UNINSTANTIATED VARIABLE
% Z=UNINSTANTIATED VARIABLE
%?- assert((q(X,Y,Z) :- Z is -X))
% X=UNINSTANTIATED VARIABLE
% Y=UNINSTANTIATED VARIABLE
% Z=UNINSTANTIATED VARIABLE
%TRUE_NO q(6,3,9)
%?- q(6,3,18)
%YES
%YES
%YES
%NO
%FAIL q(6,3,17)
%?- q(5,2,Q)
% Q=7
% Q=3
% Q=10
% Q=10
% Q=10
% Q=-5
%?- q(6,7,Q)
% Q=13
% Q=-1
% Q=42
% Q=42
% Q=42
% Q=-6
%?- q(8,4,32)
%YES
%YES
%YES
%NO
%?- asserta((q(X,Y,Z) :- Z is Y-X))
% X=UNINSTANTIATED VARIABLE
% Y=UNINSTANTIATED VARIABLE
% Z=UNINSTANTIATED VARIABLE
%?- q(5,2,Q)
% Q=-3
% Q=7
% Q=3
% Q=10
% Q=10
% Q=10
% Q=-5

% Argument must be suitably instantiated that the predicate of the clause can be determined.
%?- asserta(X)
%ERROR Expected an atom or a predicate but got a VARIABLE with value: X
%?- assertz(X)
%ERROR Expected an atom or a predicate but got a VARIABLE with value: X

%?- asserta((p :- 7))
%ERROR Expected an atom or a predicate but got a INTEGER with value: 7
%?- assertz(9)
%ERROR Expected an atom or a predicate but got a INTEGER with value: 9

%?- asserta(true)
%ERROR Cannot replace already defined built-in predicate: true/0
%?- assertz(is(1,2))
%ERROR Cannot replace already defined built-in predicate: is/2

non_dynamic_predicate(1,2,3).

%?- asserta(non_dynamic_predicate(4,5,6))
%ERROR Cannot add clause to already defined user defined predicate as it is not dynamic: non_dynamic_predicate/3 clause: non_dynamic_predicate(4, 5, 6)
%?- assertz(non_dynamic_predicate(4,5,6))
%ERROR Cannot add clause to already defined user defined predicate as it is not dynamic: non_dynamic_predicate/3 clause: non_dynamic_predicate(4, 5, 6)
*/
/**
 * <code>asserta(X)</code> / <code>assertz(X)</code> - adds a clause to the knowledge base.
 * <p>
 * <code>asserta(X)</code> adds the clause <code>X</code> to the front of the knowledge base. <code>assertz(X)</code>
 * adds the clause <code>X</code> to the end of the knowledge base. <code>X</code> must be suitably instantiated that
 * the predicate of the clause can be determined.
 * </p>
 * <p>
 * This is <i>not</i> undone as part of backtracking.
 * </p>
 */
public class Assert : AbstractSingleResultPredicate
{
    public static Assert AssertA() => new (false);

    public static Assert AssertZ() => new (true);

    private readonly bool addLast;

    private Assert(bool addLast)
    {
        this.addLast = addLast;
    }


    protected override bool Evaluate(Term clause)
    {
        var clauseModel = ClauseModel.CreateClauseModel(clause);
        var key = PredicateKey.CreateForTerm(clauseModel.Consequent);
        var userDefinedPredicate = Predicates.CreateOrReturnUserDefinedPredicate(key);
        Add(userDefinedPredicate, clauseModel);
        return true;
    }

    private void Add(UserDefinedPredicateFactory userDefinedPredicate, ClauseModel clauseModel)
    {
        if (addLast)
        {
            userDefinedPredicate.AddLast(clauseModel);
        }
        else
        {
            userDefinedPredicate.AddFirst(clauseModel);
        }
    }
}
