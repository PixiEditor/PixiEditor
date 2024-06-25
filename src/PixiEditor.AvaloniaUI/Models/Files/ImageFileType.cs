using System.Security;
using ChunkyImageLib;
using PixiEditor.AvaloniaUI.Models.IO;
using PixiEditor.AvaloniaUI.Models.IO.FileEncoders;
using PixiEditor.AvaloniaUI.ViewModels.Document;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.Numerics;

namespace PixiEditor.AvaloniaUI.Models.Files;

internal abstract class ImageFileType : IoFileType
{
    public abstract EncodedImageFormat EncodedImageFormat { get; }
    
    public override FileTypeDialogDataSet.SetKind SetKind { get; } = FileTypeDialogDataSet.SetKind.Image;

    public override SaveResult TrySave(string pathWithExtension, DocumentViewModel document, ExportConfig exportConfig)
    {
        var maybeBitmap = document.TryRenderWholeImage();
        if (maybeBitmap.IsT0)
            return SaveResult.ConcurrencyError;
        var bitmap = maybeBitmap.AsT1;

        EncodedImageFormat mappedFormat = EncodedImageFormat;

        if (mappedFormat == EncodedImageFormat.Unknown)
        {
            return SaveResult.UnknownError;
        }

        UniversalFileEncoder encoder = new(mappedFormat);
        return TrySaveAs(encoder, pathWithExtension, bitmap, exportConfig);
    }
    
    /// <summary>
    /// Saves image to PNG file. Messes with the passed bitmap.
    /// </summary>
    private static SaveResult TrySaveAs(IFileEncoder encoder, string savePath, Surface bitmap, ExportConfig config)
    {
        try
        {
            VecI? exportSize = config.ExportSize;
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
