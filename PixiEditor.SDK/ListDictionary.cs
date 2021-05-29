using System.Collections.Generic;

namespace PixiEditor.SDK
{
    internal class ListDictionary<TKey, TValue>
    {
        private readonly Dictionary<TKey, List<TValue>> dictionary;

        public ListDictionary()
        {
            dictionary = new();
        }

        public ListDictionary(ListDictionary<TKey, TValue> listDictionary)
        {
            dictionary = new Dictionary<TKey, List<TValue>>(listDictionary.dictionary);
        }

        public ListDictionary(IEnumerable<KeyValuePair<TKey, IEnumerable<TValue>>> enumerable)
        {
            dictionary = new();

            foreach (var value in enumerable)
            {
                dictionary.Add(value.Key, new List<TValue>(value.Value));
            }
        }

        public List<TValue> this[TKey key]
        {
            get => dictionary[key];
            set => dictionary[key] = value;
        }

        public void Add(TKey key, TValue value)
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary[key].Add(value);
                return;
            }

            dictionary.Add(key, new() { value });
        }

        public void Clear(TKey key) => this[key].Clear();

        public bool ContainsKey(TKey key) => dictionary.ContainsKey(key);

        public bool ContainsValue(TKey key) => this[key].Count != 0;

        public int LenghtOf(TKey key) => this[key].Count;
    }
}
