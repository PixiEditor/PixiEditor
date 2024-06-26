using System.Security;
using ChunkyImageLib;
using PixiEditor.AvaloniaUI.Helpers;
using PixiEditor.AvaloniaUI.Models.IO;
using PixiEditor.AvaloniaUI.Models.IO.FileEncoders;
using PixiEditor.AvaloniaUI.ViewModels.Document;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.ImageData;
using PixiEditor.Numerics;

namespace PixiEditor.AvaloniaUI.Models.Files;

internal abstract class ImageFileType : IoFileType
{
    public abstract EncodedImageFormat EncodedImageFormat { get; }

    public override FileTypeDialogDataSet.SetKind SetKind { get; } = FileTypeDialogDataSet.SetKind.Image;

    public override async Task<SaveResult> TrySave(string pathWithExtension, DocumentViewModel document,
        ExportConfig exportConfig)
    {
        Surface finalSurface;
        if (exportConfig.ExportAsSpriteSheet)
        {
            finalSurface = GenerateSpriteSheet(document, exportConfig);
            if(finalSurface == null)
                return SaveResult.UnknownError;
        }
        else
        {
            var maybeBitmap = document.TryRenderWholeImage();
            if (maybeBitmap.IsT0)
                return SaveResult.ConcurrencyError;
            finalSurface = maybeBitmap.AsT1;
        }

        EncodedImageFormat mappedFormat = EncodedImageFormat;

        if (mappedFormat == EncodedImageFormat.Unknown)
        {
            return SaveResult.UnknownError;
        }

        UniversalFileEncoder encoder = new(mappedFormat);
        var result = await TrySaveAs(encoder, pathWithExtension, finalSurface, exportConfig);
        finalSurface.Dispose();
        
        return result;
    }

    private Surface? GenerateSpriteSheet(DocumentViewModel document, ExportConfig config)
    {
        if (document is null)
            return null;

        int framesCount = document.AnimationDataViewModel.FramesCount;

        int rows, columns;
        if(config.SpriteSheetRows == 0 || config.SpriteSheetColumns == 0)
            (rows, columns) = SpriteSheetUtility.CalculateGridDimensionsAuto(framesCount);
        else
            (rows, columns) = (config.SpriteSheetRows, config.SpriteSheetColumns);

        Surface surface = new Surface(new VecI(document.Width * columns, document.Height * rows));

        document.RenderFramesProgressive((frame, index) =>
        {
            int x = index % columns;
            int y = index / columns;
            surface!.DrawingSurface.Canvas.DrawSurface(frame.DrawingSurface, x * document.Width, y * document.Height);
        });
        
        return surface;
    }

    /// <summary>
    /// Saves image to PNG file. Messes with the passed bitmap.
    /// </summary>
    private static async Task<SaveResult> TrySaveAs(IFileEncoder encoder, string savePath, Surface bitmap,
        ExportConfig config)
    {
        try
        {
            VecI? exportSize = config.ExportSize;
            if (exportSize is not null && exportSize != bitmap.Size)
                bitmap = bitmap.ResizeNearestNeighbor((VecI)exportSize);

            if (!encoder.SupportsTransparency)
                bitmap.DrawingSurface.Canvas.DrawColor(Colors.White, DrawingApi.Core.Surface.BlendMode.Multiply);

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
