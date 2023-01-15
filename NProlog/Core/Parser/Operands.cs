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
using Org.NProlog.Core.Exceptions;
using Org.NProlog.Core.Terms;
using System.Net.Http.Headers;

namespace Org.NProlog.Core.Parser;



/**
 * Collection of operands.
 * <p>
 * Prolog allows functors (names of predicates) to be defined as "operators". The use of operators allows syntax to be
 * easier to read.
 * <p>
 * Infix operators are placed between their two arguments. Prefix operators are placed before their single argument.
 * Postfix operators are placed after their single argument.
 * <p>
 * A common use of operators is in the definition of arithmetic operations. By declaring {@code is} and {@code -} as
 * infix operators we can write valid prolog syntax like {@code X is 1 + 2.} instead of {@code is(X, +(1, 2)).}
 * <p>
 * Each {@link org.projog.core.kb.KnowledgeBase} has a single unique {@code Operands} instance.
 *
 * @see KnowledgeBase#getOperands()
 */
public class Operands
{
    private readonly object syncRoot = new();

    private readonly Dictionary<string, Operand> infixOperands = new();

    private readonly Dictionary<string, Operand> prefixOperands = new();

    private readonly Dictionary<string, Operand> postfixOperands = new();

    /**
     * Adds a new operator.
     *
     * @param operandName the name of the new operator
     * @param associativityName the operators associativity (must be one of: xfx, xfy, yfx, fx, fy, xf or yf)
     * @param precedence used to specify the ordering of terms where it is not made explicit by the use of brackets
     */
    public void AddOperand(string operandName, string associativityName, int precedence)
    {
        var a = GetAssociativity(associativityName);
        var operandsMap = GetOperandsMap(a);
        lock (this.syncRoot)
        {
            if (operandsMap.ContainsKey(operandName))
            {
                if (operandsMap.TryGetValue(operandName, out var o))
                {
                    // if the operand is already registered throw an exception if the precedence is different else do nothing
                    if (o.precedence != precedence || o.associativity != a)
                    {
                        throw new PrologException("Operand: " + operandName + " with associativity: " + o.associativity + " and precedence: " + o.precedence + " already exists");
                    }
                }
            }
            else
            {
                operandsMap.Add(operandName, new Operand(a, precedence));
            }
        }
    }

    private static Associativity GetAssociativity(string associativityName)
    {
        try
        {
            var a = Associativity.ValueOf(associativityName);
            if (a == null)
                throw new ArgumentException(nameof(associativityName));
            return a;
        }
        catch (ArgumentException e)
        {
            throw new PrologException("Cannot add operand with associativity of: "
                                      + associativityName
                                      + " as the only values allowed are: "
                                      + StringUtils.ToString(Associativity.Values.Keys));
        }
    }

    private Dictionary<string, Operand> GetOperandsMap(Associativity a) => a==null? null :( a.location switch
    {
        Location.INFIX => infixOperands,
        Location.PREFIX => prefixOperands,
        Location.POSTFIX => postfixOperands,
        // the Associativity enum currently only has 3 values, all of which are included in the above switch statement - so should never get here
        _ => throw new PrologException("Do not support associativity: " + a),
    });

    /** Returns the priority (precedence/level) of the infix operator represented by {@code op}. */
    public int GetInfixPriority(string op) 
        => infixOperands.TryGetValue(op,out var p)? p.precedence : throw new NullReferenceException();

    /** Returns the priority (precedence/level) of the prefix operator represented by {@code op}. */
    public int GetPrefixPriority(string op)
        => prefixOperands.TryGetValue(op, out var p) ? p.precedence : throw new NullReferenceException();

    /** Returns the priority (precedence/level) of the postfix operator represented by {@code op}. */
    public int GetPostfixPriority(string op)
        => postfixOperands.TryGetValue(op, out var p) ? p.precedence : throw new NullReferenceException();

    /**
     * Returns {@code true} if {@code op} represents an infix operator, else {@code false}.
     */
    public bool Infix(string op) 
        => infixOperands.ContainsKey(op);

    /**
     * Returns {@code true} if {@code op} represents an infix operator with associativity of {@code yfx}, else
     * {@code false}.
     */
    public bool Yfx(string op)
        => Infix(op) && infixOperands[(op)].associativity == Associativity.yfx;

    /**
     * Returns {@code true} if {@code op} represents an infix operator with associativity of {@code xfy}, else
     * {@code false}.
     */
    public bool Xfy(string op)
        => Infix(op) && infixOperands[(op)].associativity == Associativity.xfy;

    /**
     * Returns {@code true} if {@code op} represents an infix operator with associativity of {@code xfx}, else
     * {@code false}.
     */
    public bool Xfx(string op) 
        => Infix(op) && infixOperands[(op)].associativity == Associativity.xfx;

    /**
     * Returns {@code true} if {@code op} represents a prefix operator, else {@code false}.
     */
    public bool Prefix(string op) 
        => prefixOperands.ContainsKey(op);

    /**
     * Returns {@code true} if {@code op} represents a prefix operator with associativity of {@code fx}, else
     * {@code false}.
     */
    public bool Fx(string op) => Prefix(op) && prefixOperands[(op)].associativity == Associativity.fx;

    /**
     * Returns {@code true} if {@code op} represents a prefix operator with associativity of {@code fy}, else
     * {@code false}.
     */
    public bool Fy(string op) => Prefix(op) && prefixOperands[(op)].associativity == Associativity.fy;

    /**
     * Returns {@code true} if {@code op} represents a postfix operator, else {@code false}.
     */
    public bool Postfix(string op) => postfixOperands.ContainsKey(op);

    /**
     * Returns {@code true} if {@code op} represents a postfix operator with associativity of {@code xf}, else
     * {@code false}.
     */
    public bool Xf(string op)
    {
        return Postfix(op) && postfixOperands[(op)].associativity == Associativity.xf;
    }

    /**
     * Returns {@code true} if {@code op} represents a postfix operator with associativity of {@code yf}, else
     * {@code false}.
     */
    public bool Yf(string op) => Postfix(op) && postfixOperands[(op)].associativity == Associativity.yf;

    /** Returns {@code true} if {@code commandName} represents any known operator, else {@code false}. */
    public bool IsDefined(string commandName) => Infix(commandName) || Prefix(commandName) || Postfix(commandName);

    public class Operand
    {
        public readonly Associativity associativity;

        public readonly int precedence;

        public Operand(Associativity associativity, int precedence)
        {
            this.associativity = associativity;
            this.precedence = precedence;
        }
    }

    /**
     * Associativity is used to specify rules over operators in the same expression that have the same priority.
     * <p>
     * A "y" means that the argument can contain operators of <i>the same</i> or lower level of priority than the
     * operator represented by "f", while a "x" means that the argument can <i>only</i> contain operators of a lower
     * priority.
     */
    public class Associativity
    {

        public static readonly Associativity xfx = new(Location.INFIX, nameof(xfx));
        public static readonly Associativity xfy = new(Location.INFIX, nameof(xfy));
        public static readonly Associativity yfx = new(Location.INFIX, nameof(yfx));
        public static readonly Associativity fx = new(Location.PREFIX, nameof(fx));
        public static readonly Associativity fy = new(Location.PREFIX, nameof(fy));
        public static readonly Associativity xf = new(Location.POSTFIX, nameof(xf));
        public static readonly Associativity yf = new(Location.POSTFIX, nameof(yf));
        public static readonly Dictionary<string, Associativity> Values = new()
        {
            [xfx.name] = xfx,
            [xfy.name] = xfy,
            [yfx.name] = yfx,
            [fx.name] = fx,
            [fy.name] = fy,
            [xf.name] = xf,
            [yf.name] = yf,
        };
        public static Associativity ValueOf(string associativityName) =>
            Values.TryGetValue(associativityName, out var associativity) ? associativity : null;

        public readonly Location location;
        public readonly string name;

        public Associativity(Location location,string name)
        {
            this.location = location;
            this.name = name;
        } 
    }

    public enum Location : uint
    {
        /**
         * An operator that is positioned directly <i>before</i> its single argument.
         * <p>
         * e.g. {@code - X} where {@code -} is the operator (negation) and {@code X} is its argument.
         */
        PREFIX = 0,
        /**
         * An operator that is positioned <i>between</i> its two arguments.
         * <p>
         * e.g. {@code X = 3} where {@code =} is the operator with the arguments {@code X} and {@code 3}.
         */
        INFIX = 1,
        /** An operator that is positioned directly <i>after</i> its single argument. */
        POSTFIX = 2
    }
}
