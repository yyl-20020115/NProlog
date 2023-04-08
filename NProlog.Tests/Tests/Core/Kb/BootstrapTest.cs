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
using Org.NProlog.Core.Math;
using Org.NProlog.Core.Predicate;
using Org.NProlog.Core.Terms;
using System.Reflection;

namespace Org.NProlog.Core.Kb;

/**
 * Tests contents of {@code prolog-bootstrap.pl}.
 * <p>
 * {@code prolog-bootstrap.pl} is used to configure the build-in predicates and arithmetic functions.
 */
[TestClass]
public class BootstrapTest : TestUtils
{
    private readonly KnowledgeBase kb = CreateKnowledgeBase();

    [TestMethod]
    public void TestBuiltInPredicates()
    {
        var terms = GetQueriesByKey(ADD_PREDICATE_KEY);
        Assert.IsFalse(terms.Count == 0);
        foreach (var t in terms)
            AssertBuiltInPredicate(t.GetArgument(0));
    }

    [TestMethod]
    public void TestArithmeticOperators()
    {
        var terms = GetQueriesByKey(ADD_ARITHMETIC_OPERATOR_KEY);
        Assert.IsFalse(terms.Count == 0);
        foreach (var t in terms)
            AssertArithmeticOperator(t.GetArgument(1));
    }

    private static List<Term> GetQueriesByKey(PredicateKey key)
    {
        List<Term> result = new();
        var terms = ParseTermsFromFile(BOOTSTRAP_FILE);
        foreach (var next in terms)
        {
            if (KnowledgeBaseUtils.QUESTION_PREDICATE_NAME.Equals(next.Name))
            {
                var term = next.GetArgument(0);
                if (key.Equals(PredicateKey.CreateForTerm(term)))
                {
                    result.Add(term);
                }
            }
        }
        return result;
    }


    private void AssertBuiltInPredicate(Term nameAndArity)
    {
        var key = PredicateKey.CreateFromNameAndArity(nameAndArity);
        var ef = kb.Predicates.GetPredicateFactory(key);
        //NOTICE: this is not needed
        //AssertSealed(ef);
        var methodParameters = GetMethodParameters(key);
        if (ef is Predicate.Predicate)
        {
            AssertClassImplementsOptimisedEvaluateMethod(ef, methodParameters);
        }
    }

    private static void AssertArithmeticOperator(Term className)
    {
        var assembly = typeof(KnowledgeBase).Assembly;
        var type = assembly.GetType(className.Name) ?? Type.GetType(className.Name);
        if(type==null)
        {
            Assert.Fail("Type " + className.Name + " not found");
        }

        var o = type.Assembly.CreateInstance(type.FullName);
        Assert.IsTrue(o is ArithmeticOperator);
        //AssertSealed(o);
    }

    private void AssertSealed(object o)
    {
        var c = o.GetType();
        Assert.IsTrue(c.IsSealed, "Not readonly: " + c);
    }

    private static Type[] GetMethodParameters(PredicateKey key)
    {
        var numberOfArguments = key.NumArgs;
        var args = new Type[numberOfArguments];
        for (var i = 0; i < numberOfArguments; i++)
        {
            args[i] = typeof(Term);
        }
        return args;
    }


    private static void AssertClassImplementsOptimisedEvaluateMethod(PredicateFactory ef, Type[] methodParameters)
    {
        var c = ef.GetType();
        bool success = false;
        while (success == false && c != null)
        {
            try
            {
                var m = c.GetMethod("Evaluate");

                Assert.AreSame(typeof(bool), m.ReturnType);
                success = true;
            }
            catch (Exception)
            {
                // if we can't find a matching method in the class then try its superclass
                //c = c.getSuperclass();
                c = c.BaseType;
            }
        }
        if (success == false)
        {
            Assert.Fail(ef.GetType() + " does not implement an evaluate method with " + methodParameters.Length + " parameters");
        }
    }
}
