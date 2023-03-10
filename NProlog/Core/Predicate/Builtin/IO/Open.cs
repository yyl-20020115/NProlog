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
 * <code>open(X,Y,Z)</code> - opens a file.
 * <p>
 * <code>X</code> is an atom representing the name of the file to open. <code>Y</code> is an atom that should have
 * either the value <code>read</code> to open the file for reading from or <code>write</code> to open the file for
 * writing to. <code>Z</code> is instantiated by <code>open</code> to a special term that must be referred to in
 * subsequent commands in order to access the stream.
 * </p>
 */
public class Open : AbstractSingleResultPredicate
{
    private const string READ = "read";
    private const string WRITE = "write";

    protected override bool Evaluate(Term fileNameAtom, Term operationAtom, Term variableToAssignTo)
    {
        var operation = TermUtils.GetAtomName(operationAtom);
        var fileName = TermUtils.GetAtomName(fileNameAtom);
        var handle = READ.Equals(operation)
            ? OpenInput(fileName)
            : WRITE.Equals(operation)
                ? OpenOutput(fileName)
                : throw new PrologException("Second argument is not '" + READ + "' or '" + WRITE + "' but: " + operation);
        variableToAssignTo.Unify(handle);
        return true;
    }

    private Atom OpenInput(string fileName)
    {
        try
        {
            return FileHandles.OpenInput(fileName);
        }
        catch (Exception e)
        {
            throw new PrologException("Unable to open input for: " + fileName, e);
        }
    }

    private Atom OpenOutput(string fileName)
    {
        try
        {
            return FileHandles.OpenOutput(fileName);
        }
        catch (Exception e)
        {
            throw new PrologException("Unable to open output for: " + fileName + " " + e, e);
        }
    }
}
