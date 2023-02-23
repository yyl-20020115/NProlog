/*
 * Copyright 2018 S. Webber
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
using Org.NProlog.Core.Event;
using static Org.NProlog.Core.Event.SpyPoints;

namespace Org.NProlog;

/** Uses {@code prolog-test} to run Prolog code and Compare the results against expectations. */
[TestClass]
public class PrologTest
{
    private const string SOURCE_PROLOG_TESTS_DIR = ("src/test/prolog");
    private const string BUILTIN_PREDICATES_PACKAGE = "org.prolog.core.predicate.builtin";
    private const string EXTRACTED_PREDICATES_TESTS_DIR = ("target/prolog-predicate-tests-extracted-from-java");
    private const string BUILTIN_OPERATORS_PACKAGE = "org.prolog.core.math.builtin";
    private const string EXTRACTED_OPERATORS_TESTS_DIR = ("target/prolog-operator-tests-extracted-from-java");

    [TestMethod]
    public void PrologTests() => AssertSuccess(SOURCE_PROLOG_TESTS_DIR);

    [TestMethod]
    public void ExtractedPredicateTests()
    {
        Extract(EXTRACTED_PREDICATES_TESTS_DIR, BUILTIN_PREDICATES_PACKAGE);
        AssertSuccess(EXTRACTED_PREDICATES_TESTS_DIR);
    }

    [TestMethod]
    public void ExtractedOperatorTests()
    {
        Extract(EXTRACTED_OPERATORS_TESTS_DIR, BUILTIN_OPERATORS_PACKAGE);
        AssertSuccess(EXTRACTED_OPERATORS_TESTS_DIR);
    }

    public class PL : PrologListener
    {
        List<string> events;
        public PL(List<string> events) => this.events = events;
        public void OnInfo(string message) => Add(message);

        public void OnWarn(string message) => Add(message);

        public void OnRedo(SpyPointEvent @event) => Add(@event);

        public void OnFail(SpyPointEvent @event) => Add(@event);

        public void OnExit(SpyPointExitEvent @event) => Add(@event);

        public void OnCall(SpyPointEvent @event) => Add(@event);

        private void Add(object message) => events.Add(message.ToString());
    }


    /** Test that a user-defined predicate with many clauses can be interpreted. */
    [TestMethod]
    public void PredicateWithManyClauses()
    {
        var source = ("target/predicateTooLargeToCompileToJava.pl");
        int numClauses = 2000;
        using (var pw = new StreamWriter(source))
        {
            for (int i = 1; i <= numClauses; i++)
            {
                pw.WriteLine("test(X,Y):-Y is X+" + i + ".");
            }
            pw.WriteLine("%?- test(7,Y)");
            for (int i = 1; i <= numClauses; i++)
            {
                pw.WriteLine("% Y=" + (7 + i));
            }
        }

        List<string> events = new();
        var listener = new PL(events);

        // assert tests pass
        //assertSuccess(source, new PrologTestRunnerConfig()
        //{
        // public Prolog createProlog()
        //{
        //    return new Prolog(listener);
        //}
        //});

        // assert that notifications
        Assert.AreEqual(2, events.Count, events.ToString());
        Assert.AreEqual("Reading prolog source in: prolog-bootstrap.pl from classpath", events[0]);
        Assert.AreEqual("Reading prolog source in: target" + Environment.NewLine + "predicateTooLargeToCompileToJava.pl from file system", events[(1)]);
    }

    private static void AssertSuccess(string scriptsDir)
    {
        //    assertSuccess(scriptsDir, new PrologTestRunnerConfig()
        //    {


        //         public bool isParallel()
        //    {
        //        return true;
        //    }
        //});
    }

    //private void assertSuccess(string scriptsDir, PrologTestRunnerConfig prologSupplier)
    //{
    //    TestResults results = PrologTestRunner.runTests(scriptsDir, prologSupplier);
    //    Console.WriteLine(results.getSummary());
    //    results.assertSuccess();
    //}

    private static void Extract(string outputDir, string packageName)
    {
        //    PrologTestExtractorConfig config = new PrologTestExtractorConfig();
        //    config.setPrologTestsDirectory(outputDir);
        //    config.setRequireJavadoc(true);
        //    config.setRequireTest(true);
        //    config.setFileFilter(new FileFilter()
        //    {

        //         public bool accept(string f)
        //    {
        //        string name = f.getPath().replace(string.separatorChar, '.');
        //        return name.Contains(packageName);
        //    }
        //});
        //PrologTestExtractor.extractTests(config);
    }
}
