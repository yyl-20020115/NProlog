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
using static Org.NProlog.Core.Event.SpyPoints;

namespace Org.NProlog.Core.Event;

/**
 * Controls the registering and notification of listeners of a {@link org.projog.core.kb.KnowledgeBase}.
 * <p>
 * Each {@link org.projog.core.kb.KnowledgeBase} has a single unique {@code ProjogListeners} instance.
 *
 * @see KnowledgeBase#getProjogListeners()
 */
public class PrologListeners
{
    private readonly HashSet<PrologListener> listeners = new();

    /**
     * Adds a listener to the set of listeners.
     *
     * @param listener a listener to be added
     * @return <tt>true</tt> if this instance did not already reference the specified listener
     */
    public bool AddListener(PrologListener listener) 
        => listeners.Add(listener);

    /**
     * Deletes an observer from the set of observers of this objects internal {@code Observable}.
     *
     * @param listener a listener to be deleted
     * @return <tt>true</tt> if this instance did reference the specified listener
     */
    public bool DeleteListener(PrologListener listener)
        => listeners.Remove(listener);

    /** Notify all listeners of a first attempt to evaluate a goal. */
    public void NotifyCall(SpyPointEvent _event)
    {
        foreach (var listener in listeners)
            listener.OnCall(_event);
    }

    /** Notify all listeners of an attempt to re-evaluate a goal. */
    public void NotifyRedo(SpyPointEvent _event)
    {
        foreach (var listener in listeners)
            listener.OnRedo(_event);
    }

    /** Notify all listeners when an attempt to evaluate a goal succeeds. */
    public void NotifyExit(SpyPointExitEvent _event)
    {
        foreach (var listener in listeners)
            listener.OnExit(_event);
    }

    /** Notify all listeners when an attempt to evaluate a goal fails. */
    public void NotifyFail(SpyPointEvent _event)
    {
        foreach (var listener in listeners)
            listener.OnFail(_event);
    }

    /** Notify all listeners of a warning. */
    public void NotifyWarn(string message)
    {
        foreach (var listener in listeners)
            listener.OnWarn(message);
    }

    /** Notify all listeners of a general information _event. */
    public void NotifyInfo(string message)
    {
        foreach (var listener in listeners)
            listener.OnInfo(message);
    }
}
