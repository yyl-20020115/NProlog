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
public class MultiSolutionsLongQueryTest : AbstractQueryTest
{
    private static readonly string EXPECTED_ATOM_EXCEPTION_MESSAGE = "Expected an atom but got: INTEGER with value: 42";
    private static readonly long FIRST_LONG_VALUE = 42;
    private static readonly long SECOND_LONG_VALUE = 180;
    private static readonly long THIRD_LONG_VALUE = -7;

    public MultiSolutionsLongQueryTest() : base("test(X).", "test(42).test(180).test(-7).") { }


    public override void TestFindFirstAsTerm()
    => FindFirstAsTerm().AreEqual(new IntegerNumber(FIRST_LONG_VALUE));


    public override void TestFindFirstAsOptionalTerm()
    => FindFirstAsOptionalTerm().AreEqual(Optional<Term>.Of(new IntegerNumber(FIRST_LONG_VALUE)));


    public override void TestFindAllAsTerm()
    => FindAllAsTerm().AreEqual(new List<Term> { new IntegerNumber(FIRST_LONG_VALUE), new IntegerNumber(SECOND_LONG_VALUE), new IntegerNumber(THIRD_LONG_VALUE) });


    public override void TestFindFirstAsAtomName()
    => FindFirstAsAtomName().assertException(EXPECTED_ATOM_EXCEPTION_MESSAGE);


    public override void TestFindFirstAsOptionalAtomName()
    => FindFirstAsOptionalAtomName().assertException(EXPECTED_ATOM_EXCEPTION_MESSAGE);


    public override void TestFindAllAsAtomName()
    => FindAllAsAtomName().assertException(EXPECTED_ATOM_EXCEPTION_MESSAGE);


    public override void TestFindFirstAsDouble()
    => FindFirstAsDouble().AreEqual((double)FIRST_LONG_VALUE);


    public override void TestFindFirstAsOptionalDouble()
    => FindFirstAsOptionalDouble().AreEqual(Optional<double>.Of((double)FIRST_LONG_VALUE));


    public override void TestFindAllAsDouble()
    => FindAllAsDouble().AreEqual(new List<double> { (double)FIRST_LONG_VALUE, (double)SECOND_LONG_VALUE, (double)THIRD_LONG_VALUE });


    public override void TestFindFirstAsLong()
    => FindFirstAsLong().AreEqual(FIRST_LONG_VALUE);


    public override void TestFindFirstAsOptionalLong()
    => FindFirstAsOptionalLong().AreEqual(Optional<long>.Of(FIRST_LONG_VALUE));


    public override void TestFindAllAsLong()
    => FindAllAsLong().AreEqual(new List<long> { FIRST_LONG_VALUE, SECOND_LONG_VALUE, THIRD_LONG_VALUE });
}
