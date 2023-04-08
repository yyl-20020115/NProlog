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



/**
 * Supports term indexing of user defined predicates.
 * <p>
 * See: https://en.wikipedia.org/wiki/Prolog#Term_indexing
 */
public class Indexes
{
    /**
     * Maximum number of arguments of a clause that will be considered indexable.
     * <p>
     * Note that this is not the same as the maximum number of arguments that can be included in a single index.
     */
    private static readonly int MAX_INDEXABLE_ARGS = 9;

    private readonly ClauseAction[] masterData;
    private readonly object _lock = new();
    private readonly SoftReference<Index>[] indexes;
    private readonly int[] indexableArgs;
    private readonly int numIndexableArgs;


    public Indexes(Clauses clauses)
    {
        this.indexableArgs = clauses.ImmutableColumns;
        this.masterData = clauses.ClauseActions;
        this.numIndexableArgs = System.Math.Min(indexableArgs.Length, MAX_INDEXABLE_ARGS);
        if (numIndexableArgs == 0)
        {
            throw new ArgumentException("invalid argument",nameof(clauses));
        }
        int size = 0;
        for (int i = 0, b = 1; i < numIndexableArgs; i++, b *= 2)
        {
            size += b;
        }
        indexes = new SoftReference<Index>[size + 1];
    }

    public ClauseAction[] Index(Term[] args)
    { // TODO rename
        int bitmask = CreateBitmask(args);

        return bitmask == 0 ? masterData : GetOrCreateIndex(bitmask).GetMatches(args);
    }

    public int ClauseCount => masterData.Length;

    private int CreateBitmask(Term[] args)
    {
        int bitmask = 0;
        for (int i = 0, b = 1, bitCount = 0; i < numIndexableArgs; i++, b *= 2)
        {
            if (args[indexableArgs[i]].IsImmutable)
            {
                bitmask += b;
                if (++bitCount == KeyFactories.MAX_ARGUMENTS_PER_INDEX)
                {
                    return bitmask;
                }
            }
        }
        return bitmask;
    }

    public Index GetOrCreateIndex(int bitmask)
    {
        var _ref = indexes[bitmask];
        var index = _ref?.Value;

        if (index == null)
        {
            lock (_lock)
            {
                while (index == null)
                {
                    _ref = indexes[bitmask];
                    index = _ref?.Value;

                    if (index == null)
                    {
                        indexes[bitmask] = new(index = CreateIndex(bitmask));
                    }
                }
            }
        }

        return index;
    }

    private Index CreateIndex(int bitmask)
    {
        var positions = CreatePositionsFromBitmask(bitmask);

        var map = GroupDataByPositions(positions);

        return new Index(positions, ConvertListsToArrays(map));
    }
    public static int BitCount(int i)
    {
        // HD, Figure 5-2
        i -= ((i >> 1) & 0x55555555);
        i = (i & 0x33333333) + ((i >> 2) & 0x33333333);
        i = (i + (i >> 4)) & 0x0f0f0f0f;
        i += (i >> 8);
        i += (i >> 16);
        return i & 0x3f;
    }
    private int[] CreatePositionsFromBitmask(int bitmask)
    {
        int bitCount = BitCount(bitmask);
        var positions = new int[bitCount];
        for (int b = 1, ctr = 0, idx = 0; idx < bitCount; b *= 2, ctr++)
        {
            if ((b & bitmask) == b)
            {
                positions[idx++] = indexableArgs[ctr];
            }
        }
        return positions;
    }

    private Dictionary<object, List<ClauseAction>> GroupDataByPositions(int[] positions)
    {
        Dictionary<object, List<ClauseAction>> map = new();
        var keyFactory = KeyFactories.GetKeyFactory(positions.Length);
        foreach (var clause in masterData)
        {
            var key = keyFactory.CreateKey(positions, clause.Model.Consequent.Args);
            if (!map.TryGetValue(key, out var list))
            {
                map.Add(key, list = new());
            }
            list.Add(clause);
        }
        return map;
    }

    private static Dictionary<object, ClauseAction[]> ConvertListsToArrays(Dictionary<object, List<ClauseAction>> map)
    {
        Dictionary<object, ClauseAction[]> result = new(map.Count);
        foreach (var e in map)
        {
            result.Add(e.Key, e.Value.ToArray());
        }
        return result;
    }

    // only used by tests
    public int CountReferences()
    {
        int ctr = 0;
        foreach (var index in indexes)
        {
            if (index != null) ctr++;
        }
        return ctr;
    }

    // only used by tests
    public int CountClearedReferences()
    {
        int ctr = 0;
        foreach (var index in indexes)
        {
            if (index != null && index.Value == null) ctr++;
        }
        return ctr;
    }
}
