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
using Org.NProlog.Core.IO;
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Predicate.Builtin.IO;



/* TEST
%LINK prolog-io
*/
/**
 * <code>told</code> - closes the current output stream.
 * <p>
 * The new input stream becomes <code>user_output</code>.
 */
public class Told : AbstractSingleResultPredicate
{

    protected override bool Evaluate()
    {
        var fileHandles = FileHandles;
        var handle = fileHandles.CurrentOutputHandle;
        Close(fileHandles, handle);
        fileHandles.SetOutput(FileHandles.USER_OUTPUT_HANDLE);
        return true;
    }

    private void Close(FileHandles fileHandles, Term handle)
    {
        try
        {
            fileHandles.Close(handle);
        }
        catch (Exception e)
        {
            throw new PrologException("Unable to close stream for: " + handle, e);
        }
    }
}
