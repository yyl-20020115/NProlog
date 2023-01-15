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
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Predicate.Udp;

[TestClass]
public class DefiniteClauseGrammerConvertorTest
{
    [TestMethod]
    public void TestIsDCG()
    {
        Assert.IsTrue(IsDCG("a --> b."));
        Assert.IsTrue(IsDCG("a --> b, c, d."));

        Assert.IsFalse(IsDCG("a :- b."));
    }

    private static bool IsDCG(string inputSyntax)
    {
        Term input = TestUtils.ParseSentence(inputSyntax);
        return DefiniteClauseGrammerConvertor.IsDCG(input);
    }

    [TestMethod]
    public void TestSingleAtomAntecedent()
    {
        PerformConversion("a --> b.", "a(A1, A0) :- b(A1, A0)");
    }

    [TestMethod]
    public void TestTwoAtomAntecedent()
    {
        PerformConversion("a --> b, c.", "a(A2, A0) :- b(A2, A1) , c(A1, A0)");
    }

    [TestMethod]
    public void TestFiveAtomAntecedent()
    {
        PerformConversion("a --> b, c, d, e, f.", "a(A5, A0) :- b(A5, A4) , c(A4, A3) , d(A3, A2) , e(A2, A1) , f(A1, A0)");
    }

    [TestMethod]
    public void TestSingleElementListAntecedent()
    {
        PerformConversion("a --> [xyz].", "a([xyz|A], A)");
    }

    [TestMethod]
    public void TestConjunctionOfSingleElementListsAntecedent()
    {
        PerformConversion("test1 --> [a], [b], [c].", "test1([a,b,c|A0], A0)");
    }

    [TestMethod]
    public void TestMixtureOfAtomsAndSingleElementLists()
    {
        PerformConversion("test1 --> p1, p2, p3, [a,x,y], p4, [c], p5.",
                    "test1(A7, A0) :- p1(A7, A6) , p2(A6, A5) , p3(A5, A4) , A4 = [a,x,y|A3] , p4(A3, A2) , A2 = [c|A1] , p5(A1, A0)");
        PerformConversion("test1 --> p1, p2, p3, [a,x,y], [b], p4, [c], p5.",
                    "test1(A7, A0) :- " + "p1(A7, A6) , " + "p2(A6, A5) , " + "p3(A5, A4) , " + "A4 = [a,x,y,b|A3] , " + "p4(A3, A2) , " + "A2 = [c|A1] , " + "p5(A1, A0)");

        PerformConversion("a --> b, [test].", "a(A2, A0) :- b(A2, A1) , A1 = [test|A0]");

        PerformConversion("a --> [test], b.", "a([test|A1], A0) :- b(A1, A0)");
    }

    [TestMethod]
    public void TestPredicateAsAntecedentAndConsequent()
    {
        PerformConversion("a(Y) --> b(X).", "a(Y, A1, A0) :- b(X, A1, A0)");
        PerformConversion("a(1,Y,X) --> b(2,X,Y).", "a(1, Y, X, A1, A0) :- b(2, X, Y, A1, A0)");
    }

    [TestMethod]
    public void TestCurlyBrackets()
    {
        PerformConversion("test1(X) --> [X], {atom(X)}.", "test1(X, [X|A0], A0) :- atom(X)");
        PerformConversion("test2(X) --> {Y is X+1}, xyz(Y).", "test2(X, A1, A0) :- Y is X + 1 , xyz(Y, A1, A0)");
        PerformConversion("test1(X) --> p, {atom(X)}.", "test1(X, A1, A0) :- p(A1, A0) , atom(X)");
    }

    [TestMethod]
    public void TestSingleListAntecedent()
    {
        PerformConversion("test(qwerty) --> [qwerty].", "test(qwerty, [qwerty|A], A)");
        PerformConversion("test(qwerty) --> [x].", "test(qwerty, [x|A], A)");
    }

    private static void PerformConversion(string inputSyntax, string expectedOutputSyntax)
    {
        Term input = TestUtils.ParseSentence(inputSyntax);
        Term output = DefiniteClauseGrammerConvertor.Convert(input);
        Assert.AreEqual(expectedOutputSyntax, TestUtils.Write(output));
    }
}
