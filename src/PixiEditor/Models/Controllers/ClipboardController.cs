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
using PixiEditor.Helpers.Extensions;
using PixiEditor.ChangeableDocument.Enums;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Helpers;
using PixiEditor.Helpers.Constants;
using PixiEditor.Models.Clipboard;
using PixiEditor.Models.Commands.Attributes.Evaluators;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.IO;
using Drawie.Numerics;
using PixiEditor.Parser;
using PixiEditor.ViewModels.Document;
using Bitmap = Avalonia.Media.Imaging.Bitmap;

namespace PixiEditor.Models.Controllers;

#nullable enable
internal static class ClipboardController
{
    public static IClipboard Clipboard { get; private set; }
    
    public static void Initialize(IClipboard clipboard)
    {
        Clipboard = clipboard;
    }

    public static readonly string TempCopyFilePath = Path.Join(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "PixiEditor",
        "Copied.png");

    /// <summary>
    ///     Copies the document elements to clipboard like selection on PNG, Bitmap and DIB formats.
    ///     Data that is copied:
    ///     1. General image data (PNG, Bitmap, DIB), either selection or selected layers of tight bounds size
    /// 
    ///     PixiEditor specific stuff: 
    ///     2. Position of the copied area
    ///     3. Layers guid, this is used to duplicate the layer when pasting
    /// </summary>
    public static async Task CopyToClipboard(DocumentViewModel document)
    {
        await Clipboard.ClearAsync();

        DataObject data = new DataObject();

        Surface surfaceToCopy = null;
        RectI copyArea = RectI.Empty;

        if (!document.SelectionPathBindable.IsEmpty)
        {
            var surface = document.TryExtractAreaFromSelected((RectI)document.SelectionPathBindable.TightBounds);
            if (surface.IsT0)
                return;

            if (surface.IsT1)
            {
                NoticeDialog.Show("SELECTED_AREA_EMPTY", "NOTHING_TO_COPY");
                return;
            }
            
            (surfaceToCopy, copyArea) = surface.AsT2;
        }
        else if(document.TransformViewModel.TransformActive)
        {
            var surface = document.TryExtractAreaFromSelected((RectI)document.TransformViewModel.Corners.AABBBounds.RoundOutwards());
            if (surface.IsT0 || surface.IsT1)
                return;
            
            (surfaceToCopy, copyArea) = surface.AsT2;
        }
        else if (document.SelectedStructureMember != null)
        {
            RectI bounds = new RectI(VecI.Zero, document.SizeBindable);
            
            var surface = document.TryExtractAreaFromSelected(bounds);
            if (surface.IsT0 || surface.IsT1)
                return;
            
            (surfaceToCopy, copyArea) = surface.AsT2;
        }

        if (surfaceToCopy == null)
        {
            return;
        }
        
        await AddImageToClipboard(surfaceToCopy, data);

        if (copyArea.Size != document.SizeBindable && copyArea.Pos != VecI.Zero && copyArea != RectI.Empty)
        {
            data.SetVecI(ClipboardDataFormats.PositionFormat, copyArea.Pos);
        }
        
        string[] layerIds = document.GetSelectedMembers().Select(x => x.ToString()).ToArray();
        string layerIdsString = string.Join(";", layerIds);
        
        byte[] layerIdsBytes = System.Text.Encoding.UTF8.GetBytes(layerIdsString);
        
        data.Set(ClipboardDataFormats.LayerIdList, layerIdsBytes);

        await Clipboard.SetDataObjectAsync(data);
    }

    private static async Task AddImageToClipboard(Surface actuallySurface, DataObject data)
    {
        using (ImgData pngData = actuallySurface.DrawingSurface.Snapshot().Encode(EncodedImageFormat.Png))
        {
            using MemoryStream pngStream = new MemoryStream();
            await pngData.AsStream().CopyToAsync(pngStream);

            var pngArray = pngStream.ToArray();
            data.Set(ClipboardDataFormats.Png, pngArray);
            data.Set(ClipboardDataFormats.ImageSlashPng, pngArray);

            pngStream.Position = 0;
            Directory.CreateDirectory(Path.GetDirectoryName(TempCopyFilePath)!);
            await using FileStream fileStream = new FileStream(TempCopyFilePath, FileMode.Create, FileAccess.Write);
            await pngStream.CopyToAsync(fileStream);
            data.SetFileDropList(new[] { TempCopyFilePath });
        }
    }

    /// <summary>
    ///     Pastes image from clipboard into new layer.
    /// </summary>
    public static bool TryPaste(DocumentViewModel document, IEnumerable<IDataObject> data, bool pasteAsNew = false)
    {
        Guid[] layerIds = GetLayerIds(data);

        bool hasPos = data.Any(x => x.Contains(ClipboardDataFormats.PositionFormat));
        
        if (layerIds is { Length: > 0 } && !hasPos)
        {
            foreach (var layerId in layerIds)
            {
                document.Operations.DuplicateLayer(layerId);
            }
            
            return true;
        }
        
        List<DataImage> images = GetImage(data);
        if (images.Count == 0)
            return false;

        if (images.Count == 1)
        {
            var dataImage = images[0];
            var position = dataImage.Position;

            if (document.SizeBindable.X < position.X || document.SizeBindable.Y < position.Y)
            {
                position = VecI.Zero;
            }

            if (pasteAsNew)
            {
                var guid = document.Operations.CreateStructureMember(StructureMemberType.Layer, new LocalizedString("NEW_LAYER"), false);

                if (guid == null)
                {
                    return false;
                }

                document.Operations.SetSelectedMember(guid.Value);
                document.Operations.PasteImageWithTransform(dataImage.Image, position, guid.Value, false);
            }
            else
            {
                document.Operations.PasteImageWithTransform(dataImage.Image, position);
            }

            return true;
        }

        document.Operations.PasteImagesAsLayers(images, document.AnimationDataViewModel.ActiveFrameBindable);
        return true;
    }

    private static Guid[] GetLayerIds(IEnumerable<IDataObject> data)
    {
        foreach (var dataObject in data)
        {
            if (dataObject.Contains(ClipboardDataFormats.LayerIdList))
            {
                byte[] layerIds = (byte[])dataObject.Get(ClipboardDataFormats.LayerIdList);
                string layerIdsString = System.Text.Encoding.UTF8.GetString(layerIds);
                return layerIdsString.Split(';').Select(Guid.Parse).ToArray();
            }
        }

        return [];
    }

    /// <summary>
    ///     Pastes image from clipboard into new layer.
    /// </summary>
    public static async Task<bool> TryPasteFromClipboard(DocumentViewModel document, bool pasteAsNew = false)
    {
        var data = await TryGetDataObject();
        if (data == null)
            return false;

        return TryPaste(document, data, pasteAsNew);
    }

    private static async Task<List<DataObject?>> TryGetDataObject()
    {
        string[] formats = await Clipboard.GetFormatsAsync();
        if (formats.Length == 0)
            return null;

        List<DataObject?> dataObjects = new();

        for (int i = 0; i < formats.Length; i++)
        {
            string format = formats[i];
            var obj = await Clipboard.GetDataAsync(format);

            if (obj == null)
                continue;

            DataObject data = new DataObject();
            data.Set(format, obj);

            dataObjects.Add(data);
        }

        return dataObjects;
    }

    public static async Task<List<DataImage>> GetImagesFromClipboard()
    {
        var dataObj = await TryGetDataObject();
        return GetImage(dataObj);
    }

    /// <summary>
    /// Gets images from clipboard, supported PNG and Bitmap.
    /// </summary>
    public static List<DataImage> GetImage(IEnumerable<IDataObject?> data)
    {
        List<DataImage> surfaces = new();

        if (data == null)
            return surfaces;

        VecI pos = VecI.Zero;

        foreach (var dataObject in data)
        {
            if (TryExtractSingleImage(dataObject, out var singleImage))
            {
                surfaces.Add(new DataImage(singleImage,
                    dataObject.Contains(ClipboardDataFormats.PositionFormat) ? dataObject.GetVecI(ClipboardDataFormats.PositionFormat) : pos));
                continue;
            }

            if (dataObject.Contains(ClipboardDataFormats.PositionFormat))
            {
                pos = dataObject.GetVecI(ClipboardDataFormats.PositionFormat);
                for (var i = 0; i < surfaces.Count; i++)
                {
                    var surface = surfaces[i];
                    surfaces[i] = surface with { Position = pos };
                }
            }

            var paths = dataObject.GetFileDropList().Select(x => x.Path.LocalPath).ToList();
            if (paths != null && dataObject.TryGetRawTextPath(out string? textPath))
            {
                paths.Add(textPath);
            }

            if (paths == null || paths.Count == 0)
            {
                continue;
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

                        imported = Surface.Load(PixiParser.ReadPreview(stream));
                    }
                    else
                    {
                        imported = Surface.Load(path);
                    }

                    string filename = Path.GetFullPath(path);
                    surfaces.Add(new DataImage(filename, imported, dataObject.GetVecI(ClipboardDataFormats.PositionFormat)));
                }
                catch
                {
                    continue;
                }
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
            return Path.Exists(path);
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
            else if (format == DataFormats.Files)
            {
                var files = await Clipboard.GetDataAsync(format);
                if (files is IEnumerable<IStorageItem> storageFiles)
                {
                    foreach (var file in storageFiles)
                    {
                        try
                        {
                            if (Importer.IsSupportedFile(file.Path.LocalPath))
                            {
                                return file.Path.LocalPath;
                            }
                        }
                        catch (UriFormatException)
                        {
                            continue;
                        }
                    }
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

        return HasData(dataObject, ClipboardDataFormats.Png, ClipboardDataFormats.ImageSlashPng);
    }

    private static bool IsImageFormat(string[] formats)
    {
        foreach (var format in formats)
        {
            if(format == ClipboardDataFormats.Png)
            {
                return true;
            }
            
            if (Importer.IsSupportedFile(format))
            {
                return true;
            }
        }

        return false;
    }

    private static Bitmap FromPNG(IDataObject data)
    {
        object obj = data.Get("PNG");
        if (obj is byte[] bytes)
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
            if (data.Contains(ClipboardDataFormats.Png) || data.Contains(ClipboardDataFormats.ImageSlashPng))
            {
                source = FromPNG(data);
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
