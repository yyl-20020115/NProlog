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
% Add three records to the recorded database.
%TRUE recordz(k,a,_), recordz(k,b,_), recordz(k,c,_)

% Confirm the records have been added.
%?- recorded(k,X)
% X=a
% X=b
% X=c

% Erase (i.e. Remove) a record.
%?- recorded(k,b,X), erase(X)
% X=1
%NO

% Confirm the record has been removed.
%?- recorded(k,X)
% X=a
% X=c
*/
/**
 * <code>erase(X)</code> - removes a record from the recorded database.
 * <p>
 * Removes from the recorded database the term associated with the reference specified by <code>X</code>. The goal
 * succeeds even if there is no term associated with the specified reference.
 */
public class Erase : AbstractSingleResultPredicate
{
    private RecordedDatabase? database;

    protected override void Init()
        => this.database = KnowledgeBaseServiceLocator.GetServiceLocator(KnowledgeBase).GetInstance
              <RecordedDatabase>(typeof(RecordedDatabase));


    protected override bool Evaluate(Term arg)
    {
        var reference = TermUtils.CastToNumeric(arg);
        database?.Erase(reference.Long);
        return true;
    }
}
