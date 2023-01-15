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
using Org.NProlog.Core.Math;
using Org.NProlog.Core.Terms;
using System.Text;

namespace Org.NProlog.Core.Predicate.Builtin.Construct;


/* TEST
%?- atom_chars(X,[a,p,p,l,e])
% X=apple

%?- atom_chars(X,[97,112,112,108,101])
% X=apple

%?- atom_chars(apple,X)
% X=[a,p,p,l,e]

%TRUE atom_chars(apple,[a,p,p,l,e])

%FAIL atom_chars(apple,[97,112,112,108,101])

%TRUE atom_chars('APPLE',['A','P','P','L','E'])

%FAIL atom_chars(apple,[a,112,p,108,101])

%FAIL atom_chars(apple,[a,p,l,l,e])

%FAIL atom_chars(apple,[a,p,p,l,e,s])

%FAIL atom_chars(apple,[a,112,p,108,102])

%FAIL atom_chars('APPLE',[a,p,p,l,e])

%FAIL atom_chars('apple',['A','P','P','L','E'])

%?- atom_chars(apple,[X,Y,Y,Z,e])
% X=a
% Y=p
% Z=l

%FAIL atom_chars(apple,[X,Y,Z,Z,e])

%?- atom_chars(X,'apple')
%ERROR As the first argument: X is a variable the second argument needs to be a list but was: apple of type: ATOM

%?- atom_codes(X,[a,p,p,l,e])
% X=apple

%?- atom_codes(X,[97,112,112,108,101])
% X=apple

%FAIL atom_codes(apple,[a,p,p,l,e])

%TRUE atom_codes(apple,[97,112,112,108,101])

%?- atom_codes(apple,X)
% X=[97,112,112,108,101]

%TRUE atom_codes('APPLE',[65,80,80,76,69])

%FAIL atom_codes(apple,[a,112,p,108,101])

%FAIL atom_codes(apple,[97,112,108,108,101])

%FAIL atom_codes(apple,[97,112,112,108,101,102])

%FAIL atom_codes(apple,[a,112,p,108,102])

%FAIL atom_codes('APPLE',[97,112,112,108,101])

%FAIL atom_codes('apple',[65,80,80,76,69])

%?- atom_codes(apple,[X,Y,Y,Z,101])
% X=97
% Y=112
% Z=108

%FAIL atom_codes(apple,[X,Y,Z,Z,101])

%?- atom_codes(X,'apple')
%ERROR As the first argument: X is a variable the second argument needs to be a list but was: apple of type: ATOM

%TRUE number_chars(-193457260, ['-', '1','9','3','4','5','7','2','6','0'])
%FAIL number_chars(-193457260, [45,49,57,51,52,53,55,50,54,48])

%?- number_chars(193457260,X)
% X=[1,9,3,4,5,7,2,6,0]

%?- number_chars(-193457260,X)
% X=[-,1,9,3,4,5,7,2,6,0]

%?- number_chars(X,['1','9','3','4','5','7','2','6','0'])
% X=193457260

%?- number_chars(X,['-', '1','9','3','4','5','7','2','6','0'])
% X=-193457260

%?- number_chars(X,['o','n','e'])
%ERROR Could not convert characters to an integer: 'one'

%?- number_chars(X,1257)
%ERROR As the first argument: X is a variable the second argument needs to be a list but was: 1257 of type: INTEGER

%?- number_chars(X,['6','.','4'])
% X=6.4

%?- number_chars(X,['-','7','2','.','4','6','3'])
% X=-72.463

%?- number_chars(X,['.','4','6','3'])
% X=0.463

%?- number_chars(X,['-','.','4','6','3'])
% X=-0.463

%?- number_chars('193457260',X)
%ERROR Unexpected type for first argument: ATOM

%TRUE number_codes(-193457260, [45,49,57,51,52,53,55,50,54,48])
%FAIL number_codes(-193457260, ['-','1','9','3','4','5','7','2','6','0'])

%?- number_codes(193457260,X)
% X=[49,57,51,52,53,55,50,54,48]

%?- number_codes(-193457260,X)
% X=[45,49,57,51,52,53,55,50,54,48]

%?- number_codes(X,[49,57,51,52,53,55,50,54,48])
% X=193457260

%?- number_codes(X,[45,49,57,51,52,53,55,50,54,48])
% X=-193457260

%?- number_codes(X,['o','n','e'])
%ERROR Could not convert characters to an integer: 'one'

%?- number_codes(X,1257)
%ERROR As the first argument: X is a variable the second argument needs to be a list but was: 1257 of type: INTEGER

%?- number_codes(X,[54,46,52])
% X=6.4

%?- number_codes(X,[45,55,50,46,52,54,51])
% X=-72.463

%?- number_codes(X,[46,52,54,51])
% X=0.463

%?- number_codes(X,[45,46,52,54,51])
% X=-0.463

%?- number_codes('193457260',X)
%ERROR Unexpected type for first argument: ATOM
*/
/**
 * <code>atom_chars</code> / <code>atom_codes</code> / <code>number_chars</code> / <code>number_codes</code>
 * <p>
 * <ul>
 * <li><code>atom_chars(A,L)</code> compares the atom <code>A</code> to the list of characters <code>L</code>.</li>
 * <li><code>atom_codes(A,L)</code> compares the atom <code>A</code> to the list of character codes <code>L</code>.</li>
 * <li><code>number_chars(N,L)</code> compares the number <code>N</code> to the list of characters <code>L</code>.</li>
 * <li><code>number_codes(N,L)</code> compares the number <code>N</code> to the list of character codes
 * <code>L</code>.</li>
 * </ul>
 */
public class TermSplit : AbstractSingleResultPredicate
{
    private readonly bool firstArgNumeric;
    private readonly bool convertToCharCodes;

    public static TermSplit AtomChars() => new(false, false);

    public static TermSplit AtomCodes() => new(false, true);

    public static TermSplit NumberChars() => new(true, false);

    public static TermSplit NumberCodes() => new(true, true);

    private TermSplit(bool firstArgNumeric, bool convertToCharCodes)
    {
        this.firstArgNumeric = firstArgNumeric;
        this.convertToCharCodes = convertToCharCodes;
    }


    protected override bool Evaluate(Term arg1, Term arg2) => arg1.Type.isVariable ? EvaluateWithVariableFirstArgument(arg1, arg2) : EvaluateWithConcreteFirstArgument(arg1, arg2);

    /**
     * Converts {@code arg2} from a list to an atom and attempts to unify it with {@code arg1}.
     * <p>
     * Example of a prolog query that would cause this method to be used: <pre>
     * ?- number_chars(X, [1,4,2]).
     * X = 142
     * </pre>
     *
     * @param arg1 a {@code Variable}
     * @param arg2 a {@code List}
     * @return {@code true} if was able to unify
     */
    private bool EvaluateWithVariableFirstArgument(Term arg1, Term arg2)
    {
        if (IsNotList(arg2))
            throw new PrologException("As the first argument: " + arg1 + " is a variable the second argument needs to be a list but was: " + arg2 + " of type: " + arg2.Type);
        var sb = new StringBuilder();
        AppendListElementsToString(sb, arg2);
        var t = ToTerm(sb.ToString());
        return arg1.Unify(t);
    }

    /**
     * Converts {@code arg1} to a list and attempts to unify it with {@code arg2}.
     * <p>
     * Example of a prolog query that would cause this method to be used: <pre>
     * ?- atom_chars(apple, X).
     * X = [a,p,p,l,e]
     * </pre>
     *
     * @param arg1 a {@code Atom} or {@code Numeric}
     * @param arg2 in order to unify, this argument must represent a {@code Atom} or {@code List}
     * @return {@code true} if was able to unify
     */
    private bool EvaluateWithConcreteFirstArgument(Term arg1, Term arg2)
    {
        IsValidConcreteFirstArgument(arg1);
        var chars = arg1.Name.ToCharArray();
        var numChars = chars.Length;
        var listElements = new Term[numChars];
        for (int i = 0; i < numChars; i++)
        {
            listElements[i] = CharToTerm(chars[i]);
        }
        var l = ListFactory.CreateList(listElements);
        return arg2.Unify(l);
    }

    private void IsValidConcreteFirstArgument(Term arg)
    {
        var t = arg.Type;
        if (t.isNumeric) return;

        if (t == TermType.ATOM && !firstArgNumeric) return;

        throw new PrologException("Unexpected type for first argument: " + t);
    }

    private static bool IsNotList(Term t)
    {
        var tt = t.Type;
        return tt != TermType.LIST && tt != TermType.EMPTY_LIST;
    }

    private void AppendListElementsToString(StringBuilder sb, Term t)
    {
        var type = t.Type;
        if (type == TermType.LIST)
        {
            AppendListElementsToString(sb, t.GetArgument(0));
            AppendListElementsToString(sb, t.GetArgument(1));
        }
        else if (type == TermType.ATOM)
        {
            var name = t.Name;
            sb.Append(StringToChar(name));
        }
        else if (type.isNumeric)
        {
            var n = ArithmeticOperators.GetNumeric(t).Long;
            sb.Append(NumericToChar(n));
        }
        else if (type != TermType.EMPTY_LIST)
            throw new PrologException("Unexpected type in list: " + type);
    }

    private static char StringToChar(string name) => name.Length == 1 
        ? name[(0)] : throw new PrologException("Expected atom in list to have exactly one character: " + name);

    protected Term CharToTerm(char c) => convertToCharCodes ? IntegerNumberCache.ValueOf(c) : new Atom(c.ToString());

    private static char NumericToChar(long n) => (char)n;

    private Term ToTerm(string s) => firstArgNumeric ? global::Org.NProlog.Core.Predicate.Builtin.Construct.TermSplit.ToNumeric(s) : new Atom(s);

    private static Numeric ToNumeric(string s) => !s.Contains('.') ? ToInteger(s) : ToDecimal(s);

    private static IntegerNumber ToInteger(string s) => IntegerNumberCache.ValueOf(int.TryParse(s, out var i) ? i : throw new PrologException("Could not convert characters to an integer: '" + s + "'")); // TODO should this be long.parseLong

    private static Numeric ToDecimal(string s) => new DecimalFraction(double.TryParse(s, out var d) ? d : throw new PrologException("Could not convert characters to a decimal: '" + s + "'"));
}
