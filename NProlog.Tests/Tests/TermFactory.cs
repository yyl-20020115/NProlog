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

    public static Atom Atom() => Atom("test");

    public static Atom Atom(string name) => new Atom(name);

    public static Structure Structure()
    {
        return Structure("test", new Term[] { Atom() });
    }

    public static Structure Structure(string name, params Term[] args)
    {
        return (Structure)Core.Terms.Structure.CreateStructure(name, args);
    }

    public static List List(params Term[] args)
    {
        return (List)ListFactory.CreateList(args);
    }

    public static IntegerNumber IntegerNumber()
    {
        return IntegerNumber(1);
    }

    public static IntegerNumber IntegerNumber(long i)
    {
        return new IntegerNumber(i);
    }

    public static DecimalFraction DecimalFraction()
    {
        return DecimalFraction(1.0);
    }

    public static DecimalFraction DecimalFraction(double d)
    {
        return new DecimalFraction(d);
    }

    public static Variable Variable()
    {
        return Variable("X");
    }

    public static Variable Variable(string name)
    {
        return new Variable(name);
    }


}
