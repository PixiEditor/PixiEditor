using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface.ImageData;
using PixiEditor.Helpers;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.IO;
using PixiEditor.ViewModels.SubViewModels.Document;

namespace PixiEditor.Models.Controllers;

#nullable enable
internal static class ClipboardController
{
    public static readonly string TempCopyFilePath = Path.Join(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "PixiEditor",
        "Copied.png");

    /// <summary>
    ///     Copies the selection to clipboard in PNG, Bitmap and DIB formats.
    /// </summary>
    public static void CopyToClipboard(DocumentViewModel document)
    {
        if (!ClipboardHelper.TryClear())
            return;

        var surface = document.MaybeExtractSelectedArea();
        if (surface.IsT0)
            return;
        if (surface.IsT1)
        {
            NoticeDialog.Show("Selected area is empty", "Nothing to copy");
            return;
        }
        var (actuallySurface, _) = surface.AsT2;
        DataObject data = new DataObject();

        using (ImgData pngData = actuallySurface.DrawingSurface.Snapshot().Encode())
        {
            // Stream should not be disposed
            MemoryStream pngStream = new MemoryStream();
            pngData.AsStream().CopyTo(pngStream);

            data.SetData("PNG", pngStream, false); // PNG, supports transparency

            pngStream.Position = 0;
            Directory.CreateDirectory(Path.GetDirectoryName(TempCopyFilePath)!);
            using FileStream fileStream = new FileStream(TempCopyFilePath, FileMode.Create, FileAccess.Write);
            pngStream.CopyTo(fileStream);
            data.SetFileDropList(new StringCollection() { TempCopyFilePath });
        }

        WriteableBitmap finalBitmap = actuallySurface.ToWriteableBitmap();
        data.SetData(DataFormats.Bitmap, finalBitmap, true); // Bitmap, no transparency
        data.SetImage(finalBitmap); // DIB format, no transparency

        ClipboardHelper.TrySetDataObject(data, true);
    }

    /// <summary>
    ///     Pastes image from clipboard into new layer.
    /// </summary>
    public static bool TryPasteFromClipboard(DocumentViewModel document)
    {
        List<(string? name, Surface image)> images = GetImagesFromClipboard();
        if (images.Count == 0)
            return false;

        if (images.Count == 1)
        {
            document.Operations.PasteImageWithTransform(images[0].image, VecI.Zero);
            return true;
        }

        document.Operations.PasteImagesAsLayers(images);
        return true;
    }

    /// <summary>
    /// Gets images from clipboard, supported PNG, Dib and Bitmap.
    /// </summary>
    public static List<(string? name, Surface image)> GetImagesFromClipboard()
    {
        DataObject data = ClipboardHelper.TryGetDataObject();
        List<(string? name, Surface image)> surfaces = new();

        if (data == null)
            return surfaces;

        if (TryExtractSingleImage(data, out Surface? singleImage))
        {
            surfaces.Add((null, singleImage));
            return surfaces;
        }
        else if (data.GetDataPresent(DataFormats.FileDrop))
        {
            foreach (string? path in data.GetFileDropList())
            {
                if (path is null || !Importer.IsSupportedFile(path))
                    continue;
                try
                {
                    Surface imported = Surface.Load(path);
                    string filename = Path.GetFileName(path);
                    surfaces.Add((filename, imported));
                }
                catch
                {
                    continue;
                }
            }
        }
        return surfaces;
    }

    public static bool IsImageInClipboard()
    {
        DataObject dao = ClipboardHelper.TryGetDataObject();
        if (dao == null)
            return false;

        try
        {
            var files = dao.GetFileDropList();
            if (files != null)
            {
                foreach (var file in files)
                {
                    if (Importer.IsSupportedFile(file))
                    {
                        return true;
                    }
                }
            }
        }
        catch (COMException)
        {
            return false;
        }

        return dao.GetDataPresent("PNG") || dao.GetDataPresent(DataFormats.Dib) ||
               dao.GetDataPresent(DataFormats.Bitmap) || dao.GetDataPresent(DataFormats.FileDrop);
    }

    private static BitmapSource FromPNG(DataObject data)
    {
        MemoryStream pngStream = (MemoryStream)data.GetData("PNG");
        PngBitmapDecoder decoder = new PngBitmapDecoder(pngStream, BitmapCreateOptions.IgnoreImageCache, BitmapCacheOption.OnLoad);

        return decoder.Frames[0];
    }

    private static bool TryExtractSingleImage(DataObject data, [NotNullWhen(true)] out Surface? result)
    {
        try
        {
            BitmapSource source;

            if (data.GetDataPresent("PNG"))
            {
                source = FromPNG(data);
            }
            else if (data.GetDataPresent(DataFormats.Dib) || data.GetDataPresent(DataFormats.Bitmap))
            {
                source = Clipboard.GetImage();
            }
            else
            {
                result = null;
                return false;
            }

            if (source.Format.IsSkiaSupported())
            {
                result = SurfaceHelpers.FromBitmapSource(source);
            }
            else
            {
                FormatConvertedBitmap newFormat = new FormatConvertedBitmap();
                newFormat.BeginInit();
                newFormat.Source = source;
                newFormat.DestinationFormat = PixelFormats.Bgra32;
                newFormat.EndInit();

                result = SurfaceHelpers.FromBitmapSource(newFormat);
            }

            return true;
        }
        catch { }

        result = null;
        return false;
    }
}
