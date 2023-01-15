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
namespace Org.NProlog.Api;

[TestClass]
public class QueryContainsMultipleVariablesQueryTest : AbstractQueryTest
{
    private static readonly string EXPECTED_ONE_VARIABLE_EXCEPTION_MESSAGE = "Expected exactly one uninstantiated variable but found X and Y";

    public QueryContainsMultipleVariablesQueryTest() : base("X=1, Y=2.") { }

    public override void TestFindFirstAsTerm()
        => FindFirstAsTerm().assertException(EXPECTED_ONE_VARIABLE_EXCEPTION_MESSAGE);


    public override void TestFindFirstAsOptionalTerm()
        => FindFirstAsOptionalTerm().assertException(EXPECTED_ONE_VARIABLE_EXCEPTION_MESSAGE);


    public override void TestFindAllAsTerm()
    => FindAllAsTerm().assertException(EXPECTED_ONE_VARIABLE_EXCEPTION_MESSAGE);


    public override void TestFindFirstAsAtomName()
    => FindFirstAsAtomName().assertException(EXPECTED_ONE_VARIABLE_EXCEPTION_MESSAGE);


    public override void TestFindFirstAsOptionalAtomName()
    => FindFirstAsOptionalAtomName().assertException(EXPECTED_ONE_VARIABLE_EXCEPTION_MESSAGE);


    public override void TestFindAllAsAtomName()
    => FindAllAsAtomName().assertException(EXPECTED_ONE_VARIABLE_EXCEPTION_MESSAGE);


    public override void TestFindFirstAsDouble()
    => FindFirstAsDouble().assertException(EXPECTED_ONE_VARIABLE_EXCEPTION_MESSAGE);


    public override void TestFindFirstAsOptionalDouble()
    => FindFirstAsOptionalDouble().assertException(EXPECTED_ONE_VARIABLE_EXCEPTION_MESSAGE);


    public override void TestFindAllAsDouble()
    => FindAllAsDouble().assertException(EXPECTED_ONE_VARIABLE_EXCEPTION_MESSAGE);


    public override void TestFindFirstAsLong()
    => FindFirstAsLong().assertException(EXPECTED_ONE_VARIABLE_EXCEPTION_MESSAGE);


    public override void TestFindFirstAsOptionalLong()
    => FindFirstAsOptionalLong().assertException(EXPECTED_ONE_VARIABLE_EXCEPTION_MESSAGE);


    public override void TestFindAllAsLong()
    => FindAllAsLong().assertException(EXPECTED_ONE_VARIABLE_EXCEPTION_MESSAGE);
}
