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
using Org.NProlog.Core.Predicate.Udp;

namespace Org.NProlog.Core.Exceptions;

/**
 * An exception that provides information on an error within the Projog environment.
 * <p>
 * Maintains a collection of all {@link org.projog.core.predicate.udp.InterpretedUserDefinedPredicate} instances that
 * form the exception's stack trace.
 */
public class PrologException : Exception
{
    private readonly List<ClauseModel> stackTrace = new();

    public PrologException(string message) : base(message, null) { }

    public PrologException(string message, Exception? exception) : base(message, exception) { }

    public void AddClause(ClauseModel clause) => stackTrace.Add(clause);

    public List<ClauseModel> GetClauses() => new(stackTrace);
}
