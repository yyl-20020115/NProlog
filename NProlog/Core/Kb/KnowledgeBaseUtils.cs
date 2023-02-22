/*
 * Copyright 2013 S. Webber
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using Org.NProlog.Core.Parser;
using Org.NProlog.Core.Predicate;
using Org.NProlog.Core.Terms;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Org.NProlog.Core.Kb;



/**
 * Helper methods for performing common tasks on {@link KnowledgeBase} instances.
 */
public static class KnowledgeBaseUtils
{
    /**
     * The functor of structures representing conjunctions ({@code ,}).
     */
    public const string CONJUNCTION_PREDICATE_NAME = ",";
    /**
     * The functor of structures representing implications ({@code :-}).
     */
    public const string IMPLICATION_PREDICATE_NAME = ":-";
    /**
     * The functor of structures representing questions (i.e. queries) ({@code ?-}).
     */
    public const string QUESTION_PREDICATE_NAME = "?-";

    /**
     * Private constructor as all methods are static.
     */

    /**
     * Constructs a new {@code KnowledgeBase} object using {@link ProjogDefaultProperties}
     */
    public static KnowledgeBase CreateKnowledgeBase() 
        => CreateKnowledgeBase(new PrologDefaultProperties());

    /**
     * Constructs a new {@code KnowledgeBase} object using the specified {@link ProjogProperties}
     */
    public static KnowledgeBase CreateKnowledgeBase(PrologProperties prologProperties)
        => new(prologProperties);

    /**
     * Consults the {@link ProjogProperties#getBootstrapScript()} for the {@code KnowledgeBase}.
     * <p>
     * This is a way to configure a new {@code KnowledgeBase} (i.e. plugging in {@link ArithmeticOperator} and
     * {@link PredicateFactory} instances).
     * <p>
     * When using {@link ProjogDefaultProperties} the resource parsed will be {@code projog-bootstrap.pl} (contained in
     * {@code projog-core.jar}).
     *
     * @see ProjogSourceReader#parseResource(KnowledgeBase, string)
     */
    public static void Bootstrap(KnowledgeBase kb)
        => PrologSourceReader.ParseResource(kb, kb.PrologProperties.BootstrapScript);

    /**
     * Returns list of all user defined predicates with the specified name.
     */
    public static List<PredicateKey> GetPredicateKeysByName(KnowledgeBase kb, string predicateName)
    {
        List<PredicateKey> matchingKeys = new();
        foreach (var key in kb.Predicates.GetUserDefinedPredicates().Keys)
            if (predicateName.Equals(key.Name))
                matchingKeys.Add(key);
        return matchingKeys;
    }

    /**
     * Returns {@code true} if the specified {@link Term} represents a question or directive, else {@code false}.
     * <p>
     * A {@link Term} is judged to represent a question if it is a structure a single argument and with a functor
     * {@link #QUESTION_PREDICATE_NAME} or {@link #IMPLICATION_PREDICATE_NAME}.
     */
    public static bool IsQuestionOrDirectiveFunctionCall(Term t)
        => t.Type == TermType.STRUCTURE && t.NumberOfArguments == 1
        && (QUESTION_PREDICATE_NAME.Equals(t.Name) || IMPLICATION_PREDICATE_NAME.Equals(t.Name));

    /**
     * Returns {@code true} if the predicate represented by the specified {@link Term} never succeeds on re-evaluation.
     */
    public static bool IsSingleAnswer(KnowledgeBase kb, Term term)
        => !term.Type.IsVariable && !kb.Predicates.GetPreprocessedPredicateFactory(term).IsRetryable;

    /**
     * Returns an array of all {@link Term}s that make up the conjunction represented by the specified {@link Term}.
     * <p>
     * If the specified {@link Term} does not represent a conjunction then it will be used as the only element in the
     * returned array.
     */
    public static Term[] ToArrayOfConjunctions(Term t)
    {
        List<Term> l = new();
        while (IsConjunction(t))
        {
            l.Insert(0, t.Args[1]);
            t = t.Args[0];
        }
        l.Insert(0, t);
        return l.ToArray();
    }

    /**
     * Returns {@code true} if the specified {@link Term} represent a conjunction, else {@code false}.
     * <p>
     * A {@link Term} is judged to represent a conjunction if is a structure with a functor of
     * {@link #CONJUNCTION_PREDICATE_NAME} and exactly two arguments.
     */
    public static bool IsConjunction(Term t)
        => t.Type == TermType.STRUCTURE && CONJUNCTION_PREDICATE_NAME.Equals(t.Name)
        && t.Args.Length == 2;

    /**
     * Returns a new object created using reflection.
     * <p>
     * The {@code input} parameter can be in one of two formats:
     * <ol>
     * <li>The class name - e.g. {@code System.String} - this will cause an attempt to create a new instance of the
     * specified class using its no argument constructor.</li>
     * <li>The class name <i>and</i> a method name (separated by a {@code /}) - e.g.
     * {@code java.util.Calendar/GetInstance} - this will cause an attempt to create a new instance of the class by
     * invoking the specified method (as a no argument static method) of the specified class.</li>
     * </ol>
     */
    public static T Instantiate<T>(KnowledgeBase knowledgeBase, string input)
    {
        var result = Instantiate<T>(input);

        if (result is KnowledgeBaseConsumer consumer)
            consumer.KnowledgeBase = knowledgeBase;

        return result;
    }

    // TODO share with KnowledgeBaseServiceLocator.newInstance pass KnowledgeBase to constructor

    public static Type GetTypeFor(string input,Type back = null)
    {

        var assemlby = typeof(KnowledgeBase).Assembly;
        var type = assemlby.GetType(input);

        if (type == null)
            type = Type.GetType(input);
        if (type == null)
            type = Assembly.GetEntryAssembly().GetType(input);
        if (type == null)
            type = Assembly.GetCallingAssembly().GetType(input);
        if (type == null)
            type = Assembly.GetExecutingAssembly().GetType(input);
        if(type == null && back != null)
        {
            type = back.Assembly.GetType(input);
        }
        return type??back;
    }
    public static T Instantiate<T>(string input)
    {
        if (!string.IsNullOrEmpty(input))
        {
            try
            {
                int i = input.IndexOf('/');
                if (i >= 0)
                {
                    var ts = input[..i];
                    var ms = input[(i + 1)..];

                    var type = GetTypeFor(ts, typeof(T));
                    var md = type.GetMethod(ms);
                    var obj = md?.Invoke(null, Array.Empty<object>());
                    return (obj is T r) ? r : default(T);
                }
                else
                {
                    var type = GetTypeFor(input,typeof(T));
                    if (type == null)
                        return default(T);
                    else if (type == typeof(string))
                    {
                        var ci = type.GetConstructor(new Type[] { typeof(char[]) });
                        var obj = ci?.Invoke(new object[] { new char[0] });
                        return (obj is T r) ? r : default(T);
                    }
                    else
                    {
                        var ci = type.GetConstructor(Array.Empty<Type>());
                        var obj = ci?.Invoke(Array.Empty<object>());
                        return (obj is T r) ? r : default(T);
                    }

                }
            }
            catch (Exception)
            {

            }
        }

        return default;
    }
}
