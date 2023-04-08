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

namespace Org.NProlog.Core.Predicate.Builtin.Compound;

public abstract class AbstractCollectionOf : Predicate
{
    private readonly PredicateFactory factory;
    private readonly Term template;
    private readonly Term goal;
    private readonly Term bag;
    private List<Variable> variablesNotInTemplate = new();
    private IEnumerator<KeyValuePair<Key, List<Term>>>? enumerator;

    protected AbstractCollectionOf(PredicateFactory factory, Term template, Term goal, Term bag)
    {
        this.factory = factory;
        this.template = template;
        this.goal = goal;
        this.bag = bag;
    }

    public virtual bool Evaluate()
    {
        enumerator ??= Init(template, goal);

        if (enumerator.MoveNext())
        {
            template.Backtrack();
            var e = enumerator.Current;
            bag.Backtrack();
            bag.Unify(ListFactory.CreateList(e.Value));
            for (int i = 0; i < variablesNotInTemplate.Count; i++)
            {
                var v = variablesNotInTemplate[(i)];
                v.Backtrack();
                v.Unify(e.Key.terms[(i)]);
            }
            return true;
        }
        return false;
    }

    private IEnumerator<KeyValuePair<Key, List<Term>>> Init(Term template, Term goal)
    {
        variablesNotInTemplate = GetVariablesNotInTemplate(template, goal);

        var predicate = factory.GetPredicate(goal.Args);

        Dictionary<Key, List<Term>> m = new();
        if (predicate.Evaluate())
        {
            do
            {
                var key = new Key(variablesNotInTemplate);
                if (!m.TryGetValue(key,out var l))
                    m.Add(key, l = new());
                Add(l, template.Term);
            } while (HasFoundAnotherSolution(predicate));
        }

        goal.Backtrack();
        return enumerator = m.GetEnumerator();
    }

    protected abstract void Add(List<Term> l, Term t);

    private static List<Variable> GetVariablesNotInTemplate(Term template, Term goal)
    {
        var variablesInGoal = TermUtils.GetAllVariablesInTerm(goal);
        var variablesInTemplate = TermUtils.GetAllVariablesInTerm(template);
        variablesInGoal.ExceptWith(variablesInTemplate);
        return new(variablesInGoal);
    }

    private static bool HasFoundAnotherSolution(Predicate predicate) 
        => predicate.CouldReevaluationSucceed && predicate.Evaluate();

    public bool CouldReevaluationSucceed => enumerator == null;// || enumerator.Current!=null;

    /** Represents a combination of possible values for the variables contained in the goal. */
    public class Key
    {
       public readonly List<Term> terms;

        public Key(List<Variable> variables)
        {
            terms = new(variables.Count);
            foreach (var v in variables)
                terms.Add(v.Term);
        }


        public override bool Equals(object? o)
        {
            if(o is Key k)
            {
                for (int i = 0; i < terms.Count; i++)
                    if (!TermUtils.TermsEqual(terms[(i)], k.terms[(i)]))
                        return false;
                return true;
            }
            return false;
        }

        public override int GetHashCode() => 0;
        // TODO is it possible to improve on returning the same hashCode for all instances?
    }
}
