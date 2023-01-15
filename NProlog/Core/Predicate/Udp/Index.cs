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
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Predicate.Udp;



public class Index
{
    private static readonly ClauseAction[] NO_MATCHES = Array.Empty<ClauseAction>();

    private readonly int[] positions;
    private readonly Dictionary<object, ClauseAction[]> result;
    private readonly KeyFactory keyFactory;

    public Index(int[] positions, Dictionary<object, ClauseAction[]> result)
    {
        this.keyFactory = KeyFactories.GetKeyFactory(positions.Length);
        this.positions = positions;
        this.result = result;
    }

    public virtual ClauseAction[] GetMatches(Term[] args)
    {
        var key = keyFactory.CreateKey(positions, args);
        return result.TryGetValue(key, out var r)?r:NO_MATCHES;
    }

    public int KeyCount => result.Count;
}
