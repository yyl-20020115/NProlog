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

namespace Org.NProlog.Core.Parser;

[TestClass]
public class SentenceParserTest : TestUtils
{
    [TestMethod]
    public void TestIncompleteSentences()
    {
        Error(":-");
        Error("a :-");
        Error("a :- .");
        Error(":- X is");
        Error(":- X is 1"); // no '.' character at end of sentence
        Error(":- X is p(a, b, c)"); // no '.' character at end of sentence
        Error(":- X is [a, b, c | d]"); // no '.' character at end of sentence
        Error(":- X = 'hello."); // no closing quote on atom
        Error(":- X = /*hello."); // no closing */ on comment
    }

    [TestMethod]
    public void TestIncompletePredicateSyntax()
    {
        Error(":- X is p(."); // no )
        Error(":- X is p()."); // no arguments
        Error(":- X is p(a, b."); // no )
        Error(":- X is p(a b)."); // no ,
    }

    [TestMethod]
    public void TestInvalidListSyntax()
    {
        Error(":- X is [."); // no ]
        Error(":- X is [a b."); // no , or |
        Error(":- X is [a, b."); // no ]
        Error(":- X is [a, b |."); // no tail
        Error(":- X is [a, b | ]."); // no tail
        Error(":- X is [a, b | c, d]."); // 2 args after |
    }

    [TestMethod]
    public void TestInvalidOperators()
    {
        Error("a xyz b.");
        Error("a $ b.");
        Error("a b.");
        Error("$ b.");
    }

    [TestMethod]
    public void TestInvalidOperatorOrder()
    {
        Error("1 :- 2 :- 3.");
        Error(":- X = 1 + 2 + 3 + 4 = 5.");
        Error("a ?- b.");
        Error("?- a ?- b.");
        Error("?- :- X.");
        Error("?- ?- true.");
    }

    [TestMethod]
    public void TestEquationPrecedence()
    {
        CheckEquation("(((1+2)-3)*4)/5", "/(*(-(+(1, 2), 3), 4), 5)");

        CheckEquation("1+2-3*4/5", "-(+(1, 2), /(*(3, 4), 5))");
        CheckEquation("1+2-3/4*5", "-(+(1, 2), *(/(3, 4), 5))");
        CheckEquation("1+2/3-4*5", "-(+(1, /(2, 3)), *(4, 5))");
        CheckEquation("1+2/3*4-5", "-(+(1, *(/(2, 3), 4)), 5)");
        CheckEquation("1/2+3*4-5", "-(+(/(1, 2), *(3, 4)), 5)");
        CheckEquation("1/2*3+4-5", "-(+(*(/(1, 2), 3), 4), 5)");

        CheckEquation("1+2+3+4+5+6+7+8+9+0", "+(+(+(+(+(+(+(+(+(1, 2), 3), 4), 5), 6), 7), 8), 9), 0)");
        CheckEquation("1*2+3+4+5+6+7+8+9+0", "+(+(+(+(+(+(+(+(*(1, 2), 3), 4), 5), 6), 7), 8), 9), 0)");
        CheckEquation("1+2+3+4+5*6+7+8+9+0", "+(+(+(+(+(+(+(+(1, 2), 3), 4), *(5, 6)), 7), 8), 9), 0)");
        CheckEquation("1+2+3+4+5+6+7+8+9*0", "+(+(+(+(+(+(+(+(1, 2), 3), 4), 5), 6), 7), 8), *(9, 0))");
        CheckEquation("1*2+3+4+5*6+7+8+9+0", "+(+(+(+(+(+(+(*(1, 2), 3), 4), *(5, 6)), 7), 8), 9), 0)");
        CheckEquation("1*2+3+4+5+6+7+8+9*0", "+(+(+(+(+(+(+(*(1, 2), 3), 4), 5), 6), 7), 8), *(9, 0))");
        CheckEquation("1+2+3+4+5*6+7+8+9*0", "+(+(+(+(+(+(+(1, 2), 3), 4), *(5, 6)), 7), 8), *(9, 0))");
        CheckEquation("1*2+3+4+5*6+7+8+9*0", "+(+(+(+(+(+(*(1, 2), 3), 4), *(5, 6)), 7), 8), *(9, 0))");

        CheckEquation("1*2*3*4*5*6*7*8*9*0", "*(*(*(*(*(*(*(*(*(1, 2), 3), 4), 5), 6), 7), 8), 9), 0)");
        CheckEquation("1+2*3*4*5*6*7*8*9*0", "+(1, *(*(*(*(*(*(*(*(2, 3), 4), 5), 6), 7), 8), 9), 0))");
        CheckEquation("1*2*3*4*5+6*7*8*9*0", "+(*(*(*(*(1, 2), 3), 4), 5), *(*(*(*(6, 7), 8), 9), 0))");
        CheckEquation("1*2*3*4*5*6*7*8*9+0", "+(*(*(*(*(*(*(*(*(1, 2), 3), 4), 5), 6), 7), 8), 9), 0)");
        CheckEquation("1+2*3*4*5+6*7*8*9*0", "+(+(1, *(*(*(2, 3), 4), 5)), *(*(*(*(6, 7), 8), 9), 0))");
        CheckEquation("1+2*3*4*5*6*7*8*9+0", "+(+(1, *(*(*(*(*(*(*(2, 3), 4), 5), 6), 7), 8), 9)), 0)");
        CheckEquation("1*2*3*4*5+6*7*8*9+0", "+(+(*(*(*(*(1, 2), 3), 4), 5), *(*(*(6, 7), 8), 9)), 0)");
        CheckEquation("1+2*3*4*5+6*7*8*9+0", "+(+(+(1, *(*(*(2, 3), 4), 5)), *(*(*(6, 7), 8), 9)), 0)");
    }

    [TestMethod]
    public void TestMultiTerm()
    {
        string[] sentences = { "p(A, B, C) :- A = 1 , B = 2 , C = 3", "p(X, Y, Z) :- X = 1 , Y = 2 , Z = 3", "p(Q, W, E) :- Q = 1 ; W = 2 ; E = 3" };
        string source = sentences[0] + ".\n" + sentences[1] + ". " + sentences[2] + ".";
        SentenceParser sp = GetSentenceParser(source);
        foreach (string sentence in sentences)
        {
            Term t = sp.ParseSentence();
            Assert.IsNotNull(t);
            Assert.AreEqual(sentence, Write(t));
        }
    }

    [TestMethod]
    public void TestVariables()
    {
        Term t = ParseSentence("test(A, A, _A, _A, B, _, _).");
        Variable a1 = (Variable)t.GetArgument(0);
        Variable a2 = (Variable)t.GetArgument(1);
        Variable _a1 = (Variable)t.GetArgument(2);
        Variable _a2 = (Variable)t.GetArgument(3);
        Variable b = (Variable)t.GetArgument(4);
        Variable _1 = (Variable)t.GetArgument(5);
        Variable _2 = (Variable)t.GetArgument(6);

        Assert.AreEqual("A", a1.Id);
        Assert.AreEqual("A", a2.Id);
        Assert.AreEqual("_A", _a1.Id);
        Assert.AreEqual("_A", _a2.Id);
        Assert.AreEqual("B", b.Id);
        Assert.AreEqual("_", _1.Id);
        Assert.AreEqual("_", _2.Id);

        // variables in same clause with same ID should reference the same object
        Assert.AreSame(a1, a2);
        Assert.AreSame(_a1, _a2);
        // variables in same clause with different IDs should never reference the same object
        Assert.AreNotSame(b, a1);
        Assert.AreNotSame(b, _a1);
        // different anonymous variables should never reference the same object (despite having the same variable ID)
        Assert.AreNotSame(_1, _2);
        // anonymous variables should never reference the same object as a named variable
        Assert.AreNotSame(_1, a1);
        Assert.AreNotSame(_1, _a1);
        Assert.AreNotSame(_1, b);
    }

    [TestMethod]
    public void TestConjunction()
    {
        AssertParse("a, b, c.", "a , b , c", ",(,(a, b), c)");
    }

    [TestMethod]
    public void TestBrackets1()
    {
        AssertParse("a(b,(c)).", "a(b, c)", "a(b, c)");
    }

    [TestMethod]
    public void TestBrackets2()
    {
        AssertParse("?- fail, fail ; true.", "?- fail , fail ; true", "?-(;(,(fail, fail), true))");
        AssertParse("?- fail, (fail;true).", "?- fail , (fail ; true)", "?-(,(fail, ;(fail, true)))");
    }

    [TestMethod]
    public void TestBrackets3()
    {
        AssertParse("?- X is 4*(2+3).", "?- X is 4 * (2 + 3)", "?-(is(X, *(4, +(2, 3))))");
    }

    [TestMethod]
    public void TestBrackets4()
    {
        AssertParse("?- Y = ( ## = @@ ).", "?- Y = (## = @@)", "?-(=(Y, =(##, @@)))");
    }

    [TestMethod]
    public void TestBrackets5()
    {
        AssertParse("?- X = a(b,(c)).", "?- X = a(b, c)", "?-(=(X, a(b, c)))");
    }

    [TestMethod]
    public void TestBrackets6()
    {
        AssertParse("X = ( A = 1 , B = 2 , C = 3).", "X = (A = 1 , B = 2 , C = 3)", "=(X, ,(,(=(A, 1), =(B, 2)), =(C, 3)))");
    }

    [TestMethod]
    public void TestParsingBrackets7()
    {
        AssertParse("X = (!).", "X = !", "=(X, !)");
    }

    [TestMethod]
    public void TestParsingBrackets8()
    {
        AssertParse("X = (a, !).", "X = (a , !)", "=(X, ,(a, !))");
    }

    [TestMethod]
    public void TestParsingBrackets9()
    {
        AssertParse("X = (a, !; b).", "X = (a , ! ; b)", "=(X, ;(,(a, !), b))");
    }

    [TestMethod]
    public void TestParsingBrackets10()
    {
        AssertParse("X = [a,'('|Y].", "X = [a,(|Y]", "=(X, .(a, .((, Y)))");
    }

    [TestMethod]
    public void TestParsingBrackets11()
    {
        AssertParse("a :- (b, c ; e), f.", "a :- b , c ; e , f", ":-(a, ,(;(,(b, c), e), f))");
    }

    [TestMethod]
    public void TestParsingBrackets12()
    {
        AssertParse("a :- z, (b, c ; e), f.", "a :- z , (b , c ; e) , f", ":-(a, ,(,(z, ;(,(b, c), e)), f))");
    }

    [TestMethod]
    public void TestExtraTextAfterFullStop()
    {
        SentenceParser sp = GetSentenceParser("?- consult(\'bench.pl\'). jkhkj");
        Term t = sp.ParseSentence();
        Assert.AreEqual("?-(consult(bench.pl))", t.ToString());
        try
        {
            sp.ParseSentence();
            Assert.Fail();
        }
        catch (ParserException pe)
        {
            // expected
        }
    }

    [TestMethod]
    public void TestMixtureOfPrefixInfixAndPostfixOperands()
    {
        AssertParse("a --> { 1 + -2 }.", "a --> { 1 + -2 }", "-->(a, {(}(+(1, -2))))");
    }

    /**
     * Test "xf" (postfix) associativity.
     * <p>
     * A "x" means that the argument can contain operators of <i>only</i> a lower level of priority than the operator
     * represented by "f".
     */
    [TestMethod]
    public void TestParseOperandXF()
    {
        Operands o = new Operands();
        o.AddOperand("~", "xf", 900);
        SentenceParser sp = SentenceParser.GetInstance("a ~.", o);
        Term t = sp.ParseSentence();
        Assert.AreEqual("~(a)", t.ToString());
        try
        {
            sp = SentenceParser.GetInstance("a ~ ~.", o);
            sp.ParseSentence();
            Assert.Fail();
        }
        catch (ParserException e)
        {
            // expected
        }
    }

    /**
     * Test "yf" (postfix) associativity.
     * <p>
     * A "y" means that the argument can contain operators of <i>the same</i> or lower level of priority than the
     * operator represented by "f".
     */
    [TestMethod]
    public void TestParseOperandYF()
    {
        Operands o = new Operands();
        o.AddOperand(":", "yf", 900);
        SentenceParser sp = SentenceParser.GetInstance("a : :.", o);
        Term t = sp.ParseSentence();
        Assert.AreEqual(":(:(a))", t.ToString());
    }

    [TestMethod]
    public void TestBuiltInPredicateNamesAsAtomArguments()
    {
        Check("[=]", ".(=, [])");
        Check("[=, = | =]", ".(=, .(=, =))");

        Check("[:-]", ".(:-, [])");
        Check("[:-, :- | :-]", ".(:-, .(:-, :-))");

        Check("p(?-)", "p(?-)");
        Check("p(:-)", "p(:-)");
        Check("p(<)", "p(<)");

        Check("p(1<1,is)", "p(<(1, 1), is)");
        Check("p(;, ',', :-, ?-)", "p(;, ,, :-, ?-)");

        Check("?- write(p(1, :-, 1))", "?-(write(p(1, :-, 1)))");
        Check("?- write(p(1, ',', 1))", "?-(write(p(1, ,, 1)))");
        Check("?- write(p(<,>,=))", "?-(write(p(<, >, =)))");

        // following fails as '\+' prefix operand has higher precedence than '/' infix operand
        // Note that need to specify '\+' as '\\\\+' (escape slash once for Java string literal and once for Prolog parser)
        Error("?- test('\\\\+'/1, 'abc').");
        // following works as explicitly specifying '/' as the functor of a structure
        Check("?- test('/'('\\\\+', 1), 'abc')", "?-(test(/(\\+, 1), abc))");

        Error("p(a :- b).");
        Check("p(:-(a, b))", "p(:-(a, b))");
        Check("p(':-'(a, b))", "p(:-(a, b))");
    }

    [TestMethod]
    public void TestListAfterPrefixOperator()
    {
        AssertParse("?- [a,b,c].", "?- [a,b,c]", "?-(.(a, .(b, .(c, []))))");
    }

    [TestMethod]
    public void TestSentenceTerminatorAsAtomName()
    {
        AssertParse("p(C) :- C=='.'.", "p(C) :- C == .", ":-(p(C), ==(C, .))");
    }

    [TestMethod]
    public void TestAlphaNumericPredicateName()
    {
        string expectedOutput = "is(X, ~(1, 1))";
        Check("X is '~'(1,1)", expectedOutput);
        Check("X is ~(1,1)", expectedOutput);
    }

    [TestMethod]
    public void TestInfixOperatorAsPredicateName()
    {
        string expectedOutput = "is(X, +(1, 1))";
        Check("X is '+'(1,1)", expectedOutput);
        Check("X is 1+1", expectedOutput);
        Check("X is +(1,1)", expectedOutput);
        Check("X = >(+(1,1),-2)", "=(X, >(+(1, 1), -2))");
    }

    private void CheckEquation(string input, string expected)
    {
        Check(input, expected);

        // apply same extra tests just because is easy to do...
        Check("X is " + input, "is(X, " + expected + ")");
        string conjunction = "X is " + input + ", Y is " + input + ", Z is " + input;
        string expectedConjunctionResult = ",(,(is(X, " + expected + "), is(Y, " + expected + ")), is(Z, " + expected + "))";
        Check(conjunction, expectedConjunctionResult);
        Check("?- " + conjunction, "?-(" + expectedConjunctionResult + ")");
        Check("test(X, Y, Z) :- " + conjunction, ":-(test(X, Y, Z), " + expectedConjunctionResult + ")");

        for (int n = 0; n < 10; n++)
        {
            input = input.Replace("" + n, "p(" + n + ")");
            expected = expected.Replace("" + n, "p(" + n + ")");
        }
    }

    private void Error(string input)
    {
        try
        {
            Term term = ParseSentence(input);
            Assert.Fail("parsing: " + input + " produced: " + term + " when expected an exception");
        }
        catch (ParserException pe)
        {
            // expected
        }
        catch (Exception e)
        {
            //e.printStackTrace();
            Assert.Fail("parsing: " + input + " produced: " + e + " when expected a ParserException");
        }
    }

    /**
     * @param input syntax (not including trailing .) to attempt to produce term for
     * @param expectedOutput what ToString method of Term should look like
     */
    private Term Check(string input, string expectedOutput)
    {
        Error(input);
        try
        {
            input += ".";
            Term t = ParseSentence(input);
            if (!expectedOutput.Equals(t.ToString()))
            {
                throw new Exception("got: " + t + " instead of: " + expectedOutput);
            }
            return t;
        }
        catch (Exception e)
        {
            throw new SystemException("Exception parsing: " + input + " " + e.GetType() + " " + e.Message, e);
        }
    }

    private void AssertParse(string input, string expectedFormatterOutput, string expectedToString)
    {
        Term t = ParseSentence(input);
        Assert.AreEqual(expectedFormatterOutput, Write(t));
        Assert.AreEqual(expectedToString, t.ToString());
    }

    private SentenceParser GetSentenceParser(string source)
    {
        return TestUtils.CreateSentenceParser(source);
    }
}
