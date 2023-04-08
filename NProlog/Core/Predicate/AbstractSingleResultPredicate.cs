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
using Org.NProlog.Core.Math;
using Org.NProlog.Core.Parser;
using Org.NProlog.Core.Terms;
using Org.NProlog.Core.Kb;
using Org.NProlog.Core.IO;
using Org.NProlog.Core.Predicate.Udp;

namespace Org.NProlog.Core.Predicate;


/**
 * Superclass of "plug-in" predicates that are not re-evaluated as part of backtracking.
 * <p>
 * Provides a skeletal implementation of {@link PredicateFactory} and {@link Predicate}. No attempt to find multiple
 * solutions will be made as part of backtracking as {@link #isRetryable()} always returns {@code false}.
 */
public abstract class AbstractSingleResultPredicate : PredicateFactory, KnowledgeBaseConsumer
{
    protected KnowledgeBase? knowledgeBase;
    public AbstractSingleResultPredicate() { }

    public KnowledgeBase? KnowledgeBase
    {
        get => this.knowledgeBase;
        set
        {
            this.knowledgeBase = value;
            Init();
        }
    }

    public Predicate GetPredicate(Term[] args) 
        => PredicateUtils.ToPredicate(Evaluate(args));

    public virtual bool Evaluate(Term[] args) => args.Length switch
    {
        0 => Evaluate(),
        1 => Evaluate(args[0]),
        2 => Evaluate(args[0], args[1]),
        3 => Evaluate(args[0], args[1], args[2]),
        4 => Evaluate(args[0], args[1], args[2], args[3]),
        _ => throw CreateWrongNumberOfArgumentsException(args.Length),
    };

    protected virtual bool Evaluate() => throw CreateWrongNumberOfArgumentsException(0);

    protected virtual bool Evaluate(Term arg) => throw CreateWrongNumberOfArgumentsException(1);

    protected virtual bool Evaluate(Term arg1, Term arg2) => throw CreateWrongNumberOfArgumentsException(2);

    protected virtual bool Evaluate(Term arg1, Term arg2, Term arg3)
    => throw CreateWrongNumberOfArgumentsException(3);

    protected virtual bool Evaluate(Term arg1, Term arg2, Term arg3, Term arg4)
    => throw CreateWrongNumberOfArgumentsException(4);

    private ArgumentException CreateWrongNumberOfArgumentsException(int numberOfArguments) 
        => new("The predicate factory: " + GetType().Name + " does not accept the number of arguments: " + numberOfArguments);


    public virtual bool IsRetryable => false;

    /**
     * This method is called by {@link #setKnowledgeBase(KnowledgeBase)}.
     * <p>
     * Can be overridden by subclasses to perform initialisation before any calls to {@link #evaluate(Term...)} are made.
     * As {@link #setKnowledgeBase(KnowledgeBase)} will have already been called before this method is invoked,
     * overridden versions will be able to access the {@code KnowledgeBase} using {@link #getKnowledgeBase()}.
     */
    protected virtual void Init()
    {
    }


    protected Predicates? Predicates => knowledgeBase?.Predicates;

    protected ArithmeticOperators? ArithmeticOperators => knowledgeBase?.ArithmeticOperators;

    protected PrologListeners? PrologListeners => knowledgeBase?.PrologListeners;

    protected Operands? Operands => knowledgeBase?.Operands;

    protected TermFormatter? TermFormatter => knowledgeBase?.TermFormatter;

    protected SpyPoints? SpyPoints => knowledgeBase?.SpyPoints;

    protected FileHandles? FileHandles => knowledgeBase?.FileHandles;

    // TODO add more convenience methods like getOutputStream() and formatTerm(Term)
}
