/*
 * Copyright 2013-2014 S. Webber
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
using Org.NProlog.Core.Parser;
using System.Text;

namespace Org.NProlog.Core.Terms;


/**
 * A {@link Term} consisting of a functor (name) and a number of other {@link Term} arguments.
 * <p>
 * Also known as a "compound term".
 */
public class Structure : Term
{
    private readonly string functor;
    private readonly Term[] args;
    private readonly bool immutable;
    private readonly int hashCode;

    /**
     * Factory method for creating {@code Structure} instances.
     * <p>
     * The reason that {@code Structure}s have to be created via a factory method, rather than a constructor, is to
     * enforce:
     * <ul>
     * <li>structures with the functor {@code .} and two arguments are created as instances of {@link List}</li>
     * <li>no structures can be created without any arguments</li>
     * </ul>
     *
     * @param functor the name of the new term
     * @param args arguments for the new term
     * @return either a new {@link Structure} or a new {@link List}
     */
    public static Term CreateStructure(string functor, Term[] args) => args.Length == 0
            ? throw new ArgumentException("Cannot create structure with no arguments")
            : ListFactory.LIST_PREDICATE_NAME.Equals(functor) && args.Length == 2
            ? ListFactory.CreateList(args[0], args[1])
            : new Structure(functor, args, IsImmutableWith(args));

    private static bool IsImmutableWith(Term[] args)
    {
        foreach (var t in args)
            if (t.IsImmutable == false) return false;
        return true;
    }

    /**
     * Private constructor to force use of {@link #createStructure(string, Term[])}
     *
     * @param immutable is this structure immutable (i.e. are all its arguments known to be immutable)?
     */
    private Structure(string functor, Term[] args, bool immutable)
    {
        this.functor = functor;
        this.args = args;
        this.immutable = immutable;
        this.hashCode = functor.GetHashCode() + Arrays.GetHashCode(args);
    }

    /**
     * Returns the functor of this structure.
     *
     * @return the functor of this structure
     */

    public string Name => functor;


    public Term[] Args => args;


    public int NumberOfArguments => args.Length;


    public Term GetArgument(int index) => args[index];

    /**
     * Returns {@link TermType#STRUCTURE}.
     *
     * @return {@link TermType#STRUCTURE}
     */

    public TermType Type => TermType.STRUCTURE;


    public bool IsImmutable => immutable;


    public Structure Term
    {
        get
        {
            if (immutable)
            {
                return this;
            }
            else
            {
                var returnThis = true;
                var newImmutable = true;
                var newArgs = new Term[args.Length];
                for (int i = 0; i < args.Length; i++)
                {
                    newArgs[i] = args[i].Term;
                    if (newArgs[i] != args[i])
                    {
                        returnThis = false;
                    }
                    if (newArgs[i].IsImmutable == false)
                    {
                        newImmutable = false;
                    }
                }
                return returnThis ? this : new Structure(functor, newArgs, newImmutable);
            }
        }
    }

    public Structure Copy(Dictionary<Variable, Variable>? sharedVariables)
    {
        if (immutable)
        {
            return this;
        }
        else
        {
            var returnThis = true;
            var newIsImmutable = true;
            var newArgs = new Term[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                newArgs[i] = args[i].Copy(sharedVariables);
                if (newArgs[i] != args[i])
                {
                    returnThis = false;
                }
                if (newArgs[i].IsImmutable == false)
                {
                    newIsImmutable = false;
                }
            }
            return returnThis ? this : new Structure(functor, newArgs, newIsImmutable);
        }
    }


    public bool Unify(Term? t)
    {
        var tType = t?.Type;
        if (tType == TermType.STRUCTURE)
        {
            var tArgs = t?.Args;
            if (args.Length != (tArgs?.Length).GetValueOrDefault())
                return false;
            if (!functor.Equals(t?.Name))
                return false;
            for (int i = 0; i < args.Length; i++)
            {
                if (!args[i].Unify(tArgs?[i])) return false;
            }
            return true;
        }
        else
        {
            return (tType?.IsVariable).GetValueOrDefault() && (t?.Unify(this)).GetValueOrDefault();
        }
    }


    public void Backtrack()
    {
        if (!immutable)
            TermUtils.Backtrack(args);
    }


    public override bool Equals(object? o) => o == this
        || (o is Structure structure && hashCode == o.GetHashCode()
            && functor.Equals(structure.functor) && Enumerable.SequenceEqual(args, structure.args));

    public override int GetHashCode() => hashCode;

    /**
     * Returns a {@code string} representation of this term.
     * <p>
     * The value returned will consist of the structure's functor followed be a comma separated list of its arguments
     * enclosed in brackets.
     * <p>
     * Example: {@code functor(arg1, arg2, arg3)}
     */

    public override string ToString()
    {
        var builder = new StringBuilder();
        builder.Append(functor);
        builder.Append('(');
        bool first = true;
        if (args != null)
        {
            foreach (var arg in args)
            {
                if (first)
                    first = false;
                else
                    builder.Append(", ");
                builder.Append(arg);
            }
        }
        builder.Append(')');
        return builder.ToString();
    }

    Term? Term.Copy(Dictionary<Variable, Variable>? sharedVariables) => this.Copy(sharedVariables);

    Term Term.Term => this.Term;

    public Term Bound => this;
}
