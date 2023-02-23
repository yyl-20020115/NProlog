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
public class MultiSolutionsAtomQueryTest : AbstractQueryTest
{
    private const string EXPECTED_NUMERIC_EXCEPTION_MESSAGE = "Expected Numeric but got: ATOM with value: a";
    private const string FIRST_ATOM_NAME = "a";
    private const string SECOND_ATOM_NAME = "b";
    private const string THIRD_ATOM_NAME = "c";

    public MultiSolutionsAtomQueryTest() : base("test(X).", "test(a).test(b).test(c).") { }
    public override void TestFindFirstAsTerm() => FindFirstAsTerm().AreEqual(new Atom(FIRST_ATOM_NAME));


    public override void TestFindFirstAsOptionalTerm() => FindFirstAsOptionalTerm().AreEqual(Optional<Term>.Of(new Atom(FIRST_ATOM_NAME)));


    public override void TestFindAllAsTerm()
    => FindAllAsTerm().AreEqual(new List<Term>() { new Atom(FIRST_ATOM_NAME), new Atom(SECOND_ATOM_NAME), new Atom(THIRD_ATOM_NAME) });


    public override void TestFindFirstAsAtomName()
    => FindFirstAsAtomName().AreEqual(FIRST_ATOM_NAME);


    public override void TestFindFirstAsOptionalAtomName()
    => FindFirstAsOptionalAtomName().AreEqual(Optional<string>.Of(FIRST_ATOM_NAME));


    public override void TestFindAllAsAtomName()
    => FindAllAsAtomName().AreEqual(new List<string>() { FIRST_ATOM_NAME, SECOND_ATOM_NAME, THIRD_ATOM_NAME });


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
