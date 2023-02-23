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
using Org.NProlog.Api;
using Org.NProlog.Core.Event;
using Org.NProlog.Core.Exceptions;
using Org.NProlog.Core.Kb;
using Org.NProlog.Core.Parser;
using Org.NProlog.Core.Predicate;
using System.Text;

namespace Org.NProlog.Tools;

/**
 * Command line interface to Prolog.
 * <p>
 * Provides a mechanism for users to interact with Projog via a read-evaluate-Write loop (REPL).
 * </p>
 * <img src="doc-files/ProjogConsole.png">
 */
public class PrologConsole
{
    /** Command user can enter to exit the console application. */
    private static readonly PredicateKey QUIT_COMMAND = new ("quit", 0);
    private const string CONTINUE_EVALUATING = ";";
    private const string STOP_EVALUATING = "q";
    private const string STOP_EVALUATING_COMMAND = "quit.";

    private readonly TextReader reader;
    private readonly TextWriter writer;
    private readonly Prolog prolog;
    private bool quit;


    public class ConsoleResultPredicate : AbstractSingleResultPredicate
    {
        private readonly PrologConsole console;
        public ConsoleResultPredicate(PrologConsole console) => this.console = console;
        protected override bool Evaluate() => console.quit = true;
    }

    public PrologConsole(TextReader reader, TextWriter writer)
    {
        this.reader = (reader);
        this.writer = writer;
        this.prolog = new Prolog(new LoggingPrologListener(writer));
        this.prolog.AddPredicateFactory(QUIT_COMMAND, new ConsoleResultPredicate(this));
    }

    public void Run(List<string> startupScriptFilenames)
    {
        writer.WriteLine("Prolog Console");
        writer.WriteLine("prolog.org");

        ConsultScripts(startupScriptFilenames);

        while (!quit)
        {
            WritePrompt();

            var inputSyntax = reader.ReadLine();
            if (inputSyntax == null)
                quit = true;
            else if (IsNotEmpty(inputSyntax))
                ParseAndExecute(inputSyntax);
        }
    }

    private void WritePrompt()
    {
        writer.WriteLine();
        writer.Write(KnowledgeBaseUtils.QUESTION_PREDICATE_NAME + " ");
    }

    private static bool IsNotEmpty(string input) => input!=null && input.Trim().Length > 0;

    private void ConsultScripts(List<string> scriptFilenames)
    {
        foreach (var startupScriptName in scriptFilenames)
            ConsultScript(startupScriptName);
    }

    private void ConsultScript(string startupScriptName)
    {
        try
        {
            prolog.ConsultFile(startupScriptName);
        }
        catch (Exception e)
        {
            writer.WriteLine();
            ProcessThrowable(e);
        }
    }

    private void ParseAndExecute(string inputSyntax)
    {
        try
        {
            var s = prolog.CreatePlan(inputSyntax).CreateStatement();
            var r = s.ExecuteQuery();
            var variableIds = r.GetVariableIds();
            while (EvaluateOnce(r, variableIds) && ShouldContinue())
            {
                // keep evaluating the query
            }
            writer.WriteLine();
        }
        catch (ParserException pe)
        {
            writer.WriteLine();
            writer.WriteLine("Error parsing query:");
            pe.GetDescription(writer);
        }
        catch (Exception e)
        {
            writer.WriteLine(e.StackTrace);
            writer.WriteLine();
            ProcessThrowable(e);
            prolog.PrintPrologStackTrace(e);
        }
    }

    private bool ShouldContinue()
    {
        while (true)
        {
            var input = reader.ReadLine();
            switch (input)
            {
                case null:
                    return false;
                case CONTINUE_EVALUATING:
                    return true;
                case STOP_EVALUATING:
                    return false;
                case STOP_EVALUATING_COMMAND:
                    return false;
                default:
                    writer.Write("Invalid. Enter ; to continue or q to quit. ");
                    break;
            }
        }
    }

    private void ProcessThrowable(Exception e)
    {
        if (e is ParserException pe)
        {
            writer.WriteLine($"ParserException at line: {pe.LineNumber}");
            pe.GetDescription(writer);
        }
        else if (e is PrologException)
        {
            writer.WriteLine(e.Message);
            var cause = e.InnerException;
            if (cause != null)
                ProcessThrowable(cause);
        }
        else
        {
            var builder = new StringBuilder();
            builder.Append("Caught: ");
            builder.Append(e.GetType().Name);
            //StackTraceElement ste = e.StackTrace;
            //sb.Append(" from class: ");
            //sb.Append(ste.getClassName());
            //sb.Append(" method: ");
            //sb.Append(ste.getMethodName());
            //sb.Append(" line: ");
            //sb.Append(ste.getLineNumber());
            writer.WriteLine(builder);
            var message = e.Message;
            if (message != null)
                writer.WriteLine($"Description: {message}");
        }
    }

    /** Returns {@code true} if {@code QueryResult} can be re-tried */
    private bool EvaluateOnce(QueryResult r, HashSet<string> variableIds)
    {
        var start = DateTime.Now.Millisecond;// System.currentTimeMillis();
        var success = r.Next();
        if (success)
            WriteVariableAssignments(r, variableIds);
        WriteOutcome(success, DateTime.Now.Millisecond - start);
        return success && !r.IsExhausted;
    }

    private void WriteVariableAssignments(QueryResult result, HashSet<string> variableIds)
    {
        if (variableIds.Count > 0)
        {
            writer.WriteLine();
            foreach (var variableId in variableIds)
            {
                writer.WriteLine(variableId + " = " 
                    + prolog.FormatTerm(result.GetTerm(variableId)));
            }
        }
    }

    private void WriteOutcome(bool success, long timing)
    {
        writer.WriteLine();
        writer.Write(success ? "yes" : "no");
        writer.Write(" (");
        writer.Write(timing);
        writer.Write(" ms)");
    }

    public static void Main(string[] args)
    {
        List<string> startupScriptFilenames = new();
        foreach (string arg in args)
        {
            if (arg.StartsWith("-"))
            {
                Console.WriteLine();
                Console.WriteLine($"don't know about argument: {arg}");
                Environment.Exit(-1);
            }
            startupScriptFilenames.Add(arg);
        }

        var console = new PrologConsole(Console.In, Console.Out);
        console.Run(startupScriptFilenames);
    }
}
