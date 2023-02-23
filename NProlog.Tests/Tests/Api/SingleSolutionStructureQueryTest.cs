/*
 * Copyright 2020 S. Webber
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

namespace Org.NProlog.Api;

[TestClass]
public class SingleSolutionStructureQueryTest : AbstractQueryTest
{
    private const string EXPECTED_NUMERIC_EXCEPTION_MESSAGE = "Expected Numeric but got: STRUCTURE with value: test(a, 1)";
    private const string EXPECTED_ATOM_EXCEPTION_MESSAGE = "Expected an atom but got: STRUCTURE with value: test(a, 1)";
    private static readonly Term STRUCTURE = Core.Terms.Structure.CreateStructure("test", new Term[] { new Atom("a"), new IntegerNumber(1) });

    public SingleSolutionStructureQueryTest() : base("X = test(a, 1).") { }

    public override void TestFindFirstAsTerm()
    => FindFirstAsTerm().AreEqual(STRUCTURE);


    public override void TestFindFirstAsOptionalTerm()
    => FindFirstAsOptionalTerm().AreEqual(Optional<Term>.Of(STRUCTURE));


    public override void TestFindAllAsTerm()
    => FindAllAsTerm().AreEqual(new List<Term>() { STRUCTURE });


    public override void TestFindFirstAsAtomName()
    => FindFirstAsAtomName().AssertException(EXPECTED_ATOM_EXCEPTION_MESSAGE);


    public override void TestFindFirstAsOptionalAtomName()
    => FindFirstAsOptionalAtomName().AssertException(EXPECTED_ATOM_EXCEPTION_MESSAGE);


    public override void TestFindAllAsAtomName()
    => FindAllAsAtomName().AssertException(EXPECTED_ATOM_EXCEPTION_MESSAGE);


    public override void TestFindFirstAsDouble()
    => FindFirstAsDouble().AssertException(EXPECTED_NUMERIC_EXCEPTION_MESSAGE);


    public override void TestFindFirstAsOptionalDouble()
    => FindFirstAsOptionalDouble().AssertException(EXPECTED_NUMERIC_EXCEPTION_MESSAGE);


    public override void TestFindAllAsDouble()
    => FindAllAsDouble().AssertException(EXPECTED_NUMERIC_EXCEPTION_MESSAGE);


    public override void TestFindFirstAsLong()
    => FindFirstAsLong().AssertException(EXPECTED_NUMERIC_EXCEPTION_MESSAGE);


    public override void TestFindFirstAsOptionalLong()
    => FindFirstAsOptionalLong().AssertException(EXPECTED_NUMERIC_EXCEPTION_MESSAGE);


    public override void TestFindAllAsLong()
    => FindAllAsLong().AssertException(EXPECTED_NUMERIC_EXCEPTION_MESSAGE);
}
