using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;
using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using Microsoft.Win32;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface.ImageData;
using PixiEditor.DrawingApi.Core.Surface.PaintImpl;
using PixiEditor.Helpers;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.Enums;
using PixiEditor.ViewModels.SubViewModels.Document;

namespace PixiEditor.Models.IO;

internal enum DialogSaveResult
{
    Success = 0,
    InvalidPath = 1,
    ConcurrencyError = 2,
    UnknownError = 3,
    Cancelled = 4,
}

internal enum SaveResult
{
    Success = 0,
    InvalidPath = 1,
    ConcurrencyError = 2,
    UnknownError = 3,
}

internal class Exporter
{
    /// <summary>
    /// Attempts to save file using a SaveFileDialog
    /// </summary>
    public static DialogSaveResult TrySaveWithDialog(DocumentViewModel document, out string path, VecI? exportSize = null)
    {
        path = "";
        SaveFileDialog dialog = new SaveFileDialog
        {
            Filter = SupportedFilesHelper.BuildSaveFilter(true),
            FilterIndex = 0,
            DefaultExt = "pixi"
        };

        bool? result = dialog.ShowDialog();
        if (result is null || result == false)
            return DialogSaveResult.Cancelled;

        var fileType = SupportedFilesHelper.GetSaveFileTypeFromFilterIndex(true, dialog.FilterIndex);

        var saveResult = TrySaveUsingDataFromDialog(document, dialog.FileName, fileType, out string fixedPath, exportSize);
        if (saveResult == SaveResult.Success)
            path = fixedPath;

        return (DialogSaveResult)saveResult;
    }

    /// <summary>
    /// Takes data as returned by SaveFileDialog and attempts to use it to save the document
    /// </summary>
    public static SaveResult TrySaveUsingDataFromDialog(DocumentViewModel document, string pathFromDialog, FileType fileTypeFromDialog, out string finalPath, VecI? exportSize = null)
    {
        finalPath = SupportedFilesHelper.FixFileExtension(pathFromDialog, fileTypeFromDialog);
        var saveResult = TrySave(document, finalPath, exportSize);
        if (saveResult != SaveResult.Success)
            finalPath = "";

        return saveResult;
    }

    /// <summary>
    /// Attempts to save the document into the given location, filetype is inferred from path
    /// </summary>
    public static SaveResult TrySave(DocumentViewModel document, string pathWithExtension, VecI? exportSize = null)
    {
        string directory = Path.GetDirectoryName(pathWithExtension);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            return SaveResult.InvalidPath;

        var typeFromPath = SupportedFilesHelper.ParseImageFormat(Path.GetExtension(pathWithExtension));

        if (typeFromPath != FileType.Pixi)
        {
            var maybeBitmap = document.MaybeRenderWholeImage();
            if (maybeBitmap.IsT0)
                return SaveResult.ConcurrencyError;
            var bitmap = maybeBitmap.AsT1;

            if (!encodersFactory.ContainsKey(typeFromPath))
            {
                return SaveResult.UnknownError;
            }
            
            if (!TrySaveAs(encodersFactory[typeFromPath](), pathWithExtension, bitmap, exportSize))
                return SaveResult.UnknownError;
        }
        else
        {
            Parser.PixiParser.Serialize(document.ToSerializable(), pathWithExtension);
        }

        return SaveResult.Success;
    }

    static Dictionary<FileType, Func<BitmapEncoder>> encodersFactory = new Dictionary<FileType, Func<BitmapEncoder>>();

    static Exporter()
    {
        encodersFactory[FileType.Png] = () => new PngBitmapEncoder();
        encodersFactory[FileType.Jpeg] = () => new JpegBitmapEncoder();
        encodersFactory[FileType.Bmp] = () => new BmpBitmapEncoder();
        encodersFactory[FileType.Gif] = () => new GifBitmapEncoder();
    }

    public static void SaveAsGZippedBytes(string path, Surface surface)
    {
        SaveAsGZippedBytes(path, surface, new RectI(VecI.Zero, surface.Size));
    }

    public static void SaveAsGZippedBytes(string path, Surface surface, RectI rectToSave)
    {
        var imageInfo = new ImageInfo(rectToSave.Width, rectToSave.Height, ColorType.RgbaF16);
        var unmanagedBuffer = Marshal.AllocHGlobal(rectToSave.Width * rectToSave.Height * 8);
        //+8 bytes for width and height
        var bytes = new byte[rectToSave.Width * rectToSave.Height * 8 + 8];
        try
        {
            surface.DrawingSurface.ReadPixels(imageInfo, unmanagedBuffer, rectToSave.Width * 8, rectToSave.Left, rectToSave.Top);
            Marshal.Copy(unmanagedBuffer, bytes, 8, rectToSave.Width * rectToSave.Height * 8);
        }
        finally
        {
            Marshal.FreeHGlobal(unmanagedBuffer);
        }
        BitConverter.GetBytes(rectToSave.Width).CopyTo(bytes, 0);
        BitConverter.GetBytes(rectToSave.Height).CopyTo(bytes, 4);
        using FileStream outputStream = new(path, FileMode.Create);
        using GZipStream compressedStream = new GZipStream(outputStream, CompressionLevel.Fastest);
        compressedStream.Write(bytes);
    }

    /// <summary>
    /// Saves image to PNG file. Messes with the passed bitmap.
    /// </summary>
    private static bool TrySaveAs(BitmapEncoder encoder, string savePath, Surface bitmap, VecI? exportSize)
    {
        try
        {
            if (exportSize is not null && exportSize != bitmap.Size)
                bitmap = bitmap.ResizeNearestNeighbor((VecI)exportSize);

            if (encoder is (JpegBitmapEncoder or BmpBitmapEncoder))
                bitmap.DrawingSurface.Canvas.DrawColor(Colors.White, DrawingApi.Core.Surface.BlendMode.Multiply);

            using var stream = new FileStream(savePath, FileMode.Create);
            encoder.Frames.Add(BitmapFrame.Create(bitmap.ToWriteableBitmap()));
            encoder.Save(stream);
        }
        catch (Exception err)
        {
            return false;
        }
        return true;
    }
}
