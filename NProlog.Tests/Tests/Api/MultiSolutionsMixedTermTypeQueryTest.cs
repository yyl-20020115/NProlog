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
public class MultiSolutionsMixedTermTypeQueryTest : AbstractQueryTest
{
    private static readonly string EXPECTED_NUMERIC_EXCEPTION_MESSAGE = "Expected Numeric but got: STRUCTURE with value: s(a, 1)";
    private static readonly string EXPECTED_ATOM_EXCEPTION_MESSAGE = "Expected an atom but got: STRUCTURE with value: s(a, 1)";
    private static readonly Term STRUCTURE = Core.Terms.Structure.CreateStructure("s", new Term[] { new Atom("a"), new IntegerNumber(1) });

    public MultiSolutionsMixedTermTypeQueryTest() : base("test(X).", "test(s(a, 1)).test(1).test(1.0).test(a).") { }

    public override void TestFindFirstAsTerm()
    => FindFirstAsTerm().AreEqual(STRUCTURE);


    public override void TestFindFirstAsOptionalTerm()
        => FindFirstAsOptionalTerm().AreEqual(Optional<Term>.Of(STRUCTURE));


    public override void TestFindAllAsTerm()
    => FindAllAsTerm().AreEqual(new List<Term>() { STRUCTURE, new IntegerNumber(1), new DecimalFraction(1), new Atom("a") });


    public override void TestFindFirstAsAtomName()
    => FindFirstAsAtomName().assertException(EXPECTED_ATOM_EXCEPTION_MESSAGE);


    public override void TestFindFirstAsOptionalAtomName()
    => FindFirstAsOptionalAtomName().assertException(EXPECTED_ATOM_EXCEPTION_MESSAGE);


    public override void TestFindAllAsAtomName()
    => FindAllAsAtomName().assertException(EXPECTED_ATOM_EXCEPTION_MESSAGE);


    public override void TestFindFirstAsDouble()
    => FindFirstAsDouble().assertException(EXPECTED_NUMERIC_EXCEPTION_MESSAGE);


    public override void TestFindFirstAsOptionalDouble()
    => FindFirstAsOptionalDouble().assertException(EXPECTED_NUMERIC_EXCEPTION_MESSAGE);


    public override void TestFindAllAsDouble()
    => FindAllAsDouble().assertException(EXPECTED_NUMERIC_EXCEPTION_MESSAGE);


    public override void TestFindFirstAsLong()
    => FindFirstAsLong().assertException(EXPECTED_NUMERIC_EXCEPTION_MESSAGE);


    public override void TestFindFirstAsOptionalLong()
    => FindFirstAsOptionalLong().assertException(EXPECTED_NUMERIC_EXCEPTION_MESSAGE);


    public override void TestFindAllAsLong()
    => FindAllAsLong().assertException(EXPECTED_NUMERIC_EXCEPTION_MESSAGE);
}
