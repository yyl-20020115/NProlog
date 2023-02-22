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
% Example of recorda/3.

%?- recorda(a,q,X)
% X=0

% Note: recorda/2 is equivalent to calling recorda/3 with the third argument as an anonymous variable.
%TRUE recorda(a,w)

%?- recorded(X,Y,Z)
% X=a
% Y=w
% Z=1
% X=a
% Y=q
% Z=0

% Note: recorded/2 is equivalent to calling recorded/3 with the third argument as an anonymous variable.
%?- recorded(a,Y)
% Y=w
% Y=q

% Example of recordz/3.

%?- recordz(b,q,X)
% X=2

% Note: recordz/2 is equivalent to calling recordz/3 with the third argument as an anonymous variable.
%TRUE recordz(b,w)

%?- recorded(b,Y)
% Y=q
% Y=w
*/
/**
 * <code>recorda(X,Y,Z)</code> / <code>recordz(X,Y,Z)</code> - associates a term with a key.
 * <p>
 * <code>recorda(X,Y,Z)</code> associates <code>Y</code> with <code>X</code>. The unique reference for this association
 * will be unified with <code>Z</code>. <code>Y</code> is added to the <i>start</i> of the list of terms already
 * associated with <code>X</code>.
 * </p>
 * <p>
 * <code>recordz(X,Y,Z)</code> associates <code>Y</code> with <code>X</code>. The unique reference for this association
 * will be unified with <code>Z</code>. <code>Y</code> is added to the <i>end</i> of the list of terms already
 * associated with <code>X</code>.
 * </p>
 */
public class InsertRecord : AbstractSingleResultPredicate
{
    public static InsertRecord RecordA() => new (false);

    public static InsertRecord RecordZ() => new (true);

    private readonly bool insertLast;
    private RecordedDatabase? database;

    private InsertRecord(bool insertLast) => this.insertLast = insertLast;

    protected override void Init() 
        => this.database = KnowledgeBaseServiceLocator.GetServiceLocator(KnowledgeBase)
              .GetInstance<RecordedDatabase>(typeof(RecordedDatabase));


    protected override bool Evaluate(Term key, Term value) 
        => Evaluate(key, value, new Variable());

    protected override bool Evaluate(Term key, Term value, Term reference)
    {
        if (!reference.Type.IsVariable) return false;
        var k = PredicateKey.CreateForTerm(key);
        var result = database?.Add(k, value, insertLast);
        return reference.Unify(result);
    }
}
