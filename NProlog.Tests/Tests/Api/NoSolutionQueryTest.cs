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
public class NoSolutionQueryTest : AbstractQueryTest
{
    private static readonly string NO_SOLUTION_EXCEPTION_MESSAGE = "No solution found.";

    public NoSolutionQueryTest() : base("X = true, fail.") { }


    public override void TestFindFirstAsTerm()
        => FindFirstAsTerm().assertException(NO_SOLUTION_EXCEPTION_MESSAGE);


    public override void TestFindFirstAsOptionalTerm()
        => FindFirstAsOptionalTerm().AreEqual(Optional<Term>.Empty());


    public override void TestFindAllAsTerm()
    => FindAllAsTerm().AreEqual(new());


    public override void TestFindFirstAsAtomName()
        => FindFirstAsAtomName().assertException(NO_SOLUTION_EXCEPTION_MESSAGE);


    public override void TestFindFirstAsOptionalAtomName()
        => FindFirstAsOptionalAtomName().AreEqual(Optional<string>.Empty());


    public override void TestFindAllAsAtomName()
    => FindAllAsAtomName().AreEqual(new());


    public override void TestFindFirstAsDouble()
    => FindFirstAsDouble().assertException(NO_SOLUTION_EXCEPTION_MESSAGE);


    public override void TestFindFirstAsOptionalDouble()
    => FindFirstAsOptionalDouble().AreEqual(Optional<double>.Empty());


    public override void TestFindAllAsDouble()
    => FindAllAsDouble().AreEqual(new());


    public override void TestFindFirstAsLong()
    => FindFirstAsLong().assertException(NO_SOLUTION_EXCEPTION_MESSAGE);


    public override void TestFindFirstAsOptionalLong()
    => FindFirstAsOptionalLong().AreEqual(Optional<long>.Empty());


    public override void TestFindAllAsLong()
    => FindAllAsLong().AreEqual(new());
}
