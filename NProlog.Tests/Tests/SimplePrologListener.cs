/*
 * Copyright 2020 S. Webber
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
using Org.NProlog.Core.Event;
using System.Text;
using static Org.NProlog.Core.Event.SpyPoints;

namespace Org.NProlog;


/** Used by tests to monitor events. */
public class SimplePrologListener : PrologListener
{
    private readonly List<string> events = new();

    public void OnInfo(string message)
    {
        throw new InvalidOperationException(message);
    }


    public void OnWarn(string message)
    {
        throw new InvalidOperationException(message);
    }


    public void OnRedo(SpyPointEvent @event)
    {
        Add("REDO", @event);
    }


    public void OnFail(SpyPointEvent @event)
    {
        Add("FAIL", @event);
    }


    public void OnExit(SpyPointExitEvent @event)
    {
        Add("EXIT", @event);
    }


    public void OnCall(SpyPointEvent @event)
    {
        Add("CALL", @event);
    }

    private void Add(string level, SpyPointEvent @event)
    {
        events.Add(level + @event);
    }

    public bool IsEmpty()
    {
        return events.Count == 0;
    }

    public string Get(int index)
    {
        return events[(index)];
    }

    public int Count => events.Count;

    public string GetResult()
    {
        var result = new StringBuilder();
        foreach (var @event in events)
        {
            result.Append(@event);
        }
        return result.ToString();
    }
}
