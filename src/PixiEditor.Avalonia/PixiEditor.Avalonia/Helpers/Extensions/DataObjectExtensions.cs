using System.Collections.Generic;
using Avalonia.Input;

namespace PixiEditor.Avalonia.Helpers.Extensions;

public static class DataObjectExtensions
{
    /// <summary>
    ///     Clears the data object and sets the specified data.
    /// </summary>
    /// <param name="data">The data object to set the data on.</param>
    /// <param name="files">File paths to set.</param>
    public static void SetFileDropList(this DataObject data, IEnumerable<string> files)
    {
        data.Set(DataFormats.Files, files);
    }

    public static string[] GetFileDropList(this DataObject data)
    {
        return (string[])data.Get(DataFormats.Files);
    }
}
