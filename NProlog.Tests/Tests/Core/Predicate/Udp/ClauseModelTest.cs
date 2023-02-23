/*
 * Copyright 2013-2014 S. Webber
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


using Org.NProlog.Core.Exceptions;
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Predicate.Udp;

[TestClass]
public class ClauseModelTest
{
    [TestMethod]
    public void TestSingleTerm() => AssertClauseModel("a.", "a", "true");

    [TestMethod]
    public void TestSimpleImplication() => AssertClauseModel("a :- true.", "a", "true");

    [TestMethod]
    public void TestConjunctionImplication() => AssertClauseModel("a :- b, c, d.", "a", ",(,(b, c), d)");

    [TestMethod]
    public void TestDefinteClauseGrammer() => AssertClauseModel("a --> b, c.", "a(A2, A0)", ",(b(A2, A1), c(A1, A0))");

    [TestMethod]
    public void TestNumeric()
    {
        try
        {
            ClauseModel.CreateClauseModel(new IntegerNumber(7));
            Assert.Fail();
        }
        catch (PrologException e)
        {
            Assert.AreEqual("Expected an atom or a predicate but got a INTEGER with value: 7", e.Message);
        }
    }

    [TestMethod]
    public void TestVariable()
    {
        try
        {
            ClauseModel.CreateClauseModel(new Variable("X"));
            Assert.Fail();
        }
        catch (PrologException e)
        {
            Assert.AreEqual("Expected an atom or a predicate but got a VARIABLE with value: X", e.Message);
        }
    }

    private static void AssertClauseModel(string inputSyntax, string consequentSyntax, string antecedentSyntax)
    {
        var t = TestUtils.ParseSentence(inputSyntax);
        var cm = ClauseModel.CreateClauseModel(t);
        AssertToString(consequentSyntax, cm.Consequent);
        AssertToString(antecedentSyntax, cm.Antecedent);
    }

    private static void AssertToString(string syntax, Term t) 
        => Assert.AreEqual(syntax, t.ToString());
}
