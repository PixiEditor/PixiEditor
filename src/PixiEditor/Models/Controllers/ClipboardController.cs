using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Unicode;
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
using Drawie.Backend.Core.Surfaces.PaintImpl;
using PixiEditor.Helpers;
using PixiEditor.Helpers.Constants;
using PixiEditor.Models.Clipboard;
using PixiEditor.Models.Commands.Attributes.Evaluators;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.IO;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.Models.Handlers;
using PixiEditor.Parser;
using PixiEditor.UI.Common.Localization;
using PixiEditor.ViewModels.Document;
using PixiEditor.ViewModels.Tools.Tools;
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
        RectD copyArea = RectD.Empty;

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

            surfaceToCopy = surface.AsT2.Item1;
            copyArea = (RectD)surface.AsT2.Item2;
        }
        else if (document.TransformViewModel.TransformActive)
        {
            var surface =
                document.TryExtractAreaFromSelected(
                    (RectI)document.TransformViewModel.Corners.AABBBounds.RoundOutwards());
            if (surface.IsT0 || surface.IsT1)
                return;

            surfaceToCopy = surface.AsT2.Item1;
            copyArea = document.TransformViewModel.Corners.AABBBounds;
        }
        else if (document.SelectedStructureMember != null)
        {
            RectI bounds = new RectI(VecI.Zero, document.SizeBindable);

            var surface = document.TryExtractAreaFromSelected(bounds);
            if (surface.IsT0 || surface.IsT1)
                return;

            surfaceToCopy = surface.AsT2.Item1;
            copyArea = (RectD)bounds;
        }

        if (surfaceToCopy == null)
        {
            return;
        }

        await AddImageToClipboard(surfaceToCopy, data);

        if (copyArea.Size != document.SizeBindable && copyArea.Pos != VecI.Zero && copyArea != RectD.Empty)
        {
            data.SetVecD(ClipboardDataFormats.PositionFormat, copyArea.Pos);
        }

        string[] layerIds = document.GetSelectedMembers().Select(x => x.ToString()).ToArray();
        string layerIdsString = string.Join(";", layerIds);

        byte[] layerIdsBytes = Encoding.UTF8.GetBytes(layerIdsString);

        data.Set(ClipboardDataFormats.LayerIdList, layerIdsBytes);
        data.Set(ClipboardDataFormats.DocumentFormat, Encoding.UTF8.GetBytes(document.Id.ToString()));

        await Clipboard.SetDataObjectAsync(data);
    }

    public static async Task CopyVisibleToClipboard(DocumentViewModel document, string? output = null)
    {
        await Clipboard.ClearAsync();

        DataObject data = new DataObject();

        RectD copyArea = new RectD(VecD.Zero, document.SizeBindable);

        if (!document.SelectionPathBindable.IsEmpty)
        {
            copyArea = document.SelectionPathBindable.TightBounds;
        }
        else if (document.TransformViewModel.TransformActive)
        {
            copyArea = document.TransformViewModel.Corners.AABBBounds;
        }

        if(copyArea.IsZeroOrNegativeArea || copyArea.HasNaNOrInfinity)
        {
            NoticeDialog.Show("SELECTED_AREA_EMPTY", "NOTHING_TO_COPY");
            return;
        }

        using Surface documentSurface = new Surface(document.SizeBindable);

        document.Renderer.RenderDocument(documentSurface.DrawingSurface,
            document.AnimationDataViewModel.ActiveFrameTime, document.SizeBindable, output);

        Surface surfaceToCopy = new Surface((VecI)copyArea.Size.Ceiling());
        using Paint paint = new Paint();

        surfaceToCopy.DrawingSurface.Canvas.DrawImage(
            documentSurface.DrawingSurface.Snapshot(),
            copyArea, new RectD(0, 0, copyArea.Size.X, copyArea.Size.Y), paint);

        await AddImageToClipboard(surfaceToCopy, data);

        await Clipboard.SetDataObjectAsync(data);
    }

    public static async Task<string> GetTextFromClipboard()
    {
        return await Clipboard.GetTextAsync();
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
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(TempCopyFilePath)!);
                await using FileStream fileStream = new FileStream(TempCopyFilePath, FileMode.Create, FileAccess.Write);
                await pngStream.CopyToAsync(fileStream);
            }
            catch (IOException ioException)
            {
                string secondaryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "PixiEditor",
                    $"Copied_{DateTime.Now:HH-mm-ss}.png");
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(secondaryPath)!);
                    await using FileStream fileStream =
                        new FileStream(secondaryPath, FileMode.Create, FileAccess.Write);
                    await pngStream.CopyToAsync(fileStream);
                }
                catch
                {
                    return;
                }
            }

            data.SetFileDropList(new[] { TempCopyFilePath });
        }
    }

    /// <summary>
    ///     Pastes image from clipboard into new layer.
    /// </summary>
    public static bool TryPaste(DocumentViewModel document, DocumentManagerViewModel manager, IEnumerable<IDataObject>
        data, bool pasteAsNew = false)
    {
        Guid sourceDocument = GetSourceDocument(data, document.Id);
        Guid[] layerIds = GetLayerIds(data);

        bool hasPos = data.Any(x => x.Contains(ClipboardDataFormats.PositionFormat));

        IDocument? targetDoc = manager.Documents.FirstOrDefault(x => x.Id == sourceDocument);

        if (targetDoc != null && pasteAsNew && layerIds is { Length: > 0 } && (!hasPos || AllMatchesPos(layerIds, data, targetDoc)))
        {
            foreach (var layerId in layerIds)
            {
                if (sourceDocument == document.Id)
                {
                    document.Operations.DuplicateMember(layerId);
                }
                else
                {
                    document.Operations.ImportMember(layerId, targetDoc);
                }
            }

            manager.Owner.ToolsSubViewModel.SetActiveTool<MoveToolViewModel>(false);

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
                var guid = document.Operations.CreateStructureMember(StructureMemberType.Layer,
                    new LocalizedString("NEW_LAYER"), false);

                if (guid == null)
                {
                    return false;
                }

                manager.Owner.ToolsSubViewModel.SetActiveTool<MoveToolViewModel>(false);
                document.Operations.SetSelectedMember(guid.Value);
                document.Operations.PasteImageWithTransform(dataImage.Image, position, guid.Value, false);
            }
            else
            {
                manager.Owner.ToolsSubViewModel.SetActiveTool<MoveToolViewModel>(false);
                document.Operations.PasteImageWithTransform(dataImage.Image, position);
            }

            return true;
        }

        document.Operations.PasteImagesAsLayers(images, document.AnimationDataViewModel.ActiveFrameBindable);
        return true;
    }

    private static bool AllMatchesPos(Guid[] layerIds, IEnumerable<IDataObject> data, IDocument doc)
    {
        var dataObjects = data as IDataObject[] ?? data.ToArray();

        var dataObjectWithPos = dataObjects.FirstOrDefault(x => x.Contains(ClipboardDataFormats.PositionFormat));
        VecD pos = VecD.Zero;

        if (dataObjectWithPos != null)
        {
            pos = dataObjectWithPos.GetVecD(ClipboardDataFormats.PositionFormat);
        }

        RectD? tightBounds = null;
        for (var i = 0; i < layerIds.Length; i++)
        {
            var layerId = layerIds[i];

            var layer = doc.StructureHelper.Find(layerId);

            if (layer == null) return false;

            if (tightBounds == null)
            {
                tightBounds = layer.TightBounds;
            }
            else if (layer.TightBounds.HasValue)
            {
                tightBounds = tightBounds.Value.Union(layer.TightBounds.Value);
            }
        }

        return tightBounds.HasValue && tightBounds.Value.Pos.AlmostEquals(pos);
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

    private static Guid GetSourceDocument(IEnumerable<IDataObject> data, Guid fallback)
    {
        foreach (var dataObject in data)
        {
            if (dataObject.Contains(ClipboardDataFormats.DocumentFormat))
            {
                byte[] guidBytes = (byte[])dataObject.Get(ClipboardDataFormats.DocumentFormat);
                string guidString = System.Text.Encoding.UTF8.GetString(guidBytes);
                return Guid.Parse(guidString);
            }
        }

        return fallback;
    }

    /// <summary>
    ///     Pastes image from clipboard into new layer.
    /// </summary>
    public static async Task<bool> TryPasteFromClipboard(DocumentViewModel document, DocumentManagerViewModel manager,
        bool pasteAsNew = false)
    {
        var data = await TryGetDataObject();
        if (data == null)
            return false;

        return TryPaste(document, manager, data, pasteAsNew);
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

        VecD pos = VecD.Zero;

        foreach (var dataObject in data)
        {
            if (TryExtractSingleImage(dataObject, out var singleImage))
            {
                surfaces.Add(new DataImage(singleImage,
                    dataObject.Contains(ClipboardDataFormats.PositionFormat)
                        ? (VecI)dataObject.GetVecD(ClipboardDataFormats.PositionFormat)
                        : (VecI)pos));
                continue;
            }

            if (dataObject.Contains(ClipboardDataFormats.PositionFormat))
            {
                pos = dataObject.GetVecD(ClipboardDataFormats.PositionFormat);
                for (var i = 0; i < surfaces.Count; i++)
                {
                    var surface = surfaces[i];
                    surfaces[i] = surface with { Position = (VecI)pos };
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
                    surfaces.Add(new DataImage(filename, imported,
                        (VecI)dataObject.GetVecD(ClipboardDataFormats.PositionFormat)));
                }
                catch
                {
                    continue;
                }
            }
        }

        return surfaces;
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
                string text = await ClipboardController.GetTextFromClipboard();
                if (Importer.IsSupportedFile(text))
                {
                    return text;
                }
            }
            else if (format == DataFormats.Files)
            {
                var files = await ClipboardController.Clipboard.GetDataAsync(format);
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

    public static bool IsImageFormat(string[] formats)
    {
        foreach (var format in formats)
        {
            if (format == ClipboardDataFormats.Png)
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

    public static async Task CopyNodes(Guid[] nodeIds, Guid docId)
    {
        await CopyIds(nodeIds, ClipboardDataFormats.NodeIdList, docId);
    }

    public static async Task<Guid[]> GetNodeIds()
    {
        return await GetIds(ClipboardDataFormats.NodeIdList);
    }

    public static async Task<Guid[]> GetCelIds()
    {
        return await GetIds(ClipboardDataFormats.CelIdList);
    }

    public static async Task<Guid[]> GetIds(string format)
    {
        var data = await TryGetDataObject();
        return GetIds(data, format);
    }

    private static Guid[] GetIds(IEnumerable<IDataObject?> data, string format)
    {
        foreach (var dataObject in data)
        {
            if (dataObject.Contains(format))
            {
                byte[] nodeIds = (byte[])dataObject.Get(format);
                string nodeIdsString = System.Text.Encoding.UTF8.GetString(nodeIds);
                return nodeIdsString.Split(';').Select(Guid.Parse).ToArray();
            }
        }

        return [];
    }

    public static async Task<bool> AreNodesInClipboard()
    {
        return await AreIdsInClipboard(ClipboardDataFormats.NodeIdList);
    }

    public static async Task<bool> AreCelsInClipboard()
    {
        return await AreIdsInClipboard(ClipboardDataFormats.CelIdList);
    }

    public static async Task<bool> AreIdsInClipboard(string format)
    {
        var formats = await Clipboard.GetFormatsAsync();
        if (formats == null || formats.Length == 0)
            return false;

        return formats.Contains(format);
    }

    public static async Task CopyCels(Guid[] celIds, Guid docId)
    {
        await CopyIds(celIds, ClipboardDataFormats.CelIdList, docId);
    }

    public static async Task CopyIds(Guid[] ids, string format, Guid docId)
    {
        await Clipboard.ClearAsync();

        DataObject data = new DataObject();

        data.Set(ClipboardDataFormats.DocumentFormat, Encoding.UTF8.GetBytes(docId.ToString()));

        byte[] idsBytes = Encoding.UTF8.GetBytes(string.Join(";", ids.Select(x => x.ToString())));

        data.Set(format, idsBytes);

        await Clipboard.SetDataObjectAsync(data);
    }

    public static async Task<Guid> GetDocumentId()
    {
        var data = await TryGetDataObject();
        if (data == null)
            return Guid.Empty;

        foreach (var dataObject in data)
        {
            if (dataObject.Contains(ClipboardDataFormats.DocumentFormat))
            {
                byte[] guidBytes = (byte[])dataObject.Get(ClipboardDataFormats.DocumentFormat);
                string guidString = System.Text.Encoding.UTF8.GetString(guidBytes);
                return Guid.Parse(guidString);
            }
        }

        return Guid.Empty;
    }
}
