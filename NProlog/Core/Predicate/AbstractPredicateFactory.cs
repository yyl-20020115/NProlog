/*
 * Copyright 2018 S. Webber
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
using Org.NProlog.Core.Kb;
using Org.NProlog.Core.Math;
using Org.NProlog.Core.Parser;
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Predicate;


public abstract class AbstractPredicateFactory : PredicateFactory, KnowledgeBaseConsumer
{
    protected KnowledgeBase? knowledgeBase;
    public KnowledgeBase? KnowledgeBase { get => knowledgeBase; set { knowledgeBase = value; this.Init(); } }


    public virtual Predicate? GetPredicate(params Term[]? args) => args?.Length switch
    {
        0 => GetPredicate(),
        1 => GetPredicate(args[0]),
        2 => GetPredicate(args[0], args[1]),
        3 => GetPredicate(args[0], args[1], args[2]),
        4 => GetPredicate(args[0], args[1], args[2], args[3]),
        _ => throw CreateWrongNumberOfArgumentsException(args.Length),
    };

    protected virtual Predicate? GetPredicate() => throw CreateWrongNumberOfArgumentsException(0);

    protected virtual Predicate? GetPredicate(Term arg) => throw CreateWrongNumberOfArgumentsException(1);

    protected virtual Predicate? GetPredicate(Term arg1, Term arg2) => throw CreateWrongNumberOfArgumentsException(2);

    protected virtual Predicate? GetPredicate(Term arg1, Term arg2, Term arg3) => throw CreateWrongNumberOfArgumentsException(3);

    protected virtual Predicate? GetPredicate(Term arg1, Term arg2, Term arg3, Term arg4) => throw CreateWrongNumberOfArgumentsException(4);

    protected virtual ArgumentException CreateWrongNumberOfArgumentsException(int numberOfArguments)
        => new ("The predicate factory: " + GetType().Name + " does not accept the number of arguments: " + numberOfArguments);


    public virtual bool IsRetryable => true;

    /**
     * This method is called by {@link #setKnowledgeBase(KnowledgeBase)}.
     * <p>
     * Can be overridden by subclasses to perform initialisation before any calls to {@link #getPredicate(Term[])} are
     * made. As {@link #setKnowledgeBase(KnowledgeBase)} will have already been called before this method is invoked,
     * overridden versions will be able to access the {@code KnowledgeBase} using {@link #getKnowledgeBase()}.
     */
    protected virtual void Init()
    {
    }


    public Predicates? Predicates => knowledgeBase?.Predicates;

    public ArithmeticOperators? ArithmeticOperators => knowledgeBase?.ArithmeticOperators;

    public PrologListeners? PrologListeners => knowledgeBase?.PrologListeners;

    public Operands? Operands => knowledgeBase?.Operands;

    public TermFormatter? TermFormatter => knowledgeBase?.TermFormatter;

    public SpyPoints? SpyPoints => knowledgeBase?.SpyPoints;

    public FileHandles? FileHandles => knowledgeBase?.FileHandles;

}
