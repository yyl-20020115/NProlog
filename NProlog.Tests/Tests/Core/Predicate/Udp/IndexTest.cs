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
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Predicate.Udp;

[TestClass]
public class IndexTest : TestUtils
{
    private static readonly Atom A = Atom("a");
    private static readonly Atom B = Atom("b");
    private static readonly Atom C = Atom("c");
    private static readonly Atom D = Atom("d");
    private static readonly Atom E = Atom("e");
    private static readonly KnowledgeBase KB = CreateKnowledgeBase();

    [TestMethod]
    public void TestSingleArg()
    {
        // Create terms of 3 args, indexed by the 2nd argument.
        int[] positions = new int[] { 1 };
        Dictionary<object, ClauseAction[]> clauses = new();
        var e1 = new ClauseAction[] { Clause(A, B, C), Clause(C, B, A) };
        clauses.Add(B, e1);
        var e2 = new ClauseAction[] { Clause(A, C, B) };
        clauses.Add(C, e2);

        // Create index to be tested.
        var i = new Index(positions, clauses);

        // Assert getting matches where 2nd arg = B.
        Assert.AreSame(e1, i.GetMatches(new Term[] { A, B, C }));
        Assert.AreSame(e1, i.GetMatches(new Term[] { C, B, A }));
        Assert.AreSame(e1, i.GetMatches(new Term[] { D, B, E }));

        // Assert getting matches where 2nd arg = C.
        Assert.AreSame(e2, i.GetMatches(new Term[] { A, C, B }));
        Assert.AreSame(e2, i.GetMatches(new Term[] { D, C, E }));

        // Assert when no match the same zero Length arrays are always returned.
        var noMatches = i.GetMatches(new Term[] { C, A, B });
        Assert.AreEqual(0, noMatches.Length);
        Assert.AreSame(noMatches, i.GetMatches(new Term[] { B, D, C }));
    }

    [TestMethod]
    public void TestTwoArgs()
    {
        // Create terms of 3 args, indexed by the 1st and 3rd arguments.
        var positions = new int[] { 0, 2 };
        var kf = KeyFactories.GetKeyFactory(positions.Length);
        Dictionary<object, ClauseAction[]> clauses = new();
        var e1 = new ClauseAction[] { Clause(A, B, C), Clause(A, D, C) };
        clauses.Add(kf.CreateKey(positions, new Term[] { A, B, C }), e1);
        var e2 = new ClauseAction[] { Clause(A, B, D) };
        clauses.Add(kf.CreateKey(positions, new Term[] { A, B, D }), e2);

        // Create index to be tested.
        var i = new Index(positions, clauses);

        // Assert getting matches where 1st arg = A and 3rd arg = C.
        Assert.AreSame(e1, i.GetMatches(new Term[] { A, B, C }));
        Assert.AreSame(e1, i.GetMatches(new Term[] { A, D, C }));
        Assert.AreSame(e1, i.GetMatches(new Term[] { A, E, C }));

        // Assert getting matches where 1st arg = A and 3rd arg = C.
        Assert.AreSame(e2, i.GetMatches(new Term[] { A, B, D }));
        Assert.AreSame(e2, i.GetMatches(new Term[] { A, C, D }));

        // Assert when no match the same zero Length arrays are always returned.
        ClauseAction[] noMatches = i.GetMatches(new Term[] { A, C, B });
        Assert.AreEqual(0, noMatches.Length);
        Assert.AreSame(noMatches, i.GetMatches(new Term[] { D, A, E }));
    }

    [TestMethod]
    public void TestThreeArgs()
    {
        // Create terms of 3 args, indexed by all its arguments.
        var positions = new int[] { 0, 1, 2 };
        var kf = KeyFactories.GetKeyFactory(positions.Length);
        Dictionary<object, ClauseAction[]> clauses = new();
        var e1 = new ClauseAction[] { Clause(A, B, C) };
        clauses.Add(kf.CreateKey(positions, new Term[] { A, B, C }), e1);
        var e2 = new ClauseAction[] { Clause(A, B, D) };
        clauses.Add(kf.CreateKey(positions, new Term[] { A, B, D }), e2);

        // Create index to be tested.
        var i = new Index(positions, clauses);

        // Assert getting matches where 1st arg = A, 2nd arg = B and 3rd arg = C.
        Assert.AreSame(e1, i.GetMatches(new Term[] { A, B, C }));

        // Assert getting matches where 1st arg = A, 2nd arg = B and 3rd arg = D.
        Assert.AreSame(e2, i.GetMatches(new Term[] { A, B, D }));

        // Assert when no match the same zero Length arrays are always returned.
        var noMatches = i.GetMatches(new Term[] { A, C, B });
        Assert.AreEqual(0, noMatches.Length);
        Assert.AreSame(noMatches, i.GetMatches(new Term[] { A, C, E }));
    }

    private static ClauseAction Clause(Term t1, Term t2, Term t3) 
        => ClauseActionFactory.CreateClauseAction(KB, ClauseModel.CreateClauseModel(Structure("test", t1, t2, t3)));
}
