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
using Org.NProlog.Core.Kb;
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Predicate.Udp;

/**
 * Provides support for Definite Clause Grammars (DCG).
 * <p>
 * DCGs provide a convenient way to express grammar rules.
 */
public class DefiniteClauseGrammerConvertor
{
    public static bool IsDCG(Term? dcgTerm) 
        => dcgTerm?.Type == TermType.STRUCTURE && dcgTerm.NumberOfArguments == 2 && dcgTerm.Name.Equals("-->");

    /**
     * @param dcgTerm predicate with name "-=>" and two arguments
     */
    public static Term Convert(Term dcgTerm)
    {
        if (IsDCG(dcgTerm) == false)
        {
            throw new PrologException("Expected two argument predicate named \"-->\" but got: " + dcgTerm);
        }

        var consequent = GetConsequent(dcgTerm);
        var antecedent = GetAntecedent(dcgTerm);
        // slightly inefficient as will have already converted to an array in validate method
        var antecedents = KnowledgeBaseUtils.ToArrayOfConjunctions(antecedent);

        return HasSingleListWithSingleAtomElement(antecedents)
            ? convertSingleListTermAntecedent(consequent, antecedents[0])
            : ConvertConjunctionOfAtomsAntecedent(consequent, antecedents);
    }

    private static Term convertSingleListTermAntecedent(Term consequent, Term antecedent)
    {
        var consequentName = consequent.Name;
        var variable = new Variable("A");
        var list = ListFactory.CreateList(antecedent.GetArgument(0), variable);
        var args = new Term[consequent.NumberOfArguments + 2];
        for (int i = 0; i < consequent.NumberOfArguments; i++)
        {
            args[i] = consequent.GetArgument(i);
        }
        args[^2] = list;
        args[^1] = variable;
        return Structure.CreateStructure(consequentName, args);
    }

    // TODO this method is too long - refactor
    private static Term ConvertConjunctionOfAtomsAntecedent(Term consequent, Term[] conjunctionOfAtoms)
    {
        List<Term> newSequence = new();

        var lastArg = new Variable("A0");

        int varctr = 1;
        Term previous = lastArg;
        Term previousList = null;
        for (int i = conjunctionOfAtoms.Length - 1; i > -1; i--)
        {
            var term = conjunctionOfAtoms[i];
            if (term.Name.Equals("{"))
            {
                var newAntecedentArg = term.GetArgument(0).GetArgument(0);
                newSequence.Insert(0, newAntecedentArg);
            }
            else if (term.Type == TermType.LIST)
            {
                if (previousList != null)
                    term = AppendToEndOfList(term, previousList);
                previousList = term;
            }
            else
            {
                if (previousList != null)
                {
                    var _next = new Variable("A" + (varctr++));
                    var _newAntecedentArg = Structure.CreateStructure("=", new Term[] { _next, AppendToEndOfList(previousList, previous) });
                    newSequence.Insert(0, _newAntecedentArg);
                    previousList = null;
                    previous = _next;
                }

                var next = new Variable("A" + (varctr++));
                var newAntecedentArg = CreateNewPredicate(term, next, previous);
                previous = next;
                newSequence.Insert(0, newAntecedentArg);
            }
        }

        Term newAntecedent;
        if (newSequence.Count == 0)
            newAntecedent = null;
        else
        {
            newAntecedent = newSequence[(0)];
            for (int i = 1; i < newSequence.Count; i++)
            {
                newAntecedent = Structure.CreateStructure(KnowledgeBaseUtils.CONJUNCTION_PREDICATE_NAME, new Term[] { newAntecedent, newSequence[(i)] });
            }
        }

        if (previousList != null)
            previous = AppendToEndOfList(previousList, previous);
        var newConsequent = CreateNewPredicate(consequent, previous, lastArg);

        return newAntecedent == null
            ? newConsequent
            : Structure.CreateStructure(KnowledgeBaseUtils.IMPLICATION_PREDICATE_NAME, new Term[] { newConsequent, newAntecedent });
    }

    private static Term AppendToEndOfList(Term list, Term newTail)
    {
        List<Term> terms = new();
        while (list.Type == TermType.LIST)
        {
            terms.Add(list.GetArgument(0));
            list = list.GetArgument(1);
        }
        return ListFactory.CreateList(terms.ToArray(), newTail);
    }

    private static Term CreateNewPredicate(Term original, Term previous, Term next)
    {
        var args = new Term[original.NumberOfArguments + 2];
        for (int a = 0; a < original.NumberOfArguments; a++)
        {
            args[a] = original.GetArgument(a);
        }
        args[original.NumberOfArguments] = previous;
        args[original.NumberOfArguments + 1] = next;
        return Structure.CreateStructure(original.Name, args);
    }

    private static Term GetConsequent(Term dcgTerm) => dcgTerm.GetArgument(0);

    private static Term GetAntecedent(Term dcgTerm) => dcgTerm.GetArgument(1);

    private static bool HasSingleListWithSingleAtomElement(Term[] terms) => terms.Length == 1
               && terms[0].Type == TermType.LIST
               && terms[0].GetArgument(0).Type == TermType.ATOM
               && terms[0].GetArgument(1).Type == TermType.EMPTY_LIST
        ;
}
