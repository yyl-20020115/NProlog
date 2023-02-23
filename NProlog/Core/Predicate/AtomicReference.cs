/*
 * Copyright 2013 S. Webber
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a Copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
namespace Org.NProlog.Core.Predicate;

public class AtomicReference<T>
    where T : class
{
    private volatile T value;

    public AtomicReference(T value = default) => this.value = value;

    public T Value
    {
        get => value;
        set => Interlocked.Exchange(ref this.value, value);
    }

    public T GetAndSet(T value)
        => Interlocked.Exchange(ref this.value, value);

    public bool CompareAndSet(T expect, T update)
        => Interlocked.CompareExchange(ref value, update, expect) == expect;

    public static implicit operator T(AtomicReference<T> reference) => reference.Value;
    public static implicit operator AtomicReference<T>(T value) => new(value);
}

