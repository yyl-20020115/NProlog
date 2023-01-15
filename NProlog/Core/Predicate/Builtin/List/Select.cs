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

namespace Org.NProlog.Core.Predicate.Builtin.List;


/* TEST
%?- select(X,[h,e,l,l,o],Z)
% X=h
% Z=[e,l,l,o]
% X=e
% Z=[h,l,l,o]
% X=l
% Z=[h,e,l,o]
% X=l
% Z=[h,e,l,o]
% X=o
% Z=[h,e,l,l]

%?- select(l,[h,e,l,l,o],Z)
% Z=[h,e,l,o]
% Z=[h,e,l,o]
%NO

%?- select(l,[h,e,l,l,o],[h,e,l,o])
%YES
%YES
%NO

%?- select(p(a,B),[p(X,q), p(a,X)],Z)
% B=q
% X=a
% Z=[p(a, a)]
% B=UNINSTANTIATED VARIABLE
% X=UNINSTANTIATED VARIABLE
% Z=[p(B, q)]

%?- select(a, Result, [x,y,z])
% Result=[a,x,y,z]
% Result=[x,a,y,z]
% Result=[x,y,a,z]
% Result=[x,y,z,a]

%?- select(a, [x|X], [x,y,z])
% X=[a,y,z]
% X=[y,a,z]
% X=[y,z,a]
*/
/**
 * <code>select(X,Y,Z)</code> - removes an element from a list.
 * <p>
 * Attempts to unify <code>Z</code> with the result of removing an occurrence of <code>X</code> from the list
 * represented by <code>Y</code>. An attempt is made to retry the goal during backtracking.
 * </p>
 */
public class Select : AbstractPredicateFactory
{

    protected override Predicate GetPredicate(Term element, Term inputList, Term outputList)
    {
        // select(X, [Head|Tail], Rest) implemented as: select(Tail, Head, X, Rest)
        if (inputList.Type == TermType.LIST)
        {
            return new SelectPredicate(inputList.GetArgument(1), inputList.GetArgument(0), element, outputList);
        }
        else if (inputList.Type.isVariable)
        {
            var head = new Variable("Head");
            var tail = new Variable("Tail");
            var newList = new Terms.List(head, tail);
            inputList.Unify(newList);
            return new SelectPredicate(tail, head, element, outputList);
        }
        else
        {
            return PredicateUtils.FALSE;
        }
    }

    public class SelectPredicate : Predicate
    {
        Term firstArg;
        Term secondArg;
        readonly Term thirdArg;
        Term fourthArg;
        bool retrying;

        public SelectPredicate(Term firstArg, Term secondArg, Term thirdArg, Term fourthArg)
        {
            this.firstArg = firstArg;
            this.secondArg = secondArg;
            this.thirdArg = thirdArg;
            this.fourthArg = fourthArg;
        }


        public virtual bool Evaluate()
        {
            while (true)
            {
                //select3_(Tail, Head, Head, Tail).
                if (!retrying && firstArg.Unify(fourthArg) && secondArg.Unify(thirdArg))
                {
                    retrying = true;
                    return true;
                }
                retrying = false;

                firstArg.Backtrack();
                secondArg.Backtrack();
                thirdArg.Backtrack();
                fourthArg.Backtrack();

                // select3_([Head2|Tail], Head, X, [Head|Rest]) :-
                //   select3_(Tail, Head2, X, Rest).
                Term tail;
                Term head2;
                if (firstArg.Type == TermType.LIST)
                {
                    head2 = firstArg.GetArgument(0);
                    tail = firstArg.GetArgument(1);
                }
                else if (firstArg.Type.isVariable)
                {
                    head2 = new Variable("Head2");
                    tail = new Variable("Tail");
                    firstArg.Unify(new Terms.List(head2, tail));
                }
                else
                {
                    return false;
                }

                Term rest;
                if (fourthArg.Type == TermType.LIST)
                {
                    if (!secondArg.Unify(fourthArg.GetArgument(0)))
                    {
                        return false;
                    }
                    rest = fourthArg.GetArgument(1);
                }
                else if (fourthArg.Type.isVariable)
                {
                    rest = new Variable("Rest");
                    fourthArg.Unify(new Terms.List(secondArg, rest));
                }
                else
                {
                    return false;
                }

                firstArg = tail.Term;
                secondArg = head2.Term;
                fourthArg = rest.Term;
            }
        }


        public virtual bool CouldReevaluationSucceed => !retrying || (firstArg != EmptyList.EMPTY_LIST && fourthArg != EmptyList.EMPTY_LIST);
    }
}
