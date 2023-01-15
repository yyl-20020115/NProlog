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

namespace Org.NProlog.Core.Predicate.Builtin.Classify;




/* TEST
%FAIL char_type(a, digit)
%TRUE char_type(a, lower)
%FAIL char_type(a, upper)
%TRUE char_type(a, alpha)
%TRUE char_type(a, alnum)
%FAIL char_type(a, white)

%FAIL char_type('A', digit)
%FAIL char_type('A', lower)
%TRUE char_type('A', upper)
%TRUE char_type('A', alpha)
%TRUE char_type('A', alnum)
%FAIL char_type('A', white)

%TRUE char_type('1', digit)
%FAIL char_type('1', lower)
%FAIL char_type('1', upper)
%FAIL char_type('1', alpha)
%TRUE char_type('1', alnum)
%FAIL char_type('1', white)

%FAIL char_type(' ', digit)
%FAIL char_type(' ', lower)
%FAIL char_type(' ', upper)
%FAIL char_type(' ', alpha)
%FAIL char_type(' ', alnum)
%TRUE char_type(' ', white)

%FAIL char_type('\\t ', digit)
%FAIL char_type('\\t', lower)
%FAIL char_type('\\t', upper)
%FAIL char_type('\\t', alpha)
%FAIL char_type('\\t', alnum)
%TRUE char_type('\\t', white)

%?- char_type(z, X)
% X=alnum
% X=alpha
% X=lower
%NO

%?- char_type(X, digit)
% X=0
% X=1
% X=2
% X=3
% X=4
% X=5
% X=6
% X=7
% X=8
% X=9
%NO

%?- char_type(X, upper)
% X=A
% X=B
% X=C
% X=D
% X=E
% X=F
% X=G
% X=H
% X=I
% X=J
% X=K
% X=L
% X=M
% X=N
% X=O
% X=P
% X=Q
% X=R
% X=S
% X=T
% X=U
% X=V
% X=W
% X=X
% X=Y
% X=Z
%NO

%?- char_type(X, lower)
% X=a
% X=b
% X=c
% X=d
% X=e
% X=f
% X=g
% X=h
% X=i
% X=j
% X=k
% X=l
% X=m
% X=n
% X=o
% X=p
% X=q
% X=r
% X=s
% X=t
% X=u
% X=v
% X=w
% X=x
% X=y
% X=z
%NO

%?- char_type(X, alnum)
% X=0
% X=1
% X=2
% X=3
% X=4
% X=5
% X=6
% X=7
% X=8
% X=9
% X=A
% X=B
% X=C
% X=D
% X=E
% X=F
% X=G
% X=H
% X=I
% X=J
% X=K
% X=L
% X=M
% X=N
% X=O
% X=P
% X=Q
% X=R
% X=S
% X=T
% X=U
% X=V
% X=W
% X=X
% X=Y
% X=Z
% X=a
% X=b
% X=c
% X=d
% X=e
% X=f
% X=g
% X=h
% X=i
% X=j
% X=k
% X=l
% X=m
% X=n
% X=o
% X=p
% X=q
% X=r
% X=s
% X=t
% X=u
% X=v
% X=w
% X=x
% X=y
% X=z
%NO

white_test :- char_type(X, white), write('>'), write(X), write('<'), nl, fail.
%?- white_test
%OUTPUT
%>\t<
%> <
%
%OUTPUT
%NO
*/
/**
 * <code>char_type(X,Y)</code> - classifies characters.
 * <p>
 * Succeeds if the character represented by <code>X</code> is a member of the character type represented by
 * <code>Y</code>. Supported character types are:
 * </p>
 * <ul>
 * <li><code>digit</code></li>
 * <li><code>upper</code> - upper case letter</li>
 * <li><code>lower</code> - lower case letter</li>
 * <li><code>alpha</code> - letter (upper or lower)</li>
 * <li><code>alnum</code> - letter (upper or lower) or digit</li>
 * <li><code>white</code> - whitespace</li>
 * </ul>
 */
public class CharType : AbstractPredicateFactory
{
    private static readonly Type[] EMPTY_TYPES_ARRAY = Array.Empty<Type>();
    private static readonly Atom[] ALL_CHARACTERS = new Atom[char.MaxValue + 2];
    static CharType()
    {
        for (int i = -1; i <= char.MaxValue; i++)
            ALL_CHARACTERS[i + 1] = new (CharToString(i));
        // populate CHARACTER_TYPES_MAP

        var digits = CreateSetFromRange('0', '9');
        var upper = CreateSetFromRange('A', 'Z');
        var lower = CreateSetFromRange('a', 'z');

        AddType("alnum", digits, upper, lower);
        AddType("alpha", upper, lower);
        AddType("digit", digits);
        AddType("upper", upper);
        AddType("lower", lower);
        AddType("white", IntsToStrings('\t', ' '));

        CHARACTER_TYPES_ARRAY = CHARACTER_TYPES_MAP.Values.ToArray();
    }
    private static readonly Dictionary<PredicateKey, Type> CHARACTER_TYPES_MAP = new();
    private static readonly Type[] CHARACTER_TYPES_ARRAY;

    /** @see GetChar#toString(int) */
    private static string CharToString(int c) => c == '\t' ? "\\t" : c.ToString();


    private static void AddType(string id, params HashSet<string>[] charIdxs)
    {
        HashSet<string> superSet = new();
        foreach (var s in charIdxs)
            superSet.UnionWith(s);
        AddType(id, superSet);
    }

    private static void AddType(string id, HashSet<string> charIdxs)
    {
        var a = new Atom(id);
        var key = PredicateKey.CreateForTerm(a);
        var type = new Type(a, charIdxs);
        CHARACTER_TYPES_MAP.Add(key, type);
    }

    private static HashSet<string> CreateSetFromRange(int from, int to) => IntsToStrings(CreateRange(from, to));

    private static int[] CreateRange(int from, int to)
    {
        var Length = to - from + 1; // +1 to be inclusive
        var result = new int[Length];
        for (int i = 0; i < Length; i++)
            result[i] = from + i;
        return result;
    }

    private static HashSet<string> IntsToStrings(params int[] ints)
    {
        HashSet<string> strings = new();
        // +1 as "end of file" (-1) is stored at idx 0
        foreach (int i in ints)
            strings.Add(ALL_CHARACTERS[i + 1].Name);
        return strings;
    }


    protected override Predicate GetPredicate(Term character, Term type)
    {
        var characters = character.Type.isVariable ? ALL_CHARACTERS : (new Term[] { character });
        var characterTypes = Array.Empty<Type>();
        if (type.Type.isVariable)
            characterTypes = CHARACTER_TYPES_ARRAY;
        else
        {
            var key = PredicateKey.CreateForTerm(type);
            if (CHARACTER_TYPES_MAP.TryGetValue(key, out var t))
                characterTypes = new Type[] { t };
            else
            {
                characters = TermUtils.EMPTY_ARRAY;
                characterTypes = EMPTY_TYPES_ARRAY;
            }
        }
        return new CharTypePredicate(character, type, new State(characters, characterTypes));
    }

    public class CharTypePredicate : Predicate
    {
        private readonly Term character;
        private readonly Term type;
        private readonly State state;

        public CharTypePredicate(Term character, Term type, State state)
        {
            this.character = character;
            this.type = type;
            this.state = state;
        }


        public bool Evaluate()
        {
            while (state.HasNext)
            {
                state.Next();
                character.Backtrack();
                type.Backtrack();
                if (character.Unify(state.Character) && state.Type.Unify(character, type))
                    return true;
            }
            return false;
        }


        public bool CouldReevaluationSucceed => state.HasNext;
    }

    public class State
    {
        readonly Term[] characters;
        readonly Type[] characterTypes;
        int characterCtr = 0;
        int characterTypeCtr = -1;

        public State(Term[] characters, Type[] characterTypes)
        {
            this.characters = characters;
            this.characterTypes = characterTypes;
        }

        public bool HasNext => characterCtr + 1 < characters.Length || characterTypeCtr + 1 < characterTypes.Length;

        public void Next()
        {
            characterTypeCtr++;
            if (characterTypeCtr == characterTypes.Length)
            {
                characterTypeCtr = 0;
                characterCtr++;
            }
        }

        public Term Character => characters[characterCtr];

        public Type Type => characterTypes[characterTypeCtr];
    }

    public class Type
    {
        readonly Atom termId;
        readonly HashSet<string> characters;

        public Type(Atom termId, HashSet<string> characters)
        {
            this.termId = termId;
            this.characters = characters;
        }

        public bool Unify(Term character, Term type) 
            => characters.Contains(TermUtils.GetAtomName(character)) && type.Unify(termId);
    }
}
