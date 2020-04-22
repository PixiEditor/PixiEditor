using System.Collections.Generic;

namespace PixiEditor.Helpers.Extensions
{
    public static class DictionaryHelper
    {
        public static void AddRangeOverride<TKey, TValue>(this IDictionary<TKey, TValue> dict, IDictionary<TKey, TValue> dictToAdd)
        {
            foreach (var item in dictToAdd)
            {
                dict[item.Key] = item.Value;
            }
        }

        public static void AddRangeNewOnly<TKey, TValue>(this IDictionary<TKey, TValue> dict, IDictionary<TKey, TValue> dictToAdd)
        {
            foreach (var item in dictToAdd)
            {
                if (!dict.ContainsKey(item.Key))
                {
                    dict.Add(item.Key, item.Value);
                }
            }
        }
    }
}
