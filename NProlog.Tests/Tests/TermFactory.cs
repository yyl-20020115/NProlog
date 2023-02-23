/*
 * Copyright 2021 S. Webber
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a Copy of the License at
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

namespace Org.NProlog;


/** Creates instances of {@code Term} for use in unit-tests. */
public class TermFactory : TermUtils
{
    /**
     * Private constructor as all methods are static.
     */

    public static Atom Atom(string name = "test") => new (name);

    public static Structure Structure() => Structure("test", new Term[] { Atom() });

    public static Structure Structure(string name, params Term[] args) 
        => (Structure)Core.Terms.Structure.CreateStructure(name, args);

    public static LinkedTermList List(params Term[] args) 
        => (LinkedTermList)ListFactory.CreateList(args);

    public static IntegerNumber IntegerNumber(long i = 1) 
        => new (i);

    public static DecimalFraction DecimalFraction(double d = 1.0)
        => new (d);

    public static Variable Variable(string name = "X") 
        => new (name);
}
