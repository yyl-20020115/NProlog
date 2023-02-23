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
%TRUE nth0(0, [a,b,c], a)
%TRUE nth1(1, [a,b,c], a)
%TRUE nth0(1, [a,b,c], b)
%TRUE nth1(2, [a,b,c], b)
%TRUE nth0(2, [a,b,c], c)
%TRUE nth1(3, [a,b,c], c)

%FAIL nth0(-1, [a,b,c], a)
%FAIL nth0(1, [a,b,c], a)
%FAIL nth0(5, [a,b,c], a)

%?- nth0(0, [a,b,c], X)
% X=a
%?- nth0(1, [a,b,c], X)
% X=b
%?- nth0(2, [a,b,c], X)
% X=c

%FAIL nth0(-1, [a,b,c], X)
%FAIL nth0(3, [a,b,c], X)

%?- nth0(X, [h,e,l,l,o], e)
% X=1
%NO
%?- nth0(X, [h,e,l,l,o], l)
% X=2
% X=3
%NO
%FAIL nth0(X, [h,e,l,l,o], z)

%?- nth0(X, [h,e,l,l,o], Y)
% X=0
% Y=h
% X=1
% Y=e
% X=2
% Y=l
% X=3
% Y=l
% X=4
% Y=o

%FAIL nth1(0, [a,b,c], a)
%FAIL nth1(2, [a,b,c], a)
%FAIL nth1(4, [a,b,c], a)

%?- nth1(1, [a,b,c], X)
% X=a
%?- nth1(2, [a,b,c], X)
% X=b
%?- nth1(3, [a,b,c], X)
% X=c

%FAIL nth1(-1, [a,b,c], X)
%FAIL nth1(0, [a,b,c], X)
%FAIL nth1(4, [a,b,c], X)

%?- nth1(X, [h,e,l,l,o], e)
% X=2
%NO
%?- nth1(X, [h,e,l,l,o], l)
% X=3
% X=4
%NO
%FAIL nth1(X, [h,e,l,l,o], z)

%?- nth1(X, [h,e,l,l,o], Y)
% X=1
% Y=h
% X=2
% Y=e
% X=3
% Y=l
% X=4
% Y=l
% X=5
% Y=o

% Note: "nth" is a synonym for "nth1".
%TRUE nth(2, [a,b,c], b)

%FAIL nth0(1, [h,e,l,l,o|Y], l)
%FAIL nth1(1, [h,e,l,l,o|Y], l)

%?- nth0(X, [h,e,l,l,o|Y], l)
% X=2
% Y=UNINSTANTIATED VARIABLE
% X=3
% Y=UNINSTANTIATED VARIABLE
% X=5
% Y=[l|_5]
% X=6
% Y=[_6,l|_5]
% X=7
% Y=[_7,_6,l|_5]
%QUIT

%?- nth1(X, [h,e,l,l,o|Y], l)
% X=3
% Y=UNINSTANTIATED VARIABLE
% X=4
% Y=UNINSTANTIATED VARIABLE
% X=6
% Y=[l|_6]
% X=7
% Y=[_7,l|_6]
% X=8
% Y=[_8,_7,l|_6]
%QUIT

%?- nth0(8,[a,b,c|X],Y)
% X=[E4,E3,E2,E1,E0,Y|T]
% Y=UNINSTANTIATED VARIABLE

%?- nth1(8,[a,b,c|X],Y)
% X=[E3,E2,E1,E0,Y|T]
% Y=UNINSTANTIATED VARIABLE

%?- nth0(X,[a,b,c|Y],Z)
% X=0
% Y=UNINSTANTIATED VARIABLE
% Z=a
% X=1
% Y=UNINSTANTIATED VARIABLE
% Z=b
% X=2
% Y=UNINSTANTIATED VARIABLE
% Z=c
% X=3
% Y=[Z|_3]
% Z=UNINSTANTIATED VARIABLE
% X=4
% Y=[_4,Z|_3]
% Z=UNINSTANTIATED VARIABLE
% X=5
% Y=[_5,_4,Z|_3]
% Z=UNINSTANTIATED VARIABLE
%QUIT

%?- nth1(X,[a,b,c|Y],Z)
% X=1
% Y=UNINSTANTIATED VARIABLE
% Z=a
% X=2
% Y=UNINSTANTIATED VARIABLE
% Z=b
% X=3
% Y=UNINSTANTIATED VARIABLE
% Z=c
% X=4
% Y=[Z|_4]
% Z=UNINSTANTIATED VARIABLE
% X=5
% Y=[_5,Z|_4]
% Z=UNINSTANTIATED VARIABLE
% X=6
% Y=[_6,_5,Z|_4]
% Z=UNINSTANTIATED VARIABLE
%QUIT

%?- nth0(X,Y,Z)
% X=0
% Y=[Z|_0]
% Z=UNINSTANTIATED VARIABLE
% X=1
% Y=[_1,Z|_0]
% Z=UNINSTANTIATED VARIABLE
% X=2
% Y=[_2,_1,Z|_0]
% Z=UNINSTANTIATED VARIABLE
%QUIT

%?- nth1(X,Y,Z)
% X=1
% Y=[Z|_1]
% Z=UNINSTANTIATED VARIABLE
% X=2
% Y=[_2,Z|_1]
% Z=UNINSTANTIATED VARIABLE
% X=3
% Y=[_3,_2,Z|_1]
% Z=UNINSTANTIATED VARIABLE
%QUIT
*/
/**
 * <code>nth0(X,Y,Z)</code> / <code>nth1(X,Y,Z)</code> - examines an element of a list.
 * <p>
 * Indexing starts at 0 when using <code>nth0</code>. Indexing starts at 1 when using <code>nth1</code>.
 * </p>
 */
public class Nth : AbstractPredicateFactory
{
    public static Nth Nth0() => new (0);
    public static Nth Nth1() => new (1);

    private readonly int startingIdx;

    private Nth(int startingIdx) => this.startingIdx = startingIdx;


    protected override Predicate GetPredicate(Term index, Term list, Term element) 
        => index.Type.IsVariable
            ? new Retryable(index, list, element, startingIdx)
            : PredicateUtils.ToPredicate(Evaluate(TermUtils.ToInt(index), list, element));

    private bool Evaluate(int index, Term list, Term element)
    {
        var current = list;
        int requiredIdx = index - startingIdx;
        int currentIdx = 0;
        while (current.Type == TermType.LIST)
        {
            if (currentIdx == requiredIdx)
                return element.Unify(current.GetArgument(0));
            current = current.GetArgument(1);
            currentIdx++;
        }

        if (current == EmptyList.EMPTY_LIST)
            return false;
        else if (current.Type.IsVariable)
        {
            int requiredLength = requiredIdx - currentIdx;
            if (requiredLength > 0)
            {
                var term = new LinkedTermList(element, new Variable("T"));
                for (int i = 0; i < requiredLength; i++)
                    term = new LinkedTermList(new Variable("E" + i), term);
                current.Unify(term);
                return true;
            }
        }
        return false;
    }

    public class Retryable : Predicate
    {
        readonly Term index;
        Term list;
        readonly Term element;
        int ctr;

        public Retryable(Term index, Term list, Term element,int startingIdx)
        {
            this.index = index;
            this.list = list;
            this.element = element;
            this.ctr = startingIdx;
        }


        public virtual bool Evaluate()
        {
            while (list.Type == TermType.LIST)
            {
                var oldList = list.Term;
                Backtrack(index, list, element);
                if (list.Type.IsVariable)
                {
                    var newList = new LinkedTermList(new Variable("_" + ctr), oldList);
                    list.Unify(newList);
                    index.Unify(IntegerNumberCache.ValueOf(ctr++));
                    return true;
                }

                var head = list.GetArgument(0);
                list = list.GetArgument(1);

                if (element.Unify(head))
                {
                    index.Unify(IntegerNumberCache.ValueOf(ctr++));
                    return true;
                }
                else
                {
                    ctr++;
                }
            }

            if (list.Type.IsVariable)
            {
                Backtrack(index, list, element);

                var tail = new Variable("_" + ctr);
                var newList = new LinkedTermList(element, tail);
                list.Unify(newList);
                index.Unify(IntegerNumberCache.ValueOf(ctr++));
                return true;
            }

            return false;
        }

        //TODO add to TermUtils (plus 1 and 2 args versions)
        private static void Backtrack(Term index, Term list, Term element)
        {
            index.Backtrack();
            list.Backtrack();
            element.Backtrack();
        }


        public virtual bool CouldReevaluationSucceed 
            => list.Type == TermType.LIST || list.Type.IsVariable;
    }
}
