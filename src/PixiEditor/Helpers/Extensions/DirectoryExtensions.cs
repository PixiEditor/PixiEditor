using System.Linq;

namespace PixiEditor.Helpers.Extensions;

internal static class DirectoryExtensions
{
    /// <summary>
    ///     Gets files in directory with multiple filters.
    /// </summary>
    /// <param name="sourceFolder">Folder to get files from.</param>
    /// <param name="filters">Filters separated by '|' character.</param>
    /// <param name="searchOption">Search option for directory.</param>
    /// <returns>List of file paths found.</returns>
    public static string[] GetFiles(string sourceFolder, string filters, System.IO.SearchOption searchOption)
    {
        return filters.Split('|').SelectMany(filter => System.IO.Directory.GetFiles(sourceFolder, $"*{filter}", searchOption)).ToArray();
    }
}
