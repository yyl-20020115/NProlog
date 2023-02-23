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
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Predicate.Builtin.IO;


/* TEST
%LINK prolog-io
*/
/**
 * <code>close(X)</code> - closes a stream.
 * <p>
 * <code>close(X)</code> closes the stream represented by <code>X</code>. The stream is closed and can no longer be
 * used.
 * </p>
 */
public class Close : AbstractSingleResultPredicate
{
    protected override bool Evaluate(Term argument)
    {
        try
        {
            FileHandles.Close(argument);
            return true;
        }
        catch (Exception e)
        {
            throw new PrologException($"Unable to close stream for: {argument}", e);
        }
    }
}
