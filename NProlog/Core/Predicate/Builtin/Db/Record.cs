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

namespace Org.NProlog.Core.Predicate.Builtin.Db;



/** Represents a record stored in a {@code RecordedDatabase}. */
public class Record
{
    private readonly PredicateKey key;
    private readonly IntegerNumber reference;
    private readonly Term value;

    public Record(PredicateKey key, IntegerNumber reference, Term value)
    {
        this.key = key;
        this.reference = reference;
        this.value = value;
    }

    public Term Key
    {
        get
        {
            var name = key.Name;
            var numArgs = key.NumArgs;
            if (numArgs == 0)
                return new Atom(name);
            else
            {
                var args = new Term[numArgs];
                ArraysHelpers.Fill(args, new Variable());
                return Structure.CreateStructure(name, args);
            }
        }
    }

    public IntegerNumber Reference => reference;

    public Term Value => value;
}
