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
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Predicate.Builtin.Kb;

/* TEST
%LINK prolog-io
*/
/**
 * <code>ensure_loaded(X)</code> - reads clauses and goals from a file.
 * <p>
 * <code>ensure_loaded(X)</code> reads clauses and goals from a file. <code>X</code> must be instantiated to the name of
 * a text file containing Prolog clauses and goals which will be added to the knowledge base. Will do nothing when
 * <code>X</code> represents a file that has already been loaded using <code>ensure_loaded(X)</code>.
 * </p>
 */
public class EnsureLoaded : AbstractSingleResultPredicate
{
    private readonly object syncRoot = new ();

    private readonly HashSet<string> loadedResources = new();


    protected override bool Evaluate(Term arg)
    {
        var resourceName = GetResourceName(arg);
        lock (syncRoot)
        {
            if (loadedResources.Contains(resourceName))
            {
                PrologListeners.NotifyInfo("Already loaded: " + resourceName);
            }
            else
            {
                PrologSourceReader.ParseResource(KnowledgeBase, resourceName);
                loadedResources.Add(resourceName);
            }
        }
        return true;
    }

    private static string GetResourceName(Term arg)
    {
        var resourceName = TermUtils.GetAtomName(arg);
        return !resourceName.Contains('.') ? resourceName + ".pl" : resourceName;
    }
}
