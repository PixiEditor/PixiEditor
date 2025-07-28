using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace PixiEditor.Models.Structures;

[DebuggerDisplay("Count = {Count}")]
internal class OneToManyDictionary<TKey, T> : ICollection<KeyValuePair<TKey, IEnumerable<T>>>
{
    private readonly Dictionary<TKey, List<T>> _dictionary;

    public OneToManyDictionary()
    {
        _dictionary = new Dictionary<TKey, List<T>>();
    }

    public OneToManyDictionary(IEnumerable<KeyValuePair<TKey, IEnumerable<T>>> enumerable)
    {
        _dictionary = new Dictionary<TKey, List<T>>(enumerable
            .Select(x => new KeyValuePair<TKey, List<T>>(x.Key, x.Value.ToList())));
    }

    public int Count => _dictionary.Count;

    public bool IsReadOnly => false;

    [NotNull]
    public List<T> this[TKey key]
    {
        get
        {
            if (_dictionary.TryGetValue(key, out List<T> values))
            {
                return values;
            }

            List<T> newList = new();
            _dictionary.Add(key, newList);

            return newList;
        }
    }

    public void Add(TKey key, T value)
    {
        if (_dictionary.TryGetValue(key, out List<T> values))
        {
            values.Add(value);
            return;
        }

        _dictionary.Add(key, new() { value });
    }

    public void AddRange(TKey key, IEnumerable<T> enumerable)
    {
        if (_dictionary.TryGetValue(key, out List<T> values))
        {
            foreach (T value in enumerable)
            {
                values.Add(value);
            }

            return;
        }

        _dictionary.Add(key, new(enumerable));
    }

    public void Add(KeyValuePair<TKey, IEnumerable<T>> item) => AddRange(item.Key, item.Value);

    public void Clear() => _dictionary.Clear();

    public void Clear(TKey key)
    {
        if (_dictionary.TryGetValue(key, out List<T> value))
        {
            value.Clear();
        }
    }

    public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);

    public bool Contains(KeyValuePair<TKey, IEnumerable<T>> item)
    {
        if (_dictionary.TryGetValue(item.Key, out List<T> values))
        {
            return item.Value.All(x => values.Contains(x));
        }

        return false;
    }

    public void CopyTo(KeyValuePair<TKey, IEnumerable<T>>[] array, int arrayIndex)
    {
        using var enumerator = GetEnumerator();

        for (int i = arrayIndex; i < array.Length; i++)
        {
            if (!enumerator.MoveNext())
            {
                break;
            }

            array[i] = enumerator.Current;
        }
    }

    public IEnumerator<KeyValuePair<TKey, IEnumerable<T>>> GetEnumerator()
    {
        foreach (var pair in _dictionary)
        {
            yield return new(pair.Key, pair.Value);
        }
    }

    public bool Remove(KeyValuePair<TKey, IEnumerable<T>> item)
    {
        if (!_dictionary.TryGetValue(item.Key, out List<T> values))
            return false;

        bool success = true;
        foreach (var enumerableItem in item.Value)
        {
            success &= values.Remove(enumerableItem);
        }

        return success;
    }

    /// <summary>
    /// Removes <paramref name="item"/> from all enumerables in the dictionary.
    /// Returns true if any entry was removed
    /// </summary>
    public bool Remove(T item)
    {
        bool anyRemoved = false;

        foreach (var enumItem in _dictionary)
        {
            anyRemoved |= enumItem.Value.Remove(item);
        }

        return anyRemoved;
    }

    public bool Remove(TKey key, T item)
    {
        if (!_dictionary.ContainsKey(key))
            return false;
        return _dictionary[key].Remove(item);
    }

    public bool Remove(TKey key) => _dictionary.Remove(key);

    IEnumerator IEnumerable.GetEnumerator() => _dictionary.GetEnumerator();
}
