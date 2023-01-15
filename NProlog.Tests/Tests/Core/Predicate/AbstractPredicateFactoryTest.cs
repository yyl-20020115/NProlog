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
using Org.NProlog.Core.Predicate.Builtin.Construct;
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Predicate;

[TestClass]
public class AbstractPredicateFactoryTest : TestUtils
{
    [TestMethod]
    public void TestIllegalArgumentException()
    {
        AbstractPredicateFactory pf = new APF1();
        for (int i = 0; i < 100; i++)
        {
            AssertIllegalArgumentException(pf, i);
        }
    }
    public class APF1 : AbstractPredicateFactory
    {

    }
    public class APF0 : AbstractPredicateFactory
    {
        Atom arg1;
        Atom arg2;
        Atom arg3;

        Predicate noArgPredicate;
        Predicate oneArgPredicate;
        Predicate twoArgsPredicate;
        Predicate threeArgsPredicate;
        Predicate fourArgsPredicate;

        public APF0(Atom arg1, Atom arg2, Atom arg3, Predicate noArgPredicate, Predicate oneArgPredicate, Predicate twoArgsPredicate, Predicate threeArgsPredicate, Predicate fourArgsPredicate)
        {
            this.arg1 = arg1;
            this.arg2 = arg2;
            this.arg3 = arg3;
            this.noArgPredicate = noArgPredicate;
            this.oneArgPredicate = oneArgPredicate;
            this.twoArgsPredicate = twoArgsPredicate;
            this.threeArgsPredicate = threeArgsPredicate;
            this.fourArgsPredicate = fourArgsPredicate;
        }

        protected override Predicate GetPredicate()
        {
            return noArgPredicate;
        }


        protected override Predicate GetPredicate(Term t)
        {
            Assert.AreSame(arg1, t);
            return oneArgPredicate;
        }


        protected override Predicate GetPredicate(Term t1, Term t2)
        {
            Assert.AreSame(arg1, t1);
            Assert.AreSame(arg2, t2);
            return twoArgsPredicate;
        }


        protected override Predicate GetPredicate(Term t1, Term t2, Term t3)
        {
            Assert.AreSame(arg1, t1);
            Assert.AreSame(arg2, t2);
            Assert.AreSame(arg3, t3);
            return threeArgsPredicate;
        }


        protected override Predicate GetPredicate(Term t1, Term t2, Term t3, Term t4)
        {
            Assert.AreSame(arg1, t1);
            Assert.AreSame(arg2, t2);
            Assert.AreSame(arg3, t3);
            return fourArgsPredicate;
        }

    }

    [TestMethod]
    public void TestOverridenMethods()
    {
        Atom arg1 = Atom("a");
        Atom arg2 = Atom("b");
        Atom arg3 = Atom("c");
        Atom arg4 = Atom("d");

        Predicate noArgPredicate = CreatePredicate();
        Predicate oneArgPredicate = CreatePredicate();
        Predicate twoArgsPredicate = CreatePredicate();
        Predicate threeArgsPredicate = CreatePredicate();
        Predicate fourArgsPredicate = CreatePredicate();

        AbstractPredicateFactory pf = new APF0(
            arg1, arg2, arg3, noArgPredicate, oneArgPredicate, twoArgsPredicate, threeArgsPredicate, fourArgsPredicate);

        Assert.AreSame(noArgPredicate, pf.GetPredicate(new Term[0]));
        Assert.AreSame(oneArgPredicate, pf.GetPredicate(new Term[] { arg1 }));
        Assert.AreSame(twoArgsPredicate, pf.GetPredicate(new Term[] { arg1, arg2 }));
        Assert.AreSame(threeArgsPredicate, pf.GetPredicate(new Term[] { arg1, arg2, arg3 }));
        Assert.AreSame(fourArgsPredicate, pf.GetPredicate(new Term[] { arg1, arg2, arg3, arg4 }));

        AssertIllegalArgumentException(pf, 5);
    }

    public class Prd0 : Predicate
    {
        public bool Evaluate()
        {
            return false;
        }


        public bool CouldReevaluationSucceed => false;
    }
    private static Predicate CreatePredicate()
    {
        return new Prd0();
    }

    private static void AssertIllegalArgumentException(AbstractPredicateFactory pf, int numberOfArguments)
    {
        try
        {
            pf.GetPredicate(new Term[numberOfArguments]);
            Assert.Fail();
        }
        catch (ArgumentException e)
        {
            Assert.AreEqual("The predicate factory: " + pf.GetType().Name + " does not accept the number of arguments: " + numberOfArguments, e.Message);
        }
    }
}
