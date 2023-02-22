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
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Predicate.Builtin.Db;




/* TEST
%FAIL recorded(X,Y,Z)

% Note: recorded/2 is equivalent to calling recorded/3 with the third argument as an anonymous variable.
%FAIL recorded(X,Y)
*/
/**
 * <code>recorded(X,Y,Z)</code> - checks if a term is associated with a key.
 * <p>
 * <code>recorded(X,Y,Z)</code> succeeds if there exists an association between the key represented by <code>X</code>
 * and the term represented by <code>Y</code>, with the reference represented by <code>Z</code>.
 */
public class Recorded : AbstractPredicateFactory
{

    protected override Predicate GetPredicate(Term key, Term value) => GetPredicate(key, value, new Variable());

    protected override Predicate GetPredicate(Term key, Term value, Term reference)
    {
        var database = KnowledgeBaseServiceLocator.GetServiceLocator(KnowledgeBase).
              GetInstance<RecordedDatabase>(
            typeof(RecordedDatabase));
        var itr = GetIterator(key, database);
        return new RecordedPredicate(key, value, reference, itr);
    }

    private static IEnumerator<Record> GetIterator(Term key, RecordedDatabase database) 
        => key.Type.IsVariable ? database.GetAll() : database.GetChain(PredicateKey.CreateForTerm(key));

    public class RecordedPredicate : Predicate
    {
        private readonly Term key;
        private readonly Term value;
        private readonly Term reference;
        private readonly IEnumerator<Record> itr;

        public RecordedPredicate(Term key, Term value, Term reference, IEnumerator<Record> itr)
        {
            this.key = key;
            this.value = value;
            this.reference = reference;
            this.itr = itr;
        }


        public virtual bool Evaluate()
        {
            while (CouldReevaluationSucceed && itr.MoveNext())
            {
                var current = itr.Current;
                key.Backtrack();
                value.Backtrack();
                reference.Backtrack();
                if (Unify(current, key, value, reference)) return true;
            }
            return false;
        }

        private static bool Unify(Record record, Term key, Term value, Term reference) 
            => key.Unify(record.Key) && value.Unify(record.Value) && reference.Unify(record.Reference);


        public virtual bool CouldReevaluationSucceed => itr.Current!=null;
    }
}
