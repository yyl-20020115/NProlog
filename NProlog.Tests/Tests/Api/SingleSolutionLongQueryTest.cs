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
public class SingleSolutionLongQueryTest : AbstractQueryTest
{
    private const string EXPECTED_ATOM_EXCEPTION_MESSAGE = "Expected an atom but got: INTEGER with value: 42";
    private const long LONG_VALUE = 42;

    public SingleSolutionLongQueryTest() : base("X = 42.") { }


    public override void TestFindFirstAsTerm() => FindFirstAsTerm().AreEqual(new IntegerNumber(LONG_VALUE));


    public override void TestFindFirstAsOptionalTerm() => FindFirstAsOptionalTerm().AreEqual(Optional<Term>.Of(new IntegerNumber(LONG_VALUE)));


    public override void TestFindAllAsTerm() => FindAllAsTerm().AreEqual(new List<Term> { new IntegerNumber(LONG_VALUE) });


    public override void TestFindFirstAsAtomName() => FindFirstAsAtomName().AssertException(EXPECTED_ATOM_EXCEPTION_MESSAGE);


    public override void TestFindFirstAsOptionalAtomName() => FindFirstAsOptionalAtomName().AssertException(EXPECTED_ATOM_EXCEPTION_MESSAGE);


    public override void TestFindAllAsAtomName() => FindAllAsAtomName().AssertException(EXPECTED_ATOM_EXCEPTION_MESSAGE);


    public override void TestFindFirstAsDouble() => FindFirstAsDouble().AreEqual((double)LONG_VALUE);


    public override void TestFindFirstAsOptionalDouble() => FindFirstAsOptionalDouble().AreEqual(Optional<double>.Of((double)LONG_VALUE));


    public override void TestFindAllAsDouble() => FindAllAsDouble().AreEqual(new List<double> { (double)LONG_VALUE });


    public override void TestFindFirstAsLong() => FindFirstAsLong().AreEqual(LONG_VALUE);


    public override void TestFindFirstAsOptionalLong() => FindFirstAsOptionalLong().AreEqual(Optional<long>.Of(LONG_VALUE));


    public override void TestFindAllAsLong() => FindAllAsLong().AreEqual(new List<long> { LONG_VALUE });
}
