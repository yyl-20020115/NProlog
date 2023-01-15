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
namespace Org.NProlog.Core.Terms;

public static class IntegerNumberCache
{
    public static readonly IntegerNumber ZERO = new (0);
    private static readonly int MIN_CACHED_VALUE = -128;
    private static readonly int MAX_CACHED_VALUE = 127;
    private static readonly int OFFSET = -MIN_CACHED_VALUE;

    static readonly IntegerNumber[] CACHE = new IntegerNumber[OFFSET + MAX_CACHED_VALUE + 1];

    static IntegerNumberCache()
    {
        for (int i = 0; i < CACHE.Length; i++)
        {
            int n = i - OFFSET;
            CACHE[i] = n == 0 ? ZERO : new (n);
        }
    }

    public static IntegerNumber ValueOf(long l) 
        => l >= MIN_CACHED_VALUE && l <= MAX_CACHED_VALUE ? CACHE[(int)l + OFFSET] : new IntegerNumber(l);
}
