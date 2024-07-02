using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using ChunkyImageLib;
using PixiEditor.AvaloniaUI.Helpers;
using PixiEditor.AvaloniaUI.Helpers.Constants;
using PixiEditor.AvaloniaUI.Helpers.Extensions;
using PixiEditor.AvaloniaUI.Models.Clipboard;
using PixiEditor.AvaloniaUI.Models.Commands.Attributes.Evaluators;
using PixiEditor.AvaloniaUI.Models.Dialogs;
using PixiEditor.AvaloniaUI.Models.IO;
using PixiEditor.AvaloniaUI.ViewModels.Document;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface.ImageData;
using PixiEditor.Numerics;
using PixiEditor.Parser;
using PixiEditor.Parser.Deprecated;
using Bitmap = Avalonia.Media.Imaging.Bitmap;

namespace PixiEditor.AvaloniaUI.Models.Controllers;

#nullable enable
internal static class ClipboardController
{
    public static IClipboard Clipboard { get; private set; }
    private const string PositionFormat = "PixiEditor.Position";

    public static void Initialize(IClipboard clipboard)
    {
        Clipboard = clipboard;
    }
    
    public static readonly string TempCopyFilePath = Path.Join(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "PixiEditor",
        "Copied.png");

    /// <summary>
    ///     Copies the selection to clipboard in PNG, Bitmap and DIB formats.
    /// </summary>
    public static async Task CopyToClipboard(DocumentViewModel document)
    {
        await Clipboard.ClearAsync();

        var surface = document.MaybeExtractSelectedArea();
        if (surface.IsT0)
            return;
        if (surface.IsT1)
        {
            NoticeDialog.Show("SELECTED_AREA_EMPTY", "NOTHING_TO_COPY");
            return;
        }

        var (actuallySurface, area) = surface.AsT2;
        DataObject data = new DataObject();

        using (ImgData pngData = actuallySurface.DrawingSurface.Snapshot().Encode())
        {
            using MemoryStream pngStream = new MemoryStream();
            await pngData.AsStream().CopyToAsync(pngStream);

            data.Set(ClipboardDataFormats.Png, pngStream.ToArray()); // PNG, supports transparency

            pngStream.Position = 0;
            Directory.CreateDirectory(Path.GetDirectoryName(TempCopyFilePath)!);
            await using FileStream fileStream = new FileStream(TempCopyFilePath, FileMode.Create, FileAccess.Write);
            await pngStream.CopyToAsync(fileStream);
            data.SetFileDropList(new [] { TempCopyFilePath });
        }

        WriteableBitmap finalBitmap = actuallySurface.ToWriteableBitmap();
        data.Set(ClipboardDataFormats.Bitmap, finalBitmap); // Bitmap, no transparency
        data.Set(ClipboardDataFormats.Dib, finalBitmap); // DIB format, no transparency

        if (area.Size != document.SizeBindable && area.Pos != VecI.Zero)
        {
            data.SetVecI(PositionFormat, area.Pos);
        }

        await Clipboard.SetDataObjectAsync(data);
    }

    /// <summary>
    ///     Pastes image from clipboard into new layer.
    /// </summary>
    public static bool TryPaste(DocumentViewModel document, IDataObject data, bool pasteAsNew = false)
    {
        List<DataImage> images = GetImage(data);
        if (images.Count == 0)
            return false;

        if (images.Count == 1)
        {
            var dataImage = images[0];
            var position = dataImage.position;

            if (document.SizeBindable.X < position.X || document.SizeBindable.Y < position.Y)
            {
                position = VecI.Zero;
            }

            if (pasteAsNew)
            {
                var guid = document.Operations.CreateStructureMember(StructureMemberType.Layer, "New Layer", false);

                if (guid == null)
                {
                    return false;
                }

                document.Operations.SetSelectedMember(guid.Value);
                document.Operations.PasteImageWithTransform(dataImage.image, position, guid.Value, false);
            }
            else
            {
                document.Operations.PasteImageWithTransform(dataImage.image, position);
            }

            return true;
        }

        document.Operations.PasteImagesAsLayers(images, document.AnimationDataViewModel.ActiveFrameBindable);
        return true;
    }

    /// <summary>
    ///     Pastes image from clipboard into new layer.
    /// </summary>
    public static async Task<bool> TryPasteFromClipboard(DocumentViewModel document, bool pasteAsNew = false)
    {
        //TODO: maybe if we have access to more formats, we can check them as well
        var data = await TryGetDataObject();
        if (data == null)
            return false;

        return TryPaste(document, data, pasteAsNew);
    }

    private static async Task<DataObject?> TryGetDataObject()
    {
        string[] formats = await Clipboard.GetFormatsAsync();
        if (formats.Length == 0)
            return null;

        string format = formats[0];
        var obj = await Clipboard.GetDataAsync(format);

        if (obj == null)
            return null;

        DataObject data = new DataObject();
        data.Set(format, obj);
        return data;
    }

    public static async Task<List<DataImage>> GetImagesFromClipboard()
    {
        var dataObj = await TryGetDataObject();
        return GetImage(dataObj);
    }

    /// <summary>
    /// Gets images from clipboard, supported PNG, Dib and Bitmap.
    /// </summary>
    public static List<DataImage> GetImage(IDataObject? data)
    {
        List<DataImage> surfaces = new();

        if (data == null)
            return surfaces;

        if (TryExtractSingleImage(data, out var singleImage))
        {
            surfaces.Add(new DataImage(singleImage, data.GetVecI(PositionFormat)));
            return surfaces;
        }

        var paths = data.GetFileDropList().Select(x => x.Path.LocalPath).ToList();
        if(paths != null && data.TryGetRawTextPath(out string? textPath))
        {
            paths.Add(textPath);
        }

        if (paths == null || paths.Count == 0)
        {
            return surfaces;
        }

        foreach (string? path in paths)
        {
            if (path is null || !Importer.IsSupportedFile(path))
                continue;
            try
            {
                Surface imported;

                if (Path.GetExtension(path) == ".pixi")
                {
                    using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);

                    try
                    {
                        imported = Surface.Load(PixiParser.Deserialize(path).PreviewImage);
                    }
                    catch (InvalidFileException e)
                    {
                        // Check if it could be a old file
                        if (!e.Message.StartsWith("Header"))
                        {
                            throw;
                        }

                        stream.Position = 0;
                        using var bitmap = DepractedPixiParser.Deserialize(stream).RenderOldDocument();
                        var size = new VecI(bitmap.Width, bitmap.Height);
                        imported = new Surface(size);
                        imported.DrawBytes(size, bitmap.Bytes, ColorType.RgbaF32, AlphaType.Premul);

                        System.Diagnostics.Debug.Write(imported.ToString());
                    }
                }
                else
                {
                    imported = Surface.Load(path);
                }

                string filename = Path.GetFullPath(path);
                surfaces.Add(new DataImage(filename, imported, data.GetVecI(PositionFormat)));
            }
            catch
            {
                continue;
            }
        }

        return surfaces;
    }

    [Evaluator.CanExecute("PixiEditor.Clipboard.HasImageInClipboard")]
    public static async Task<bool> IsImageInClipboard()
    {
        var formats = await Clipboard.GetFormatsAsync();
        if (formats == null || formats.Length == 0)
            return false;

        bool isImage = IsImageFormat(formats);

        if (!isImage)
        {
            string path = await TryFindImageInFiles(formats);
            return path != string.Empty;
        }

        return isImage;
    }

    private static async Task<string> TryFindImageInFiles(string[] formats)
    {
        foreach (string format in formats)
        {
            if (format == DataFormats.Text)
            {
                string text = await Clipboard.GetTextAsync();
                if (Importer.IsSupportedFile(text))
                {
                    return text;
                }
            }
        }

        return string.Empty;
    }

    public static bool IsImage(IDataObject? dataObject)
    {
        if (dataObject == null)
            return false;

        try
        {
            var files = dataObject.GetFileDropList();
            if (files != null)
            {
                if (IsImageFormat(files.Select(x => x.Path.LocalPath).ToArray()))
                {
                    return true;
                }
            }
        }
        catch (COMException)
        {
            return false;
        }

        return HasData(dataObject, "PNG", ClipboardDataFormats.Dib, ClipboardDataFormats.Bitmap);
    }

    private static bool IsImageFormat(string[] files)
    {
        foreach (var file in files)
        {
            if (Importer.IsSupportedFile(file))
            {
                return true;
            }
        }

        return false;
    }

    private static Bitmap FromPNG(IDataObject data)
    {
        object obj = data.Get("PNG");
        if(obj is byte[] bytes)
        {
            using MemoryStream stream = new MemoryStream(bytes);
            return new Bitmap(stream);
        }

        if (obj is MemoryStream memoryStream)
        {
            return new Bitmap(memoryStream);
        }

        throw new InvalidDataException("PNG data is not in a supported format.");
    }

    private static bool HasData(IDataObject dataObject, params string[] formats) => formats.Any(dataObject.Contains);
    
    private static bool TryExtractSingleImage(IDataObject data, [NotNullWhen(true)] out Surface? result)
    {
        try
        {
            Bitmap source;
            if (data.Contains("PNG"))
            {
                source = FromPNG(data);
            }
            else if (HasData(data, ClipboardDataFormats.Dib, ClipboardDataFormats.Bitmap))
            {
                var imgs = GetImage(data);
                if (imgs == null || imgs.Count == 0)
                {
                    result = null;
                    return false;
                }

                result = imgs[0].image;
                return true;
            }
            else
            {
                result = null;
                return false;
            }

            if (source.Format.Value.IsSkiaSupported())
            {
                result = SurfaceHelpers.FromBitmap(source);
            }
            else
            {
                source.ExtractPixels(out IntPtr address);
                Bitmap newFormat = new Bitmap(PixelFormats.Bgra8888, AlphaFormat.Premul, address, source.PixelSize,
                    source.Dpi, source.PixelSize.Width * 4);

                result = SurfaceHelpers.FromBitmap(newFormat);
            }

            return true;
        }
        catch { }

        result = null;
        return false;
    }
}
