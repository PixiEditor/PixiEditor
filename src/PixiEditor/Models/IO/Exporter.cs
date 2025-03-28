using System.IO.Compression;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using ChunkyImageLib;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Surfaces.ImageData;
using PixiEditor.Helpers;
using PixiEditor.Models.Files;
using Drawie.Numerics;
using PixiEditor.ViewModels.Document;

namespace PixiEditor.Models.IO;

internal enum SaveResultType
{
    Success = 0,
    InvalidPath = 1,
    ConcurrencyError = 2,
    SecurityError = 3,
    IoError = 4,
    CustomError = 5,
    UnknownError = 6,
    Cancelled = 7,
}

internal class SaveResult
{
    public SaveResultType ResultType { get; set; }
    public string? ErrorMessage { get; set; }

    public SaveResult(SaveResultType resultType, string? errorMessage = null)
    {
        ResultType = resultType;
        ErrorMessage = errorMessage;
    }
}

internal class ExporterResult
{
    public SaveResult Result { get; set; }
    public string Path { get; set; }

    public ExporterResult(SaveResult result, string path)
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
    public static async Task<ExporterResult> TrySaveWithDialog(DocumentViewModel document, ExportConfig exportConfig,
        ExportJob? job)
    {
        ExporterResult result = new(new SaveResult(SaveResultType.UnknownError), null);

        if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var file = await desktop.MainWindow.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                FileTypeChoices = SupportedFilesHelper.BuildSaveFilter(
                    FileTypeDialogDataSet.SetKind.Any & ~FileTypeDialogDataSet.SetKind.Video),
                DefaultExtension = "pixi"
            });

            if (file is null)
            {
                result.Result.ResultType = SaveResultType.Cancelled;
                return result;
            }

            var fileType = SupportedFilesHelper.GetSaveFileType(FileTypeDialogDataSet.SetKind.Any, file);

            (SaveResult Result, string finalPath) saveResult =
                await TrySaveUsingDataFromDialog(document, file.Path.LocalPath, fileType, exportConfig, job);
            if (saveResult.Result.ResultType == SaveResultType.Success)
            {
                result.Path = saveResult.finalPath;
            }

            result.Result = saveResult.Result;
        }

        return result;
    }

    /// <summary>
    /// Takes data as returned by SaveFileDialog and attempts to use it to save the document
    /// </summary>
    public static async Task<(SaveResult result, string finalPath)> TrySaveUsingDataFromDialog(
        DocumentViewModel document, string pathFromDialog, IoFileType fileTypeFromDialog, ExportConfig exportConfig,
        ExportJob? job)
    {
        string finalPath = SupportedFilesHelper.FixFileExtension(pathFromDialog, fileTypeFromDialog);
        var saveResult = await TrySaveAsync(document, finalPath, exportConfig, job);
        if (saveResult.ResultType != SaveResultType.Success)
            finalPath = "";

        return (saveResult, finalPath);
    }

    /// <summary>
    /// Attempts to save the document into the given location, filetype is inferred from path
    /// </summary>
    public static async Task<SaveResult> TrySaveAsync(DocumentViewModel document, string pathWithExtension,
        ExportConfig exportConfig, ExportJob? job)
    {
        string directory = Path.GetDirectoryName(pathWithExtension);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            return new SaveResult(SaveResultType.InvalidPath);

        var typeFromPath = SupportedFilesHelper.ParseImageFormat(Path.GetExtension(pathWithExtension));

        if (typeFromPath is null)
            return new SaveResult(SaveResultType.CustomError, "ERR_UNKNOWN_FILE_FORMAT");

        try
        {
            var result = await typeFromPath.TrySaveAsync(pathWithExtension, document, exportConfig, job);
            job?.Finish();
            return result;
        }
        catch (Exception e)
        {
            job?.Finish();
            Console.WriteLine(e);
            CrashHelper.SendExceptionInfo(e);
            return new SaveResult(SaveResultType.UnknownError);
        }
    }

    public static SaveResult TrySave(DocumentViewModel document, string pathWithExtension,
        ExportConfig exportConfig, ExportJob? job)
    {
        string directory = Path.GetDirectoryName(pathWithExtension);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            return new SaveResult(SaveResultType.InvalidPath);

        var typeFromPath = SupportedFilesHelper.ParseImageFormat(Path.GetExtension(pathWithExtension));

        if (typeFromPath is null)
            return new SaveResult(SaveResultType.CustomError, "ERR_UNKNOWN_FILE_FORMAT");

        try
        {
            var result = typeFromPath.TrySave(pathWithExtension, document, exportConfig, job);
            job?.Finish();
            return result;
        }
        catch (Exception e)
        {
            job?.Finish();
            Console.WriteLine(e);
            CrashHelper.SendExceptionInfo(e);
            return new SaveResult(SaveResultType.UnknownError);
        }
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
            surface.DrawingSurface.ReadPixels(imageInfo, unmanagedBuffer, rectToSave.Width * 8, rectToSave.Left,
                rectToSave.Top);
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
}
