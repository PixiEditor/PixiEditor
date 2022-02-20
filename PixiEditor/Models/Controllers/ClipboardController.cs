using PixiEditor.Exceptions;
using PixiEditor.Helpers;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.ImageManipulation;
using PixiEditor.Models.IO;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Undo;
using PixiEditor.Parser;
using PixiEditor.ViewModels;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PixiEditor.Models.Controllers
{
    public static class ClipboardController
    {
        public static readonly string TempCopyFilePath = Path.Join(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "PixiEditor",
                    "Copied.png");

        /// <summary>
        /// Copies the selection to clipboard in PNG, Bitmap and DIB formats. <para/>
        /// Also serailizes the <paramref name="document"/> in the PIXI format and copies it to the clipboard.
        /// </summary>
        public static void CopyToClipboard(Document document)
        {
            CopyToClipboard(
                document.Layers.Where(x => document.GetFinalLayerIsVisible(x) && x.IsActive).ToArray(),
                document.ActiveSelection.SelectionLayer,
                document.LayerStructure,
                document.Width,
                document.Height,
                null/*document.ToSerializable()*/);
        }

        private static Surface CreateMaskedCombinedSurface(Layer[] layers, LayerStructure structure, Layer selLayer)
        {
            if (layers.Length == 0)
                throw new ArgumentException("Can't combine 0 layers");
            selLayer.ClipCanvas();

            Surface combined = BitmapUtils.CombineLayers(new Int32Rect(selLayer.OffsetX, selLayer.OffsetY, selLayer.Width, selLayer.Height), layers, structure);
            using SKImage snapshot = selLayer.LayerBitmap.SkiaSurface.Snapshot();
            combined.SkiaSurface.Canvas.DrawImage(snapshot, 0, 0, Surface.MaskingPaint);
            return combined;
        }

        /// <summary>
        ///     Copies the selection to clipboard in PNG, Bitmap and DIB formats.
        /// </summary>
        /// <param name="layers">Layers where selection is.</param>
        public static void CopyToClipboard(Layer[] layers, Layer selLayer, LayerStructure structure, int originalImageWidth, int originalImageHeight, SerializableDocument document = null)
        {
            if (!ClipboardHelper.TryClear())
                return;
            if (layers.Length == 0)
                return;

            using Surface surface = CreateMaskedCombinedSurface(layers, structure, selLayer);
            DataObject data = new DataObject();


            //BitmapSource croppedBmp = BitmapSelectionToBmpSource(finalBitmap, selLayer, out int offsetX, out int offsetY, out int width, out int height);

            //Remove for now
            //data.SetData(typeof(CropData), new CropData(width, height, offsetX, offsetY).ToStream());

            using (SKData pngData = surface.SkiaSurface.Snapshot().Encode())
            {
                // Stream should not be disposed
                MemoryStream pngStream = new MemoryStream();
                pngData.AsStream().CopyTo(pngStream);

                data.SetData("PNG", pngStream, false); // PNG, supports transparency

                pngStream.Position = 0;
                Directory.CreateDirectory(Path.GetDirectoryName(TempCopyFilePath));
                using FileStream fileStream = new FileStream(TempCopyFilePath, FileMode.Create, FileAccess.Write);
                pngStream.CopyTo(fileStream);
                data.SetFileDropList(new StringCollection() { TempCopyFilePath });
            }

            WriteableBitmap finalBitmap = surface.ToWriteableBitmap();
            data.SetData(DataFormats.Bitmap, finalBitmap, true); // Bitmap, no transparency
            data.SetImage(finalBitmap); // DIB format, no transparency

            // Remove pixi copying for now
            /*
            if (document != null)
            {
                MemoryStream memoryStream = new();
                PixiParser.Serialize(document, memoryStream);
                data.SetData("PIXI", memoryStream); // PIXI, supports transparency, layers, groups and swatches
                ClipboardHelper.TrySetDataObject(data, true);
            }
            */

            ClipboardHelper.TrySetDataObject(data, true);
        }

        /// <summary>
        ///     Pastes image from clipboard into new layer.
        /// </summary>
        public static void PasteFromClipboard(Document document)
        {
            IEnumerable<Layer> layers;
            try
            {
                layers = GetLayersFromClipboard(document);
            }
            catch
            {
                return;
            }

            int startIndex = document.Layers.Count;

            int resizedCount = 0;

            foreach (var layer in layers)
            {
                if(layer.Width > document.Width || layer.Height > document.Height)
                {
                    document.ResizeCanvas(Math.Max(document.Width, layer.Width), Math.Max(document.Height, layer.Height), Enums.AnchorPoint.Left | Enums.AnchorPoint.Top);
                    resizedCount++;
                }

                document.Layers.Add(layer);
            }

            document.UndoManager.AddUndoChange(
                new Change(RemoveLayersProcess, new object[] { startIndex }, AddLayersProcess, new object[] { layers }) { DisposeProcess = DisposeProcess });

            document.UndoManager.SquashUndoChanges(resizedCount + 1, "Paste from clipboard");
        }

        /// <summary>
        ///     Gets image from clipboard, supported PNG, Dib and Bitmap.
        /// </summary>
        private static IEnumerable<Layer> GetLayersFromClipboard(Document document)
        {
            DataObject data = ClipboardHelper.TryGetDataObject();
            if (data == null)
                yield break;

            //Remove pixi for now
            /*
            if (data.GetDataPresent("PIXI"))
            {
                SerializableDocument document = GetSerializable(data, out CropData crop);
                SKRectI cropRect = SKRectI.Create(crop.OffsetX, crop.OffsetY, crop.Width, crop.Height);

                foreach (SerializableLayer sLayer in document)
                {
                    SKRectI intersect;

                    if (//layer.OffsetX > crop.OffsetX + crop.Width || layer.OffsetY > crop.OffsetY + crop.Height ||
                        !sLayer.IsVisible || sLayer.Opacity == 0 ||
                        (intersect = SKRectI.Intersect(cropRect, sLayer.GetRect())) == SKRectI.Empty)
                    {
                        continue;
                    }

                    var layer = sLayer.ToLayer();

                    layer.Crop(intersect);

                    yield return layer;
                }
            }
            else */
            if (TryFromSingleImage(data, out Surface singleImage))
            {
                yield return new Layer("Image", singleImage, document.Width, document.Height);
            }
            else if (data.GetDataPresent(DataFormats.FileDrop))
            {
                foreach (string path in data.GetFileDropList())
                {
                    if (!Importer.IsSupportedFile(path))
                    {
                        continue;
                    }

                    Layer layer = null;

                    try
                    {
                        layer = new(Path.GetFileName(path), Importer.ImportSurface(path), document.Width, document.Height);
                    }
                    catch (CorruptedFileException)
                    {
                    }

                    yield return layer ?? new($"Corrupt {path}", document.Width, document.Height);
                }
            }
            else
            {
                yield break;
            }
        }

        public static bool IsImageInClipboard()
        {
            DataObject dao = ClipboardHelper.TryGetDataObject();
            if (dao == null)
                return false;

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

            return dao.GetDataPresent("PNG") || dao.GetDataPresent(DataFormats.Dib) ||
                   dao.GetDataPresent(DataFormats.Bitmap) || dao.GetDataPresent(DataFormats.FileDrop) ||
                   dao.GetDataPresent("PIXI");
        }

        private static BitmapSource BitmapSelectionToBmpSource(WriteableBitmap bitmap, Coordinates[] selection, out int offsetX, out int offsetY, out int width, out int height)
        {
            offsetX = selection.Min(min => min.X);
            offsetY = selection.Min(min => min.Y);
            width = selection.Max(max => max.X) - offsetX + 1;
            height = selection.Max(max => max.Y) - offsetY + 1;
            return bitmap.Crop(offsetX, offsetY, width, height);
        }

        private static BitmapSource FromPNG(DataObject data)
        {
            MemoryStream pngStream = data.GetData("PNG") as MemoryStream;
            PngBitmapDecoder decoder = new PngBitmapDecoder(pngStream, BitmapCreateOptions.IgnoreImageCache, BitmapCacheOption.OnLoad);

            return decoder.Frames[0];
        }

        private static unsafe SerializableDocument GetSerializable(DataObject data, out CropData cropData)
        {
            MemoryStream pixiStream = data.GetData("PIXI") as MemoryStream;
            SerializableDocument document = PixiParser.Deserialize(pixiStream);

            if (data.GetDataPresent(typeof(CropData)))
            {
                cropData = CropData.FromStream(data.GetData(typeof(CropData)) as MemoryStream);
            }
            else
            {
                cropData = new CropData(document.Width, document.Height, 0, 0);
            }

            return document;
        }

        private static bool TryFromSingleImage(DataObject data, out Surface result)
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
                    result = new Surface(source);
                }
                else
                {
                    FormatConvertedBitmap newFormat = new FormatConvertedBitmap();
                    newFormat.BeginInit();
                    newFormat.Source = source;
                    newFormat.DestinationFormat = PixelFormats.Bgra32;
                    newFormat.EndInit();

                    result = new Surface(newFormat);
                }

                return true;
            }
            catch { }

            result = null;
            return false;
        }

        private static void RemoveLayersProcess(object[] parameters)
        {
            if (parameters.Length == 0 || parameters[0] is not int i)
            {
                return;
            }

            Document document = ViewModelMain.Current.BitmapManager.ActiveDocument;

            while (i < document.Layers.Count)
            {
                document.RemoveLayer(i, false);
            }
        }

        private static void AddLayersProcess(object[] parameters)
        {
            if (parameters.Length == 0 || parameters[0] is not IEnumerable<Layer> layers)
            {
                return;
            }

            foreach (var layer in layers)
            {
                ViewModelMain.Current.BitmapManager.ActiveDocument.Layers.Add(layer);
            }
        }

        private static void DisposeProcess(object[] rev, object[] proc)
        {
            if (proc[0] is IEnumerable<Layer> layers)
            {
                foreach (var layer in layers)
                {
                    layer.LayerBitmap.Dispose();
                }
            }
        }
    }
}
