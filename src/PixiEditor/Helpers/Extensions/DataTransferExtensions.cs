using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Drawie.Backend.Core.Numerics;
using Drawie.Numerics;

namespace PixiEditor.Helpers.Extensions;

public static class DataTransferExtensions
{
    /*/// <summary>
    ///     Clears the data object and sets the specified data.
    /// </summary>
    /// <param name="data">The data object to set the data on.</param>
    /// <param name="files">File paths to set.</param>
    public static void SetFileDropList(this DataTransfer data, IEnumerable<string> files)
    {
    }*/

    /*public static IStorageItem[] GetFileDropList(this IDataObject data)
    {
        if (!data.Contains(DataFormats.Files))
            return[];

        return ((IEnumerable<IStorageItem>)data.Get(DataFormat.File)).ToArray();
    }*/

    public static bool TryGetRawTextPath(this IDataObject data, out string? path)
    {
        if (!data.Contains(DataFormats.Text))
        {
            path = null;
            return false;
        }

        try
        {
            var text = data.GetText();
            if (Directory.Exists(text) || File.Exists(text))
            {
                path = text;
                return true;
            }
        }
        catch(InvalidCastException ex) // bug on x11
        {
            path = null;
            return false;
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
    
    public static VecD GetVecD(this IDataObject data, string format)
    {
        if (!data.Contains(format))
            return new VecD(-1, -1);

        byte[] bytes = (byte[])data.Get(format);

        if (bytes is { Length: < 16 })
            return new VecD(-1, -1);

        return VecD.FromBytes(bytes);
    }

    public static void SetVecI(this DataTransfer data, DataFormat<byte[]> format, VecI value) =>
        data.Add(DataTransferItem.Create(format, value.ToByteArray()));

    public static void SetVecD(this DataTransfer data, DataFormat<byte[]> format, VecD value) =>
        data.Add(DataTransferItem.Create(format, value.ToByteArray()));
}
