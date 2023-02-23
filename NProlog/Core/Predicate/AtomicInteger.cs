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

public class AtomicInteger
{
    private volatile int value;

    /// <summary>
    /// Creates a new <c>Atomic</c> instance with an initial value of <c>null</c>.
    /// </summary>

    /// <summary>
    /// Creates a new <c>Atomic</c> instance with the initial value provided.
    /// </summary>
    public AtomicInteger(int value = 0)
        => this.value = value;

    /// <summary>
    /// This method returns the current value.
    /// </summary>
    /// <returns>
    /// The <c>T</c> instance.
    /// </returns>
    /// <summary>
    /// This method sets the current value atomically.
    /// </summary>
    /// <param name="value">
    /// The new value to set.
    /// </param>
    public int Value
    {
        get => value;
        set => Interlocked.Exchange(ref this.value, value);
    }

    public int GetAndIncrement()
        => this.GetAndSet(this.value + 1);

    /// <summary>
    /// This method atomically sets the value and returns the original value.
    /// </summary>
    /// <param name="value">
    /// The new value.
    /// </param>
    /// <returns>
    /// The value before setting to the new value.
    /// </returns>
    public int GetAndSet(int value)
        => Interlocked.Exchange(ref this.value, value);

    /// <summary>
    /// Atomically sets the value to the given updated value if the current value <c>==</c> the expected value.
    /// </summary>
    /// <param name="expected">
    /// The value to compare against.
    /// </param>
    /// <param name="result">
    /// The value to set if the value is equal to the <c>expected</c> value.
    /// </param>
    /// <returns>
    /// <c>true</c> if the comparison and set was successful. A <c>false</c> indicates the comparison failed.
    /// </returns>
    public bool CompareAndSet(int expected, int result)
        => Interlocked.CompareExchange(ref value, result, expected) == expected;

    /// <summary>
    /// This operator allows an implicit cast from <c>Atomic&lt;T&gt;</c> to <c>T</c>.
    /// </summary>
    public static implicit operator int(AtomicInteger value)
        => value.Value;
    public static implicit operator AtomicInteger(int value)
        => new(value);
}

