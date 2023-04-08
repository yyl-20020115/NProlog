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
% Examples of when all three terms are lists:
%TRUE Append([a,b,c], [d,e,f], [a,b,c,d,e,f])
%TRUE Append([a,b,c], [a,b,c], [a,b,c,a,b,c])
%TRUE Append([a], [b,c,d,e,f], [a,b,c,d,e,f])
%TRUE Append([a,b,c,d,e], [f], [a,b,c,d,e,f])
%TRUE Append([a,b,c,d,e,f], [], [a,b,c,d,e,f])
%TRUE Append([], [a,b,c,d,e,f], [a,b,c,d,e,f])
%TRUE Append([], [], [])
%FAIL Append([a,b], [d,e,f], [a,b,c,d,e,f])
%FAIL Append([a,b,c], [e,f], [a,b,c,d,e,f])
%?- Append([W,b,c], [d,Y,f], [a,X,c,d,e,Z])
% W=a
% X=b
% Y=e
% Z=f

% Examples of when first term is a variable:
%?- Append(X, [d,e,f], [a,b,c,d,e,f])
% X=[a,b,c]
%NO
%?- Append(X, [f], [a,b,c,d,e,f])
% X=[a,b,c,d,e]
%NO
%?- Append(X, [b,c,d,e,f], [a,b,c,d,e,f])
% X=[a]
%NO
%?- Append(X, [a,b,c,d,e,f], [a,b,c,d,e,f])
% X=[]
%NO
%?- Append(X, [], [a,b,c,d,e,f])
% X=[a,b,c,d,e,f]

% Examples of when second term is a variable:
%?- Append([a,b,c], X, [a,b,c,d,e,f])
% X=[d,e,f]
%?- Append([a,b,c,d,e], X, [a,b,c,d,e,f])
% X=[f]
%?- Append([a], X, [a,b,c,d,e,f])
% X=[b,c,d,e,f]
%?- Append([], X, [a,b,c,d,e,f])
% X=[a,b,c,d,e,f]
%?- Append([a,b,c,d,e,f], X, [a,b,c,d,e,f])
% X=[]

% Examples of when third term is a variable:
%?- Append([a,b,c], [d,e,f], X)
% X=[a,b,c,d,e,f]
%?- Append([a], [b,c,d,e,f], X)
% X=[a,b,c,d,e,f]
%?- Append([a,b,c,d,e], [f], X)
% X=[a,b,c,d,e,f]
%?- Append([a,b,c,d,e,f], [], X)
% X=[a,b,c,d,e,f]
%?- Append([], [a,b,c,d,e,f], X)
% X=[a,b,c,d,e,f]
%?- Append([], [], X)
% X=[]

% Examples of when first and second terms are variables:
%?- Append(X, Y, [a,b,c,d,e,f])
% X=[]
% Y=[a,b,c,d,e,f]
% X=[a]
% Y=[b,c,d,e,f]
% X=[a,b]
% Y=[c,d,e,f]
% X=[a,b,c]
% Y=[d,e,f]
% X=[a,b,c,d]
% Y=[e,f]
% X=[a,b,c,d,e]
% Y=[f]
% X=[a,b,c,d,e,f]
% Y=[]
%?- Append(X, Y, [a])
% X=[]
% Y=[a]
% X=[a]
% Y=[]
%?- Append(X, Y, [])
% X=[]
% Y=[]

% Examples when combination of term types cause failure:
%FAIL Append(a, b, Z)
%FAIL Append(a, b, c)
%FAIL Append(a, [], [])
%FAIL Append([], b, [])
%FAIL Append([], [], c)

%?- Append([], tail, Z)
% Z=tail

%?- Append([], Z, tail)
% Z=tail

%?- Append([a], b, X)
% X=[a|b]

%?- Append([a,b,c], d, X)
% X=[a,b,c|d]

%?- Append([a], [], X)
% X=[a]

%?- Append([a], [b], X)
% X=[a,b]

%?- Append([X|Y],['^'],[a,b,c,^])
% X=a
% Y=[b,c]
%NO

%FAIL Append([X|Y],['^'],[a,b,c,^,z])

%?- Append([X|Y],['^'],[a,b,c,^,z,^])
% X=a
% Y=[b,c,^,z]
%NO

%?- Append([X|Y],['^'],[a,b,c,^,^])
% X=a
% Y=[b,c,^]
%NO

%FAIL Append([a|b], [b|c], X)
%FAIL Append([a|b], [b|c], [a,b,c,d])
%FAIL Append([a|b], X, [a,b,c,d])
%FAIL Append(X, [b|c], [a,b,c,d])
%FAIL Append([a|b], X, Y)

%FAIL Append([a, a], X, [a])
%FAIL Append(X,[a,a],[a])
%FAIL Append([a,a],X,[])
%FAIL Append(X,[a,a],[])

%?- Append(X,[a,a],[a,a])
% X=[]
%NO
%?- Append([a,a],X,[a,a])
% X=[]
%?- Append(X,[],[a,a])
% X=[a,a]
%?- Append([],X,[a,a])
% X=[a,a]
%?- Append([],[],X)
% X=[]

%?- Append(Left,[x|Right],[a,x,b,c,d,x,e,f])
% Left=[a]
% Right=[b,c,d,x,e,f]
% Left=[a,x,b,c,d]
% Right=[e,f]
%NO

%?- Append(Left,[x,b|Right],[a,x,b,c,d,x,e,f])
% Left=[a]
% Right=[c,d,x,e,f]
%NO

%?- Append([a|X],[a|Y],[a,a,a,a,a,a,a])
% X=[]
% Y=[a,a,a,a,a]
% X=[a]
% Y=[a,a,a,a]
% X=[a,a]
% Y=[a,a,a]
% X=[a,a,a]
% Y=[a,a]
% X=[a,a,a,a]
% Y=[a]
% X=[a,a,a,a,a]
% Y=[]
%NO

%?- Append([a,a|X],[a|Y],[a,a,a,a,a,a,a])
% X=[]
% Y=[a,a,a,a]
% X=[a]
% Y=[a,a,a]
% X=[a,a]
% Y=[a,a]
% X=[a,a,a]
% Y=[a]
% X=[a,a,a,a]
% Y=[]
%NO

%?- Append([a|X],[Y|[a]],[a,a,a,a,a,a,a])
% X=[a,a,a,a]
% Y=a
%NO

%?- Append([X|[a]],Y,[a,a,a,a,a,a,a])
% X=a
% Y=[a,a,a,a,a]

%FAIL Append([X|[a]],[Y|[a]],[a,a,a,a,a,a,a])

%?- Append([a,a,a],[a,a,a],[a|X])
% X=[a,a,a,a,a]

%?- Append([a,b|X],[d,e|Y],Z)
% X=[]
% Y=UNINSTANTIATED VARIABLE
% Z=[a,b,d,e|Y]
% X=[X]
% Y=UNINSTANTIATED VARIABLE
% Z=[a,b,X,d,e|Y]
% X=[X,X]
% Y=UNINSTANTIATED VARIABLE
% Z=[a,b,X,X,d,e|Y]
% X=[X,X,X]
% Y=UNINSTANTIATED VARIABLE
% Z=[a,b,X,X,X,d,e|Y]
%QUIT

%?- Append(X,Y,Z)
% X=[]
% Y=UNINSTANTIATED VARIABLE
% Z=UNINSTANTIATED VARIABLE
% X=[X]
% Y=UNINSTANTIATED VARIABLE
% Z=[X|L3]
% X=[X,X]
% Y=UNINSTANTIATED VARIABLE
% Z=[X,X|L3]
% X=[X,X,X]
% Y=UNINSTANTIATED VARIABLE
% Z=[X,X,X|L3]
%QUIT

%?- Append(X,[],Z)
% X=[]
% Z=[]
% X=[X]
% Z=[X]
% X=[X,X]
% Z=[X,X]
% X=[X,X,X]
% Z=[X,X,X]
%QUIT

%?- Append(X,[b|c],Z)
% X=[]
% Z=[b|c]
% X=[X]
% Z=[X,b|c]
% X=[X,X]
% Z=[X,X,b|c]
% X=[X,X,X]
% Z=[X,X,X,b|c]
%QUIT

%?- Append([a,b|X],[d,e|Y],[a|Z])
% X=[]
% Y=UNINSTANTIATED VARIABLE
% Z=[b,d,e|Y]
% X=[X]
% Y=UNINSTANTIATED VARIABLE
% Z=[b,X,d,e|Y]
% X=[X,X]
% Y=UNINSTANTIATED VARIABLE
% Z=[b,X,X,d,e|Y]
% X=[X,X,X]
% Y=UNINSTANTIATED VARIABLE
% Z=[b,X,X,X,d,e|Y]
%QUIT

%?- Append([a,b|X],[d,e|Y],[a,b|Z])
% X=[]
% Y=UNINSTANTIATED VARIABLE
% Z=[d,e|Y]
% X=[X]
% Y=UNINSTANTIATED VARIABLE
% Z=[X,d,e|Y]
% X=[X,X]
% Y=UNINSTANTIATED VARIABLE
% Z=[X,X,d,e|Y]
% X=[X,X,X]
% Y=UNINSTANTIATED VARIABLE
% Z=[X,X,X,d,e|Y]
%QUIT

%?- Append([a,b|X],[d,e|Y],[a,b,c|Z])
% X=[c]
% Y=UNINSTANTIATED VARIABLE
% Z=[d,e|Y]
% X=[c,X]
% Y=UNINSTANTIATED VARIABLE
% Z=[X,d,e|Y]
% X=[c,X,X]
% Y=UNINSTANTIATED VARIABLE
% Z=[X,X,d,e|Y]
% X=[c,X,X,X]
% Y=UNINSTANTIATED VARIABLE
% Z=[X,X,X,d,e|Y]
%QUIT

%?- Append([a|X],Y,Z)
% X=[]
% Y=UNINSTANTIATED VARIABLE
% Z=[a|L3]
% X=[X]
% Y=UNINSTANTIATED VARIABLE
% Z=[a,X|L3]
% X=[X,X]
% Y=UNINSTANTIATED VARIABLE
% Z=[a,X,X|L3]
% X=[X,X,X]
% Y=UNINSTANTIATED VARIABLE
% Z=[a,X,X,X|L3]
%QUIT

%?- Append([a|X],[z],Z)
% X=[]
% Z=[a,z]
% X=[X]
% Z=[a,X,z]
% X=[X,X]
% Z=[a,X,X,z]
% X=[X,X,X]
% Z=[a,X,X,X,z]
%QUIT

%?- Append([a|X],z,Z)
% X=[]
% Z=[a|z]
% X=[X]
% Z=[a,X|z]
% X=[X,X]
% Z=[a,X,X|z]
% X=[X,X,X]
% Z=[a,X,X,X|z]
%QUIT

%?- Append([a|X],[z|Y],Z)
% X=[]
% Y=UNINSTANTIATED VARIABLE
% Z=[a,z|Y]
% X=[X]
% Y=UNINSTANTIATED VARIABLE
% Z=[a,X,z|Y]
% X=[X,X]
% Y=UNINSTANTIATED VARIABLE
% Z=[a,X,X,z|Y]
% X=[X,X,X]
% Y=UNINSTANTIATED VARIABLE
% Z=[a,X,X,X,z|Y]
%QUIT

%FAIL Append(a,b,Z)
%FAIL Append([a],b,c)
%FAIL Append(a,b,c)

%?- Append([a],b,Z)
% Z=[a|b]

%?- Append(X,Y,c)
% X=[]
% Y=c
%NO

%?- Append([a|X],b,Z)
% X=[]
% Z=[a|b]
% X=[X]
% Z=[a,X|b]
% X=[X,X]
% Z=[a,X,X|b]
% X=[X,X,X]
% Z=[a,X,X,X|b]
%QUIT

%?- Append([a,b,c|X],z,Z)
% X=[]
% Z=[a,b,c|z]
% X=[X]
% Z=[a,b,c,X|z]
% X=[X,X]
% Z=[a,b,c,X,X|z]
% X=[X,X,X]
% Z=[a,b,c,X,X,X|z]
%QUIT

%?- Append([a,b,c|X],[z],Z)
% X=[]
% Z=[a,b,c,z]
% X=[X]
% Z=[a,b,c,X,z]
% X=[X,X]
% Z=[a,b,c,X,X,z]
% X=[X,X,X]
% Z=[a,b,c,X,X,X,z]
%QUIT

%?- Append([a,b,c|X],[],Z)
% X=[]
% Z=[a,b,c]
% X=[X]
% Z=[a,b,c,X]
% X=[X,X]
% Z=[a,b,c,X,X]
% X=[X,X,X]
% Z=[a,b,c,X,X,X]
%QUIT

%?- Append([a],[a,b,c|X],Z)
% X=UNINSTANTIATED VARIABLE
% Z=[a,a,b,c|X]

%?- Append(X,[a,b,c|Y],Z)
% X=[]
% Y=UNINSTANTIATED VARIABLE
% Z=[a,b,c|Y]
% X=[X]
% Y=UNINSTANTIATED VARIABLE
% Z=[X,a,b,c|Y]
% X=[X,X]
% Y=UNINSTANTIATED VARIABLE
% Z=[X,X,a,b,c|Y]
% X=[X,X,X]
% Y=UNINSTANTIATED VARIABLE
% Z=[X,X,X,a,b,c|Y]
%QUIT

%?- Append(X,z,Z)
% X=[]
% Z=z
% X=[X]
% Z=[X|z]
% X=[X,X]
% Z=[X,X|z]
% X=[X,X,X]
% Z=[X,X,X|z]
%QUIT

%?- Append(X,Y,[Z|1])
% X=[]
% Y=[Z|1]
% Z=UNINSTANTIATED VARIABLE
% X=[Z]
% Y=1
% Z=UNINSTANTIATED VARIABLE
%NO

%?- Append(X,Y,[Z,b|1])
% X=[]
% Y=[Z,b|1]
% Z=UNINSTANTIATED VARIABLE
% X=[Z]
% Y=[b|1]
% Z=UNINSTANTIATED VARIABLE
% X=[Z,b]
% Y=1
% Z=UNINSTANTIATED VARIABLE
%NO

%?- Append(X,Y,[a,b,c,d|1])
% X=[]
% Y=[a,b,c,d|1]
% X=[a]
% Y=[b,c,d|1]
% X=[a,b]
% Y=[c,d|1]
% X=[a,b,c]
% Y=[d|1]
% X=[a,b,c,d]
% Y=1
%NO

%?- Append(X,Y,[a,b,c,d|Z])
% X=[]
% Y=[a,b,c,d|Z]
% Z=UNINSTANTIATED VARIABLE
% X=[a]
% Y=[b,c,d|Z]
% Z=UNINSTANTIATED VARIABLE
% X=[a,b]
% Y=[c,d|Z]
% Z=UNINSTANTIATED VARIABLE
% X=[a,b,c]
% Y=[d|Z]
% Z=UNINSTANTIATED VARIABLE
% X=[a,b,c,d]
% Y=UNINSTANTIATED VARIABLE
% Z=UNINSTANTIATED VARIABLE
% X=[a,b,c,d,X]
% Y=UNINSTANTIATED VARIABLE
% Z=[X|L3]
% X=[a,b,c,d,X,X]
% Y=UNINSTANTIATED VARIABLE
% Z=[X,X|L3]
% X=[a,b,c,d,X,X,X]
% Y=UNINSTANTIATED VARIABLE
% Z=[X,X,X|L3]
%QUIT

%?- Append([a|X],[x,y,z],[a,b,c|Z])
% X=[b,c]
% Z=[x,y,z]
% X=[b,c,X]
% Z=[X,x,y,z]
% X=[b,c,X,X]
% Z=[X,X,x,y,z]
% X=[b,c,X,X,X]
% Z=[X,X,X,x,y,z]
%QUIT

%?- Append([a,b|X],[c,d|X],Y)
% X=[]
% Y=[a,b,c,d]
% X=[X]
% Y=[a,b,X,c,d,X]
% X=[X,X]
% Y=[a,b,X,X,c,d,X,X]
% X=[X,X,X]
% Y=[a,b,X,X,X,c,d,X,X,X]
%QUIT

%?- Append([a,b|X],[c,d|X],Y), numbervars(Y)
% X=[]
% Y=[a,b,c,d]
% X=[$VAR(0)]
% Y=[a,b,$VAR(0),c,d,$VAR(0)]
% X=[$VAR(0),$VAR(1)]
% Y=[a,b,$VAR(0),$VAR(1),c,d,$VAR(0),$VAR(1)]
% X=[$VAR(0),$VAR(1),$VAR(2)]
% Y=[a,b,$VAR(0),$VAR(1),$VAR(2),c,d,$VAR(0),$VAR(1),$VAR(2)]
%QUIT
*/
/**
 * <code>Append(X,Y,Z)</code> - concatenates two lists.
 * <p>
 * The <code>Append(X,Y,Z)</code> goal succeeds if the concatenation of lists <code>X</code> and <code>Y</code> matches
 * the list <code>Z</code>.
 * </p>
 */
public class Append : AbstractPredicateFactory
{
    protected override Predicate GetPredicate(Term prefix, Term suffix, Term concatenated)
        => new AppendPredicate(prefix, suffix, concatenated);

    public class AppendPredicate : Predicate
    {
        Term prefix;
        Term suffix;
        Term concatenated;
        bool retrying;

        public AppendPredicate(Term prefix, Term suffix, Term concatenated)
        {
            this.prefix = prefix;
            this.suffix = suffix;
            this.concatenated = concatenated;
        }

        public bool Evaluate()
        {
            while (true)
            {
                // conc([],L,L).
                if (!retrying && prefix.Unify(EmptyList.EMPTY_LIST) && suffix.Unify(concatenated))
                {
                    retrying = true;
                    return true;
                }
                retrying = false;

                prefix.Backtrack();
                suffix.Backtrack();
                concatenated.Backtrack();

                //conc([X|L1],L2,[X|L3]) :- conc(L1,L2,L3).
                Term? x = null;
                Term? l1 = null;
                Term? l3 = null;
                if (prefix.Type == TermType.LIST)
                {
                    x = prefix.GetArgument(0);
                    l1 = prefix.GetArgument(1);
                }
                if (concatenated.Type == TermType.LIST)
                {
                    if (x == null)
                        x = concatenated.GetArgument(0);
                    else if (!x.Unify(concatenated.GetArgument(0)))
                        return false;
                    l3 = concatenated.GetArgument(1);
                }
                x ??= new Variable("X");
                if (prefix.Type.IsVariable)
                {
                    l1 = new Variable("L1");
                    prefix.Unify(new Terms.LinkedTermList(x, l1));
                }
                if (l1 == null)
                    return false;
                if (concatenated.Type.IsVariable)
                {
                    l3 = new Variable("L3");
                    concatenated.Unify(new Terms.LinkedTermList(x, l3));
                }
                if (l3 == null)
                    if (concatenated.Type == TermType.LIST)
                        l3 = concatenated.GetArgument(1);   
                    else
                        return false;

                prefix = l1.Term;
                suffix = suffix.Term;
                concatenated = l3.Term;
            }
        }


        public bool CouldReevaluationSucceed 
            => !retrying || (prefix != EmptyList.EMPTY_LIST && concatenated != EmptyList.EMPTY_LIST);
    }
}
