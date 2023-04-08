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
 * Represents a clause.
 * <p>
 * A clause consists of a head and a body. Where a clause is not explicitly specified it defaults to having a body of
 * {@code true}.
 * <p>
 * Called {@code ClauseModel} to differentiate it from {@link org.prolog.core.predicate.udp.ClauseAction}.
 */
public class ClauseModel
{
    private static readonly Term TRUE = new Atom("true");

    private readonly Term original;
    private readonly Term consequent;
    private readonly Term antecedent;

    public static ClauseModel CreateClauseModel(Term? original)
    {
        var type = original?.Type;
        if (type != TermType.STRUCTURE && type != TermType.ATOM)
            throw new PrologException("Expected an atom or a predicate but got a " + type + " with value: " + original);

        Term consequent;
        Term antecedent;

        if (DefiniteClauseGrammerConvertor.IsDCG(original))
            original = DefiniteClauseGrammerConvertor.Convert(original);

        if (original.Name.Equals(KnowledgeBaseUtils.IMPLICATION_PREDICATE_NAME))
        {
            var implicationArgs = original.Args;
            consequent = implicationArgs[0];
            if (implicationArgs.Length == 2)
            {
                // TODO set to TRUE if equal to it
                antecedent = implicationArgs[1];
            }
            else if (implicationArgs.Length == 1)
            {
                antecedent = TRUE;
            }
            else
            {
                throw new PrologException("Unexcepted arg length");
            }
        }
        else
        {
            consequent = original;
            antecedent = TRUE;
        }

        return new (original, consequent, antecedent);
    }

    private ClauseModel(Term original, Term consequent, Term antecedent)
    {
        this.original = original;
        this.consequent = consequent;
        this.antecedent = antecedent;
    }

    /** Returns the body of the clause. i.e. the bit after the {@code :-} */
    public Term Antecedent => antecedent;

    /** Returns the head of the clause. i.e. the bit before the {@code :-} */
    public Term Consequent => consequent;

    public Term Original => original;

    public PredicateKey PredicateKey => PredicateKey.CreateForTerm(consequent);

    public ClauseModel Copy()
    {
        var newTerms = TermUtils.Copy(original, consequent, antecedent);
        return new ClauseModel(newTerms[0], newTerms[1], newTerms[2]);
    }

    public bool IsFact => TRUE.Equals(antecedent);

    public override string ToString() => "[" + base.ToString() + " " + consequent + " " + antecedent + "]";
}
