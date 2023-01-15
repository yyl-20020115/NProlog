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

namespace Org.NProlog.Core.Predicate.Builtin.IO;



/* TEST
%LINK prolog-io
*/
/**
 * <code>read(X)</code> - reads a term from the input stream.
 * <p>
 * <code>read(X)</code> reads the next term from the input stream and matches it with <code>X</code>.
 * </p>
 * <p>
 * Succeeds only once.
 * </p>
 */
public class Read : AbstractSingleResultPredicate
{

    protected override bool Evaluate(Term argument)
    {
        var reader = (FileHandles.CurrentReader);
        var parser = SentenceParser.GetInstance(reader, Operands);
        var term = parser.ParseTerm();
        return argument.Unify(term);
    }
}
