using System.Security;
using ChunkyImageLib;
using PixiEditor.Helpers;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces;
using PixiEditor.Models.IO;
using PixiEditor.Models.IO.FileEncoders;
using Drawie.Numerics;
using PixiEditor.AnimationRenderer.FFmpeg;
using PixiEditor.UI.Common.Localization;
using PixiEditor.ViewModels.Document;

namespace PixiEditor.Models.Files;

internal abstract class ImageFileType : IoFileType
{
    public abstract EncodedImageFormat EncodedImageFormat { get; }

    public override FileTypeDialogDataSet.SetKind SetKind { get; } = FileTypeDialogDataSet.SetKind.Image;

    public override async Task<SaveResult> TrySaveAsync(string pathWithExtension, DocumentViewModel document,
        ExportConfig exportConfig, ExportJob? job)
    {
        Surface finalSurface;
        if (exportConfig.ExportAsSpriteSheet)
        {
            job?.Report(0, new LocalizedString("GENERATING_SPRITE_SHEET"));
            finalSurface = GenerateSpriteSheet(document, exportConfig, job);
            if (finalSurface == null)
                return new SaveResult(SaveResultType.CustomError, "ERR_FAILED_GENERATE_SPRITE_SHEET");
        }
        else
        {
            job?.Report(0, new LocalizedString("RENDERING_IMAGE"));

            var exportSize = exportConfig.ExportSize;
            if (exportSize.X <= 0 || exportSize.Y <= 0)
            {
                return new SaveResult(SaveResultType.CustomError, "ERR_EXPORT_SIZE_INVALID");
            }

            var maybeBitmap = document.TryRenderWholeImage(0, exportSize, exportConfig.ExportOutput);
            if (maybeBitmap.IsT0)
                return new SaveResult(SaveResultType.ConcurrencyError);

            finalSurface = maybeBitmap.AsT1;
        }

        EncodedImageFormat mappedFormat = EncodedImageFormat;

        if (mappedFormat == EncodedImageFormat.Unknown)
        {
            return new SaveResult(SaveResultType.CustomError, new LocalizedString("ERR_UNKNOWN_IMG_FORMAT", EncodedImageFormat));
        }

        UniversalFileEncoder encoder = new(mappedFormat);
        var result = await TrySaveAsAsync(encoder, pathWithExtension, finalSurface);
        finalSurface.Dispose();

        job?.Report(1, new LocalizedString("FINISHED"));

        return result;
    }

    public override SaveResult TrySave(string pathWithExtension, DocumentViewModel document, ExportConfig config,
        ExportJob? job)
    {
        Surface finalSurface;
        if (config.ExportAsSpriteSheet)
        {
            job?.Report(0, new LocalizedString("GENERATING_SPRITE_SHEET"));
            finalSurface = GenerateSpriteSheet(document, config, job);
            if (finalSurface == null)
                return new SaveResult(SaveResultType.CustomError, "ERR_FAILED_GENERATE_SPRITE_SHEET");
        }
        else
        {
            job?.Report(0, new LocalizedString("RENDERING_IMAGE"));

            var exportSize = config.ExportSize;
            if (exportSize.X <= 0 || exportSize.Y <= 0)
            {
                return new SaveResult(SaveResultType.CustomError, "ERR_EXPORT_SIZE_INVALID");
            }

            var maybeBitmap = document.TryRenderWholeImage(0, exportSize, config.ExportOutput);
            if (maybeBitmap.IsT0)
                return new SaveResult(SaveResultType.ConcurrencyError);

            finalSurface = maybeBitmap.AsT1;
        }

        EncodedImageFormat mappedFormat = EncodedImageFormat;

        if (mappedFormat == EncodedImageFormat.Unknown)
        {
            return new SaveResult(SaveResultType.CustomError, new LocalizedString("ERR_UNKNOWN_IMG_FORMAT", EncodedImageFormat));
        }

        UniversalFileEncoder encoder = new(mappedFormat);
        var result = TrySaveAs(encoder, pathWithExtension, finalSurface);
        finalSurface.Dispose();

        job?.Report(1, new LocalizedString("FINISHED"));

        return result;
    }

    private Surface? GenerateSpriteSheet(DocumentViewModel document, ExportConfig config, ExportJob? job)
    {
        if (document is null)
            return null;

        var (rows, columns) = (config.SpriteSheetRows, config.SpriteSheetColumns);

        rows = Math.Max(1, rows);
        columns = Math.Max(1, columns);

        Surface surface = new Surface(new VecI(config.ExportSize.X * columns, config.ExportSize.Y * rows));

        job?.Report(0, new LocalizedString("RENDERING_FRAME", 0, document.AnimationDataViewModel.FramesCount));

        document.RenderFramesProgressive(
            (frame, index) =>
            {
                job?.CancellationTokenSource.Token.ThrowIfCancellationRequested();

                job?.Report(index / (double)document.AnimationDataViewModel.FramesCount,
                    new LocalizedString("RENDERING_FRAME", index, document.AnimationDataViewModel.FramesCount));
                int x = index % columns;
                int y = index / columns;
                Surface target = frame;
                if (config.ExportSize != frame.Size)
                {
                    target =
                        frame.ResizeNearestNeighbor(new VecI(config.ExportSize.X, config.ExportSize.Y));
                }

                surface!.DrawingSurface.Canvas.DrawSurface(target.DrawingSurface, x * config.ExportSize.X,
                    y * config.ExportSize.Y);
                target.Dispose();
            }, job?.CancellationTokenSource.Token ?? CancellationToken.None, config.ExportOutput);

        return surface;
    }

    /// <summary>
    /// Saves image to PNG file. Messes with the passed bitmap.
    /// </summary>
    private static async Task<SaveResult> TrySaveAsAsync(IFileEncoder encoder, string savePath, Surface bitmap)
    {
        try
        {
            await using var stream = new FileStream(savePath, FileMode.Create);
            await encoder.SaveAsync(stream, bitmap);
        }
        catch (SecurityException)
        {
            return new SaveResult(SaveResultType.SecurityError);
        }
        catch (UnauthorizedAccessException)
        {
            return new SaveResult(SaveResultType.SecurityError);
        }
        catch (IOException)
        {
            return new SaveResult(SaveResultType.IoError);
        }
        catch
        {
            return new SaveResult(SaveResultType.UnknownError);
        }

        return new SaveResult(SaveResultType.Success);
    }

    /// <summary>
    /// Saves image to PNG file. Messes with the passed bitmap.
    /// </summary>
    private static SaveResult TrySaveAs(IFileEncoder encoder, string savePath, Surface bitmap)
    {
        try
        {
            if (!encoder.SupportsTransparency)
                bitmap.DrawingSurface.Canvas.DrawColor(Colors.White, BlendMode.Multiply);

            using var stream = new FileStream(savePath, FileMode.Create);
            encoder.Save(stream, bitmap);
        }
        catch (SecurityException)
        {
            return new SaveResult(SaveResultType.SecurityError);
        }
        catch (UnauthorizedAccessException)
        {
            return new SaveResult(SaveResultType.SecurityError);
        }
        catch (IOException)
        {
            return new SaveResult(SaveResultType.IoError);
        }
        catch
        {
            return new SaveResult(SaveResultType.UnknownError);
        }

        return new SaveResult(SaveResultType.Success);
    }
}
