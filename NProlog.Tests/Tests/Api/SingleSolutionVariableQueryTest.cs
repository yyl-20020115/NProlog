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
    private static readonly string EXPECTED_NUMERIC_EXCEPTION_MESSAGE = "Expected Numeric but got: VARIABLE with value: X";
    private static readonly string EXPECTED_ATOM_EXCEPTION_MESSAGE = "Expected an atom but got: VARIABLE with value: X";

    public SingleSolutionVariableQueryTest() : base("var(X).") { }


    public override void TestFindFirstAsTerm()
    {
        Term result = FindFirstAsTerm().InvokeStatement();
        Assert.IsTrue(result.Type.IsVariable);
        Assert.AreEqual("X", ((Variable)result).Id);
    }


    public override void TestFindFirstAsOptionalTerm()
    {
        Optional<Term> result = FindFirstAsOptionalTerm().InvokeStatement();
        Assert.IsTrue(result.Value.Type.IsVariable);
        Assert.AreEqual("X", ((Variable)result.Value).Id);
    }


    public override void TestFindAllAsTerm()
    {
        List<Term> results = FindAllAsTerm().InvokeStatement();
        Assert.AreEqual(1, results.Count);
        Term result = results[(0)];
        Assert.IsTrue(result.Type.IsVariable);
        Assert.AreEqual("X", ((Variable)result).Id);
    }


    public override void TestFindFirstAsAtomName() => FindFirstAsAtomName().assertException(EXPECTED_ATOM_EXCEPTION_MESSAGE);


    public override void TestFindFirstAsOptionalAtomName() => FindFirstAsOptionalAtomName().assertException(EXPECTED_ATOM_EXCEPTION_MESSAGE);


    public override void TestFindAllAsAtomName()
    => FindAllAsAtomName().assertException(EXPECTED_ATOM_EXCEPTION_MESSAGE);


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
