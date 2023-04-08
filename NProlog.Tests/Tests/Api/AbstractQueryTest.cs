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
using Org.NProlog.Core.Exceptions;
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Api;

/**
 * Tests various methods of both {@link QueryStatement} and {@link QueryResult} against the same Prolog query.
 */
[TestClass]
public abstract class AbstractQueryTest : TestUtils
{
    private static int nextMethodId = 1;
    private readonly Prolog prolog;
    private readonly string query;
    private readonly StatementMethod<Term> findFirstAsTerm;
    private readonly StatementMethod<Optional<Term>> findFirstAsOptionalTerm;
    private readonly StatementMethod<List<Term>> findAllAsTerm;
    private readonly StatementMethod<string> findFirstAsAtomName;
    private readonly StatementMethod<Optional<string>> findFirstAsOptionalAtomName;
    private readonly StatementMethod<List<string>> findAllAsAtomName;
    private readonly StatementMethod<double> findFirstAsDouble;
    private readonly StatementMethod<Optional<double>> findFirstAsOptionalDouble;
    private readonly StatementMethod<List<double>> findAllAsDouble;
    private readonly StatementMethod<long> findFirstAsLong;
    private readonly StatementMethod<Optional<long>> findFirstAsOptionalLong;
    private readonly StatementMethod<List<long>> findAllAsLong;

    private static int METHOD_INVOCATIONS_CTR;

    public AbstractQueryTest(string query)
    {
        this.prolog = new Prolog();
        this.query = query;
        var s = prolog.CreateStatement(this.query);
        var q = prolog.CreatePlan(this.query);

        findFirstAsTerm = new(s.FindFirstAsTerm, q.FindFirstAsTerm);
        findFirstAsOptionalTerm = new(s.FindFirstAsOptionalTerm, q.FindFirstAsOptionalTerm);
        findAllAsTerm = new(s.FindAllAsTerm, q.FindAllAsTerm);
        findFirstAsAtomName = new(s.FindFirstAsAtomName, q.FindFirstAsAtomName);
        findFirstAsOptionalAtomName = new(s.FindFirstAsOptionalAtomName, q.FindFirstAsOptionalAtomName);
        findAllAsAtomName = new(s.FindAllAsAtomName, q.FindAllAsAtomName);
        findFirstAsDouble = new(s.FindFirstAsDouble, q.FindFirstAsDouble);
        findFirstAsOptionalDouble = new(s.FindFirstAsOptionalDouble, q.FindFirstAsOptionalDouble);
        findAllAsDouble = new(s.FindAllAsDouble, q.FindAllAsDouble);
        findFirstAsLong = new(s.FindFirstAsLong, q.FindFirstAsLong);
        findFirstAsOptionalLong = new(s.FindFirstAsOptionalLong, q.FindFirstAsOptionalLong);
        findAllAsLong = new(s.FindAllAsLong, q.FindAllAsLong);
    }

    public AbstractQueryTest(string query, string clauses) : this(query) => prolog.ConsultReader(new StringReader(clauses));

    [TestInitialize]
    public static void BeforeClass() => METHOD_INVOCATIONS_CTR = 0;

    [TestCleanup]
    public static void AfterClass()
    {
        Assert.AreEqual(0b111111111111, METHOD_INVOCATIONS_CTR, "not all methods have been asserted");
    }
    [TestMethod]
    public abstract void TestFindFirstAsTerm();

    protected StatementMethod<Term> FindFirstAsTerm()
    => findFirstAsTerm;

    [TestMethod]
    public abstract void TestFindFirstAsOptionalTerm();

    protected StatementMethod<Optional<Term>> FindFirstAsOptionalTerm()
    => findFirstAsOptionalTerm;

    [TestMethod]
    public abstract void TestFindAllAsTerm();

    protected StatementMethod<List<Term>> FindAllAsTerm()
    => findAllAsTerm;

    [TestMethod]
    public abstract void TestFindFirstAsAtomName();

    protected StatementMethod<string> FindFirstAsAtomName()
    => findFirstAsAtomName;

    [TestMethod]
    public abstract void TestFindFirstAsOptionalAtomName();

    protected StatementMethod<Optional<string>> FindFirstAsOptionalAtomName()
    => findFirstAsOptionalAtomName;

    [TestMethod]
    public abstract void TestFindAllAsAtomName();

    protected StatementMethod<List<string>> FindAllAsAtomName()
    => findAllAsAtomName;

    [TestMethod]
    public abstract void TestFindFirstAsDouble();

    protected StatementMethod<double> FindFirstAsDouble()
    => findFirstAsDouble;

    [TestMethod]
    public abstract void TestFindFirstAsOptionalDouble();

    protected StatementMethod<Optional<double>> FindFirstAsOptionalDouble()
    => findFirstAsOptionalDouble;

    [TestMethod]
    public abstract void TestFindAllAsDouble();

    protected StatementMethod<List<double>> FindAllAsDouble()
    => findAllAsDouble;

    [TestMethod]
    public abstract void TestFindFirstAsLong();

    protected StatementMethod<long> FindFirstAsLong() => findFirstAsLong;

    [TestMethod]
    public abstract void TestFindFirstAsOptionalLong();

    protected StatementMethod<Optional<long>> FindFirstAsOptionalLong() => findFirstAsOptionalLong;

    [TestMethod]
    public abstract void TestFindAllAsLong();

    protected StatementMethod<List<long>> FindAllAsLong() => findAllAsLong;

    public class StatementMethod<T>
    {
        public readonly Func<T> statementMethod;
        public readonly Func<T> planMethod;
        public readonly int id;
        public Prolog prolog;
        public string query;

        public StatementMethod(Func<T> statementMethod, Func<T> planMethod)
        {
            this.statementMethod = statementMethod;
            this.planMethod = planMethod;
            this.id = AbstractQueryTest.nextMethodId;
            AbstractQueryTest.nextMethodId <<= 1;
        }


        public T InvokeStatement() => statementMethod.Invoke();

        public void AreEqual(T expected)
        {
            var s = CreateStatement();
            Assert.AreEqual(expected, statementMethod.Invoke());

            // run twice to confirm QueryPlan is reusable
            var p = prolog.CreatePlan(query);
            Assert.AreEqual(expected, planMethod.Invoke());
            Assert.AreEqual(expected, planMethod.Invoke());
        }

        public void AssertException(string expectedMessage)
        {
            var s = CreateStatement();
            try
            {
                statementMethod.Invoke();
                Assert.Fail();
            }
            catch (PrologException e)
            {
                Assert.AreEqual(expectedMessage, e.Message);
            }

            var p = prolog.CreatePlan(query);
            try
            {
                planMethod.Invoke();
                Assert.Fail();
            }
            catch (PrologException e)
            {
                Assert.AreEqual(expectedMessage, e.Message);
            }
        }

        private QueryStatement CreateStatement()
        {
            var s = prolog.CreateStatement(query);
            if ((METHOD_INVOCATIONS_CTR & id) != 0)
            {
                throw new InvalidOperationException(id + " has been invoked twice by the same test");
            }
            METHOD_INVOCATIONS_CTR += id;
            return s;
        }
    }
}
