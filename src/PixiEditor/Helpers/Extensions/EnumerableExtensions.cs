using System.Buffers;
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

    public static T IndexOrNextInDirection<T>(this IEnumerable<T> collection, Predicate<T> predicate, int index, NextToDirection direction, bool overrun = true) => direction switch
    {
        NextToDirection.Forwards => IndexOrNext(collection, predicate, index, overrun),
        NextToDirection.Backwards => IndexOrPrevious(collection, predicate, index, overrun),
        _ => throw new ArgumentOutOfRangeException(nameof(direction)),
    };
    
    /// <summary>
    /// Returns the element that comes immediately after the specified <paramref name="index"/> 
    /// in the given <paramref name="enumerable"/>, wrapping around to the first element if 
    /// the end of the sequence is reached.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the enumerable.</typeparam>
    /// <param name="enumerable">The source enumerable.</param>
    /// <param name="index">The index of the reference element. Must be non-negative.</param>
    /// <returns>
    /// The element immediately after the specified index, or the first element if the index 
    /// refers to the last element. Returns <c>default</c> if the enumerable is empty.
    /// </returns>
    /// <remarks>
    /// This method does not check whether the specified <paramref name="index"/> is within the 
    /// bounds of the enumerable's size. Passing a positive out-of-range index may yield unexpected results.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="enumerable"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index"/> is negative.</exception>
    public static T? WrapNextAfterIndex<T>(this IEnumerable<T> enumerable, int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentNullException.ThrowIfNull(enumerable);

        switch (enumerable)
        {
            case ICollection<T> collection:
                return NextWithKnownCount(collection, index, collection.Count);
            case IReadOnlyCollection<T> readOnlyCollection:
                return NextWithKnownCount(readOnlyCollection, index, readOnlyCollection.Count);
        }

        using var enumerator = enumerable.GetEnumerator();

        // If the enumerable is empty, return null
        if (!enumerator.MoveNext())
            return default;

        var steps = index + 1;
        var firstElement = enumerator.Current;

        while (steps-- > 0)
        {
            if (!enumerator.MoveNext())
                return firstElement;
        }
        
        return enumerator.Current;
    }

    /// <summary>
    /// Returns the element that comes immediately before the specified <paramref name="index"/> 
    /// in the given <paramref name="enumerable"/>, wrapping around to the last element if 
    /// the start of the sequence is reached.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the enumerable.</typeparam>
    /// <param name="enumerable">The source enumerable.</param>
    /// <param name="index">The index of the reference element. Must be non-negative.</param>
    /// <returns>
    /// The element immediately before the specified index, or the last element if the index 
    /// is <c>0</c>. Returns <c>default</c> if the enumerable is empty.
    /// </returns>
    /// <remarks>
    /// This method does not check whether the specified <paramref name="index"/> is within the 
    /// bounds of the enumerable's size. Passing a positive out-of-range index may yield unexpected results.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="enumerable"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index"/> is negative.</exception>
    public static T? WrapPreviousBeforeIndex<T>(this IEnumerable<T> enumerable, int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentNullException.ThrowIfNull(enumerable);
        
        return index == 0
            ? enumerable.LastOrDefault()
            : enumerable.ElementAtOrDefault(index - 1);
    }

    /// <summary>
    /// Returns the element next to the specified <paramref name="index"/> in the given 
    /// <paramref name="enumerable"/>, in the direction specified by <paramref name="direction"/>, 
    /// wrapping around if necessary.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the enumerable.</typeparam>
    /// <param name="enumerable">The source enumerable.</param>
    /// <param name="index">The index of the reference element. Must be non-negative.</param>
    /// <param name="direction">
    /// The direction in which to look for the next element (forwards or backwards).
    /// </param>
    /// <returns>
    /// The element next to the specified index in the chosen direction, with wrap-around behavior.
    /// Returns <c>default</c> if the enumerable is empty.
    /// </returns>
    /// <remarks>
    /// This method does not check whether the specified <paramref name="index"/> is within the 
    /// bounds of the enumerable's size. Passing a positive out-of-range index may yield unexpected results.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="enumerable"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index"/> is negative.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="direction"/> is not a valid <see cref="NextToDirection"/> value.</exception>
    public static T? WrapInDirectionOfIndex<T>(this IEnumerable<T> enumerable, int index, NextToDirection direction) =>
        direction switch
        {
            NextToDirection.Forwards => WrapNextAfterIndex(enumerable, index),
            NextToDirection.Backwards => WrapPreviousBeforeIndex(enumerable, index),
            _ => throw new ArgumentOutOfRangeException(nameof(direction)),
        };

    private static T? NextWithKnownCount<T>(IEnumerable<T> collection, int index, int count)
    {
        if (count == 0)
            return default;
        
        var newIndex = index + 1;

        if (newIndex < 0 || newIndex >= count)
            newIndex = newIndex < 0 ? count - 1 : 0;
        
        return collection.ElementAtOrDefault(newIndex);
    }
}

enum NextToDirection
{
    Forwards = 1,
    Backwards = -1
}
