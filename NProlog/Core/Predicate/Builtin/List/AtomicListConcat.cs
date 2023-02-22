/*
 * Copyright 2020 S. Webber
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
using System.Text;

namespace Org.NProlog.Core.Predicate.Builtin.List;

/* TEST
%TRUE atomic_list_concat([a,b,c], abc)
%FAIL atomic_list_concat([a,b,c], xyz)

%?- atomic_list_concat([a,b,c], X)
% X=abc

%TRUE atomic_list_concat([a,b,c], -, 'a-b-c')
%FAIL atomic_list_concat([a,b,c], 'x-y-z')

%?- atomic_list_concat([a,b,c], -, X)
% X=a-b-c

%?- atomic_list_concat(X, -, 'a-b-c')
% X=[a,b,c]

%?- atomic_list_concat([X,Y,Z], -, 'a-b-c')
% X=a
% Y=b
% Z=c

%?- atomic_list_concat([X,Y,Z], -, '6-12-9'), atom(X), atom(Y), atom(Z)
% X=6
% Y=12
% Z=9

%TRUE atomic_list_concat([], '')
%TRUE atomic_list_concat([''], -, '')

%TRUE atomic_list_concat(['1','2','3'], '123')
%FAIL atomic_list_concat(['1','2','3'], 123)

%?- atomic_list_concat([Y], X)
%ERROR Expected an atom but got: VARIABLE with value: Y

%?- atomic_list_concat([1], X)
%ERROR Expected an atom but got: INTEGER with value: 1

%?- atomic_list_concat([p(a)], X)
%ERROR Expected an atom but got: STRUCTURE with value: p(a)

%?- atomic_list_concat([[a]], X)
%ERROR Expected an atom but got: LIST with value: .(a, [])

%?- atomic_list_concat([Y], -, X)
%ERROR Expected an atom but got: VARIABLE with value: Y

%?- atomic_list_concat([1], -, X)
%ERROR Expected an atom but got: INTEGER with value: 1

%?- atomic_list_concat([p(a)], -, X)
%ERROR Expected an atom but got: STRUCTURE with value: p(a)

%?- atomic_list_concat([[a]], -, X)
%ERROR Expected an atom but got: LIST with value: .(a, [])

%?- atomic_list_concat([a], Y, X)
%ERROR Expected an atom but got: VARIABLE with value: Y

%?- atomic_list_concat([a], 1, X)
%ERROR Expected an atom but got: INTEGER with value: 1

%?- atomic_list_concat([a], p(a), X)
%ERROR Expected an atom but got: STRUCTURE with value: p(a)

%?- atomic_list_concat([a], [a], X)
%ERROR Expected an atom but got: LIST with value: .(a, [])
*/
/**
 * <code>atomic_list_concat(List,Separator,Atom)</code> / <code>atomic_list_concat(List,Atom)</code>
 * <p>
 * Concatenates the elements of <code>List</code> and attempts to unify with <code>Atom</code>. If using the 3 argument
 * version then <code>Separator</code> will be inserted between each element of <code>List</code>.
 */
public class AtomicListConcat : AbstractSingleResultPredicate
{

    protected override bool Evaluate(Term atomList, Term concatenatedResultAtom)
    {
        var list = ListUtils.ToList(atomList);
        var builder = new StringBuilder();
        foreach (var atom in list)
            builder.Append(TermUtils.GetAtomName(atom));
        return concatenatedResultAtom.Unify(new Atom(builder.ToString()));
    }


    protected override bool Evaluate(Term atomList, Term separatorAtom, Term concatenatedResultAtom)
    {
        var separator = TermUtils.GetAtomName(separatorAtom);
        return concatenatedResultAtom.Type == TermType.ATOM
            ? atomList.Unify(Split(concatenatedResultAtom, separator))
            : concatenatedResultAtom.Unify(Concat(atomList, separator));
    }

    private static Term Split(Term concatenatedResultAtom, string separator)
    {
        var concatenatedResult = TermUtils.GetAtomName(concatenatedResultAtom);
        var splitStrings = concatenatedResult.Split(separator);
        var splitTerms = new Term[splitStrings.Length];
        for (int i = 0; i < splitStrings.Length; i++)
            splitTerms[i] = new Atom(splitStrings[i]);
        return ListFactory.CreateList(splitTerms);
    }

    private static Atom Concat(Term atomList, string separator)
    {
        var list = ListUtils.ToList(atomList);
        var builder = new StringBuilder();
        for (int i = 0; i < list.Count; i++)
        {
            if (i != 0)
                builder.Append(separator);
            builder.Append(TermUtils.GetAtomName(list[(i)]));
        }
        return new Atom(builder.ToString());
    }
}
