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
using System.Reflection;

namespace Org.NProlog.Core.Kb;

/**
 * Associates arbitrary objects with a {@code KnowledgeBase}.
 * <p>
 * Provides a way to implement a one-to-one relationship between a {@code KnowledgeBase} and its services. i.e. A
 * {@code KnowledgeBase} can be associated with one, and only one, {@code RecordedDatabase} - and a
 * {@code RecordedDatabase} can be associated with one, and only one, {@code KnowledgeBase}.
 * </p>
 */
public class KnowledgeBaseServiceLocator
{
    private static readonly Dictionary<KnowledgeBase, KnowledgeBaseServiceLocator> CACHE = new();

    /**
     * Returns the {@code KnowledgeBaseServiceLocator} associated with the specified {@code KnowledgeBase}.
     * <p>
     * If no {@code KnowledgeBaseServiceLocator} is already associated with the specified {@code KnowledgeBase} then a
     * new {@code KnowledgeBaseServiceLocator} will be created.
     * </p>
     */
    public static KnowledgeBaseServiceLocator GetServiceLocator(KnowledgeBase kb)
        => CACHE.TryGetValue(kb, out var serviceLocator)
        ? serviceLocator 
        : CreateServiceLocator(kb);

    private static KnowledgeBaseServiceLocator CreateServiceLocator(KnowledgeBase kb)
    {
        lock (CACHE)
        {
            if (!CACHE.TryGetValue(kb, out var l))
                CACHE.Add(kb, l = new(kb));
            return l;
        }
    }

    private readonly KnowledgeBase kb;
    private readonly Dictionary<Type, object> services = new();

    /** @see #getServiceLocator */
    private KnowledgeBaseServiceLocator(KnowledgeBase kb) 
        => this.kb = Objects.RequireNonNull(kb);

    /**
     * Adds the specified {@code instance} with the specified {@code referenceType} as its key.
     *
     * @throws ArgumentException If {@code instance} is not an instance of {@code ReferenceType}.
     * @throws InvalidOperationException If there is already a service associated with {@code referenceType}.
     */
    public void AddInstance(Type referenceType, object instance)
    {
        AssertInstanceOf(referenceType, instance);
        lock (services)
        {
            if (!services.TryGetValue(referenceType,out var r))
                services.Add(referenceType, instance);
            else
                throw new InvalidOperationException("Already have a service with key: " + referenceType);
        }
    }

    /**
     * Returns the {@code object} associated the specified {@code instanceType}.
     * <p>
     * If no {@code object} is already associated with {@code instanceType} then a new instance of {@code instanceType}
     * will be created and associated with {@code instanceType} for future use.
     * </p>
     *
     * @throws SystemException if an attempt to instantiate a new instance of the {@code instanceType} fails. e.g. If it
     * does not have a public constructor that accepts either no arguments or a single {@code KnowledgeBase} argument.
     */
    public T GetInstance<T>(Type instanceType)
        => GetInstance<T>(instanceType, instanceType);

    /**
     * Returns the {@code object} associated the specified {@code referenceType}.
     * <p>
     * If no {@code object} is already associated with {@code referenceType} then a new instance of {@code instanceType}
     * will be created and associated with {@code referenceType} for future use.
     * </p>
     *
     * @param referenceType The class to use as the key to retrieve an existing service.
     * @param instanceType The class to create a new instance of if there is no existing service associated with
     * {@code referenceType}.
     * @throws SystemException If an attempt to instantiate a new instance of the {@code instanceType} fails. e.g. If
     * {@code instanceType} does not have a public constructor that accepts either no arguments or a single
     * {@code KnowledgeBase} argument - or if {@code referenceType} is not the same as, nor is a superclass or
     * superinterface of, {@code instanceType}.
     */

    public T GetInstance<T>(Type referenceType, Type instanceType)
    {
        if (!services.TryGetValue(referenceType,out var r))
            r = CreateInstance(referenceType, instanceType);
        return (T)r;
    }

    private object? CreateInstance(Type referenceType, Type instanceType)
    {
        lock (services)
        {
            if (!services.TryGetValue(referenceType,out var r))
            {
                AssertAssignableFrom(referenceType, instanceType);
                r = NewInstance(instanceType);
                if (r != null)
                    services.Add(referenceType, r);
            }
            return r;
        }
    }

    private static void AssertAssignableFrom(Type referenceType, Type instanceType)
    {
        if (!referenceType.IsAssignableFrom(instanceType))
            throw new ArgumentException($"{instanceType} is not of type: {referenceType}");
    }

    private static void AssertInstanceOf(Type referenceType, object instance)
    {
        if (instance.GetType() != referenceType)
            throw new ArgumentException($"{instance} is not of type: {referenceType}");
    }

    /**
     * Returns a new instance of the specified class.
     * <p>
     * If the class has a constructor that takes a KnowledgeBase as its single argument then an attempt is made to use
     * that to construct the new instance - else an attempt is made to construct a new instance using the no-arg
     * constructor.
     */
    private object? NewInstance(Type c)
    {
        if (c == typeof(string))
        {
            return string.Empty;
        }
        try
        {
            var constructor = GetKnowledgeBaseArgumentConstructor(c);
            return constructor != null ? constructor.Invoke(new object?[] { kb }) 
                : Assembly.GetAssembly(c)?.CreateInstance(c?.FullName??"");
        }
        catch (Exception e)
        {
            throw new SystemException($"Could not create new instance of service: {c}", e);
        }
    }

    private static ConstructorInfo? GetKnowledgeBaseArgumentConstructor(Type type)
    {
        foreach (var constructor in type.GetConstructors())
        {
            var ps = constructor.GetParameters();
            if (ps != null && ps.Length == 1 && ps[0].ParameterType == typeof(KnowledgeBase))
                return constructor;
        }
        return null;
    }
}
