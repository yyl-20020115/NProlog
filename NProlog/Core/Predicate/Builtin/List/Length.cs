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
using Org.NProlog.Core.Predicate.Udp;
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Predicate.Builtin.List;



/* TEST
%?- Length([],X)
% X=0
%?- Length([a],X)
% X=1
%?- Length([a,b],X)
% X=2
%?- Length([a,b,c],X)
% X=3

%FAIL Length([a,b],1)
%FAIL Length([a,b],3)

%?- Length(X,0)
% X=[]

%?- Length(X,1)
% X=[E0]

%?- Length(X,3)
% X=[E0,E1,E2]

%?- Length(X,Y)
% X=[]
% Y=0
% X=[E0]
% Y=1
% X=[E0,E1]
% Y=2
% X=[E0,E1,E2]
% Y=3
% X=[E0,E1,E2,E3]
% Y=4
% X=[E0,E1,E2,E3,E4]
% Y=5
% X=[E0,E1,E2,E3,E4,E5]
% Y=6
% X=[E0,E1,E2,E3,E4,E5,E6]
% Y=7
%QUIT

%?- Length([a,b|X],Y)
% X=[]
% Y=2
% X=[E0]
% Y=3
% X=[E0,E1]
% Y=4
% X=[E0,E1,E2]
% Y=5
% X=[E0,E1,E2,E3]
% Y=6
% X=[E0,E1,E2,E3,E4]
% Y=7
% X=[E0,E1,E2,E3,E4,E5]
% Y=8
% X=[E0,E1,E2,E3,E4,E5,E6]
% Y=9
%QUIT

% TODO fix documentation generator to handle QUIT

%?- Length([a,b|X],8)
% X=[E0,E1,E2,E3,E4,E5]
%?- Length([a,b|X],3)
% X=[E0]
%?- Length([a,b|X],2)
% X=[]
%FAIL Length([a,b|X],1)

%FAIL Length([a,b,c],a)

%FAIL Length(X,X)
%FAIL Length([a,b,c|X],X)

%?- Length(abc,X)
%ERROR Expected list but got: ATOM with value: abc
%?- Length([a,b|c],X)
%ERROR Expected list but got: LIST with value: .(a, .(b, c))
%?- Length([a,b|X],z)
%ERROR Expected Numeric but got: ATOM with value: z
*/
/**
 * <code>Length(X,Y)</code> - determines the Length of a list.
 * <p>
 * The <code>Length(X,Y)</code> goal succeeds if the number of elements in the list <code>X</code> matches the integer
 * value <code>Y</code>.
 * </p>
 */
public class Length : AbstractPredicateFactory
{

    protected override Predicate GetPredicate(Term list, Term expectedLength)
    {
        int actualLength = 0;
        var tail = list;
        while (tail.Type == TermType.LIST)
        {
            actualLength++;
            tail = tail.GetArgument(1);
        }

        if (tail == EmptyList.EMPTY_LIST)
        {
            return PredicateUtils.ToPredicate(expectedLength.Unify(IntegerNumberCache.ValueOf(actualLength)));
        }
        else if (!tail.Type.IsVariable)
        {
            throw new PrologException("Expected list but got: " + list.Type + " with value: " + list);
        }
        else if (expectedLength.Type.IsVariable)
        {
            return new Retryable(actualLength, tail, expectedLength);
        }
        else
        {
            int requiredLength = TermUtils.ToInt(expectedLength) - actualLength;
            return PredicateUtils.ToPredicate(requiredLength > -1 && tail.Unify(ListFactory.CreateListOfLength(requiredLength)));
        }
    }

    public class Retryable : Predicate
    {
        readonly int startLength;
        readonly Term list;
        readonly Term Length;
        int currentLength = 0;

        public Retryable(int startLength, Term list, Term Length)
        {
            this.startLength = startLength;
            this.list = list;
            this.Length = Length;
        }


        public virtual bool Evaluate()
        {
            list.Backtrack();
            Length.Backtrack();
            if (!list.Unify(ListFactory.CreateListOfLength(currentLength)))
            {
                return false;
            }
            if (!Length.Unify(IntegerNumberCache.ValueOf(startLength + currentLength)))
            {
                return false;
            }
            currentLength++;
            return true;
        }


        public virtual bool CouldReevaluationSucceed => true;
    }
}
