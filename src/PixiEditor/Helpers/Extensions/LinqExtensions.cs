using System.Collections.Generic;
using System.Threading.Tasks;

namespace PixiEditor.Helpers.Extensions;

public static class LinqExtensions
{
    // This is for async predicates with either a sync or async source.
    // This is the only one you need for your example
    public static async Task<bool> AllAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, Task<bool>> predicate)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));
        foreach (var item in source)
        {
            var result = await predicate(item);
            if (!result)
                return false;
        }
        return true;
    }

    // This is for synchronous predicates with an async source.
    public static async Task<bool> AllAsync<TSource>(this IEnumerable<Task<TSource>> source, Func<TSource, bool> predicate)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));
        foreach (var item in source)
        {
            var awaitedItem = await item;
            if (!predicate(awaitedItem))
                return false;
        }
        return true;
    }
}
