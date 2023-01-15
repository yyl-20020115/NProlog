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
using Org.NProlog.Api;
using Org.NProlog.Core.Exceptions;
using Org.NProlog.Core.Parser;
using Org.NProlog.Core.Predicate.Builtin.Construct;
using Org.NProlog.Core.Predicate.Udp;
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Predicate.Builtin.List;

[TestClass]
public class ListPredicateAssert : TestUtils
{
    private readonly string builtInPredicateName;
    private readonly string userDefinedPredicateName;
    private readonly int arity;
    private readonly Prolog prolog;

    public ListPredicateAssert(string builtInPredicateName, int arity, string prologSource)
    {
        this.builtInPredicateName = builtInPredicateName;
        this.userDefinedPredicateName = builtInPredicateName + "_";
        this.arity = arity;
        prolog = new Prolog();
        prolog.ConsultReader(new StringReader(prologSource));
        Predicates predicates = prolog.KnowledgeBase.Predicates;
        PredicateFactory builtInPredicateFactory = predicates.GetPredicateFactory(new PredicateKey(builtInPredicateName, arity));
        Assert.AreNotSame(typeof(UnknownPredicate), builtInPredicateFactory.GetType(), builtInPredicateName);
        PredicateFactory userDefinedPredicateFactory = predicates.GetPredicateFactory(new PredicateKey(userDefinedPredicateName, arity));
        Assert.AreSame(typeof(StaticUserDefinedPredicateFactory), userDefinedPredicateFactory.GetType(), userDefinedPredicateName);
    }

    public void AssertQuery(string query)
    {
        string query2 = query.Replace(builtInPredicateName, userDefinedPredicateName);
        Assert.AreNotEqual(query, query2);
        AssertQueries(query, query2);
    }

    public void AssertArgs(params string[] arguments)
    {
        Assert.AreEqual(arity, arguments.Length);
        AssertQueries(ConstructQuery(builtInPredicateName, arguments), ConstructQuery(userDefinedPredicateName, arguments));
    }

    private static string ConstructQuery(string predicateName, params string[] arguments)
    {
        return predicateName + "(" + string.Join(",", arguments) + ").";
    }

    private void AssertQueries(string query1, string query2)
    {
        QueryResult r1;
        try
        {
            r1 = prolog.ExecuteQuery(query1);
        }
        catch (PrologException /*| OutOfMemoryError */e1)
        {
            Assert.IsFalse(e1 is ParserException, e1.Message);
            try
            {
                prolog.ExecuteQuery(query2).Next();
                throw new SystemException("No exception " + query2, e1);
            }
            catch (PrologException /*| OutOfMemoryError*/ e2)
            {
                Assert.AreSame(e1.GetType(), e2.GetType());
                return;
            }
        }
        var r2 = prolog.ExecuteQuery(query2);
        var variableIds = r1.GetVariableIds();
        //Assert.AreEqual(StringUtils.ToString(variableIds),StringUtils.ToString(r2.GetVariableIds()));
        Assert.IsTrue(variableIds.SetEquals(r2.GetVariableIds()));
        int ctr = 0;
        while (true)
        {
            try
            {
                if (!r1.Next())
                {
                    break;
                }
            }
            catch (PrologException /*| OutOfMemoryError*/ e1)
            {
                try
                {
                    r2.Next();
                    throw new SystemException("No exception " + query2, e1);
                }
                catch (PrologException /*| OutOfMemoryError*/ e2)
                {
                    Assert.AreSame(e1.GetType(), e2.GetType());
                    return;
                }
            }

            Assert.IsTrue(r2.Next());
            foreach (string variableId in variableIds)
            {
                Term t1;
                try
                {
                    t1 = r1.GetTerm(variableId);
                }
                catch (StackOverflowException /*| OutOfMemoryError*/ e1)
                {
                    try
                    {
                        r2.GetTerm(variableId);
                        throw new SystemException("No exception " + query2, e1);
                    }
                    catch (StackOverflowException /*| OutOfMemoryError*/ e2)
                    {
                        Assert.AreSame(e1.GetType(), e2.GetType());
                        return;
                    }
                }
                Term t2 = r2.GetTerm(variableId);
                Assert.AreEqual(NumberVariables(t1).ToString(), NumberVariables(t2).ToString(), query1);
            }

            if (ctr++ > 50)
            {
                // to avoid getting stuck evaluating infinite queries, exit after max number of iterations
                // TODO make limit configurable
                return;
            }
        }

        Assert.IsFalse(r2.Next());
    }

    private static Term NumberVariables(Term t)
    {
        var Copy = t.Copy(new());
        new NumberVars().Evaluate(new Term[] { Copy });
        return Copy.Term;
    }
}
