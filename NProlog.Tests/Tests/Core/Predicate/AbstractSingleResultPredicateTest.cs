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
using Org.NProlog.Core.Kb;
using Org.NProlog.Core.Predicate;
using Org.NProlog.Core.Predicate.Udp;
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Predicate;

[TestClass]
public class AbstractSingleResultPredicateTest : TestUtils
{
    private static readonly Atom ARG1 = Atom("a");
    private static readonly Atom ARG2 = Atom("b");
    private static readonly Atom ARG3 = Atom("c");
    private static readonly Atom ARG4 = Atom("d");

    public class TestPredicate : AbstractSingleResultPredicate
    {
        public KnowledgeBase kb;


        protected override void Init()
        {
            kb = base.KnowledgeBase;
        }
    }

    /**
     * Check {@code AbstractSingletonPredicate#KnowledgeBase=(KnowledgeBase)} invokes
     * {@code AbstractSingletonPredicate#init()} after setting the knowledge base.
     */
    [TestMethod]
    public void TestInit()
    {
        TestPredicate pf = new TestPredicate();
        KnowledgeBase kb = TestUtils.CreateKnowledgeBase();
        pf.KnowledgeBase = (kb);
        Assert.AreSame(kb, pf.kb);
        Assert.AreSame(kb, pf.KnowledgeBase);
    }

    public class ASPX : AbstractSingleResultPredicate
    {

    }
    [TestMethod]
    public void TestIllegalArgumentException()
    {
        AbstractSingleResultPredicate pf = new ASPX();

        for (int i = 0; i < 100; i++)
        {
            AssertIllegalArgumentException(pf, i);
        }
    }

    public class ASP0 : AbstractSingleResultPredicate
    {
        AtomicBoolean ab;
        public ASP0(AtomicBoolean ab)
        {
            this.ab = ab;
        }

        protected override bool Evaluate()
        {
            return ab.Value;
        }
    }

    [TestMethod]
    public void TestNoArguments()
    {
        AtomicBoolean ab = new AtomicBoolean();
        AbstractSingleResultPredicate pf = new ASP0(ab);

        ab.Value = true;
        Assert.AreSame(PredicateUtils.TRUE, pf.GetPredicate(new Term[0]));

        ab.Value = false;
        Assert.AreSame(PredicateUtils.FALSE, pf.GetPredicate(new Term[0]));
    }

    public class ASP1 : AbstractSingleResultPredicate
    {
        protected override bool Evaluate(Term t)
        {
            return t == ARG1;
        }
    };
    [TestMethod]
    public void TestOneArgument()
    {
        AbstractSingleResultPredicate pf = new ASP1();

        Assert.AreSame(PredicateUtils.TRUE, pf.GetPredicate(new Term[] { ARG1 }));

        Assert.AreSame(PredicateUtils.FALSE, pf.GetPredicate(new Term[] { ARG2 }));
    }

    public class ASP2 : AbstractSingleResultPredicate
    {
        protected override bool Evaluate(Term t1, Term t2)
        {
            return t1 == ARG1 && t2 == ARG2;
        }
    }
    [TestMethod]
    public void TestTwoArguments()
    {
        AbstractSingleResultPredicate pf = new ASP2();

        Assert.AreSame(PredicateUtils.TRUE, pf.GetPredicate(new Term[] { ARG1, ARG2 }));

        Assert.AreSame(PredicateUtils.FALSE, pf.GetPredicate(new Term[] { ARG1, ARG1 }));
    }

    public class ASP3 : AbstractSingleResultPredicate
    {

        protected override bool Evaluate(Term t1, Term t2, Term t3)
        {
            return t1 == ARG1 && t2 == ARG2 && t3 == ARG3;
        }
    }
    [TestMethod]
    public void TestThreeArguments()
    {
        AbstractSingleResultPredicate pf = new ASP3(); ;

        Assert.AreSame(PredicateUtils.TRUE, pf.GetPredicate(new Term[] { ARG1, ARG2, ARG3 }));

        Assert.AreSame(PredicateUtils.FALSE, pf.GetPredicate(new Term[] { ARG1, ARG1, ARG1 }));
    }
    public class ASP4 : AbstractSingleResultPredicate
    {

        protected override bool Evaluate(Term t1, Term t2, Term t3, Term t4)
        {
            return t1 == ARG1 && t2 == ARG2 && t3 == ARG3 && t4 == ARG4;
        }
    }
    [TestMethod]
    public void TestFourArguments()
    {
        AbstractSingleResultPredicate pf = new ASP4();

        Assert.AreSame(PredicateUtils.TRUE, pf.GetPredicate(new Term[] { ARG1, ARG2, ARG3, ARG4 }));

        Assert.AreSame(PredicateUtils.FALSE, pf.GetPredicate(new Term[] { ARG1, ARG1, ARG1, ARG1 }));
    }

    private static void AssertIllegalArgumentException(AbstractSingleResultPredicate pf, int numberOfArguments)
    {
        try
        {
            pf.GetPredicate(new Term[numberOfArguments]);
            Assert.Fail();
        }
        catch (Exception e)
        {
            Assert.AreEqual("The predicate factory: " + pf.GetType().Name + " does not accept the number of arguments: " + numberOfArguments, e.Message);
        }
    }
}
