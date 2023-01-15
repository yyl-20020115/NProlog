/*
 * Copyright 2021 S. Webber
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a Copy of the License at
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

namespace Org.NProlog.Core.Predicate.Udp;

/**
 * Used by system tests in src/test/prolog/udp/predicate-meta-data
 */
[TestClass]
public class PredicateMetaData : AbstractPredicateFactory
{

    protected override Predicate GetPredicate(Term input, Term variable)
    {
        List<Term> attributes = new();

        PredicateFactory pf = Predicates.GetPredicateFactory(input);
        attributes.AddRange(ToTerms("factory", pf));
        if (pf is StaticUserDefinedPredicateFactory)
        {
            PredicateFactory apf = ((StaticUserDefinedPredicateFactory)pf).GetActualPredicateFactory();
            attributes.AddRange(ToTerms("actual", apf));
        }
        if (pf is PreprocessablePredicateFactory)
        {
            PredicateFactory preprocessed = ((PreprocessablePredicateFactory)pf).Preprocess(input);
            attributes.AddRange(ToTerms("processed", preprocessed));
        }

        return new MetaDataPredicate(variable, attributes);
    }

    private static List<Term> ToTerms(string type, PredicateFactory pf)
    {
        List<Term> attributes = new();
        attributes.Add(Structure.CreateStructure(":", new Term[] { new Atom(type + "_class"), new Atom(pf.GetType().Name) }));
        attributes.Add(Structure.CreateStructure(":", new Term[] { new Atom(type + "_isRetryable"), new Atom("" + pf.IsRetryable) }));
        attributes.Add(Structure.CreateStructure(":", new Term[] { new Atom(type + "_isAlwaysCutOnBacktrack"), new Atom("" + pf.IsAlwaysCutOnBacktrack) }));
        return attributes;
    }

    private class MetaDataPredicate : Predicate
    {
        readonly Term variable;
        readonly ICheckedEnumerator<Term> iterator;

        public MetaDataPredicate(Term variable, List<Term> attributes)
        {
            this.variable = variable;
            this.iterator = ListCheckedEnumerator<Term>.Of(attributes);
        }


        public bool Evaluate()
        {
            while (iterator.MoveNext())
            {
                variable.Backtrack();
                var next = iterator.Current;
                if (variable.Unify(next))
                {
                    return true;
                }
            }
            return false;
        }


        public bool CouldReevaluationSucceed => iterator.CanMoveNext;
    }
}
