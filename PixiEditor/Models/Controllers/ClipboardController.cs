using PixiEditor.Models.ImageManipulation;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Undo;
using PixiEditor.ViewModels;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

namespace PixiEditor.Models.Controllers
{
    public static class ClipboardController
    {
        /// <summary>
        ///     Copies selection to clipboard in PNG, Bitmap and DIB formats.
        /// </summary>
        /// <param name="layers">Layers where selection is.</param>
        public static void CopyToClipboard(Layer[] layers, Coordinates[] selection, int originalImageWidth, int originalImageHeight)
        {
            Clipboard.Clear();
            WriteableBitmap combinedBitmaps = BitmapUtils.CombineLayers(originalImageWidth, originalImageHeight, layers);
            using (MemoryStream pngStream = new MemoryStream())
            {
                DataObject data = new DataObject();
                BitmapSource croppedBmp = BitmapSelectionToBmpSource(combinedBitmaps, selection);
                data.SetData(DataFormats.Bitmap, croppedBmp, true); // Bitmap, no transparency support

                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(croppedBmp));
                encoder.Save(pngStream);
                data.SetData("PNG", pngStream, false); // PNG, supports transparency

                Clipboard.SetImage(croppedBmp); // DIB format
                Clipboard.SetDataObject(data, true);
            }
        }

        /// <summary>
        ///     Pastes image from clipboard into new layer.
        /// </summary>
        public static void PasteFromClipboard()
        {
            WriteableBitmap image = GetImageFromClipboard();
            if (image != null)
            {
                AddImageToLayers(image);
                int latestLayerIndex = ViewModelMain.Current.BitmapManager.ActiveDocument.Layers.Count - 1;
                ViewModelMain.Current.BitmapManager.ActiveDocument.UndoManager.AddUndoChange(
                    new Change(RemoveLayerProcess, new object[] { latestLayerIndex }, AddLayerProcess, new object[] { image }));
            }
        }

        /// <summary>
        ///     Gets image from clipboard, supported PNG, Dib and Bitmap.
        /// </summary>
        /// <returns>WriteableBitmap.</returns>
        public static WriteableBitmap GetImageFromClipboard()
        {
            DataObject dao = (DataObject)Clipboard.GetDataObject();
            WriteableBitmap finalImage = null;
            if (dao.GetDataPresent("PNG"))
            {
                using (MemoryStream pngStream = dao.GetData("PNG") as MemoryStream)
                {
                    if (pngStream != null)
                    {
                        PngBitmapDecoder decoder = new PngBitmapDecoder(pngStream, BitmapCreateOptions.IgnoreImageCache, BitmapCacheOption.OnLoad);
                        finalImage = new WriteableBitmap(decoder.Frames[0].Clone());
                    }
                }
            }
            else if (dao.GetDataPresent(DataFormats.Dib))
            {
                finalImage = new WriteableBitmap(Clipboard.GetImage()!);
            }
            else if (dao.GetDataPresent(DataFormats.Bitmap))
            {
                finalImage = new WriteableBitmap((dao.GetData(DataFormats.Bitmap) as BitmapSource)!);
            }

            return finalImage;
        }

        public static bool IsImageInClipboard()
        {
            DataObject dao = (DataObject)Clipboard.GetDataObject();
            if (dao == null)
            {
                return false;
            }

            return dao.GetDataPresent("PNG") || dao.GetDataPresent(DataFormats.Dib) ||
                   dao.GetDataPresent(DataFormats.Bitmap);
        }

        public static BitmapSource BitmapSelectionToBmpSource(WriteableBitmap bitmap, Coordinates[] selection)
        {
            int offsetX = selection.Min(x => x.X);
            int offsetY = selection.Min(x => x.Y);
            int width = selection.Max(x => x.X) - offsetX + 1;
            int height = selection.Max(x => x.Y) - offsetY + 1;
            return bitmap.Crop(offsetX, offsetY, width, height);
        }

        private static void AddImageToLayers(WriteableBitmap image)
        {
            ViewModelMain.Current.BitmapManager.ActiveDocument.AddNewLayer("Image", image);
        }

        private static void RemoveLayerProcess(object[] parameters)
        {
            if (parameters.Length == 0 || !(parameters[0] is int))
            {
                return;
            }

            ViewModelMain.Current.BitmapManager.ActiveDocument.RemoveLayer((int)parameters[0]);
        }

        private static void AddLayerProcess(object[] parameters)
        {
            if (parameters.Length == 0 || !(parameters[0] is WriteableBitmap))
            {
                return;
            }

            AddImageToLayers((WriteableBitmap)parameters[0]);
        }
    }
}