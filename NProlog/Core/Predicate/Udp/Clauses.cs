/*
 * Copyright 2020 S. Webber
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
using Org.NProlog.Core.Kb;
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Predicate.Udp;



// TODO add Javadoc and review method and variable names
public class Clauses
{
    private static readonly Clauses EMPTY = new(new(), Array.Empty<int>());

    private readonly List<ClauseAction> clauses;
    private readonly int[] immutableColumns;

    public static Clauses CreateFromModels(KnowledgeBase kb, List<ClauseModel> models)
    {
        List<ClauseAction> actions = new();
        foreach (var model in models)
        {
            var action = ClauseActionFactory.CreateClauseAction(kb, model);
            actions.Add(action);
        }
        return CreateFromActions(kb, actions, null);
    }

    public static Clauses CreateFromActions(KnowledgeBase kb, List<ClauseAction> actions, Term arg = null)
    {
        if (actions.Count == 0) return EMPTY;

        int numArgs = actions[(0)].Model.Consequent.NumberOfArguments;
        bool[] muttableColumns = CreateArray(numArgs, arg);
        int muttableColumnCtr = Count(muttableColumns);

        List<ClauseAction> clauses = new(actions.Count);
        foreach (var action in actions)
        {
            clauses.Add(action);
            for (int i = 0; i < numArgs; i++)
            {
                if (!muttableColumns[i] && !action.Model.Consequent.GetArgument(i).IsImmutable)
                {
                    muttableColumns[i] = true;
                    muttableColumnCtr++;
                }
            }
        }

        int[] immutableColumns = new int[numArgs - muttableColumnCtr];
        for (int i = 0, ctr = 0; ctr < immutableColumns.Length; i++)
        {
            if (!muttableColumns[i])
            {
                immutableColumns[ctr++] = i;
            }
        }

        return new Clauses(actions, immutableColumns);
    }

    private static bool[] CreateArray(int numArgs, Term query)
    {
        var result = new bool[numArgs];
        if (query != null)
        {
            for (int i = 0; i < result.Length; i++)
            {
                Term arg = query.GetArgument(i);
                result[i] = arg.IsImmutable || IsAnonymousVariable(arg);
            }
        }
        return result;
    }

    private static bool IsAnonymousVariable(Term arg)
        => arg.Type.isVariable && ((Variable)arg.Term).IsAnonymous;

    private static int Count(bool[] a)
    {
        int ctr = 0;
        for (int i = 0; i < a.Length; i++)
        {
            if (a[i])
            {
                ctr++;
            }
        }
        return ctr;
    }

    public Clauses(List<ClauseAction> actions, int[] immutableColumns)
    {
        this.clauses = actions;
        this.immutableColumns = immutableColumns;

    }

    public int[] ImmutableColumns => immutableColumns;

    public ClauseAction[] ClauseActions => clauses.ToArray();
}
