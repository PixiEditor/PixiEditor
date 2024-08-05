﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Numerics;

namespace PixiEditor.Helpers.Extensions;

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

    public static IStorageItem[] GetFileDropList(this IDataObject data)
    {
        if (!data.Contains(DataFormats.Files))
            return[];

        return ((IEnumerable<IStorageItem>)data.Get(DataFormats.Files)).ToArray();
    }

    public static bool TryGetRawTextPath(this IDataObject data, out string? path)
    {
        if (!data.Contains(DataFormats.Text))
        {
            path = null;
            return false;
        }

        string text = data.GetText();

        if (Directory.Exists(text) || File.Exists(text))
        {
            path = text;
            return true;
        }

        path = null;
        return false;
    }

    public static VecI GetVecI(this IDataObject data, string format)
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