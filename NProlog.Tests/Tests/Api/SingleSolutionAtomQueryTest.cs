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
public class SingleSolutionAtomQueryTest : AbstractQueryTest
{
    private static readonly string EXPECTED_NUMERIC_EXCEPTION_MESSAGE = "Expected Numeric but got: ATOM with value: test";
    private static readonly string ATOM_NAME = "test";

    public SingleSolutionAtomQueryTest() : base("X = test.") { }

    public override void TestFindFirstAsTerm() => FindFirstAsTerm().AreEqual(new Atom(ATOM_NAME));


    public override void TestFindFirstAsOptionalTerm() => FindFirstAsOptionalTerm().AreEqual(Optional<Term>.Of(new Atom(ATOM_NAME)));


    public override void TestFindAllAsTerm()
    => FindAllAsTerm().AreEqual(new List<Term>() { new Atom(ATOM_NAME) });


    public override void TestFindFirstAsAtomName()
    => FindFirstAsAtomName().AreEqual(ATOM_NAME);


    public override void TestFindFirstAsOptionalAtomName()
    => FindFirstAsOptionalAtomName().AreEqual(Optional<string>.Of(ATOM_NAME));


    public override void TestFindAllAsAtomName()
    => FindAllAsAtomName().AreEqual(new List<string>() { ATOM_NAME });


    public override void TestFindFirstAsDouble() => FindFirstAsDouble().assertException(EXPECTED_NUMERIC_EXCEPTION_MESSAGE);


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
