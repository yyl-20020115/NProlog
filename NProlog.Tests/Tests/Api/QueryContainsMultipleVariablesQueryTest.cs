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
    private const string EXPECTED_ONE_VARIABLE_EXCEPTION_MESSAGE = "Expected exactly one uninstantiated variable but found X and Y";

    public QueryContainsMultipleVariablesQueryTest() : base("X=1, Y=2.") { }

    public override void TestFindFirstAsTerm()
        => FindFirstAsTerm().AssertException(EXPECTED_ONE_VARIABLE_EXCEPTION_MESSAGE);


    public override void TestFindFirstAsOptionalTerm()
        => FindFirstAsOptionalTerm().AssertException(EXPECTED_ONE_VARIABLE_EXCEPTION_MESSAGE);


    public override void TestFindAllAsTerm()
    => FindAllAsTerm().AssertException(EXPECTED_ONE_VARIABLE_EXCEPTION_MESSAGE);


    public override void TestFindFirstAsAtomName()
    => FindFirstAsAtomName().AssertException(EXPECTED_ONE_VARIABLE_EXCEPTION_MESSAGE);


    public override void TestFindFirstAsOptionalAtomName()
    => FindFirstAsOptionalAtomName().AssertException(EXPECTED_ONE_VARIABLE_EXCEPTION_MESSAGE);


    public override void TestFindAllAsAtomName()
    => FindAllAsAtomName().AssertException(EXPECTED_ONE_VARIABLE_EXCEPTION_MESSAGE);


    public override void TestFindFirstAsDouble()
    => FindFirstAsDouble().AssertException(EXPECTED_ONE_VARIABLE_EXCEPTION_MESSAGE);


    public override void TestFindFirstAsOptionalDouble()
    => FindFirstAsOptionalDouble().AssertException(EXPECTED_ONE_VARIABLE_EXCEPTION_MESSAGE);


    public override void TestFindAllAsDouble()
    => FindAllAsDouble().AssertException(EXPECTED_ONE_VARIABLE_EXCEPTION_MESSAGE);


    public override void TestFindFirstAsLong()
    => FindFirstAsLong().AssertException(EXPECTED_ONE_VARIABLE_EXCEPTION_MESSAGE);


    public override void TestFindFirstAsOptionalLong()
    => FindFirstAsOptionalLong().AssertException(EXPECTED_ONE_VARIABLE_EXCEPTION_MESSAGE);


    public override void TestFindAllAsLong()
    => FindAllAsLong().AssertException(EXPECTED_ONE_VARIABLE_EXCEPTION_MESSAGE);
}
