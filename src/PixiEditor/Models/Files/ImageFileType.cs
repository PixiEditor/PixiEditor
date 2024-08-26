using System.Security;
using ChunkyImageLib;
using PixiEditor.Helpers;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.Bridge;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Models.IO;
using PixiEditor.Models.IO.FileEncoders;
using PixiEditor.Numerics;
using PixiEditor.ViewModels.Document;

namespace PixiEditor.Models.Files;

internal abstract class ImageFileType : IoFileType
{
    public abstract EncodedImageFormat EncodedImageFormat { get; }

    public override FileTypeDialogDataSet.SetKind SetKind { get; } = FileTypeDialogDataSet.SetKind.Image;

    public override async Task<SaveResult> TrySave(string pathWithExtension, DocumentViewModel document,
        ExportConfig exportConfig, ExportJob? job)
    {
        Surface finalSurface;
        if (exportConfig.ExportAsSpriteSheet)
        {
            job?.Report(0, new LocalizedString("GENERATING_SPRITE_SHEET"));
            finalSurface = GenerateSpriteSheet(document, exportConfig, job);
            if (finalSurface == null)
                return SaveResult.UnknownError;
        }
        else
        {
            job?.Report(0, new LocalizedString("RENDERING_IMAGE")); 
            var maybeBitmap = document.TryRenderWholeImage(0);
            if (maybeBitmap.IsT0)
                return SaveResult.ConcurrencyError;

            finalSurface = maybeBitmap.AsT1;
            if (maybeBitmap.AsT1.Size != exportConfig.ExportSize)
            {
                finalSurface = finalSurface.ResizeNearestNeighbor(exportConfig.ExportSize);
                maybeBitmap.AsT1.Dispose();
            }
        }

        EncodedImageFormat mappedFormat = EncodedImageFormat;

        if (mappedFormat == EncodedImageFormat.Unknown)
        {
            return SaveResult.UnknownError;
        }

        UniversalFileEncoder encoder = new(mappedFormat);
        var result = await TrySaveAs(encoder, pathWithExtension, finalSurface);
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
            
            job?.Report(index / (double)document.AnimationDataViewModel.FramesCount, new LocalizedString("RENDERING_FRAME", index, document.AnimationDataViewModel.FramesCount));
            int x = index % columns;
            int y = index / columns;
            Surface target = frame;
            if (config.ExportSize != frame.Size)
            {
                target =
                    frame.ResizeNearestNeighbor(new VecI(config.ExportSize.X, config.ExportSize.Y));
            }
            
            surface!.DrawingSurface.Canvas.DrawSurface(target.DrawingSurface, x * config.ExportSize.X, y * config.ExportSize.Y);
            target.Dispose();
        }, job?.CancellationTokenSource.Token ?? CancellationToken.None);

        return surface;
    }

    /// <summary>
    /// Saves image to PNG file. Messes with the passed bitmap.
    /// </summary>
    private static async Task<SaveResult> TrySaveAs(IFileEncoder encoder, string savePath, Surface bitmap)
    {
        try
        {
            if (!encoder.SupportsTransparency)
                bitmap.DrawingSurface.Canvas.DrawColor(Colors.White, BlendMode.Multiply);

            await using var stream = new FileStream(savePath, FileMode.Create);
            await encoder.SaveAsync(stream, bitmap);
        }
        catch (SecurityException)
        {
            return SaveResult.SecurityError;
        }
        catch (UnauthorizedAccessException)
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
