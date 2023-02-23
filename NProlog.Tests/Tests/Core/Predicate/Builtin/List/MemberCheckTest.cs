/*
 * Copyright 2021 S. Webber
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
using Org.NProlog.Core.Kb;
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Predicate.Builtin.List;

[TestClass]
public class MemberCheckTest : TestUtils
{
    [TestMethod]
    public void TestPreprocessed()
    {
        var kb = CreateKnowledgeBase();
        var term = ParseTerm("memberchk(X, [a,b,c]).");
        var m = (MemberCheck)kb.Predicates.GetPredicateFactory(term);
        var optimised = m.Preprocess(term);
        Assert.AreEqual("PreprocessedMemberCheck", optimised.GetType().Name);
    }

    readonly string[] vs = {
               "[]",
               "[a|b]",
               "[X,Y,Z]",
               "[X,b,c]",
               "[a,X,c]",
               "[a,b,X]",
               "[[X],b,c]",
               "[[a,b,X],b,c]",
               "[a,[b|X],c]",
               "[a,b,c(X,b,c)]",
               "[a,b,c(a,X,c)]",
               "[a,b,c(a,b,X)]",};

    [TestMethod]
    public void TestNotPreprocessed()
    {
        foreach(var value in vs)
        {
            var kb = CreateKnowledgeBase();
            var term = ParseTerm("memberchk(X, " + value + ").");
            var m = (MemberCheck)kb.Predicates.GetPredicateFactory(term);

            var optimised = m.Preprocess(term);

            Assert.AreSame(m, optimised);
        }
    }
}
