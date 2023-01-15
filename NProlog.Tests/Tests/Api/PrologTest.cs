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
using Org.NProlog.Core.Math;
using Org.NProlog.Core.Predicate;
using Org.NProlog.Core.Predicate.Builtin.Flow;
using Org.NProlog.Core.Terms;
using System.Runtime.Intrinsics.Arm;
using System.Text;

namespace Org.NProlog.Api;

[TestClass]
public class PrologTest : TestUtils
{
    [TestMethod]
    public void TestSetUserOutput()
    {
        var prolog = new Prolog();

        var sw = new StringWriter();
        // given the user output has been reassigned to a new stream
        prolog.SetUserOutput(sw);

        // when we execute a query that writes to output
        prolog.ExecuteOnce("write(hello).");

        // then the new stream should be written to
        Assert.AreEqual("hello", sw.ToString());
    }

    [TestMethod]
    public void TestSetUserInput()
    {
        var prolog = new Prolog();

        // given the user input has been reassigned to a new stream
        prolog.SetUserInput(new StringReader("hello"));

        // when we execute a query that reads from input
        QueryResult result = prolog.ExecuteQuery("read(X).");
        result.Next();

        // then the new stream should be read from
        Assert.AreEqual("hello", TermUtils.GetAtomName(result.GetTerm("X")));
    }

    [TestMethod]
    public void TestAddPredicateFactory()
    {
        Prolog prolog = new Prolog();

        // associate testAddPredicateFactory/1 with an instance of RepeatSetAmount
        PredicateKey key = new PredicateKey("testAddPredicateFactory", 1);
        PredicateFactory pf = new RepeatSetAmount();
        prolog.AddPredicateFactory(key, pf);

        // confirm that queries can use testAddPredicateFactory/1
        QueryResult result = prolog.CreateStatement("testAddPredicateFactory(3).").ExecuteQuery();

        Assert.IsTrue(result.Next());
        Assert.IsTrue(result.Next());
        Assert.IsTrue(result.Next());
        Assert.IsFalse(result.Next()); // expect false on 4th attempt as used 3 as argument
    }

    public class AO : ArithmeticOperator
    {
        public Numeric Calculate(Term[] args)
        {
            Numeric n = TermUtils.CastToNumeric(args[0]);
            return new IntegerNumber(n.Long + 7);
        }

    }

    [TestMethod]
    public void TestArithmeticOperator()
    {
        Prolog prolog = new Prolog();

        // associate testArithmeticOperator/1 with an operator that adds 7 to its argument
        PredicateKey key = new PredicateKey("testArithmeticOperator", 1);
        ArithmeticOperator pf = new AO();
        prolog.AddArithmeticOperator(key, pf);

        // confirm that queries can use testAddPredicateFactory/1
        QueryResult result = prolog.CreateStatement("X is testArithmeticOperator(3).").ExecuteQuery();
        //TODO:
        //PrologTest(result.Next());
        //PrologTest(10, TermUtils.CastToNumeric(result.GetTerm("X")).PrologTest()); // 3 + 7 = 10
    }
    [TestMethod]
    public void TestCreatePlan()
    {
        Prolog prolog = new Prolog();
        QueryPlan plan = prolog.CreatePlan("X = 1.");
        QueryResult result = plan.ExecuteQuery();
        Assert.IsTrue(result.Next());
        Assert.AreEqual(1, result.GetLong("X"));
    }

    [TestMethod]
    public void TestCreateStatement()
    {
        Prolog prolog = new Prolog();
        QueryStatement statement = prolog.CreateStatement("X = 1.");
        QueryResult result = statement.ExecuteQuery();
        Assert.IsTrue(result.Next());
        Assert.AreEqual(1, result.GetLong("X"));
    }

    [TestMethod]
    public void TestExecuteQuery()
    {
        Prolog prolog = new Prolog();
        QueryResult result = prolog.ExecuteQuery("X = 1.");
        Assert.IsTrue(result.Next());
        Assert.AreEqual(1, result.GetLong("X"));
    }

    [TestMethod]
    public void TestExecuteOnceNoSolution()
    {
        Prolog prolog = new Prolog();
        try
        {
            prolog.ExecuteOnce("true, true, fail.");
            Assert.Fail();
        }
        catch (PrologException prologException)
        {
            Assert.AreEqual("Failed to find a solution for: ,(,(true, true), fail)", prologException.Message);
        }
    }

    /** Attempts to open a file that doesn't exist to see how non-PrologException exceptions are dealt with. */
    [TestMethod]
    public void TestIOExceptionWhileEvaluatingQueries()
    {
        AssertStackTraceOfIOExceptionWhileEvaluatingQueries(PROLOG_DEFAULT_PROPERTIES);
    }

    private void AssertStackTraceOfIOExceptionWhileEvaluatingQueries(PrologProperties prologProperties)
    {
        var p = new Prolog(prologProperties);
        var inputSource = new StringBuilder();
        inputSource.Append("x(A) :- fail. x(A) :- y(A). x(A). ");
        inputSource.Append("y(A) :- Q is 4 + 5, z(A, A, Q). ");
        inputSource.Append("z(A, B, C) :- fail. z(A, B, C) :- 7<3. z(A, B, C) :- open(A,'read',Z). z(A, B, C). ");
        var sr = new StringReader(inputSource.ToString());
        p.ConsultReader(sr);
        var s = p.CreateStatement("x('a_directory_that_doesnt_exist/another_directory_that_doesnt_exist/some_file.xyz').");
        var r = s.ExecuteQuery();
        try
        {
            r.Next();
            Assert.Fail();
        }
        catch (PrologException prologException)
        {
            var s1 = "Unable to open input for: a_directory_that_doesnt_exist/another_directory_that_doesnt_exist/some_file.xyz";
            var s2 = prologException.Message;
            var t = prologException.InnerException.GetType();
            Assert.AreEqual(s1, s2);
            Assert.IsTrue(t==typeof(DirectoryNotFoundException)||t==typeof(FileNotFoundException) );

            // retrieve and check stack trace elements
            PrologStackTraceElement[] elements = Prolog.GetStackTrace(prologException);
            Assert.AreEqual(3, elements.Length);
            AssertPrologStackTraceElement(elements[0], "z/3", ":-(z(A, B, C), open(A, read, Z))");
            AssertPrologStackTraceElement(elements[1], "y/1", ":-(y(A), ,(is(Q, +(4, 5)), z(A, A, Q)))");
            AssertPrologStackTraceElement(elements[2], "x/1", ":-(x(A), y(A))");

            // Write stack trace to OutputStream so it can be compared against the expected result.
            var ps = new StringWriter();

            p.PrintPrologStackTrace(prologException, ps);

            // Generate expected stack trace.
            var expectedResult = new StringBuilder();
            expectedResult.Append("z/3 clause: z(A, B, C) :- open(A, read, Z)");
            expectedResult.Append(LineSeparator());
            expectedResult.Append("y/1 clause: y(A) :- Q is 4 + 5 , z(A, A, Q)");
            expectedResult.Append(LineSeparator());
            expectedResult.Append("x/1 clause: x(A) :- y(A)");
            expectedResult.Append(LineSeparator());

            // Confirm contents of stack trace
            Assert.AreEqual(expectedResult.ToString(), ps.ToString());
        }
    }

    private static string LineSeparator()
    {
        return Environment.NewLine;
    }

    [TestMethod]
    public void TestFormatTerm()
    {
        Prolog p = new Prolog();
        Term inputTerm = TestUtils.ParseSentence("X is 1 + 1 ; 3 < 5.");
        Assert.AreEqual("X is 1 + 1 ; 3 < 5", p.FormatTerm(inputTerm));
    }

    private static void AssertPrologStackTraceElement(PrologStackTraceElement actual, string expectedKey, string expectedTerm)
    {
        Assert.AreEqual(expectedKey, actual.PredicateKey.ToString());
        Assert.AreEqual(expectedTerm, actual.Term.ToString());
    }
}