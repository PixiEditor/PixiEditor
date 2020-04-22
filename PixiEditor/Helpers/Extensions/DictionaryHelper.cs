using System.Collections.Generic;

namespace PixiEditor.Helpers.Extensions
{
    public static class DictionaryHelper
    {
        public static void AddRangeOverride<TKey, TValue>(this IDictionary<TKey, TValue> dic, IDictionary<TKey, TValue> dictToAdd)
        {
            foreach (var item in dictToAdd)
            {
                dic[item.Key] = item.Value;
            }
        }
    }
}
