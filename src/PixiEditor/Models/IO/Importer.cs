using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Exceptions;
using PixiEditor.Helpers;
using PixiEditor.Models.DataHolders;
using PixiEditor.ViewModels.SubViewModels.Document;

namespace PixiEditor.Models.IO;

internal class Importer : NotifyableObject
{
    /// <summary>
    ///     Imports image from path and resizes it to given dimensions.
    /// </summary>
    /// <param name="path">Path of the image.</param>
    /// <param name="size">New size of the image.</param>
    /// <returns>WriteableBitmap of imported image.</returns>
    public static Surface ImportImage(string path, VecI size)
    {
        Surface original = Surface.Load(path);
        if (original.Size != size)
        {
            Surface resized = original.ResizeNearestNeighbor(size);
            original.Dispose();
            return resized;
        }
        return original;
    }

    public static WriteableBitmap ImportWriteableBitmap(string path)
    {
        try
        {
            Uri uri = new Uri(path, UriKind.RelativeOrAbsolute);
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = uri;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();

            return BitmapFactory.ConvertToPbgra32Format(bitmap);
        }
        catch (NotSupportedException)
        {
            throw new CorruptedFileException();
        }
        catch (FileFormatException)
        {
            throw new CorruptedFileException();
        }
    }

    public static DocumentViewModel ImportDocument(string path)
    {
        /*
        try
        {
            Document doc = PixiEditor.Parser.PixiParser.Deserialize(path).ToDocument();
            doc.DocumentFilePath = path;
            return doc;
        }
        catch (InvalidFileException)
        {
            throw new CorruptedFileException();
        }*/
        return null;
    }

    public static bool IsSupportedFile(string path)
    {
        return SupportedFilesHelper.IsSupportedFile(path);
    }

    public static Surface LoadFromGZippedBytes(string path)
    {
        using FileStream compressedData = new(path, FileMode.Open);
        using GZipStream uncompressedData = new(compressedData, CompressionMode.Decompress);
        using MemoryStream resultBytes = new();
        uncompressedData.CopyTo(resultBytes);

        byte[] bytes = resultBytes.ToArray();
        int width = BitConverter.ToInt32(bytes, 0);
        int height = BitConverter.ToInt32(bytes, 4);

        SKImageInfo info = new SKImageInfo(width, height, SKColorType.RgbaF16);
        IntPtr ptr = Marshal.AllocHGlobal(bytes.Length - 8);
        try
        {
            Marshal.Copy(bytes, 8, ptr, bytes.Length - 8);
            SKPixmap map = new(info, ptr);
            SKSurface surface = SKSurface.Create(map);
            Surface finalSurface = new Surface(new VecI(width, height));
            using SKPaint paint = new() { BlendMode = SKBlendMode.Src };
            surface.Draw(finalSurface.DrawingSurface.Canvas, 0, 0, paint);
            return finalSurface;
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
    }
}
