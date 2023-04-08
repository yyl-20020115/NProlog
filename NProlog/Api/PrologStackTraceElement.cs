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
using Org.NProlog.Core.Predicate;
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Api;

/**
 * An element in a stack trace, as returned by {@link Prolog#getStackTrace(Exception)}.
 * <p>
 * Each element represents a single stack frame. All stack frames represent the evaluation of a clause in a user defined
 * function. The frame at the top of the stack represents the execution point at which the stack trace was generated.
 */
public class PrologStackTraceElement
{
    private readonly PredicateKey key;
    private readonly Term term;

    /**
     * @param term the clause this stack trace element was generated for
     */
    public PrologStackTraceElement(PredicateKey key, Term term)
    {
        this.key = key;
        this.term = term;
    }

    /**
     * Represents the user defined predicate this stack trace element was generated for.
     *
     * @return key Represents the user defined predicate this stack trace element was generated for
     */
    public PredicateKey PredicateKey => key;

    /**
     * Represents the clause this stack trace element was generated for.
     *
     * @return the clause this stack trace element was generated for
     */
    public Term Term => term;
}
