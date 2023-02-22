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

using Org.NProlog.Core.Kb;
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Predicate.Udp;

[TestClass]
public class TailRecursivePredicateMetaDataTest
{
    private readonly KnowledgeBase kb = TestUtils.CreateKnowledgeBase();
    private List<ClauseModel> clauses;


    [TestInitialize]
    public void SetUp()
    {
        clauses = null;
    }

    [TestMethod]
    public void TestPrefix()
    {
        SetClauses(":- prefix([],Ys).", "prefix([X|Xs],[X|Ys]) :- prefix(Xs,Ys).");
        AssertSingleResultTailRecursive("prefix([a],[a,b,c]).");
        AssertMultipleResultsTailRecursive("prefix(X,[a,b,c]).");
    }

    [TestMethod]
    public void TestAppend()
    {
        SetClauses(":- append([],Ys,Ys).", "append([X|Xs],Ys,[X|Zs]) :- append(Xs,Ys,Zs).");
        AssertSingleResultTailRecursive("append([a,b,c],[d,e,f],Zs).");
        AssertMultipleResultsTailRecursive("append(As,[X,Y|Ys],Zs)."); // query finds adjacent terms
    }

    [TestMethod]
    public void TestMember()
    {
        SetClauses(":- member(X,[X|Xs]).", "member(X,[Y|Ys]) :- member(X,Ys).");
        AssertMultipleResultsTailRecursive("member(a,[a,b,c]).");
        AssertMultipleResultsTailRecursive("member(X,[a,b,c]).");
    }

    [TestMethod]
    public void TestRepeat()
    {
        SetClauses("repeat(N).", "repeat(N) :- N > 1, N1 is N-1, repeat(N1).");
        AssertMultipleResultsTailRecursive("repeat(10000).");
    }

    [TestMethod]
    public void TestRepeatWithWrite()
    {
        SetClauses("writeAndRepeat(N) :- write(N).", "writeAndRepeat(N) :- N > 1, N1 is N-1, writeAndRepeat(N1).");
        AssertMultipleResultsTailRecursive("writeAndRepeat(10000).");
    }

    [TestMethod]
    public void TestRepeatWithSingleResultConjunction()
    {
        SetClauses("writeNewLineAndRepeat(N) :- write(N), nl.", "writeNewLineAndRepeat(N) :- N > 1, N1 is N-1, writeNewLineAndRepeat(N1).");
        AssertMultipleResultsTailRecursive("writeNewLineAndRepeat(10000).");
    }

    [TestMethod]
    public void TestNonTailRecursivePredicates()
    {
        AssertNotTailRecursive("t(Y) :- t(Y).");

        AssertNotTailRecursive("repeat(N).", "repeat(N) :- N > 1, N1 is N-1, repeat(N1), write(N1).");

        AssertNotTailRecursive("repeat(N).", "repeat(N) :- N > 1, N1 is N-1, repeat(N1).", "repeat(N) :- N < 1.");
    }

    private void AssertSingleResultTailRecursive(string input)
    {
        Term parsedSentence = TestUtils.ParseSentence(input);
        Assert.IsTrue(IsSingleResultTailRecursive(CopyClauses(), parsedSentence.Args));
    }

    private void AssertMultipleResultsTailRecursive(string input)
    {
        Term parsedSentence = TestUtils.ParseSentence(input);
        Assert.IsFalse(IsSingleResultTailRecursive(CopyClauses(), parsedSentence.Args));
    }

    private bool IsSingleResultTailRecursive(List<ClauseModel> facts, Term[] args)
    {
        TailRecursivePredicateMetaData metaData = TailRecursivePredicateMetaData.Create(kb, facts);
        Assert.IsNotNull(metaData);
        for (int i = 0; i < args.Length; i++)
        {
            if (metaData.isSingleResultIfArgumentImmutable[i] && args[i].Type.IsVariable == false)
            {
                return true;
            }
        }
        return false;
    }

    private List<ClauseModel> SetClauses(params string[] sentences)
    {
        clauses = new();

        foreach (string sentence in sentences)
        {
            ClauseModel clause = TestUtils.CreateClauseModel(sentence);
            clauses.Add(clause);
        }

        return clauses;
    }

    private List<ClauseModel> CopyClauses()
    {
        List<ClauseModel> Copy = new(clauses.Count);
        foreach (ClauseModel clause in clauses)
        {
            Copy.Add(clause.Copy());
        }
        return Copy;
    }

    private void AssertNotTailRecursive(params string[] prologClauses)
    {
        SetClauses(prologClauses);
        TailRecursivePredicateMetaData metaData = TailRecursivePredicateMetaData.Create(kb, clauses);
        Assert.IsNull(metaData);
    }
}
