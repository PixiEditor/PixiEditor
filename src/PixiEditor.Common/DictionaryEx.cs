namespace PixiEditor.Common;

public static class DictionaryEx
{
    public static void AddRangeOverride<TKey, TValue>(
        this IDictionary<TKey, TValue> dict,
        IDictionary<TKey, TValue> dictToAdd)
    {
        foreach (KeyValuePair<TKey, TValue> item in dictToAdd)
        {
            dict[item.Key] = item.Value;
        }
    }

    public static void AddRangeNewOnly<TKey, TValue>(
        this IDictionary<TKey, TValue> dict,
        IDictionary<TKey, TValue> dictToAdd)
    {
        foreach (KeyValuePair<TKey, TValue> item in dictToAdd)
        {
            dict.TryAdd(item.Key, item.Value);
        }
    }
}
