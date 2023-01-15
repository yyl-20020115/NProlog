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
public class MultiSolutionsDoubleQueryTest : AbstractQueryTest
{
    private static readonly string EXPECTED_ATOM_EXCEPTION_MESSAGE = "Expected an atom but got: FRACTION with value: 42.5";
    private static readonly double FIRST_DOUBLE_VALUE = 42.5;
    private static readonly double SECOND_DOUBLE_VALUE = 180.2;
    private static readonly double THIRD_DOUBLE_VALUE = -7;

    public MultiSolutionsDoubleQueryTest() : base("test(X).", "test(42.5).test(180.2).test(-7.0).") { }


    public override void TestFindFirstAsTerm() => FindFirstAsTerm().AreEqual(new DecimalFraction(FIRST_DOUBLE_VALUE));


    public override void TestFindFirstAsOptionalTerm()
    => FindFirstAsOptionalTerm().AreEqual(Optional<Term>.Of(new DecimalFraction(FIRST_DOUBLE_VALUE)));


    public override void TestFindAllAsTerm()
    => FindAllAsTerm().AreEqual(new List<Term>() { new DecimalFraction(FIRST_DOUBLE_VALUE), new DecimalFraction(SECOND_DOUBLE_VALUE), new DecimalFraction(THIRD_DOUBLE_VALUE) });


    public override void TestFindFirstAsAtomName()
    => FindFirstAsAtomName().assertException(EXPECTED_ATOM_EXCEPTION_MESSAGE);


    public override void TestFindFirstAsOptionalAtomName()
    => FindFirstAsOptionalAtomName().assertException(EXPECTED_ATOM_EXCEPTION_MESSAGE);


    public override void TestFindAllAsAtomName()
    => FindAllAsAtomName().assertException(EXPECTED_ATOM_EXCEPTION_MESSAGE);


    public override void TestFindFirstAsDouble()
    => FindFirstAsDouble().AreEqual(FIRST_DOUBLE_VALUE);


    public override void TestFindFirstAsOptionalDouble()
    => FindFirstAsOptionalDouble().AreEqual(Optional<double>.Of(FIRST_DOUBLE_VALUE));


    public override void TestFindAllAsDouble()
    => FindAllAsDouble().AreEqual(new List<double>() { FIRST_DOUBLE_VALUE, SECOND_DOUBLE_VALUE, THIRD_DOUBLE_VALUE });


    public override void TestFindFirstAsLong()
    => FindFirstAsLong().AreEqual((long)FIRST_DOUBLE_VALUE);


    public override void TestFindFirstAsOptionalLong()
    => FindFirstAsOptionalLong().AreEqual(Optional<long>.Of((long)FIRST_DOUBLE_VALUE));


    public override void TestFindAllAsLong()
    => FindAllAsLong().AreEqual(new List<long>() { (long)FIRST_DOUBLE_VALUE, (long)SECOND_DOUBLE_VALUE, (long)THIRD_DOUBLE_VALUE });
}
