/*
 * Copyright 2022 S. Webber
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
using System.Linq.Expressions;

namespace Org.NProlog.Core.Predicate.Builtin.Clp;

/** A {@code Term} that could represent a number of possible numeric values. */
public class ClpVariable : Numeric, LeafExpression
{
    private static readonly int TRUE = 1;
    private static readonly int FALSE = 0;

    private ClpVariable child;
    private readonly VariableState state;
    private readonly List<Constraint> rules;

    public ClpVariable()
    {
        this.state = new VariableState();
        this.rules = new();
    }

    public ClpVariable(ClpVariable parent)
    {
        this.state = parent.state.copy();
        this.rules = new(parent.rules);
    }

    public ClpVariable(VariableState state, ICollection<Constraint> rules)
    {
        this.state = state;
        this.rules = new(rules);
    }

    public List<Constraint> Constraints
    {
        get
        {
            if (child != null)
            {
                throw new InvalidOperationException();
            }
            return new(rules);
        }
    }

    public void addConstraint(Constraint c)
    {
        if (child != null)
        {
            throw new InvalidOperationException();
        }
        rules.Add(c);
    }

    public VariableState State => Term.state;

    public ClpVariable Copy()
    {
        if (child != null)
        {
            throw new InvalidOperationException();
        }
        ClpVariable copy = new ClpVariable(this);
        this.child = copy;
        return copy;
    }


    public string Name => throw new InvalidOperationException();


    public Term[] Args => throw new InvalidOperationException();


    public int NumberOfArguments => 0;


    public Term GetArgument(int index)
    {
        throw new InvalidOperationException();
    }


    public TermType Type => State.IsSingleValue ? TermType.INTEGER : TermType.CLP_VARIABLE;


    public bool IsImmutable => child == null && !state.IsCorrupt && state.IsSingleValue;


    public ClpVariable Copy(Dictionary<Variable, Variable> sharedVariables)
    {
        ClpVariable t = Term;
        if (t.IsImmutable)
        {
            return t;
        }
        else
        {
            // TODO is there a better alternative to throwing an exception?
            throw new PrologException(TermType.CLP_VARIABLE + " does not support copy, so is not suitable for use in this scenario");
        }
    }


    public ClpVariable Term
    {
        get
        {
            ClpVariable c = this;
            while (c.child != null)
            {
                c = c.child;
            }
            return c;
        }
    }

    public bool Unify(Term t)
    {
        return UnifyClpVariable(Term, t);
    }

    private static bool UnifyClpVariable(ClpVariable a, Term b)
    {
        if (a == b)
        {
            return true;
        }
        else if (b.Type == TermType.CLP_VARIABLE)
        {
            ClpVariable other = (ClpVariable)b.Term;

            if (a.child != null || other.child != null)
            {
                throw new InvalidOperationException();
            }

            VariableState s = VariableState.and(a.state, other.state);
            if (s == null)
            {
                return false;
            }

            if (s == a.state)
            {
                other.child = a;
            }
            else if (s == other.state)
            {
                a.child = other;
            }
            else
            {
                HashSet<Constraint> newRules = new();
                newRules.UnionWith(a.rules);
                newRules.UnionWith(other.rules);
                ClpVariable newChild = new ClpVariable(s, newRules);
                a.child = newChild;
                other.child = newChild;
            }

            return true;
        }
        else if (b.Type == TermType.INTEGER)
        {
            return a.unifyLong(b);
        }
        else if (b.Type == TermType.VARIABLE)
        {
            return b.Unify(a);
        }
        else
        {
            return false;
        }
    }

    private bool unifyLong(Term t)
    {
        long value = TermUtils.CastToNumeric(t).Long;
        ClpVariable copy = Copy();
        CoreConstraintStore environment = new CoreConstraintStore();
        if (copy.setMin(environment, value) == ExpressionResult.INVALID || copy.setMax(environment, value) == ExpressionResult.INVALID)
        {
            return false;
        }
        else
        {
            return environment.resolve();
        }
    }


    public void Backtrack()
    {
        this.child = null;
    }


    public long getMin(ReadConstraintStore s)
    {
        return State.getMin();
    }


    public long getMax(ReadConstraintStore s)
    {
        return State.getMax();
    }


    public ExpressionResult setNot(ConstraintStore s, long not)
    {
        return s.setNot(this, not);
    }


    public ExpressionResult setMin(ConstraintStore s, long min)
    {
        return s.setMin(this, min);
    }


    public ExpressionResult setMax(ConstraintStore s, long max)
    {
        return s.setMax(this, max);
    }


    public ConstraintResult enforce(ConstraintStore s)
    {
        long min = getMin(s);
        long max = getMax(s);
        if (min > TRUE || max < FALSE)
        {
            throw new InvalidOperationException("Expected 0 or 1");
        }
        else if (s.setValue(this, TRUE) == ExpressionResult.INVALID)
        {
            return ConstraintResult.FAILED;
        }
        else
        {
            return ConstraintResult.MATCHED;
        }
    }


    public ConstraintResult prevent(ConstraintStore s)
    {
        long min = getMin(s);
        long max = getMax(s);
        if (min > TRUE || max < FALSE)
        {
            throw new InvalidOperationException("Expected 0 or 1");
        }
        else if (s.setValue(this, FALSE) == ExpressionResult.INVALID)
        {
            return ConstraintResult.FAILED;
        }
        else
        {
            return ConstraintResult.MATCHED;
        }
    }


    public ConstraintResult reify(ReadConstraintStore s)
    {
        long min = getMin(s);
        long max = getMax(s);

        if (min != max)
        {
            return ConstraintResult.UNRESOLVED;
        }
        else if (min == TRUE)
        {
            return ConstraintResult.MATCHED;
        }
        else if (min == FALSE)
        {
            return ConstraintResult.FAILED;
        }
        else
        {
            throw new InvalidOperationException("Expected 0 or 1 but got " + min);
        }
    }


    public void walk(Consumer<Expression> r)
    {
        r.accept(this);
    }


    public LeafExpression replace(Function<LeafExpression, LeafExpression> function)
    {
        LeafExpression r = function.apply(this);
        if (r != null)
        {
            return r;
        }
        return this;
    }


    public Numeric Calculate(Term[] args)
    {
        return this;
    }


    public long Long
    {
        get
        {
            VariableState s = State;
            if (s.isSingleValue())
            {
                return s.getMax();
            }
            else
            {
                throw new PrologException("Cannot use " + TermType.CLP_VARIABLE + " as a number as has more than one possible value: " + s);
            }
        }
    }

    public double Double => Long;


    public bool Equals(object o)
    {
        if (o == this)
        {
            return true;
        }
        else if (IsImmutable && o is Numeric)
        {
            Numeric n = (Numeric)o;
            return n.IsImmutable && n.Type == TermType.INTEGER && state.getMax() == n.Long;
        }
        else
        {
            return false;
        }
    }


    public int GetHashCode()
    {
        return IsImmutable ? long.GetHashCode(state.getMax()) : base.GetHashCode();
    }


    public override string ToString()
    {
        return State.ToString();
    }

    Term Term.Copy(Dictionary<Variable, Variable> sharedVariables)
    {
        throw new NotImplementedException();
    }

    Term Term.Term => throw new NotImplementedException();
}
