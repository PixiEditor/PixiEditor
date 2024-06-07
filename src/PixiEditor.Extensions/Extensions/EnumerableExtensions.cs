namespace PixiEditor.Extensions.Extensions;

public static class EnumerableExtensions
{
    public static T ElementAtOrDefault<T>(this IEnumerable<T> enumerable, int index, T defaultValue)
    {
        if (index < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Index must be greater than or equal to 0.");
        }

        if (enumerable is IList<T> list)
        {
            return index < list.Count ? list[index] : defaultValue;
        }

        using IEnumerator<T> enumerator = enumerable.GetEnumerator();
        for (int i = 0; i <= index; i++)
        {
            if (!enumerator.MoveNext())
            {
                return defaultValue;
            }
        }

        return enumerator.Current;
    }
}
