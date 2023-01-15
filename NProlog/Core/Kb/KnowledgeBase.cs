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
using Org.NProlog.Core.Event;
using Org.NProlog.Core.IO;
using Org.NProlog.Core.Math;
using Org.NProlog.Core.Parser;
using Org.NProlog.Core.Predicate;
using Org.NProlog.Core.Predicate.Builtin.Kb;
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Kb;

/**
 * The central object that connects the various components of an instance of the "core" inference engine.
 * <p>
 * <img src="doc-files/KnowledgeBase.png">
 * </p>
 */
public class KnowledgeBase
{
    /**
     * Represents the {@code pl_add_predicate/2} predicate hard-coded in every {@code KnowledgeBase}.
     * <p>
     * The {@code pl_add_predicate/2} predicate allows other implementations of {@link PredicateFactory} to be
     * "plugged-in" to a {@code KnowledgeBase} at runtime using Prolog syntax.
     *
     * @see AddPredicateFactory#evaluate(Term[])
     */
    private static readonly PredicateKey ADD_PREDICATE_KEY = new("pl_add_predicate", 2);

    private readonly PrologProperties prologProperties;
    private readonly Predicates predicates;
    private readonly ArithmeticOperators arithmeticOperators;
    private readonly PrologListeners prologListeners;
    private readonly Operands operands;
    private readonly TermFormatter termFormatter;
    private readonly SpyPoints spyPoints;
    private readonly FileHandles fileHandles;
    public KnowledgeBase()
        :this(new PrologDefaultProperties())
    {

    }
    /**
     * @see KnowledgeBaseUtils#createKnowledgeBase()
     * @see KnowledgeBaseUtils#createKnowledgeBase(ProjogProperties)
     */
    public KnowledgeBase(PrologProperties prologProperties)
    {
        this.prologProperties = prologProperties;
        this.predicates = new Predicates(this);
        this.predicates.AddPredicateFactory(ADD_PREDICATE_KEY, new AddPredicateFactory(this));
        this.arithmeticOperators = new ArithmeticOperators(this);
        this.prologListeners = new PrologListeners();
        this.operands = new Operands();
        this.termFormatter = new TermFormatter(operands);
        this.spyPoints = new SpyPoints(this);
        this.fileHandles = new FileHandles();
    }

    public PrologProperties PrologProperties => prologProperties;

    public Predicates Predicates => predicates;

    public ArithmeticOperators ArithmeticOperators => arithmeticOperators;

    public PrologListeners PrologListeners => prologListeners;

    public Operands Operands => operands;

    public TermFormatter TermFormatter => termFormatter;

    public SpyPoints SpyPoints => spyPoints;

    public FileHandles FileHandles => fileHandles;
}
