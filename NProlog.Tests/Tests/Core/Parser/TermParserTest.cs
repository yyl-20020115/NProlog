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
public class TermParserTest : TestUtils
{
    [TestMethod]
    public void TestAtoms()
    {
        AssertNonVariableTerm(new Atom("x"), "x");
        AssertNonVariableTerm(new Atom("xyz"), "xyz");
        AssertNonVariableTerm(new Atom("xYz"), "xYz");
        AssertNonVariableTerm(new Atom("xYZ"), "xYZ");
    }

    [TestMethod]
    public void TestAtomsWithUnderscores()
    {
        AssertNonVariableTerm(new Atom("x_1"), "x_1");
        AssertNonVariableTerm(new Atom("xttRytf_uiu"), "xttRytf_uiu");
    }

    [TestMethod]
    public void TestAtomsEnclosedInSingleQuotes()
    {
        AssertNonVariableTerm(new Atom("Abc"), "'Abc'");
        AssertNonVariableTerm(new Atom("a B 1.2,3;4 !:$% c"), "'a B 1.2,3;4 !:$% c'");
    }

    [TestMethod]
    public void TestAtomsContainingSingleQuotes()
    {
        AssertNonVariableTerm(new Atom("'"), "''''");
        AssertNonVariableTerm(new Atom("Ab'c"), "'Ab''c'");
        AssertNonVariableTerm(new Atom("A'b''c"), "'A''b''''c'");
    }

    [TestMethod]
    public void TestAtomsWithSingleNonAlphanumericCharacter()
    {
        AssertNonVariableTerm(new Atom("~"), "~");
        AssertNonVariableTerm(new Atom("!"), "!");
    }

    [TestMethod]
    public void TestAtomsWithEscapedCharacters()
    {
        AssertNonVariableTerm(new Atom("\t"), "'\\t'");
        AssertNonVariableTerm(new Atom("\b"), "'\\b'");
        AssertNonVariableTerm(new Atom("\n"), "'\\n'");
        AssertNonVariableTerm(new Atom("\r"), "'\\r'");
        AssertNonVariableTerm(new Atom("\f"), "'\\f'");
        AssertNonVariableTerm(new Atom("\'"), "'\\''");
        AssertNonVariableTerm(new Atom("\""), "'\\\"'");
        AssertNonVariableTerm(new Atom("\\"), "'\\\\'");
        AssertNonVariableTerm(new Atom("abc\t\t\tdef\n"), "'abc\\t\\t\\tdef\\n'");
    }

    [TestMethod]
    public void TestAtomsWithUnicodeCharacters()
    {
        AssertNonVariableTerm(new Atom("Hello"), "'\u0048\u0065\u006C\u006c\u006F'");
        AssertNonVariableTerm(new Atom("Hello"), "'\u0048ello'");
        AssertNonVariableTerm(new Atom("Hello"), "'H\u0065l\u006co'");
        AssertNonVariableTerm(new Atom("Hello"), "'Hell\u006F'");
    }

    [TestMethod]
    public void TestIntegerNumbers()
    {
        for (int i = 0; i < 10; i++)
            AssertNonVariableTerm(new IntegerNumber(i), i.ToString());
        AssertNonVariableTerm(new IntegerNumber(long.MaxValue), long.MaxValue.ToString());
        AssertNonVariableTerm(new IntegerNumber(long.MinValue), long.MinValue.ToString());
    }
    [TestMethod]
    public void TestDecimalFractions()
    {
        double[] testData = { 0, 1, 2, 10, 3.14, 1.0000001, 0.2 };
        foreach (var d in testData)
        {
            AssertNonVariableTerm(new DecimalFraction(d),(d).PatchDoubleString());
            AssertNonVariableTerm(new DecimalFraction(-d), (-d).PatchDoubleString());
        }
        AssertNonVariableTerm(new DecimalFraction(3.4028235E38), "3.4028235E38");
        AssertNonVariableTerm(new DecimalFraction(3.4028235E38), "3.4028235e38");
        AssertNonVariableTerm(new DecimalFraction(340.28235), "3.4028235E2");
    }

    [TestMethod]
    public void TestInvalidDecimalFractions()
    {
        // must have extra digits after the 'e' or 'E'
        ParseInvalid("3.403e");
        ParseInvalid("3.403E");
    }

    [TestMethod]
    public void TestPredicates()
    {
        TestPredicate("p(a,b,c)");
        TestPredicate("p( a, b, c )");
        TestPredicate("p(1, a, [a,b,c|d])");
        TestPredicate("p(1, 'a b c?', [a,b,c|d])");
        TestPredicate("p(_Test1, _Test2, _Test3)");
        TestPredicate("~(a,b,c)");
        TestPredicate(">(a,b,c)");
    }

    private void TestPredicate(string syntax)
    {
        var t = ParseTerm(syntax);
        Assert.IsNotNull(t);
        Assert.AreSame(typeof(Structure), t.GetType());
    }

    [TestMethod]
    public void TestInvalidPredicateSyntax()
    {
        ParseInvalid("p(");
        ParseInvalid("p(a ,b, c");
        ParseInvalid("p(a b, c)");
        ParseInvalid("p(a, b c)");
    }

    [TestMethod]
    public void TestLists()
    {
        var a = new Atom("a");
        var b = new Atom("b");
        var c = new Atom("c");
        var d = new Atom("d");
        var e = new Atom("e");
        var f = new Atom("f");
        TestList("[a,b,c]", new Term[] { a, b, c }, null);
        TestList("[a,b,c|d]", new Term[] { a, b, c }, d);
        TestList("[ a, b, c | d ]", new Term[] { a, b, c }, d);
        TestList("[a,b,c|[d,e,f]]", new Term[] { a, b, c, d, e, f }, null);
        TestList("[a,b,c|[d,e|f]]", new Term[] { a, b, c, d, e }, f);
        TestList("[a,b,c|[]]]", new Term[] { a, b, c }, null);
    }

    private static void TestList(string input, Term[] expectedArgs, Term expectedTail)
    {
        var expected = expectedTail == null ? ListFactory.CreateList(expectedArgs) : ListFactory.CreateList(expectedArgs, expectedTail);
        var actual = ParseTerm(input);
        Assert.AreSame(TermType.LIST, actual.Type);
        Assert.IsTrue(actual is LinkedTermList);
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void TestInvalidListSyntax()
    {
        ParseInvalid("[a, b c]");
        ParseInvalid("[a, b; c]");
        ParseInvalid("[a, b, c");
        ParseInvalid("[a, b, c | d ");
        ParseInvalid("[a, b, c | d | e]");
        ParseInvalid("[a, b, c | d , e]");
    }

    [TestMethod]
    public void TestEmptyList() => Assert.AreSame(EmptyList.EMPTY_LIST, ParseTerm("[]"));

    [TestMethod]
    public void TestNoArgumentStructure() => ParseInvalid("p()");

    [TestMethod]
    public void TestListsUsingPredicateSyntax()
    {
        TestPredicate(".(1)");
        TestPredicate(".(1, 2, 3)");

        var a = new Atom("a");
        var b = new Atom("b");
        var c = new Atom("c");
        TestList(".(a, b)", new Term[] { a }, b);
        TestList(".(a, .(b, c))", new Term[] { a, b }, c);
        TestList(".(a, .(b, .(c, [])))", new Term[] { a, b, c }, EmptyList.EMPTY_LIST);

        TestList("'.'(a, '.'(b, '.'(c, [])))", new Term[] { a, b, c }, EmptyList.EMPTY_LIST);
    }

    [TestMethod]
    public void TestVariables()
    {
        AssertVariableTerm(new Variable("X"), "X");
        AssertVariableTerm(new Variable("XYZ"), "XYZ");
        AssertVariableTerm(new Variable("Xyz"), "Xyz");
        AssertVariableTerm(new Variable("XyZ"), "XyZ");
        AssertVariableTerm(new Variable("X_1"), "X_1");
        AssertVariableTerm(new Variable("_123"), "_123");
        AssertVariableTerm(new Variable("_Test"), "_Test");
    }

    [TestMethod]
    public void TestAnonymousVariable()
    {
        AssertVariableTerm(new Variable(), "_");
        AssertVariableTerm(new Variable("_"), "_");
    }

    // Check if a term appears twice in a single sentence whether two Term objects are created or if the same instance is referenced twice.
    // Currently only certain integer terms are cached.
    // A possible future performance improvement would be to enable caching for all terms.
    [TestMethod]
    public void TestNoCache()
    {
        var t = ParseTerm("p(1,a,0.5)=p(a,0.5,1).");

        // the integer number 1 will be reused due to th use of IntegerNumberCache
        Assert.AreEqual(t.GetArgument(0).GetArgument(0), t.GetArgument(1).GetArgument(2));
        Assert.AreSame(t.GetArgument(0).GetArgument(0), t.GetArgument(1).GetArgument(2));

        Assert.AreEqual(t.GetArgument(0).GetArgument(1), t.GetArgument(1).GetArgument(0));
        Assert.AreNotSame(t.GetArgument(0).GetArgument(1), t.GetArgument(1).GetArgument(0));

        Assert.AreEqual(t.GetArgument(0).GetArgument(2), t.GetArgument(1).GetArgument(1));
        Assert.AreNotSame(t.GetArgument(0).GetArgument(2), t.GetArgument(1).GetArgument(1));
    }

    private static void AssertNonVariableTerm(Term expected, string input)
    {
        var actual = ParseTerm(input);
        Assert.IsNotNull(actual);
        Assert.AreEqual(expected.GetType(), actual.GetType());
        Assert.AreEqual(expected.Type, actual.Type);
        Assert.AreEqual(expected.Name, actual.Name);
        Assert.AreEqual(expected.ToString(), actual.ToString());
        Assert.AreEqual(expected, actual);
    }

    private static void AssertVariableTerm(Term expected, string input)
    {
        var actual = ParseTerm(input);
        Assert.IsNotNull(actual);
        Assert.AreEqual(expected.GetType(), actual.GetType());
        Assert.AreEqual(expected.Type, actual.Type);
        Assert.AreEqual(expected.ToString(), actual.ToString());
        Assert.IsTrue(expected.Unify(actual));
    }

    private static void ParseInvalid(string source)
    {
        try
        {
            ParseTerm(source);
            Assert.Fail("No exception thrown parsing: " + source);
        }
        catch (ParserException e)
        {
            // expected
        }
        catch (Exception e)
        {
            Assert.Fail("parsing: " + source + " caused: " + e);
        }
    }
}
