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
public class SingleSolutionVariableQueryTest : AbstractQueryTest
{
    private const string EXPECTED_NUMERIC_EXCEPTION_MESSAGE = "Expected Numeric but got: VARIABLE with value: X";
    private const string EXPECTED_ATOM_EXCEPTION_MESSAGE = "Expected an atom but got: VARIABLE with value: X";

    public SingleSolutionVariableQueryTest() : base("var(X).") { }


    public override void TestFindFirstAsTerm()
    {
        var result = FindFirstAsTerm().InvokeStatement();
        Assert.IsTrue(result.Type.IsVariable);
        Assert.AreEqual("X", ((Variable)result).Id);
    }


    public override void TestFindFirstAsOptionalTerm()
    {
        var result = FindFirstAsOptionalTerm().InvokeStatement();
        Assert.IsTrue(result.Value?.Type.IsVariable);
        Assert.AreEqual("X", (result.Value as Variable).Id);
    }


    public override void TestFindAllAsTerm()
    {
        var results = FindAllAsTerm().InvokeStatement();
        Assert.AreEqual(1, results.Count);
        var result = results[0];
        Assert.IsTrue(result.Type.IsVariable);
        Assert.AreEqual("X", ((Variable)result).Id);
    }


    public override void TestFindFirstAsAtomName() => FindFirstAsAtomName().AssertException(EXPECTED_ATOM_EXCEPTION_MESSAGE);


    public override void TestFindFirstAsOptionalAtomName() => FindFirstAsOptionalAtomName().AssertException(EXPECTED_ATOM_EXCEPTION_MESSAGE);


    public override void TestFindAllAsAtomName()
    => FindAllAsAtomName().AssertException(EXPECTED_ATOM_EXCEPTION_MESSAGE);


    public override void TestFindFirstAsDouble() => FindFirstAsDouble().AssertException(EXPECTED_NUMERIC_EXCEPTION_MESSAGE);


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
