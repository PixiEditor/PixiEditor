using PixiEditor.Exceptions;
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
        ///     Copies the selection to clipboard in PNG, Bitmap and DIB formats.
        /// </summary>
        /// <param name="layers">Layers where selection is.</param>
        public static void CopyToClipboard(Layer[] layers, Coordinates[] selection, int originalImageWidth, int originalImageHeight, SerializableDocument document = null)
        {
            Clipboard.Clear();
            using Surface surface = BitmapUtils.CombineLayers(new Int32Rect(0, 0, originalImageWidth, originalImageHeight), layers);
            DataObject data = new DataObject();

            WriteableBitmap combinedBitmaps = surface.ToWriteableBitmap();
            BitmapSource croppedBmp = BitmapSelectionToBmpSource(combinedBitmaps, selection, out int offsetX, out int offsetY, out int width, out int height);
            data.SetData(typeof(CropData), new CropData(width, height, offsetX, offsetY).ToStream());

            using (SKData pngData = surface.SkiaSurface.Snapshot(SKRectI.Create(offsetX, offsetY, width, height)).Encode())
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

            data.SetData(DataFormats.Bitmap, croppedBmp, true); // Bitmap, no transparency
            data.SetImage(croppedBmp); // DIB format, no transparency

            if (document != null)
            {
                MemoryStream memoryStream = new();
                PixiParser.Serialize(document, memoryStream);
                data.SetData("PIXI", memoryStream); // PIXI, supports transparency, layers, groups and swatches
                Clipboard.SetDataObject(data, true);
            }

            Clipboard.SetDataObject(data, true);
        }

        /// <summary>
        /// Copies the selection to clipboard in PNG, Bitmap and DIB formats. <para/>
        /// Also serailizes the <paramref name="document"/> in the PIXI format and copies it to the clipboard.
        /// </summary>
        public static void CopyToClipboard(Document document)
        {
            CopyToClipboard(
                document.Layers.Where(x => document.GetFinalLayerIsVisible(x)).ToArray(),
                document.ActiveSelection.SelectedPoints.ToArray(),
                //new Coordinates[] { (0, 0), (15, 15) },
                document.Width,
                document.Height,
                document.ToSerializable());
        }

        /// <summary>
        ///     Pastes image from clipboard into new layer.
        /// </summary>
        public static void PasteFromClipboard()
        {
            var layers = GetLayersFromClipboard();

            Document activeDocument = ViewModelMain.Current.BitmapManager.ActiveDocument;
            int startIndex = activeDocument.Layers.Count;

            foreach (var layer in layers)
            {
                activeDocument.Layers.Add(layer);
            }

            activeDocument.UndoManager.AddUndoChange(
                new Change(RemoveLayersProcess, new object[] { startIndex }, AddLayersProcess, new object[] { layers }) { DisposeProcess = DisposeProcess });
        }

        /// <summary>
        ///     Gets image from clipboard, supported PNG, Dib and Bitmap.
        /// </summary>
        /// <returns>WriteableBitmap.</returns>
        public static IEnumerable<Layer> GetLayersFromClipboard()
        {
            DataObject data = (DataObject)Clipboard.GetDataObject();

            if (data.GetDataPresent("PIXI"))
            {
                SerializableDocument document = GetSerializable(data, out CropData crop);
                SKRectI cropRect = SKRectI.Create(crop.OffsetX, crop.OffsetY, crop.Width, crop.Height);

                foreach (SerializableLayer sLayer in document)
                {
                    SKRectI intersect;

                    if (/*layer.OffsetX > crop.OffsetX + crop.Width || layer.OffsetY > crop.OffsetY + crop.Height ||*/
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
            else if (TryFromSingleImage(data, out Surface singleImage))
            {
                yield return new Layer("Image", singleImage);
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
                        layer = new(Path.GetFileName(path), Importer.ImportSurface(path));
                    }
                    catch (CorruptedFileException)
                    {
                    }

                    yield return layer ?? new($"Corrupt {path}");
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static bool IsImageInClipboard()
        {
            DataObject dao = (DataObject)Clipboard.GetDataObject();
            if (dao == null)
            {
                return false;
            }

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

        public static BitmapSource BitmapSelectionToBmpSource(WriteableBitmap bitmap, Coordinates[] selection, out int offsetX, out int offsetY, out int width, out int height)
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
                    newFormat.DestinationFormat = PixelFormats.Rgba64;
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
                document.RemoveLayer(i, true);
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
