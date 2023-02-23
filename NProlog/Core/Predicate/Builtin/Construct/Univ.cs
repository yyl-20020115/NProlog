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
%?- p(a,b,c) =.. X
% X=[p,a,b,c]

%TRUE p(a,b,c) =.. [p,a,b,c]

%FAIL p(a,b,c) =.. [p,x,y,z]

%FAIL p(a,b,c) =.. []

%?- [a,b,c,d] =.. X
% X=[.,a,[b,c,d]]

%?- [a,b,c,d] =.. [X|Y]
% X=.
% Y=[a,[b,c,d]]

%?- X =.. [a,b,c,d]
% X=a(b, c, d)

%?- X =.. [a,[b,c],d]
% X=a([b,c], d)

%?- a+b =.. X
% X=[+,a,b]

%?- a+b =.. [+, X, Y]
% X=a
% Y=b

%TRUE a =.. [a]

%FAIL a =.. [b]

%FAIL p =.. [p,x,y,z]

%FAIL p(a,b,c) =.. [p]

%?- X =.. [a]
% X=a

%?- a+b =.. '+ X Y'
%ERROR Expected second argument to be a variable or a list but got a ATOM with value: + X Y

%?- X =.. Y
%ERROR Both arguments are variables: X and: Y
*/
/**
 * <code>X=..L</code> - "univ".
 * <p>
 * The <code>X=..L</code> predicate (pronounced "univ") provides a way to obtain the arguments of a structure as a list
 * or construct a structure or atom from a list of arguments.
 * </p>
 */
public class Univ : AbstractSingleResultPredicate
{
    protected override bool Evaluate(Term arg1, Term arg2)
    {
        var argType1 = arg1.Type;
        var argType2 = arg2.Type;
        var isFirstArgumentVariable = argType1.IsVariable;
        var isFirstArgumentPredicate = argType1.IsStructure;
        var isFirstArgumentAtom = argType1 == TermType.ATOM;
        var isSecondArgumentVariable = argType2.IsVariable;
        var isSecondArgumentList = IsList(argType2);

        if (!isFirstArgumentPredicate && !isFirstArgumentVariable && !isFirstArgumentAtom)
            throw new PrologException($"Expected first argument to be a variable or a predicate but got a {argType1} with value: {arg1}");
        else if (!isSecondArgumentList && !isSecondArgumentVariable)
            throw new PrologException($"Expected second argument to be a variable or a list but got a {argType2} with value: {arg2}");
        else if (isFirstArgumentVariable && isSecondArgumentVariable)
            throw new PrologException($"Both arguments are variables: {arg1} and: {arg2}");
        else
            return isFirstArgumentPredicate || isFirstArgumentAtom ? ToList(arg1).Unify(arg2) : arg1.Unify(ToPredicate(arg2));
    }

    private static bool IsList(TermType tt) => tt == TermType.LIST || tt == TermType.EMPTY_LIST;

    private static Term ToPredicate(Term t)
    {
        if (t.GetArgument(0).Type != TermType.ATOM)
            throw new PrologException($"First argument is not an atom in list: {t}");

        var predicateName = t.GetArgument(0).Name;

        List<Term> predicateArgs = new();
        var arg = t.GetArgument(1);
        while (arg.Type == TermType.LIST)
        {
            predicateArgs.Add(arg.GetArgument(0));
            arg = arg.GetArgument(1);
        }
        if (arg.Type != TermType.EMPTY_LIST)
            predicateArgs.Add(arg);

        return predicateArgs.Count == 0 
            ? new Atom(predicateName) 
            : Structure.CreateStructure(predicateName, predicateArgs.ToArray());
    }

    private static Term ToList(Term t)
    {
        var predicateName = t.Name;
        var numArgs = t.NumberOfArguments;
        var listArgs = new Term[numArgs + 1];
        listArgs[0] = new Atom(predicateName);
        for (int i = 0; i < numArgs; i++)
            listArgs[i + 1] = t.GetArgument(i);
        return ListFactory.CreateList(listArgs);
    }
}
