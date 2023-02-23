/*
 * Copyright 2013 S. Webber
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
using Org.NProlog.Core.Kb;
using Org.NProlog.Core.Predicate;
using Org.NProlog.Core.Predicate.Udp;

namespace Org.NProlog.Core.Parser;

[TestClass]
public class PrologSourceReaderTest : TestUtils
{
    [TestMethod]
    public void TestParseFileNotFound()
    {
        string f = new string("does_not_exist");
        try
        {
            PrologSourceReader.ParseFile(CreateKnowledgeBase(), f);
            Assert.Fail();
        }
        catch (PrologException e)
        {
            AssertMessageContainsText(e, 
                "Could not read prolog source from file: does_not_exist due to: System.IO.FileNotFoundException");
        }
    }

    [TestMethod]
    public void TestParserException()
    {
        var message = "While parsing arguments of test_dynamic expected ) or , but got: d";
        var lineWithSyntaxError = "test_dynamic(a,b,c d). % Line 3";
        try
        {
            var f = WriteToFile("test_dynamic(a,b).\n" + "test_dynamic(a,b,c).\n" + lineWithSyntaxError + "\n" + "test_dynamic(a,b,c,d,e).");
            PrologSourceReader.ParseFile(CreateKnowledgeBase(), f);
            Assert.Fail();
        }
        catch (PrologException e)
        {
            var p = (ParserException)e.InnerException;
            Assert.AreEqual(message + " Line: " + lineWithSyntaxError, p.Message);
            Assert.AreEqual(lineWithSyntaxError, p.Line);
            Assert.AreEqual(3, p.LineNumber);
            Assert.AreEqual(20, p.ColumnNumber);
            AssertParserExceptionDescription(p, message, lineWithSyntaxError);
        }
    }

    private static void AssertParserExceptionDescription(ParserException p, string message, string line)
    {
        var writer = new StringWriter();
        p.GetDescription(writer);
        writer.Close();
        var lines = writer.ToString().Split("\n");
        Assert.AreEqual(message, lines[0].Trim());
        Assert.AreEqual(line, lines[1].Trim());
        Assert.AreEqual("^", lines[2].Trim());
    }

    [TestMethod]
    public void TestDynamicKeywordForAlreadyDefinedFunction()
    {
        var kb = CreateKnowledgeBase();

        var f1 = WriteToFile("test_not_dynamic(a,b,c).");
        PrologSourceReader.ParseFile(kb, f1);

        var f2 = WriteToFile("?- dynamic(test_not_dynamic/3)." + "test_not_dynamic(x,y,z).");
        try
        {
            PrologSourceReader.ParseFile(kb, f2);
            Assert.Fail();
        }
        catch (PrologException e)
        {
            AssertMessageContainsText(e, "Predicate has already been defined and is not dynamic: test_not_dynamic/3");
        }
    }

    [TestMethod]
    public void TestDynamicKeyword()
    {
        var kb = CreateKnowledgeBase();
        var f = WriteToFile("?- dynamic(test_dynamic/3).\n"
                    + "test_dynamic(a,b).\n"
                    + "test_dynamic(a,b,c).\n"
                    + "test_dynamic(x,y,z).\n"
                    + "test_dynamic(q,w,e).\n"
                    + "test_dynamic(a,b,c,d).\n"
                    + "test_dynamic2(1,2,3).\n"
                    + "test_dynamic2(4,5,6).\n"
                    + "test_dynamic2(7,8,9).");
        PrologSourceReader.ParseFile(kb, f);

        AssertDynamicUserDefinedPredicate(kb, new ("test_dynamic", 3));
        AssertClauseModels(kb, new ("test_dynamic", 3), "a, b, c", "x, y, z", "q, w, e");

        AssertStaticUserDefinedPredicate(kb, new ("test_dynamic", 2));
        AssertStaticUserDefinedPredicate(kb, new ("test_dynamic", 4));
        AssertStaticUserDefinedPredicate(kb, new ("test_dynamic2", 3));
        AssertClauseModels(kb, new("test_dynamic2", 3), "1, 2, 3", "4, 5, 6", "7, 8, 9");
    }

    private static void AssertDynamicUserDefinedPredicate(KnowledgeBase kb, PredicateKey key)
    {
        Assert.AreSame(typeof(DynamicUserDefinedPredicateFactory), GetUserDefinedPredicate(kb, key).GetType());
    }

    private static void AssertStaticUserDefinedPredicate(KnowledgeBase kb, PredicateKey key)
    {
        var udp = GetUserDefinedPredicate(kb, key);
        Assert.AreSame(typeof(StaticUserDefinedPredicateFactory), udp.GetType());
    }

    private static void AssertClauseModels(KnowledgeBase kb, PredicateKey key, params string[] expectedArgs)
    {
        var udp = GetUserDefinedPredicate(kb, key);
        for (int i = 0; i < expectedArgs.Length; i++)
        {
            var actual = udp.GetClauseModel(i).Original.ToString();
            var expected = key.Name + "(" + expectedArgs[i] + ")";
            Assert.AreEqual(expected, actual);
        }
        Assert.IsNull(udp.GetClauseModel(expectedArgs.Length));
    }

    private static UserDefinedPredicateFactory GetUserDefinedPredicate(KnowledgeBase kb, PredicateKey key)
    {
        var ef = kb.Predicates.GetPredicateFactory(key);
        Assert.IsNotNull(ef);
        var udp = kb.Predicates.CreateOrReturnUserDefinedPredicate(key);
        Assert.AreSame(ef, udp);
        return udp;
    }

    private static void AssertMessageContainsText(PrologException e, string text) 
        => Assert.IsTrue(e.Message.IndexOf(text) > -1, e.Message);

    private string WriteToFile(string contents) 
        => WriteToTempFile(this.GetType(), contents);
}
