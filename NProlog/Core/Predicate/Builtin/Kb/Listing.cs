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
using Org.NProlog.Core.Kb;
using Org.NProlog.Core.Predicate.Udp;
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Predicate.Builtin.Kb;

/* TEST
test(X) :- X < 3.
test(X) :- X > 9.
test(X) :- X = 5.
%?- listing(test)
%OUTPUT
%test(X) :- X < 3
%test(X) :- X > 9
%test(X) :- X = 5
%
%OUTPUT
%YES

overloaded_predicate_name(X) :- X = this_rule_has_one_argument.
overloaded_predicate_name(X, Y) :- X = this_rule_has_two_arguments, X = Y.
%?- listing(overloaded_predicate_name)
%OUTPUT
%overloaded_predicate_name(X) :- X = this_rule_has_one_argument
%overloaded_predicate_name(X, Y) :- X = this_rule_has_two_arguments , X = Y
%
%OUTPUT
%YES

%TRUE listing(predicate_name_that_doesnt_exist_in_knowledge_base)

%?- listing(X)
%ERROR Expected an atom but got: VARIABLE with value: X
*/
/**
 * <code>listing(X)</code> - outputs current clauses.
 * <p>
 * <code>listing(X)</code> allows you to inspect the clauses you currently have loaded. Causes all clauses with
 * <code>X</code> as the predicate name to be written to the current output stream.
 * </p>
 */
public class Listing : AbstractSingleResultPredicate
{

    protected override bool Evaluate(Term arg)
    {
        foreach (var key in KnowledgeBaseUtils.GetPredicateKeysByName(KnowledgeBase, TermUtils.GetAtomName(arg)))
            ListClauses(key);
        return true;
    }

    private void ListClauses(PredicateKey key)
    {
        var it = this.GetClauses(key);
        while(it.MoveNext())
            this.ListClause(it.Current);
    }

    private IEnumerator<ClauseModel> GetClauses(PredicateKey key) 
        => Predicates.GetUserDefinedPredicates().TryGetValue(key, out var userDefinedPredicate)
            ? userDefinedPredicate.GetImplications()
            : new List<ClauseModel>().GetEnumerator();

    private void ListClause(ClauseModel clauseModel) 
        => FileHandles.CurrentWriter.WriteLine(TermFormatter.FormatTerm(clauseModel.Original));
}
