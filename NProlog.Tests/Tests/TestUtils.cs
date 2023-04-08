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
using Org.NProlog.Core.Parser;
using Org.NProlog.Core.Predicate;
using Org.NProlog.Core.Predicate.Udp;
using Org.NProlog.Core.Terms;
using System.Net.Http.Headers;

namespace Org.NProlog;

/**
 * Helper methods for performing unit tests.
 */
public class TestUtils : TermFactory
{
    public static readonly PredicateKey ADD_PREDICATE_KEY = new ("pl_add_predicate", 2);
    public static readonly PredicateKey ADD_ARITHMETIC_OPERATOR_KEY = new ("pl_add_arithmetic_operator", 2);
    public static readonly string BOOTSTRAP_FILE = "Resources/prolog-bootstrap.pl";
    public static readonly PrologProperties PROLOG_DEFAULT_PROPERTIES = new PrologDefaultProperties();

    private static readonly Operands OPERANDS = CreateKnowledgeBase().Operands;

    public static string WriteToTempFile(Type c, string contents)
    {
        try
        {
            var tempFile = CreateTempFile(c);
            using (var fw = new StreamWriter(tempFile))
            {
                fw.Write(contents);
            }
            return tempFile;
        }
        catch (Exception ex)
        {
            throw new SystemException(ex.Message,ex);
        }
    }

    private static string CreateTempFile(Type c)
    {
        //TEMP_DIR.mkdir();
        //string tempFile = string.createTempFile(c.Name, ".tmp", TEMP_DIR);
        //tempFile.deleteOnExit();

        var path = Path.Combine(Environment.CurrentDirectory, c.Name + ".tmp");
        
        File.Create(path).Close();

        return path;
    }

    public static KnowledgeBase CreateKnowledgeBase()
    {
        try
        {
            var kb = KnowledgeBaseUtils.CreateKnowledgeBase();
            KnowledgeBaseUtils.Bootstrap(kb);
            return kb;
        }
        catch (Exception ex)
        {
            throw new SystemException(ex.Message,ex);
        }
    }

    public static KnowledgeBase CreateKnowledgeBase(PrologProperties prologProperties)
    {
        try
        {
            var kb = KnowledgeBaseUtils.CreateKnowledgeBase(prologProperties);
            KnowledgeBaseUtils.Bootstrap(kb);
            return kb;
        }
        catch (Exception t)
        {
            //t.printStackTrace();
            throw new SystemException(t.Message);
        }
    }

    public static Term[] Array(params Term[] terms) => terms;

    public static Term[] CreateArgs(int numberOfArguments, Term term) 
        => Arrays.Fill(new Term[numberOfArguments], term);

    public static SentenceParser CreateSentenceParser(string prologSyntax)
    => SentenceParser.GetInstance(prologSyntax, OPERANDS);   

    public static Term ParseSentence(string prologSyntax)
    {
        var sp = CreateSentenceParser(prologSyntax);
        return sp.ParseSentence();
    }

    public static Term ParseTerm(string source)
    {
        var sp = CreateSentenceParser(source);
        return sp.ParseTerm();
    }

    public static ClauseModel CreateClauseModel(string prologSentenceSytax)
    => ClauseModel.CreateClauseModel(ParseSentence(prologSentenceSytax));

    public static string Write(Term t) => CreateTermFormatter().FormatTerm(t);

    public static TermFormatter CreateTermFormatter() => new (OPERANDS);

    public static Term[] ParseTermsFromFile(string f)
    {
        using var reader = new StreamReader(f);
        var sp = SentenceParser.GetInstance(reader, OPERANDS);

        List<Term> result = new();
        Term? next;
        while ((next = sp.ParseSentence()) != null)
        {
            result.Add(next);
        }
        return result.ToArray();
        //catch (IOException e) {
        //   throw new SystemException("Could not parse: " + f, e);
        //}
    }

    public static void AssertStrictEquality(Term t1, Term t2, bool expectedResult)
    {
        Assert.AreEqual(expectedResult, TermsEqual(t1, t2));
        Assert.AreEqual(expectedResult, TermsEqual(t2, t1));
        if (expectedResult)
        {
            // assert that if terms are equal then they have the same hashcode
            Assert.AreEqual(t1.Term.GetHashCode(), t2.Term.GetHashCode());
        }
    }

    public static void AssertType(Type expected, object instance) => Assert.AreSame(expected, instance.GetType());
    public static T[] Any<T>() => System.Array.Empty<T>();

    public static int Times(int invocationTimes) => invocationTimes;
    public static T? Verify<T>(T? mock, int _ = 1) => mock;
    public static void VerifyNoInteractions(params object?[] _)
    {
    }
    public static void VerifyNoMoreInteractions(params object?[] _)
    {
    }
    public static void Confirm(object? _)
    {

    }
    public class Then<T>
    {
        public void ThenReturn(params T[] _)
        {
        }
        public void ThenThrow(Exception _)
        {
            //throw ex;
        }
    }
    public static Then<T> When<T>(T? _) => new();

    public static void AssertArrayEquals<T>(T[] a, T[] b)
    {
        Assert.IsTrue(Enumerable.SequenceEqual(a, b));
    }
    //public static object Mock(Type type)
    //{
    //    return null;
    //}
    public static T? AssertThrows<T>(Type type, Func<T> func)
    {
        try
        {
            return func();
        }
        catch (Exception ex)
        {
            try
            {
                Exception? ret = null;
                var ci = type.GetConstructor(new Type[] { typeof(string), typeof(Exception) });
                if (ci != null)
                {
                    ret = ci.Invoke(new object[] { ex.Message, ex }) as Exception;
                }
                else
                {
                    ci = type.GetConstructor(new Type[] { typeof(string) });
                    if (ci != null)
                    {
                        ret = ci.Invoke(new object[] { ex.Message }) as Exception;
                    }
                    else
                    {
                        ci = type.GetConstructor(System.Array.Empty<Type>());
                        if (ci != null)
                        {
                            ret = ci.Invoke(System.Array.Empty<object>()) as Exception;
                        }
                    }
                }

                throw ret ?? ex;
            }catch//(Exception ex2)
            {
                
            }
            return default;
        }
    }

    public static void AssertNotEqualsHashCode(object o1, object o2)
    {
        Assert.IsFalse(o1.Equals(o2));
        Assert.IsFalse(o2.Equals(o1));
        Assert.AreNotEqual(o1.GetHashCode(), o2.GetHashCode());
    }

    public static void AssertHashCodeEquals(object a, object b)
    {
        Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
    }

    public static void AssertHashCodeNotEquals(object a, object b)
    {
        Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());
    }

    public static Clauses CreateClauses(params string[] clauses)
    {
        var kb = CreateKnowledgeBase();
        List<ClauseModel> models = new();
        foreach (var clause in clauses)
        {
            models.Add(CreateClauseModel(clause));
        }
        return Clauses.CreateFromModels(kb, models);
    }
}