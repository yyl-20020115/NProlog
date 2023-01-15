/*
 * Copyright 2013-2014 S. Webber
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
using Org.NProlog.Core.Exceptions;
using Org.NProlog.Core.Kb;
using Org.NProlog.Core.Predicate;
using Org.NProlog.Core.Predicate.Builtin.Compare;
using Org.NProlog.Core.Predicate.Udp;
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Parser;


/**
 * Populates a {@link KnowledgeBase} with clauses parsed from Prolog syntax.
 * <p>
 * <img src="doc-files/ProjogSourceReader.png">
 * </p>
 */
public class PrologSourceReader
{
    private readonly KnowledgeBase kb;
    private readonly Dictionary<PredicateKey, UserDefinedPredicateFactory> userDefinedPredicates = new();

    /**
     * Populates the KnowledgeBase with clauses defined in the file.
     *
     * @param kb the KnowledgeBase to add the clauses to
     * @param prologSourceFile source of the prolog syntax defining clauses to add to the KnowledgeBase
     * @throws org.projog.core.ProjogException if there is any problem parsing the syntax or adding the new clauses to the KnowledgeBase
     */
    public static void ParseFile(KnowledgeBase kb, string prologSourceFile)
    {
        try
        {
            NotifyReadingFromFileSystem(kb, prologSourceFile);
            using var reader = new StreamReader(prologSourceFile);
            var prologSourceReader = new PrologSourceReader(kb);
            prologSourceReader.Parse(reader);
        }
        catch(Exception ex)
        {
            throw new PrologException("Could not read prolog source from file: " + prologSourceFile + " due to: " + ex, ex);
        }
    }

    /**
     * Populates the KnowledgeBase with clauses defined in the specified resource.
     * <p>
     * If {@code prologSourceResourceName} refers to an existing file on the file system then that file is used as the
     * source of the prolog syntax else {@code prologSourceResourceName} is read from the classpath.
     *
     * @param kb the KnowledgeBase to add the clauses to
     * @param prologSourceResourceName source of the prolog syntax defining clauses to add to the KnowledgeBase
     * @throws org.projog.core.ProjogException if there is any problem parsing the syntax or adding the new clauses to the KnowledgeBase
     */
    public static void ParseResource(KnowledgeBase kb, string prologSourceResourceName)
    {
        try
        {
            //NotifyReadingFromResource(kb, prologSourceResourceName);
            using var reader = GetReader(kb, prologSourceResourceName);
            var prologSourceReader = new PrologSourceReader(kb);
            prologSourceReader.Parse(reader);
        }catch(Exception ex)
        {
            throw new PrologException("Could not read prolog source from resource: " + prologSourceResourceName, ex);
        }
    }

    /**
     * Populates the KnowledgeBase with clauses read from the TextReader.
     * <p>
     * Note that this method will call {@code close()} on the specified reader - regardless of whether this method
     * completes successfully or if an exception is thrown.
     *
     * @param kb the KnowledgeBase to add the clauses to
     * @param reader source of the prolog syntax defining clauses to add to the KnowledgeBase
     * @throws org.projog.core.ProjogException if there is any problem parsing the syntax or adding the new clauses to the KnowledgeBase
     */
    public static void ParseReader(KnowledgeBase kb, TextReader reader)
    {
        try
        {
            var prologSourceReader = new PrologSourceReader(kb);
            prologSourceReader.Parse(reader);
        }
        catch (Exception e)
        {
            throw new PrologException("Could not read prolog source from java.io.TextReader: " + reader, e);
        }
        finally
        {
            try
            {
                reader.Close();
            }
            catch (Exception e)
            {
            }
        }
    }

    /**
     * Creates a new {@code TextReader} for the specified resource.
     * <p>
     * If {@code resourceName} refers to an existing file on the filesystem then that file is used as the source of the
     * {@code TextReader}. If there is no existing file on the filesystem matching {@code resourceName} then an attempt is
     * made to read the resource from the classpath.
     */
    private static TextReader GetReader(KnowledgeBase kb, string resourceName)
    {
        var path = resourceName;
        if (File.Exists(path))
        {
            NotifyReadingFromFileSystem(kb, resourceName);
            return new StreamReader(resourceName);
        }
        else
        {
            NotifyReadingFromResource(kb, resourceName);
            var stream = Properties.Resources.ResourceManager.GetStream(path
                );
            if(stream == null)
                throw new PrologException("Cannot find resource: " + resourceName);
            return new StreamReader(stream);
        }
    }

    private static void NotifyReadingFromFileSystem(KnowledgeBase kb, string file)
    {
        kb.PrologListeners.NotifyInfo("Reading prolog source in: " + file + " from file system");
    }

    private static void NotifyReadingFromResource(KnowledgeBase kb, string resourceName)
    {
        kb.PrologListeners.NotifyInfo("Reading prolog source in: " + resourceName + " from classpath");
    }

    private PrologSourceReader(KnowledgeBase kb)
    {
        this.kb = kb;
    }

    private void Parse(TextReader reader)
    {
        try
        {
            ParseTerms(reader);
            AddUserDefinedPredicatesToKnowledgeBase();
        }
        finally
        {
            try
            {
                reader.Close();
            }
            catch (Exception e)
            {
            }
        }
    }

    private void ParseTerms(TextReader reader)
    {
        var sp = SentenceParser.GetInstance(reader, kb.Operands);
        Term t;
        while ((t = sp.ParseSentence()) != null)
        {
            if (KnowledgeBaseUtils.IsQuestionOrDirectiveFunctionCall(t))
            {
                ProcessQuestion(t);
            }
            else
            {
                StoreParsedTerm(t);
            }
        }
    }

    /**
     * @param t structure with name of {@code ?-} and a single argument.
     */
    private void ProcessQuestion(Term t)
    {
        var e = kb.Predicates.GetPredicate(t.GetArgument(0));
        if (e != null)
        {
            while (e.Evaluate() && e.CouldReevaluationSucceed)
            {
                // keep re-evaluating until fail
            }
        }
    }

    private void StoreParsedTerm(Term parsedTerm)
    {
        var clauseModel = ClauseModel.CreateClauseModel(parsedTerm);
        var parsedTermConsequent = clauseModel.Consequent;
        var userDefinedPredicate = CreateOrReturnUserDefinedPredicate(parsedTermConsequent);
        userDefinedPredicate.AddLast(clauseModel);
    }

    private UserDefinedPredicateFactory CreateOrReturnUserDefinedPredicate(Term t)
    {
        var key = PredicateKey.CreateForTerm(t);
        if (!userDefinedPredicates.TryGetValue(key,out var userDefinedPredicate))
        {   userDefinedPredicates.Add(key,
                userDefinedPredicate = new StaticUserDefinedPredicateFactory(kb, key));
        }
        return userDefinedPredicate;
    }

    private void AddUserDefinedPredicatesToKnowledgeBase()
    {
        foreach (var userDefinedPredicate in userDefinedPredicates.Values)
            kb.Predicates.AddUserDefinedPredicate(userDefinedPredicate);
        foreach (var userDefinedPredicate in userDefinedPredicates.Values)
            if (userDefinedPredicate is StaticUserDefinedPredicateFactory factory)
                factory.Compile();
    }
}
