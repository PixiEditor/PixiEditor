using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using ChunkyImageLib.DataHolders;

namespace ChunkyImageLib;

public class MultiResStore<T> : IReadOnlyDictionary<ChunkResolution, T>
{
    private const int ResolutionCount = 4;
    private readonly T[] items = new T[ResolutionCount];
    private ChunkResolution initializedFlags;
    private ChunkResolution[] keys = [];
    private T[] values = [];

    public static MultiResStore<T> WithFactory(Func<ChunkResolution, T> factory)
    {
        var store = new MultiResStore<T>();
        
        for (var i = 0; i < ResolutionCount; i++)
        {
            var res = GetResolutionFromIndex(i);
            
            store.items[i] = factory(res);
            store.initializedFlags |= res;
        }
        
        store.RebuildCache();
        return store;
    }

    public T this[ChunkResolution key]
    {
        get
        {
            return !ContainsKey(key) ? throw new KeyNotFoundException($"Key {key} not found.") : items[GetIndex(key)];
        }
    }

    public int Count => BitOperations.PopCount((uint)initializedFlags);

    public IEnumerable<ChunkResolution> Keys => keys;
    public IEnumerable<T> Values => values;

    public bool ContainsKey(ChunkResolution key)
    {
        if (!BitOperations.IsPow2((uint)key))
            throw new ArgumentException($"Can't use a composition of resolutions ({key}) as a key.");

        return (initializedFlags & key) == key;
    }

    public bool TryGetValue(ChunkResolution key, [MaybeNullWhen(false)] out T value)
    {
        if (ContainsKey(key))
        {
            value = items[GetIndex(key)];
            return true;
        }

        value = default;
        return false;
    }

    public IEnumerator<KeyValuePair<ChunkResolution, T>> GetEnumerator()
    {
        for (var i = 0; i < ResolutionCount; i++)
        {
            var res = GetResolutionFromIndex(i);
            if (ContainsKey(res))
            {
                yield return new KeyValuePair<ChunkResolution, T>(res, items[i]);
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private void RebuildCache()
    {
        var count = Count;
        var newKeys = new ChunkResolution[count];
        var newValues = new T[count];
        var idx = 0;
        for (var i = 0; i < ResolutionCount; i++)
        {
            var res = GetResolutionFromIndex(i);
            if ((initializedFlags & res) != 0)
            {
                newKeys[idx] = res;
                newValues[idx] = items[i];
                idx++;
            }
        }
        keys = newKeys;
        values = newValues;
    }

    private static int GetIndex(ChunkResolution resolution)
    {
        if (!BitOperations.IsPow2((uint)resolution))
            throw new ArgumentException($"Can't use a composition of resolutions ({resolution}).");

        var index = BitOperations.TrailingZeroCount((int)resolution);

        return index is >= 0 and < ResolutionCount
            ? index
            : throw new ArgumentOutOfRangeException(nameof(resolution), $"Resolution ({resolution}) is not supported.");
    }

    private static ChunkResolution GetResolutionFromIndex(int index) => (ChunkResolution)(1 << index);
}
