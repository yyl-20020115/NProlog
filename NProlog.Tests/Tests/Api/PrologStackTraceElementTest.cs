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
using Org.NProlog.Core.Predicate;
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Api;
/**
 * Simply tests get methods of {@link PrologStackTraceElement} (as that is the only functionality the class provides).
 * <p>
 * For a more thorough test, including how it is used by {@link Prolog#getStackTrace(Exception)}, see
 * {@link ProjogTest#testIOExceptionWhileEvaluatingQueries()}.
 */
[TestClass]
public class PrologStackTraceElementTest
{
    [TestMethod]
    public void Test()
    {
        var key = new PredicateKey("test", 1);
        Term term = new Atom("test");
        var e = new PrologStackTraceElement(key, term);
        Assert.AreSame(key, e.PredicateKey);
        Assert.AreSame(term, e.Term);
    }
}
