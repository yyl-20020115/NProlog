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

namespace Org.NProlog.Core.Predicate.Builtin.Construct;

/* TEST
% Examples of when all three terms are atoms:
%TRUE atom_concat(abc, def, abcdef)
%TRUE atom_concat(a, bcdef, abcdef)
%TRUE atom_concat(abcde, f, abcdef)
%TRUE atom_concat(abcdef, '', abcdef)
%TRUE atom_concat('', abcdef, abcdef)
%TRUE atom_concat('', '', '')
%FAIL atom_concat(ab, def, abcdef)
%FAIL atom_concat(abc, ef, abcdef)

% Examples of when first term is a variable:
%?- atom_concat(abc, X, abcdef)
% X=def
%?- atom_concat(abcde, X, abcdef)
% X=f
%?- atom_concat(a, X, abcdef)
% X=bcdef
%?- atom_concat('', X, abcdef)
% X=abcdef
%?- atom_concat(abcdef, X, abcdef)
% X=

% Examples of when second term is a variable:
%?- atom_concat(X, def, abcdef)
% X=abc
%?- atom_concat(X, f, abcdef)
% X=abcde
%?- atom_concat(X, bcdef, abcdef)
% X=a
%?- atom_concat(X, abcdef, abcdef)
% X=
%?- atom_concat(X, '', abcdef)
% X=abcdef

% Examples of when third term is a variable:
%?- atom_concat(abc, def, X)
% X=abcdef
%?- atom_concat(a, bcdef, X)
% X=abcdef
%?- atom_concat(abcde, f, X)
% X=abcdef
%?- atom_concat(abcdef, '', X)
% X=abcdef
%?- atom_concat('', abcdef, X)
% X=abcdef
%?- atom_concat('', '', X)
% X=

% Examples of when first and second terms are variables:
%?- atom_concat(X, Y, abcdef)
% X=
% Y=abcdef
% X=a
% Y=bcdef
% X=ab
% Y=cdef
% X=abc
% Y=def
% X=abcd
% Y=ef
% X=abcde
% Y=f
% X=abcdef
% Y=
%?- atom_concat(X, Y, a)
% X=
% Y=a
% X=a
% Y=
%?- atom_concat(X, Y, '')
% X=
% Y=

% Examples when combination of term types cause failure:
%?- atom_concat(X, Y, Z)
%ERROR Expected an atom but got: VARIABLE with value: Z
%?- atom_concat('', Y, Z)
%ERROR Expected an atom but got: VARIABLE with value: Z
%?- atom_concat(X, '', Z)
%ERROR Expected an atom but got: VARIABLE with value: Z
%FAIL atom_concat(a, b, c)
%FAIL atom_concat(a, '', '')
%FAIL atom_concat('', b, '')
%FAIL atom_concat('', '', c)
*/
/**
 * <code>atom_concat(X, Y, Z)</code> - concatenates atom names.
 * <p>
 * <code>atom_concat(X, Y, Z)</code> succeeds if the name of atom <code>Z</code> matches the concatenation of the names
 * of atoms <code>X</code> and <code>Y</code>.
 * </p>
 */
public class AtomConcat : AbstractPredicateFactory
{

    protected override Predicate GetPredicate(Term prefix, Term suffix, Term combined) => prefix.Type.isVariable && suffix.Type.isVariable
            ? new Retryable(prefix, suffix, TermUtils.GetAtomName(combined))
            : PredicateUtils.ToPredicate(Evaluate(prefix, suffix, combined));

    private bool Evaluate(Term arg1, Term arg2, Term arg3)
    { // TODO rename arguments
        AssertAtomOrVariable(arg1);
        AssertAtomOrVariable(arg2);
        AssertAtomOrVariable(arg3);

        bool isArg1Atom = IsAtom(arg1);
        bool isArg2Atom = IsAtom(arg2);
        if (isArg1Atom && isArg2Atom)
        {
            var concat = new Atom(arg1.Name + arg2.Name);
            return arg3.Unify(concat);
        }
        else
        {
            var atomName = TermUtils.GetAtomName(arg3);
            if (isArg1Atom)
            {
                var prefix = arg1.Name;
                return (atomName.StartsWith(prefix) && arg2.Unify(new Atom(atomName.Substring(prefix.Length))));
            }
            else if (isArg2Atom)
            {
                var suffix = arg2.Name;
                return (atomName.EndsWith(suffix) && arg1.Unify(new Atom(atomName.Substring(0, (atomName.Length - suffix.Length)))));
            }
            else
            {
                throw new PrologException("If third argument is not an atom then both first and second arguments must be: " + arg1 + " " + arg2 + " " + arg3);
            }
        }
    }

    private static void AssertAtomOrVariable(Term t)
    {
        var type = t.Type;
        if (type != TermType.ATOM && !type.isVariable)
        {
            throw new PrologException("Expected an atom or variable but got: " + type + " with value: " + t);
        }
    }

    private static bool IsAtom(Term t) => t.Type == TermType.ATOM;

    public class Retryable : Predicate
    {
        readonly Term arg1;
        readonly Term arg2;
        readonly string combined;
        int ctr;

        public Retryable(Term arg1, Term arg2, string combined)
        {
            this.arg1 = arg1;
            this.arg2 = arg2;
            this.combined = combined;
        }


        public virtual bool Evaluate()
        {
            while (CouldReevaluationSucceed)
            {
                arg1.Backtrack();
                arg2.Backtrack();

                Atom prefix = new Atom(combined.Substring(0, ctr));
                Atom suffix = new Atom(combined.Substring(ctr));
                ctr++;

                return arg1.Unify(prefix) && arg2.Unify(suffix);
            }
            return false;
        }


        public virtual bool CouldReevaluationSucceed => ctr <= combined.Length;
    }
}
