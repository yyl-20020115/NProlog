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
 * <code>see(X)</code> - opens a file and sets it as the current input stream.
 * <p>
 * If <code>X</code> refers to a handle, rather than a filename, then the current input stream is set to the stream
 * represented by the handle.
 */
public class See : AbstractSingleResultPredicate
{

    protected override bool Evaluate(Term source)
    {
        var fileName = TermUtils.GetAtomName(source);
        try
        {
            var fileHandles = FileHandles;
            if (!fileHandles.IsHandle(fileName))
            {
                Atom handle = fileHandles.OpenInput(fileName);
                fileHandles.SetInput(handle);
            }
            else
            {
                fileHandles.SetInput(source);
            }
            return true;
        }
        catch (Exception e)
        {
            throw new PrologException("Unable to open input for: " + source, e);
        }
    }
}
