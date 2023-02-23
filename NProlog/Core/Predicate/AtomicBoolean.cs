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

public class AtomicBoolean
{
    private volatile int value;

    public AtomicBoolean(bool b = false)
        => this.value = b ? 1 : 0;
    public bool Value
    {
        get => this.value != 0;
        set => Interlocked.Exchange(ref this.value, value ? 1 : 0);
    }
    public bool Flip()
        => this.GetAndSet((this.value +1)%2==1);

    public bool GetAndSet(bool value)
    => Interlocked.Exchange(ref this.value, value ? 1 : 0) != 0;

    public bool CompareAndSet(bool expected, bool result)
        => Interlocked.CompareExchange(ref value, result ? 1 : 0, expected ? 1 : 0) == (expected ? 1 : 0);

    public static implicit operator bool(AtomicBoolean value)
        => value.Value;

    public static implicit operator AtomicBoolean(bool value)
        => new(value);
}