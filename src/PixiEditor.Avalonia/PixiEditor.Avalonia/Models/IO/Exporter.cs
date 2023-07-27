using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using ChunkyImageLib;
using PixiEditor.Avalonia.Helpers.Extensions;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.ImageData;
using PixiEditor.Helpers;
using PixiEditor.Models.Files;
using PixiEditor.Models.IO.FileEncoders;
using PixiEditor.ViewModels.SubViewModels.Document;

namespace PixiEditor.Models.IO;

internal enum DialogSaveResult
{
    Success = 0,
    InvalidPath = 1,
    ConcurrencyError = 2,
    SecurityError = 3,
    IoError = 4,
    UnknownError = 5,
    Cancelled = 6,
}

internal enum SaveResult
{
    Success = 0,
    InvalidPath = 1,
    ConcurrencyError = 2,
    SecurityError = 3,
    IoError = 4,
    UnknownError = 5,
}

internal class ExporterResult
{
    public DialogSaveResult Result { get; set; }
    public string Path { get; set; }

    public ExporterResult(DialogSaveResult result, string path)
    {
        Result = result;
        Path = path;
    }
}

internal class Exporter
{
    /// <summary>
    /// Attempts to save file using a SaveFileDialog
    /// </summary>
    public static async Task<ExporterResult> TrySaveWithDialog(DocumentViewModel document, VecI? exportSize = null)
    {
        ExporterResult result = new(DialogSaveResult.UnknownError, null);

        if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var file = await desktop.MainWindow.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                FileTypeChoices = SupportedFilesHelper.BuildSaveFilter(true), DefaultExtension = "pixi"
            });

            if (file is null)
            {
                result.Result = DialogSaveResult.Cancelled;
                return result;
            }

            var fileType = SupportedFilesHelper.GetSaveFileType(true, file);

            var saveResult = TrySaveUsingDataFromDialog(document, file.Path.AbsolutePath, fileType, out string fixedPath, exportSize);
            if (saveResult == SaveResult.Success)
            {
                result.Path = fixedPath;
            }

            result.Result = (DialogSaveResult)saveResult;
        }

        return result;
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

            EncodedImageFormat mappedFormat = typeFromPath.ToEncodedImageFormat();

            if (mappedFormat == EncodedImageFormat.Unknown)
            {
                return SaveResult.UnknownError;
            }

            UniversalFileEncoder encoder = new(mappedFormat);

            return TrySaveAs(encoder, pathWithExtension, bitmap, exportSize);
        }
        else
        {
            Parser.PixiParser.Serialize(document.ToSerializable(), pathWithExtension);
        }

        return SaveResult.Success;
    }

    static Exporter()
    {
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
    private static SaveResult TrySaveAs(IFileEncoder encoder, string savePath, Surface bitmap, VecI? exportSize)
    {
        try
        {
            if (exportSize is not null && exportSize != bitmap.Size)
                bitmap = bitmap.ResizeNearestNeighbor((VecI)exportSize);

            if (!encoder.SupportsTransparency)
                bitmap.DrawingSurface.Canvas.DrawColor(Colors.White, DrawingApi.Core.Surface.BlendMode.Multiply);

            using var stream = new FileStream(savePath, FileMode.Create);
            encoder.SaveAsync(stream, bitmap);
        }
        catch (SecurityException)
        {
            return SaveResult.SecurityError;
        }
        catch (IOException)
        {
            return SaveResult.IoError;
        }
        catch
        {
            return SaveResult.UnknownError;
        }

        return SaveResult.Success;
    }
}
