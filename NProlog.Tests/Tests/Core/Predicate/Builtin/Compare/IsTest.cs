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
using Org.NProlog.Core.Predicate.Udp;
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Predicate.Builtin.Compare;

[TestClass]
public class IsTest : TestUtils
{
    [TestMethod]
    public void TestPreprocessVariable()
    {
        var kb = CreateKnowledgeBase();
        var isTerm = ParseTerm("X is Y.");

        var _is = (Is)kb.Predicates.GetPredicateFactory(isTerm);
        Assert.AreSame(_is, _is.Preprocess(isTerm));
    }

    [TestMethod]
    public void TestPreprocessNumeric()
    {
        var kb = CreateKnowledgeBase();
        var isTerm = ParseTerm("X is 1.");

        var _is = (Is)kb.Predicates.GetPredicateFactory(isTerm);
        var optimised = _is.Preprocess(isTerm);
        Assert.AreEqual("Unify", optimised.GetType().Name);
        Assert.AreSame(PredicateUtils.TRUE, optimised.GetPredicate(isTerm.Args));
        Assert.AreEqual("is(1, 1)", isTerm.ToString());
    }

    [TestMethod]
    public void TestPreprocessBinaryExpressionWithoutVariable()
    {
        var kb = CreateKnowledgeBase();
        var isTerm = ParseTerm("X is 3 * 7.");

        var _is = (Is)kb.Predicates.GetPredicateFactory(isTerm);
        var optimised = _is.Preprocess(isTerm);
        Assert.AreEqual("Unify", optimised.GetType().Name);
        Assert.AreSame(PredicateUtils.TRUE, optimised.GetPredicate(isTerm.Args));
        Assert.AreEqual("is(21, *(3, 7))", isTerm.ToString());
    }

    [TestMethod]
    public void TestPreprocessBinaryExpressionWithVariable()
    {
        var kb = CreateKnowledgeBase();
        var isTerm = ParseTerm("F is C * 9 / 5 + 32.");

        var _is = (Is)kb.Predicates.GetPredicateFactory(isTerm);
        var optimised = _is.Preprocess(isTerm);
        Assert.AreEqual("PreprocessedIs", optimised.GetType().Name);

        Dictionary<Variable, Variable> sharedVariables = new();
        var Copy = isTerm.Copy(sharedVariables);
        Variable f = null;
        Variable c = null;
        foreach (Variable v in sharedVariables.Values)
        {
            if ("F".Equals(v.Id))
            {
                f = v;
            }
            else if ("C".Equals(v.Id))
            {
                c = v;
            }
        }
        c.Unify(new IntegerNumber(100));
        Assert.AreSame(PredicateUtils.TRUE, optimised.GetPredicate(Copy.Args));
        Assert.AreEqual("is(212, +(/(*(100, 9), 5), 32))", Copy.ToString());
        Assert.AreEqual(new IntegerNumber(212), f.Term);
    }

    [TestMethod]
    public void TestPreprocessUnaryExpressionWithoutVariable()
    {
        var kb = CreateKnowledgeBase();
        var isTerm = ParseTerm("X is abs(-3 * 7).");

        var _is = (Is)kb.Predicates.GetPredicateFactory(isTerm);
        var optimised = _is.Preprocess(isTerm);
        Assert.AreEqual("Unify", optimised.GetType().Name);
        Assert.AreSame(PredicateUtils.TRUE, optimised.GetPredicate(isTerm.Args));
        Assert.AreEqual("is(21, abs(*(-3, 7)))", isTerm.ToString());
    }

    [TestMethod]
    public void TestPreprocessUnaryExpressionWithVariable()
    {
        var kb = CreateKnowledgeBase();
        var isTerm = ParseTerm("X is abs(1+Y).");

        var _is = (Is)kb.Predicates.GetPredicateFactory(isTerm);
        PredicateFactory optimised = _is.Preprocess(isTerm);
        Assert.AreEqual("PreprocessedIs", optimised.GetType().Name);

        Dictionary<Variable, Variable> sharedVariables = new();
        var Copy = isTerm.Copy(sharedVariables);
        Variable x = null;
        Variable y = null;
        foreach (Variable v in sharedVariables.Values)
        {
            if ("X".Equals(v.Id))
            {
                x = v;
            }
            else if ("Y".Equals(v.Id))
            {
                y = v;
            }
        }
        y.Unify(new IntegerNumber(-7));
        Assert.AreSame(PredicateUtils.TRUE, optimised.GetPredicate(Copy.Args));
        Assert.AreEqual("is(6, abs(+(1, -7)))", Copy.ToString());
        Assert.AreEqual(new IntegerNumber(6), x.Term);
    }


    [TestMethod]
    public void TestPreprocessTimeTest()
    {
        var kb = CreateKnowledgeBase();
        var isTerm = ParseTerm("F is C * 9 / 5 + 32.");

        Is _is = (Is)kb.Predicates.GetPredicateFactory(isTerm);
        var optimised = _is.Preprocess(isTerm);

        Dictionary<Variable, Variable> sharedVariables = new();
        var Copy = isTerm.Copy(sharedVariables);
        Variable f = null;
        Variable c = null;
        foreach (Variable v in sharedVariables.Values)
        {
            if ("F".Equals(v.Id))
            {
                f = v;
            }
            else if ("C".Equals(v.Id))
            {
                c = v;
            }
        }
        c.Unify(new IntegerNumber(100));
        int numBatches = 1000;
        int batchSize = 10000;
        int betterCtr = 0;
        for (int i2 = 0; i2 < numBatches; i2++)
        {
            long now = DateTime.Now.Millisecond;
            for (int i = 0; i < batchSize; i++)
            {
                optimised.GetPredicate(Copy.Args);
                f.Backtrack();
            }
            long duration1 = DateTime.Now.Millisecond - now;
            now = DateTime.Now.Millisecond;
            for (int i = 0; i < batchSize; i++)
            {
                _is.GetPredicate(Copy.Args);
                f.Backtrack();
            }

            long duration2 = DateTime.Now.Millisecond - now;
            if (duration1 < duration2)
            {
                betterCtr++;
            }
        }
        //NOTICE: fixed betterCtr should be > 90%
        // confirm that preprocessed is faster more than 90% of the time
        Assert.IsTrue(betterCtr > numBatches * .9, "was: " + betterCtr);
    }
}
