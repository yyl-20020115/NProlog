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
using static Org.NProlog.Core.Event.SpyPoints;

namespace Org.NProlog.Core.Event;

public class LoggingPrologListener : PrologListener
{
    private readonly TextWriter writer;
    public LoggingPrologListener(TextWriter writer) 
        => this.writer = writer;
    public void OnCall(SpyPointEvent @event) 
        => Log("CALL", @event);
    public void OnRedo(SpyPointEvent @event) 
        => Log("REDO", @event);
    public void OnExit(SpyPointExitEvent @event) 
        => Log("EXIT", @event);
    public void OnFail(SpyPointEvent @event) 
        => Log("FAIL", @event);
    public void OnWarn(string message) 
        => Log("WARN " + message);
    public void OnInfo(string message) 
        => Log("INFO " + message);
    private void Log(string level, SpyPointEvent @event) 
        => Log("[" + @event.GetSourceId() + "] " + level + " " + @event.GetFormattedTerm());
    private void Log(string message) 
        => writer.WriteLine(message);
}
