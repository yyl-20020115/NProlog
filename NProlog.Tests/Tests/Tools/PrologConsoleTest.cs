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
using System.Text;
using System.Text.RegularExpressions;

namespace Org.NProlog.Tools;

[TestClass]
public class PrologConsoleTest : TestUtils
{
    private static readonly Regex ThreadIdRegex = new (@"\[\d+\]");
    private static readonly Regex TimingsRegex = new (@"\(\d+ ms\)");
    private const string ERROR_MESSAGE = "Invalid. Enter ; to continue or q to quit. ";
    private const string PROMPT = "?- ";
    private const string YES = "yes (0 ms)";
    private const string NO = "no (0 ms)";
    private static readonly string EXPECTED_HEADER = Concatenate("INFO Reading prolog source in: Resources\\prolog-bootstrap.pl from file system", "Prolog Console", "prolog.org", "");
    private static readonly string EXPECTED_FOOTER = Environment.NewLine + PROMPT + Environment.NewLine + YES + Environment.NewLine;
    private static readonly string QUIT_COMMAND = Concatenate("quit.");


    [TestMethod]
    public void TestTrue()
    {
        var input = CreateInput("true.");
        var expected = CreateExpectedOutput(PROMPT, YES);
        var actual = GetOutput(input);
        Compare(expected, actual);
    }

    [TestMethod]
    public void TestFail()
    {
        var input = CreateInput("fail.");
        var expected = CreateExpectedOutput(PROMPT, NO);
        var actual = GetOutput(input);
        Compare(expected, actual);
    }

    [TestMethod]
    public void TestSingleVariable()
    {
        var input = CreateInput("X = y.");
        var expected = CreateExpectedOutput(PROMPT, "X = y", "", YES);
        var actual = GetOutput(input);
        Compare(expected, actual);
    }

    [TestMethod]
    public void TestMultipleVariables()
    {
        var input = CreateInput("W=X, X=1+1, Y is W, Z is -W.");
        var expected = CreateExpectedOutput(PROMPT, "W = 1 + 1", "X = 1 + 1", "Y = 2", "Z = -2", "", YES);
        var actual = GetOutput(input);
        Compare(expected, actual);
    }

    [TestMethod]
    public void TestInvalidSyntax()
    {
        var input = CreateInput("X is 1 + 1");
        var expected = CreateExpectedOutput(PROMPT, "Error parsing query:", "Unexpected end of stream", "X is 1 + 1", "          ^");
        var actual = GetOutput(input);
        Compare(expected, actual);
    }

    /** Test inputting {@code ;} to continue evaluation and {@code q} to quit, plus validation of invalid input. */
    [TestMethod]
    public void TestRepeat()
    {
        var input = CreateInput("repeat.", ";", ";", "z", "", "qwerty", "q");
        var expected = CreateExpectedOutput(PROMPT, YES, YES, YES + ERROR_MESSAGE + ERROR_MESSAGE + ERROR_MESSAGE);
        var actual = GetOutput(input);
        Compare(expected, actual);
    }

    /** Tests {@code trace} functionality using query against terms input using {@code consult}>. */
    [TestMethod]
    public void TestConsultAndTrace()
    {
        var tempFile = CreateFileToConsult("test(a).", "test(b).", "test(c).");
        var path = tempFile;
        var input = CreateInput("consult('" + path.Replace("\\", "\\\\") + "').", "trace.", "test(X).", ";", ";");
        var expected = CreateExpectedOutput(PROMPT + "INFO Reading prolog source in: " + path + " from file system", "", YES, "", PROMPT, YES, "",
                    PROMPT + "[THREAD-ID] CALL test(X)", "[THREAD-ID] EXIT test(a)", "", "X = a", "", YES + "[THREAD-ID] REDO test(a)", "[THREAD-ID] EXIT test(b)", "", "X = b", "",
                    YES + "[THREAD-ID] REDO test(b)", "[THREAD-ID] EXIT test(c)", "", "X = c", "", YES);
        var actual = GetOutput(input);
        Compare(expected, actual);
    }

    private string CreateFileToConsult(params string[] lines)
    {
        return WriteToTempFile(this.GetType(), Concatenate(lines));
    }

    private static string CreateInput(params string[] lines)
    {
        return Concatenate(lines) + QUIT_COMMAND;
    }

    private static string CreateExpectedOutput(params string[] lines)
    {
        return EXPECTED_HEADER + Concatenate(lines) + EXPECTED_FOOTER;
    }

    private static string GetOutput(string input)
    {
        var writer = new StringWriter();
        var c = new PrologConsole(new StringReader(input), writer);
        c.Run(new List<string>());
        return writer.ToString();

    }

    private static void Compare(string expected, string actual)
    {
        var tidiedExpected = MakeSuitableForComparison(expected);
        var tidiedActual = MakeSuitableForComparison(actual);
        Assert.AreEqual(tidiedExpected, tidiedActual);
    }

    /**
     * Output from the console application is unpredictable - some information returned (that is incidental to the core
     * functionality) will vary between multiple executions of the same query against the same knowledge base. In order
     * to check the actual input meets our expectations we first need to "tidy it" to remove inconsistencies (i.e. thread
     * IDs and timings).
     */
    private static string MakeSuitableForComparison(string _in)
    {
        return ReplaceTimings(ReplaceThreadId(_in));
    }

    /**
     * Return a version of the input with the thread IDs removed.
     * <p>
     * Output sometimes includes thread IDs contained in square brackets. e.g.: <code>[31966667]</code>
     */
    private static string ReplaceThreadId(string _in) => ThreadIdRegex.Replace(_in, "[THREAD-ID]");

    /**
     * Return a version of the input with the timings removed.
     * <p>
     * Output sometimes Contains info on how long a query took to execute. e.g.: <code>(15 ms)</code>
     */
    private static string ReplaceTimings(string _in) => TimingsRegex.Replace(_in, "(n ms)");

    private static string Concatenate(params string[] lines)
    {
        var result = new StringBuilder();
        foreach (string line in lines)
        {
            result.Append(line);
            result.Append(LineSeparator);
        }
        return result.ToString();
    }
    private static string LineSeparator => Environment.NewLine;
}
