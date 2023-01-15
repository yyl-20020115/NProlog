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

namespace Org.NProlog.Core.Predicate.Builtin.IO;


/* TEST
%?- write_canonical( 1+1 )
%OUTPUT +(1, 1)
%YES
%?- write_canonical( '+'(1,1) )
%OUTPUT +(1, 1)
%YES
*/
/**
 * <code>write_canonical(X)</code> - writes a term to the output stream.
 * <p>
 * Writes the term <code>X</code> to the current output stream. <code>write_canonical</code> does not take account of
 * current operator declarations - thus any structures are Writeed _out with the functor first and the arguments in
 * brackets.
 * </p>
 * <p>
 * Succeeds only once.
 * </p>
 */
public class WriteCanonical : AbstractSingleResultPredicate
{

    protected override bool Evaluate(Term arg)
    {
        FileHandles.CurrentWriter.Write(arg.ToString());
        return true;
    }
}
