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

namespace Org.NProlog.Core.Predicate.Builtin.List;


/* TEST
%TRUE_NO member(a, [a,b,c])
%TRUE_NO member(b, [a,b,c])
%TRUE member(c, [a,b,c])

%FAIL member(d, [a,b,c])
%FAIL member(d, [])
%FAIL member([], [])

%?- member(X, [a,b,c])
% X=a
% X=b
% X=c

%?- member(p(X,b), [p(a,b), p(z,Y), p(x(Y), Y)])
% X=a
% Y=UNINSTANTIATED VARIABLE
% X=z
% Y=b
% X=x(b)
% Y=b

%?- member(X, [a,b,c,a|d])
% X=a
% X=b
% X=c
% X=a
%?- member(a, [a,b,c,a|a])
%YES
%YES
%?- member(a, [a,b,c,a|d])
%YES
%YES
%TRUE_NO member(b, [a,b,c,a|d])
%TRUE_NO member(c, [a,b,c,a|d])
%FAIL member(d, [a,b,c,a|d])
%FAIL member(z, [a,b,c,a|d])

%FAIL member(X, a)
%FAIL member(X, p(a,b))
%FAIL member(X, 1)
%FAIL member(X, 1.5)

%?- member(a, [a,a,a|X])
% X=UNINSTANTIATED VARIABLE
% X=UNINSTANTIATED VARIABLE
% X=UNINSTANTIATED VARIABLE
% X=[a|_]
% X=[_,a|_]
% X=[_,_,a|_]
% X=[_,_,_,a|_]
%QUIT
%?- member(a, [a,b,c|X])
% X=UNINSTANTIATED VARIABLE
% X=[a|_]
% X=[_,a|_]
% X=[_,_,a|_]
% X=[_,_,_,a|_]
%QUIT
%?- member(d, [a,b,c|X])
% X=[d|_]
% X=[_,d|_]
% X=[_,_,d|_]
% X=[_,_,_,d|_]
%QUIT
%?- member(a, X)
% X=[a|_]
% X=[_,a|_]
% X=[_,_,a|_]
%QUIT
%?- member(X, Y)
% X=UNINSTANTIATED VARIABLE
% Y=[X|_]
% X=UNINSTANTIATED VARIABLE
% Y=[_,X|_]
% X=UNINSTANTIATED VARIABLE
% Y=[_,_,X|_]
%QUIT
%?- X=[a,b,c|Z], member(a,X)
% X=[a,b,c|Z]
% Z=UNINSTANTIATED VARIABLE
% X=[a,b,c,a|_]
% Z=[a|_]
% X=[a,b,c,_,a|_]
% Z=[_,a|_]
% X=[a,b,c,_,_,a|_]
% Z=[_,_,a|_]
%QUIT
%?- member(p(X),[p(a),p(b),p(c)|Z])
% X=a
% Z=UNINSTANTIATED VARIABLE
% X=b
% Z=UNINSTANTIATED VARIABLE
% X=c
% Z=UNINSTANTIATED VARIABLE
% X=UNINSTANTIATED VARIABLE
% Z=[p(X)|_]
% X=UNINSTANTIATED VARIABLE
% Z=[_,p(X)|_]
% X=UNINSTANTIATED VARIABLE
% Z=[_,_,p(X)|_]
% X=UNINSTANTIATED VARIABLE
% Z=[_,_,_,p(X)|_]
%QUIT
*/
/**
 * <code>member(E, L)</code> - enumerates members of a list.
 * <p>
 * <code>member(E, L)</code> succeeds if <code>E</code> is a member of the list <code>L</code>. An attempt is made to
 * retry the goal during backtracking - so it can be used to enumerate the members of a list.
 * </p>
 */
public class Member : AbstractPredicateFactory
{

    protected override Predicate GetPredicate(Term element, Term list)
        => new MemberPredicate(element, list);

    public class MemberPredicate : Predicate
    {
        private readonly Term element;
        private readonly Term originalList;
        private Term currentList;
        private bool isTailVariable;

        public MemberPredicate(Term element, Term originalList)
        {
            this.element = element;
            this.originalList = originalList;
            this.currentList = originalList;
        }


        public virtual bool Evaluate()
        {
            if (isTailVariable)
            {
                var n = new Terms.List(new Variable(), currentList.Term);
                currentList.Backtrack();
                currentList.Unify(n);
                return true;
            }

            while (true)
            {
                if (currentList.Type == TermType.LIST)
                {
                    element.Backtrack();
                    originalList.Backtrack();
                    Term head = currentList.GetArgument(0);
                    currentList = currentList.GetArgument(1);
                    if (element.Unify(head))
                    {
                        return true;
                    }
                }
                else if (currentList.Type.IsVariable)
                {
                    isTailVariable = true;
                    element.Backtrack();
                    Terms.List n = new Terms.List(element, new Variable());
                    currentList.Unify(n);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }


        public virtual bool CouldReevaluationSucceed 
            => currentList.Type == TermType.LIST || currentList.Type.IsVariable;
    }
}
