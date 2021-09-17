using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.ImageManipulation;
using PixiEditor.Models.IO;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Undo;
using PixiEditor.Parser;
using PixiEditor.Parser.Skia;
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
            using Surface surface = BitmapUtils.CombineLayers(originalImageWidth, originalImageHeight, layers);
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
                //doc.ActiveSelection.SelectedPoints.ToArray(),
                new Coordinates[] { (0, 0), (15, 15) },
                document.Width,
                document.Height,
                document.ToSerializable());
        }

        /// <summary>
        ///     Pastes image from clipboard into new layer.
        /// </summary>
        public static void PasteFromClipboard()
        {
            var images = GetImagesFromClipboard();

            foreach (var (surface, name) in images)
            {
                AddImageToLayers(surface, name);
                int latestLayerIndex = ViewModelMain.Current.BitmapManager.ActiveDocument.Layers.Count - 1;
                ViewModelMain.Current.BitmapManager.ActiveDocument.UndoManager.AddUndoChange(
                    new Change(RemoveLayerProcess, new object[] { latestLayerIndex }, AddLayerProcess, new object[] { images }));
            }
        }


        /// <summary>
        ///     Gets image from clipboard, supported PNG, Dib and Bitmap.
        /// </summary>
        /// <returns>WriteableBitmap.</returns>
        public static IEnumerable<(Surface, string name)> GetImagesFromClipboard()
        {
            DataObject data = (DataObject)Clipboard.GetDataObject();

            if (data.GetDataPresent("PIXI"))
            {
                SerializableDocument document = GetSerializable(data, out CropData crop);

                foreach (SerializableLayer layer in document)
                {
                    if (layer.OffsetX > crop.OffsetX + crop.Width || layer.OffsetY > crop.OffsetY + crop.Height ||
                        !layer.IsVisible || layer.Opacity == 0)
                    {
                        continue;
                    }

                    using Surface tempSurface = new Surface(layer.ToSKImage());

                    yield return (tempSurface.Crop(crop.OffsetX, crop.OffsetY, crop.Width, crop.Height), layer.Name);
                }
            }
            else if(data.GetDataPresent(DataFormats.FileDrop))
            {
                foreach (string path in data.GetFileDropList())
                {
                    yield return (Importer.ImportSurface(path), Path.GetFileName(path));
                }

                yield break;
            }
            else if (TryFromSingleImage(data, out Surface singleImage))
            {
                yield return (singleImage, "Copied");
                yield break;
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

            return dao.GetDataPresent("PNG") || dao.GetDataPresent(DataFormats.Dib) ||
                   dao.GetDataPresent(DataFormats.Bitmap) || dao.GetDataPresent(DataFormats.FileDrop) ||
                   dao.GetDataPresent("PIXI");
        }

        public static BitmapSource BitmapSelectionToBmpSource(WriteableBitmap bitmap, Coordinates[] selection, out int offsetX, out int offsetY, out int width, out int height)
        {
            offsetX = selection.Min(x => x.X);
            offsetY = selection.Min(x => x.Y);
            width = selection.Max(x => x.X) - offsetX + 1;
            height = selection.Max(x => x.Y) - offsetY + 1;
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

            if (source.Format == PixelFormats.Pbgra32)
            {
                result = new Surface(source);
            }
            else
            {
                FormatConvertedBitmap newFormat = new FormatConvertedBitmap();
                newFormat.BeginInit();
                newFormat.Source = source;
                newFormat.DestinationFormat = PixelFormats.Pbgra32;
                newFormat.EndInit();

                result = new Surface(newFormat);
            }

            return true;
        }

        private static void RemoveLayerProcess(object[] parameters)
        {
            if (parameters.Length == 0 || !(parameters[0] is int))
            {
                return;
            }

            ViewModelMain.Current.BitmapManager.ActiveDocument.RemoveLayer((int)parameters[0], true);
        }

        private static void AddLayerProcess(object[] parameters)
        {
            if (parameters.Length == 0 || !(parameters[0] is Surface))
            {
                return;
            }

            AddImageToLayers((Surface)parameters[0]);
        }

        private static void AddImageToLayers(Surface image, string name = "Image")
        {
            ViewModelMain.Current.BitmapManager.ActiveDocument.AddNewLayer(name, image);
        }
    }
}
