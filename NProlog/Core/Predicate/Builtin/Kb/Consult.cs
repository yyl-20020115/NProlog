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
using Org.NProlog.Core.Parser;
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Predicate.Builtin.Kb;

/* TEST
%LINK prolog-io
*/
/**
 * <code>consult(X)</code> - reads clauses and goals from a file.
 * <p>
 * <code>consult(X)</code> reads clauses and goals from a file. <code>X</code> must be instantiated to the name of a
 * text file containing Prolog clauses and goals which will be added to the knowledge base.
 * </p>
 */
public class Consult : AbstractSingleResultPredicate
{

    protected override bool Evaluate(Term arg)
    {
        PrologSourceReader.ParseResource(KnowledgeBase, TermUtils.GetAtomName(arg));
        return true;
    }
}
