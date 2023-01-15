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
using Org.NProlog.Core.Kb;
using Org.NProlog.Core.Predicate;
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Math.Builtin;

[TestClass]
public class RandomTest
{
    /** Because random is not pure we do not want it to be preprocessed. */
    [TestMethod]
    public void TestNotPreprocessed()
    {
        KnowledgeBase kb = TestUtils.CreateKnowledgeBase();
        Term expression = TestUtils.ParseSentence("random(" + long.MinValue + ").");
        ArithmeticOperators operators = kb.ArithmeticOperators;
        Random r = (Random)operators.GetArithmeticOperator(PredicateKey.CreateForTerm(expression));

        Assert.IsFalse(r.IsPure);
        Assert.AreSame(r, r.Preprocess(expression));
        Assert.AreSame(r, r.Preprocess(expression));
    }
}
