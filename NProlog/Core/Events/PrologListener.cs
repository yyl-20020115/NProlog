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


public interface PrologListener
{
    /** The _event generated when an attempt is first made to evaluate a goal. */
    void OnCall(SpyPointEvent @event);

    /** The _event generated when an attempt is made to re-evaluate a goal. */
    void OnRedo(SpyPointEvent @event);

    /** The _event generated when an attempt to evaluate a goal succeeds. */
    void OnExit(SpyPointExitEvent @event);

    /** The _event generated when all attempts to evaluate a goal have failed. */
    void OnFail(SpyPointEvent @event);

    /** The _event generated to warn clients of an _event. */
    void OnWarn(string message);

    /** The _event generated to inform clients of an _event. */
    void OnInfo(string message);
}
