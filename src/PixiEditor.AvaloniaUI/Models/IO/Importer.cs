using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using Avalonia.Media.Imaging;
using ChunkyImageLib;
using CommunityToolkit.Mvvm.ComponentModel;
using PixiEditor.AvaloniaUI.Exceptions;
using PixiEditor.AvaloniaUI.Helpers;
using PixiEditor.AvaloniaUI.Helpers.Extensions;
using PixiEditor.AvaloniaUI.ViewModels.Document;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.ImageData;
using PixiEditor.DrawingApi.Core.Surface.PaintImpl;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Extensions.Exceptions;
using PixiEditor.Parser;
using PixiEditor.Parser.Deprecated;
using Bitmap = Avalonia.Media.Imaging.Bitmap;
using BlendMode = PixiEditor.DrawingApi.Core.Surface.BlendMode;

namespace PixiEditor.AvaloniaUI.Models.IO;

internal class Importer : ObservableObject
{
    /// <summary>
    ///     Imports image from path and resizes it to given dimensions.
    /// </summary>
    /// <param name="path">Path of the image.</param>
    /// <param name="size">New size of the image.</param>
    /// <returns>WriteableBitmap of imported image.</returns>
    public static Surface? ImportImage(string path, VecI size)
    {
        if (!Path.Exists(path)) 
            throw new MissingFileException();
        
        Surface original;
        try
        {
            original = Surface.Load(path);
        }
        catch (Exception e) when (e is ArgumentException or FileNotFoundException)
        {
            throw new CorruptedFileException(e);
        }
        
        if (original.Size == size || size == VecI.NegativeOne)
        {
            return original;
        }

        Surface resized = original.ResizeNearestNeighbor(size);
        original.Dispose();
        return resized;
    }

    public static Bitmap ImportBitmap(string path)
    {
        try
        {
            return new Bitmap(path);
        }
        catch (NotSupportedException e)
        {
            throw new InvalidFileTypeException(new LocalizedString("FILE_EXTENSION_NOT_SUPPORTED", Path.GetExtension(path)), e);
        }
        /*catch (FileFormatException e) TODO: Not found in Avalonia
        {
            throw new CorruptedFileException("FAILED_TO_OPEN_FILE", e);
        }*/
        catch (Exception e)
        {
            throw new RecoverableException("ERROR_IMPORTING_IMAGE", e);
        }
    }

    public static WriteableBitmap ImportWriteableBitmap(string path)
    {
        return ImportBitmap(path).ToWriteableBitmap();
    }

    public static DocumentViewModel ImportDocument(string path, bool associatePath = true)
    {
        try
        {
            var doc = PixiParser.Deserialize(path).ToDocument();
            
            if (associatePath)
            {
                doc.FullFilePath = path;
            }

            return doc;
        }
        catch (InvalidFileException)
        {
            try
            {
                var doc = DepractedPixiParser.Deserialize(path).ToDocument();
                
                if (associatePath)
                {
                    doc.FullFilePath = path;
                }

                return doc;
            }
            catch (Exception e)
            {
                throw new CorruptedFileException("FAILED_TO_OPEN_FILE", e);
            }
        }
    }

    public static DocumentViewModel ImportDocument(byte[] file, string? originalFilePath)
    {
        try
        {
            var doc = PixiParser.Deserialize(file).ToDocument();
            doc.FullFilePath = originalFilePath;
            return doc;
        }
        catch (InvalidFileException)
        {
            try
            {
                var doc = DepractedPixiParser.Deserialize(file).ToDocument();
                doc.FullFilePath = originalFilePath;
                return doc;
            }
            catch (InvalidFileException e)
            {
                throw new CorruptedFileException("FAILED_TO_OPEN_FILE", e);
            }
        }
    }

    public static WriteableBitmap GetPreviewBitmap(string path)
    {
        if (!IsSupportedFile(path))
        {
            throw new InvalidFileTypeException(new LocalizedString("FILE_EXTENSION_NOT_SUPPORTED", Path.GetExtension(path)));
        }
        return Path.GetExtension(path) != ".pixi" ? ImportWriteableBitmap(path) : PixiParser.Deserialize(path).ToDocument().PreviewBitmap;
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

        ImageInfo info = new ImageInfo(width, height, ColorType.RgbaF16);
        IntPtr ptr = Marshal.AllocHGlobal(bytes.Length - 8);
        try
        {
            Marshal.Copy(bytes, 8, ptr, bytes.Length - 8);
            Pixmap map = new(info, ptr);
            DrawingSurface surface = DrawingSurface.Create(map);
            Surface finalSurface = new Surface(new VecI(width, height));
            using Paint paint = new() { BlendMode = BlendMode.Src };
            surface.Draw(finalSurface.DrawingSurface.Canvas, 0, 0, paint);
            return finalSurface;
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
    }
}
