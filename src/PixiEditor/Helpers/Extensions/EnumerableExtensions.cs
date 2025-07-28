using System.Collections.Generic;

namespace PixiEditor.Helpers.Extensions;

internal static class EnumerableExtensions
{
    /// <summary>
    /// Get's the item at the <paramref name="index"/> if it matches the <paramref name="predicate"/> or the first that matches after the <paramref name="index"/>.
    /// </summary>
    /// <param name="overrun">Should the enumerator start from the bottom if it can't find the first item in the higher part</param>
    /// <returns>The first item or null if no item can be found.</returns>
    public static T IndexOrNext<T>(this IEnumerable<T> collection, Predicate<T> predicate, int index, bool overrun = true)
    {
        if (index < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        var enumerator = collection.GetEnumerator();

        // Iterate to the target index
        for (int i = 0; i < index; i++)
        {
            if (!enumerator.MoveNext())
            {
                return default;
            }
        }

        while (enumerator.MoveNext())
        {
            if (predicate(enumerator.Current))
            {
                return enumerator.Current;
            }
        }

        if (!overrun)
        {
            return default;
        }

        enumerator.Reset();

        for (int i = 0; i < index; i++)
        {
            enumerator.MoveNext();
            if (predicate(enumerator.Current))
            {
                return enumerator.Current;
            }
        }

        return default;
    }

    /// <summary>
    /// Get's the item at the <paramref name="index"/> if it matches the <paramref name="predicate"/> or the first item that matches before the <paramref name="index"/>.
    /// </summary>
    /// <param name="underrun">Should the enumerator start from the top if it can't find the first item in the lower part</param>
    /// <returns>The first item or null if no item can be found.</returns>
    public static T IndexOrPrevious<T>(this IEnumerable<T> collection, Predicate<T> predicate, int index, bool underrun = true)
    {
        if (index < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        var enumerator = collection.GetEnumerator();
        T[] previousItems = new T[index + 1];

        // Iterate to the target index
        for (int i = 0; i <= index; i++)
        {
            if (!enumerator.MoveNext())
            {
                return default;
            }

            previousItems[i] = enumerator.Current;
        }

        for (int i = index; i >= 0; i--)
        {
            if (predicate(previousItems[i]))
            {
                return previousItems[i];
            }
        }

        if (!underrun)
        {
            return default;
        }

        return IndexOrNext(collection, predicate, index, false);
    }
}
