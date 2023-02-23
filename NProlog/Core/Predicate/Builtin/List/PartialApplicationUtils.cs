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

namespace Org.NProlog.Core.Predicate.Builtin.List;


// Moved methods to separate class so can be used by both MapList and SubList. TODO move to TermUtils
public static class PartialApplicationUtils
{
    private const string KEY_VALUE_PAIR_FUNCTOR = "-";

    public static bool IsAtomOrStructure(Term arg)
    {
        var type = arg.Type;
        return type == TermType.STRUCTURE || type == TermType.ATOM;
    }

    public static bool IsList(Term arg)
    {
        var type = arg.Type;
        return type == TermType.EMPTY_LIST || type == TermType.LIST;
    }

    public static PredicateFactory GetCurriedPredicateFactory(Predicates predicates, Term partiallyAppliedFunction) 
        => GetPartiallyAppliedPredicateFactory(predicates, partiallyAppliedFunction, 1);

    public static PredicateFactory GetPreprocessedCurriedPredicateFactory(Predicates predicates, Term partiallyAppliedFunction)
        => GetPreprocessedPartiallyAppliedPredicateFactory(predicates, partiallyAppliedFunction, 1);

    public static PredicateFactory GetPreprocessedPartiallyAppliedPredicateFactory(Predicates predicates, Term partiallyAppliedFunction, int extraArgs)
    {
        var args = new Term[partiallyAppliedFunction.NumberOfArguments + extraArgs];
        for (int i = 0; i < partiallyAppliedFunction.NumberOfArguments; i++)
        {
            args[i] = partiallyAppliedFunction.GetArgument(i);
        }
        for (int i = partiallyAppliedFunction.NumberOfArguments; i < args.Length; i++)
        {
            args[i] = new Variable();
        }
        // TODO check not numeric before calling .Name
        return predicates.GetPreprocessedPredicateFactory(
            Structure.CreateStructure(partiallyAppliedFunction.Name, args));
    }

    public static PredicateFactory GetPartiallyAppliedPredicateFactory(Predicates predicates, Term partiallyAppliedFunction, int numberOfExtraArguments)
    {
        int numArgs = partiallyAppliedFunction.NumberOfArguments + numberOfExtraArguments;
        // TODO check not numeric before calling .Name
        var key = new PredicateKey(partiallyAppliedFunction.Name, numArgs);
        return predicates.GetPredicateFactory(key);
    }

    // TODO have overloaded version that avoids varargs
    public static Term[] CreateArguments(Term partiallyAppliedFunction, params Term[] extraArguments)
    {
        int originalNumArgs = partiallyAppliedFunction.NumberOfArguments;
        var result = new Term[originalNumArgs + extraArguments.Length];

        for (int i = 0; i < originalNumArgs; i++)
        {
            result[i] = partiallyAppliedFunction.GetArgument(i).Term;
        }

        for (int i = 0; i < extraArguments.Length; i++)
        {
            result[originalNumArgs + i] = extraArguments[i].Term;
        }

        return result;
    }

    public static bool Apply(PredicateFactory pf, Term[] args)
    {
        var p = pf.GetPredicate(args);
        if (p.Evaluate())
        {
            return true;
        }
        else
        {
            TermUtils.Backtrack(args);
            return false;
        }
    }

    public static Predicate GetPredicate(PredicateFactory factory, Term action, params Term[] args) 
        => action.NumberOfArguments == 0 
        ? factory.GetPredicate(args) 
        : factory.GetPredicate(CreateArguments(action, args));

    public static bool IsKeyValuePair(Term term)
        => term.Type == TermType.STRUCTURE
        && KEY_VALUE_PAIR_FUNCTOR.Equals(term.Name)
        && term.NumberOfArguments == 2;
}
