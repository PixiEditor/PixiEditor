using System.Collections.Generic;
using Avalonia.Input;
using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.AvaloniaUI.Helpers.Extensions;

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
        if (!data.Contains(DataFormats.Files))
            return Array.Empty<string>();

        return (string[])data.Get(DataFormats.Files);
    }

    public static VecI GetVecI(this DataObject data, string format)
    {
        if (!data.Contains(format))
            return VecI.NegativeOne;

        byte[] bytes = (byte[])data.Get(format);

        if (bytes is { Length: < 8 })
            return VecI.NegativeOne;

        return VecI.FromBytes(bytes);
    }

    public static void SetVecI(this DataObject data, string format, VecI value) => data.Set(format, value.ToByteArray());
}
