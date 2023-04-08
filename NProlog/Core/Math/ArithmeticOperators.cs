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
using Org.NProlog.Core.Exceptions;
using Org.NProlog.Core.Kb;
using Org.NProlog.Core.Predicate;
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Math;

/**
 * Maintains a collection of {@link ArithmeticOperator} instances.
 * <p>
 * This class provides a mechanism for "plugging in" or "injecting" implementations of {@link ArithmeticOperator} at
 * runtime. This mechanism provides an easy way to configure and extend the arithmetic operations supported by Prolog.
 * </p>
 * <p>
 * Each {@link org.prolog.core.kb.KnowledgeBase} has a single unique {@code ArithmeticOperators} instance.
 * </p>
 */
public class ArithmeticOperators
{
    private readonly KnowledgeBase kb;
    private readonly object syncRoot = new();
    private readonly Dictionary<PredicateKey, string> operatorClassNames = new();
    private readonly Dictionary<PredicateKey, ArithmeticOperator?> operatorInstances = new();

    public ArithmeticOperators(KnowledgeBase kb) => this.kb = kb;

    /**
     * Associates a {@link ArithmeticOperator} with this {@code KnowledgeBase}.
     *
     * @param key The name and arity to associate the {@link ArithmeticOperator} with.
     * @param operator The instance of {@code ArithmeticOperator} to be associated with {@code key}.
     * @throws PrologException if there is already a {@link ArithmeticOperator} associated with the {@code PredicateKey}
     */
    public void AddArithmeticOperator(PredicateKey key, ArithmeticOperator _operator)
    {
        lock (this.syncRoot)
        {
            if (operatorClassNames.ContainsKey(key))
            {
                throw new PrologException($"Already defined operator: {key}");
            }
            else
            {
                operatorClassNames.Add(key, _operator.GetType().Name);
                operatorInstances.Add(key, _operator);
            }
        }
    }
    public void AddArithmeticOperator(PredicateKey key, Type _operator)
    {
        lock (this.syncRoot)
        {
            if (operatorClassNames.ContainsKey(key))
            {
                throw new PrologException($"Already defined operator: {key}");
            }
            else
            {
                operatorClassNames.Add(key, _operator.GetType().Name);
                var operatorInstance = _operator.Assembly.CreateInstance(_operator?.FullName??"")
                    as ArithmeticOperator;
                if(operatorInstance is KnowledgeBaseConsumer consumer)
                {
                    consumer.KnowledgeBase = this.kb;
                }
                operatorInstances.Add(key, operatorInstance);
            }
        }
    }
    /**
     * Associates a {@link ArithmeticOperator} with this {@code KnowledgeBase}.
     *
     * @param key The name and arity to associate the {@link ArithmeticOperator} with.
     * @param operatorClassName The class name of the {@link ArithmeticOperator} to be associated with {@code key}.
     * @throws PrologException if there is already a {@link ArithmeticOperator} associated with the {@code PredicateKey}
     */
    public void AddArithmeticOperator(PredicateKey key, string operatorClassName)
    {
        lock (this.syncRoot)
        {
            if (!operatorClassNames.ContainsKey(key))
            {
                operatorClassNames.Add(key, operatorClassName);
            }
            else
            {
                throw new PrologException($"Already defined operator: {key}");
            }
        }
    }

    /**
     * Returns the result of evaluating the specified arithmetic expression.
     *
     * @param t a {@code Term} that can be evaluated as an arithmetic expression (e.g. a {@code Structure} of the form
     * {@code +(1,2)} or a {@code Numeric})
     * @return the result of evaluating the specified arithmetic expression
     * @throws PrologException if the specified term does not represent an arithmetic expression
     */
    public Numeric? GetNumeric(Term? t) => t?.Type switch
    {
        var tt when tt == TermType.FRACTION => TermUtils.CastToNumeric(t),
        var tt when tt == TermType.INTEGER => TermUtils.CastToNumeric(t),
        var tt when tt == TermType.STRUCTURE => Calculate(t, t?.Args),
        var tt when tt == TermType.ATOM => Calculate(t, TermUtils.EMPTY_ARRAY),
        _ => throw new PrologException($"Cannot get Numeric for term: {t} of type: {t?.Type}"),
    };

    private Numeric? Calculate(Term? term, Term[]? args)
        => GetArithmeticOperator(PredicateKey.CreateForTerm(term))?.Calculate(args);

    /**
     * @return null if not found
     */
    public ArithmeticOperator? GetPreprocessedArithmeticOperator(Term? argument) => (argument?.Type?.IsNumeric).GetValueOrDefault()
            ? argument?.Term as Numeric
            : argument?.Type == TermType.ATOM || argument?.Type == TermType.STRUCTURE
                ? GetPreprocessedArithmeticOperator(PredicateKey.CreateForTerm(argument), argument)
                : null;

    private ArithmeticOperator? GetPreprocessedArithmeticOperator(PredicateKey key, Term argument)
    {
        if (operatorInstances.ContainsKey(key) || operatorClassNames.ContainsKey(key))
        {
            var ao = GetArithmeticOperator(key);
            return ao is PreprocessableArithmeticOperator op ? op.Preprocess(argument) : ao;
        }
        else
        {
            return null;
        }
    }

    /**
     * @throws PrologException if not found
     */
    public ArithmeticOperator? GetArithmeticOperator(PredicateKey key) => operatorInstances.TryGetValue(key, out var e)
            ? e
            : operatorClassNames.ContainsKey(key)
                ? InstantiateArithmeticOperator(key)
                : throw new PrologException($"Cannot find arithmetic operator: {key}");

    private ArithmeticOperator? InstantiateArithmeticOperator(PredicateKey key)
    {
        lock (this.syncRoot)
        {
            if (!operatorInstances.TryGetValue(key, out var _operator))
                if(operatorClassNames.TryGetValue(key, out var _operatorClassNames))
                    operatorInstances.Add(key,
                        _operator = InstantiateArithmeticOperator(_operatorClassNames));
            return _operator;
        }
    }

    private ArithmeticOperator? InstantiateArithmeticOperator(string className)
    {
        try
        {
            var o = KnowledgeBaseUtils.Instantiate<ArithmeticOperator>(kb, className)
                ?? throw new TypeLoadException(className);
            return o;
        }
        catch (Exception e)
        {
            throw new SystemException($"Could not create new ArithmeticOperator using: {className}", e);
        }
    }
}
