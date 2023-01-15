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
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Predicate.Builtin.Construct;



/* TEST
%?- functor(f(a,b,c(Z)),F,N)
% Z=UNINSTANTIATED VARIABLE
% F=f
% N=3

%?- functor(a+b,F,N)
% F=+
% N=2

%?- functor([a,b,c],F,N)
% F=.
% N=2

%?- functor(atom,F,N)
% F=atom
% N=0

%?- functor(X,x,0)
% X=x

%?- functor(X,x,1)
% X=x(_)

%?- functor(X,x,2)
% X=x(_, _)

%?- functor(X,x,3)
% X=x(_, _, _)

%TRUE functor(x,x,0)
%FAIL functor(x,x,3)

%TRUE functor(x(1,2,3),x,3)
%FAIL functor(x(1,2,3),y,3)
%FAIL functor(x(1,2,3),x,0)
%FAIL functor(x(1,2,3),x,1)
%FAIL functor(x(1,2,3),x,2)
%FAIL functor(x(1,2,3),x,4)

%FAIL functor([a,b,c],'.',3)
%FAIL functor([a,b,c],a,Z)

copy(Old, New) :- functor(Old, F, N), functor(New, F, N).

%?- copy(sentence(a,b), X)
% X=sentence(_, _)
*/
/**
 * <code>functor(T,F,N)</code>
 * <p>
 * Predicate <code>functor(T,F,N)</code> means "<code>T</code> is a structure with name (functor) <code>F</code> and
 * <code>N</code> number of arguments".
 * </p>
 */
public class Functor : AbstractSingleResultPredicate
{

    protected override bool Evaluate(Term term, Term functor, Term arity) => term.Type switch
    {
        var tt when tt == TermType.ATOM =>
            functor.Unify(term) && arity.Unify(IntegerNumberCache.ZERO),
        var tt when (tt == TermType.STRUCTURE || tt == TermType.LIST || tt == TermType.EMPTY_LIST) =>
            functor.Unify(new Atom(term.Name)) && arity.Unify(IntegerNumberCache.ValueOf(term.NumberOfArguments)),
        var tt when tt == TermType.VARIABLE =>
            term.Unify(CreateTerm(functor, arity)),
        _ =>
           throw new PrologException("Invalid type for first argument of Functor command: " + term.Type)
    };

    /**
     * Creates a term using the given functor (name) and arity (number of arguments).
     *
     * @param functor an atom representing the name of the term to create
     * @param arity a numeric representing the number of arguments of the term to create
     * @return if arity is 0 then an atom will be returned, else a structure will be created.
     */
    private static Term CreateTerm(Term functor, Term arity)
    {
        if (functor.Type != TermType.ATOM)
        {
            throw new PrologException("Expected atom but got: " + functor.Type + " " + functor);
        }

        int numArgs = TermUtils.ToInt(arity);
        if (numArgs == 0)
        {
            return functor;
        }
        else
        {
            var args = new Term[numArgs];
            for (int i = 0; i < numArgs; i++)
            {
                args[i] = new Variable();
            }
            return Structure.CreateStructure(functor.Name, args);
        }
    }
}
