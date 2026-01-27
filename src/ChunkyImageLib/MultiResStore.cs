using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using ChunkyImageLib.DataHolders;

namespace ChunkyImageLib;

public class MultiResStore<T> : IDictionary<ChunkResolution, T>
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
        set
        {
            items[GetIndex(key)] = value;
            initializedFlags |= key;
            RebuildCache();
        }
    }

    public int Count => BitOperations.PopCount((uint)initializedFlags);

    public bool IsReadOnly => false;

    public ICollection<ChunkResolution> Keys => keys;
    public ICollection<T> Values => values;

    public void Add(ChunkResolution key, T value)
    {
        if (ContainsKey(key)) throw new ArgumentException("An item with the same key has already been added.");
        
        this[key] = value;
    }

    public bool ContainsKey(ChunkResolution key)
    {
        if (!BitOperations.IsPow2((uint)key))
            throw new ArgumentException($"Can't use a composition of resolutions ({key}) as a key.");

        return (initializedFlags & key) == key;
    }

    public bool Remove(ChunkResolution key)
    {
        if (!ContainsKey(key)) return false;

        initializedFlags &= ~key;
        items[GetIndex(key)] = default!;
        RebuildCache();
        return true;
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

    public void Clear()
    {
        Array.Clear(items, 0, items.Length);
        initializedFlags = 0;
        RebuildCache();
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

    void ICollection<KeyValuePair<ChunkResolution, T>>.Add(KeyValuePair<ChunkResolution, T> item) =>
        Add(item.Key, item.Value);

    bool ICollection<KeyValuePair<ChunkResolution, T>>.Contains(KeyValuePair<ChunkResolution, T> item) =>
        ContainsKey(item.Key) && EqualityComparer<T>.Default.Equals(this[item.Key], item.Value);

    void ICollection<KeyValuePair<ChunkResolution, T>>.CopyTo(KeyValuePair<ChunkResolution, T>[] array, int arrayIndex)
    {
        foreach (var item in this)
        {
            array[arrayIndex++] = item;
        }
    }

    bool ICollection<KeyValuePair<ChunkResolution, T>>.Remove(KeyValuePair<ChunkResolution, T> item) =>
        ((ICollection<KeyValuePair<ChunkResolution, T>>)this).Contains(item) &&
        Remove(item.Key);

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
