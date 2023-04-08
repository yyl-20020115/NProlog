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
using Org.NProlog.Core.Exceptions;
using Org.NProlog.Core.Kb;
using Org.NProlog.Core.Predicate.Udp;
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Predicate;



/** Acts as a repository of rules and facts. */
public class Predicates
{
    /**
     * Used to coordinate access to {@link javaPredicateClassNames}, {@link #javaPredicateInstances} and
     * {@link #userDefinedPredicates}
     */
    private readonly object predicatesLock = new ();

    /**
     * The class names of "built-in" Java predicates (i.e. not defined using Prolog syntax) associated with this
     * {@code KnowledgeBase}.
     */
    private readonly Dictionary<PredicateKey, string> platformPredicateClassNames = new();

    /**
     * The instances of "built-in" Java predicates (i.e. not defined using Prolog syntax) associated with this
     * {@code KnowledgeBase}.
     */
    private readonly Dictionary<PredicateKey, PredicateFactory> platformPredicateInstances = new();

    /**
     * The user-defined predicates (i.e. defined using Prolog syntax) associated with this {@code KnowledgeBase}.
     * <p>
     * Uses TreeMap to enforce predictable ordering for when iterated (e.g. by <code>listing(X)</code>).
     */
    private readonly Dictionary<PredicateKey, UserDefinedPredicateFactory> userDefinedPredicates = new();

    private readonly KnowledgeBase kb;

    public Predicates(KnowledgeBase kb) 
        => this.kb = kb;

    public Predicate GetPredicate(Term term) 
        => GetPredicateFactory(term).GetPredicate(term.Args);

    /**
     * Returns details of all predicates, both user-defined and built-in predicates.
     */
    public HashSet<PredicateKey> GetAllDefinedPredicateKeys()
    {
        HashSet<PredicateKey> result = new();
        result.UnionWith(platformPredicateClassNames.Keys);
        result.UnionWith(userDefinedPredicates.Keys);
        return result;
    }

    /**
     * Returns details of all the user define predicates of this object.
     */
    public Dictionary<PredicateKey, UserDefinedPredicateFactory> GetUserDefinedPredicates() => new (userDefinedPredicates);

    /**
     * Returns the {@code UserDefinedPredicateFactory} for the specified {@code PredicateKey}.
     * <p>
     * If this object does not already have a {@code UserDefinedPredicateFactory} for the specified {@code PredicateKey}
     * then it will create it.
     *
     * @throws PrologException if the specified {@code PredicateKey} represents an existing "plugin" predicate
     */
    public UserDefinedPredicateFactory CreateOrReturnUserDefinedPredicate(PredicateKey key)
    {
        UserDefinedPredicateFactory userDefinedPredicate = null;
        lock (predicatesLock)
        { // TODO if already in userDefinedPredicates then avoid need to synch
            if (IsExistingPlatformPredicate(key))
            {
                throw new PrologException("Cannot replace already defined built-in predicate: " + key);
            }

            if (!userDefinedPredicates.TryGetValue(key, out var outuserDefinedPredicate))
            {
                // assume dynamic
                AddUserDefinedPredicate(userDefinedPredicate = new DynamicUserDefinedPredicateFactory(kb, key));
            }
            else
            {
                userDefinedPredicate = outuserDefinedPredicate;
            }
        }
        return userDefinedPredicate;
    }

    /**
     * Adds a user defined predicate to this object.
     * <p>
     * Any existing {@code UserDefinedPredicateFactory} with the same {@code PredicateKey} will be replaced.
     *
     * @throws PrologException if the {@code PredicateKey} of the specified {@code UserDefinedPredicateFactory}
     * represents an existing "plugin" predicate
     */
    public void AddUserDefinedPredicate(UserDefinedPredicateFactory userDefinedPredicate)
    {
        var key = userDefinedPredicate.PredicateKey;
        lock (predicatesLock)
        {
            if (IsExistingPredicate(key))
            {
                UpdateExistingPredicate(key, userDefinedPredicate);
            }
            else
            {
                userDefinedPredicates.Add(key, userDefinedPredicate);
            }
        }
    }

    private void UpdateExistingPredicate(PredicateKey key, UserDefinedPredicateFactory userDefinedPredicate)
    {
        if (IsExistingPlatformPredicate(key))
        {
            throw new PrologException("Cannot replace already defined built-in predicate: " + key);
        }

        //      UserDefinedPredicateFactory existingUserDefinedPredicateFactory = userDefinedPredicates.get(key);

        if (userDefinedPredicates.TryGetValue(key, out var existingUserDefinedPredicateFactory))
            if (!existingUserDefinedPredicateFactory.IsDynamic)
            {
                throw new PrologException(
                            "Cannot Append to already defined user defined predicate as it is not dynamic. You can set the predicate to dynamic by adding the following line to start of the file that the predicate is defined in:\n?- dynamic("
                                        + key
                                        + ").");
            }

        var it = userDefinedPredicate.GetImplications();
        while(it.MoveNext())
        {
            existingUserDefinedPredicateFactory.AddLast(it.Current);
        }
    }

    public PredicateFactory GetPreprocessedPredicateFactory(Term? term)
    {
        var pf = GetPredicateFactory(term);
        return pf is PreprocessablePredicateFactory factory ? factory.Preprocess(term) : pf;
    }

    /**
     * Returns the {@code PredicateFactory} associated with the specified {@code Term}.
     * <p>
     * If this object has no {@code PredicateFactory} associated with the {@code PredicateKey} of the specified
     * {@code Term} then a new instance of {@link UnknownPredicate} is returned.
     */
    public PredicateFactory GetPredicateFactory(Term? term) => GetPredicateFactory(PredicateKey.CreateForTerm(term));

    /**
     * Returns the {@code PredicateFactory} associated with the specified {@code PredicateKey}.
     * <p>
     * If this object has no {@code PredicateFactory} associated with the specified {@code PredicateKey} then a new
     * instance of {@link UnknownPredicate} is returned.
     */
    public PredicateFactory GetPredicateFactory(PredicateKey key)
    {
        var predicateFactory = GetExistingPredicateFactory(key);
        return predicateFactory ?? (platformPredicateClassNames.ContainsKey(key) ? InstantiatePredicateFactory(key) : UnknownPredicate(key));
    }

    private PredicateFactory GetExistingPredicateFactory(PredicateKey key) 
        => (platformPredicateInstances.TryGetValue(key, out var p) ? p : null)
            ?? (userDefinedPredicates.TryGetValue(key, out var v) ? v : null);

    private PredicateFactory InstantiatePredicateFactory(PredicateKey key)
    {
        lock (predicatesLock)
        {
            var predicateFactory = GetExistingPredicateFactory(key);
            return predicateFactory != null
                ? predicateFactory
                : platformPredicateClassNames.TryGetValue(key, out var p)
                    ? (platformPredicateInstances[key]= predicateFactory 
                    = InstantiatePredicateFactory(p))
                    : null;
        }
    }

    private PredicateFactory InstantiatePredicateFactory(string className)
    {
        try
        {
            return KnowledgeBaseUtils.Instantiate<PredicateFactory>(kb, className);
        }
        catch (Exception e)
        {
            throw new SystemException("Could not create new PredicateFactory using: " + className, e);
        }
    }

    private PredicateFactory UnknownPredicate(PredicateKey key) => new UnknownPredicate(kb, key);

    /**
     * Associates a {@link PredicateFactory} with this {@code KnowledgeBase}.
     * <p>
     * This method provides a mechanism for "plugging in" or "injecting" implementations of {@link PredicateFactory} at
     * runtime. This mechanism provides an easy way to configure and extend the functionality of Prolog - including
     * adding functionality not possible to define in pure Prolog syntax.
     * </p>
     *
     * @param key The name and arity to associate the {@link PredicateFactory} with.
     * @param predicateFactoryClassName The name of a class that : {@link PredicateFactory}.
     * @throws PrologException if there is already a {@link PredicateFactory} associated with the {@code PredicateKey}
     */
    public void AddPredicateFactory(PredicateKey key, string predicateFactoryClassName)
    {
        lock (predicatesLock)
        {
            if (IsExistingPredicate(key))
            {
                throw new PrologException("Already defined: " + key);
            }
            else
            {
                platformPredicateClassNames.Add(key, predicateFactoryClassName);
            }
        }
    }

    /**
     * Associates a {@link PredicateFactory} with this {@code KnowledgeBase}.
     * <p>
     * This method provides a mechanism for "plugging in" or "injecting" implementations of {@link PredicateFactory} at
     * runtime. This mechanism provides an easy way to configure and extend the functionality of Prolog - including
     * adding functionality not possible to define in pure Prolog syntax.
     * </p>
     *
     * @param key The name and arity to associate the {@link PredicateFactory} with.
     * @param predicateFactory The {@link PredicateFactory} to be added.
     * @throws PrologException if there is already a {@link PredicateFactory} associated with the {@code PredicateKey}
     */
    public void AddPredicateFactory(PredicateKey key, PredicateFactory predicateFactory)
    {
        lock (predicatesLock)
        {
            if (IsExistingPredicate(key))
            {
                throw new PrologException("Already defined: " + key);
            }
            else
            {
                platformPredicateClassNames.Add(key, predicateFactory?.GetType()?.Name??"");
                platformPredicateInstances.Add(key, predicateFactory);
            }
        }
    }
    public void AddPredicateFactory(PredicateKey key, Type predicateFactory, string method = "")
    {
        lock (predicatesLock)
        {
            if (IsExistingPredicate(key))
            {
                throw new PrologException("Already defined: " + key);
            }
            else
            {
                platformPredicateClassNames.Add(key, predicateFactory.Name);
                PredicateFactory factory = null;
                if (!string.IsNullOrEmpty(method))
                {
                    var ci = predicateFactory.GetMethod(method, System.Reflection.BindingFlags.Static| System.Reflection.BindingFlags.Public);
                    factory = ci?.Invoke(null, new object[0]) as PredicateFactory;
                }
                else
                {
                    factory = predicateFactory.Assembly.CreateInstance(predicateFactory.FullName) as PredicateFactory;
                }
                if (factory is KnowledgeBaseConsumer consumer)
                {
                    consumer.KnowledgeBase = this.kb;
                }
                platformPredicateInstances.Add(key, factory);
            }
        }
    }
    private bool IsExistingPredicate(PredicateKey key) 
        => IsExistingPlatformPredicate(key) || IsExistingUserDefinedPredicate(key);

    private bool IsExistingPlatformPredicate(PredicateKey key)
        => platformPredicateClassNames.ContainsKey(key);

    private bool IsExistingUserDefinedPredicate(PredicateKey key)
        => userDefinedPredicates.ContainsKey(key);
}
