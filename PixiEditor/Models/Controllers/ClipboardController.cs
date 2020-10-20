using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using PixiEditor.Models.ImageManipulation;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.ViewModels;

namespace PixiEditor.Models.Controllers
{
    public static class ClipboardController
    {
        /// <summary>
        ///     Copies selection to clipboard in PNG, Bitmap and DIB formats.
        /// </summary>
        /// <param name="layers">Layers where selection is</param>
        /// <param name="selection"></param>
        /// <param name="originalImageWidth">Output </param>
        /// <param name="originalImageHeight"></param>
        public static void CopyToClipboard(Layer[] layers, Coordinates[] selection, int originalImageWidth, int originalImageHeight)
        {
            Clipboard.Clear();
            var combinedBitmaps = BitmapUtils.CombineLayers(layers, originalImageWidth, originalImageHeight);
            using (var pngStream = new MemoryStream())
            {
                var data = new DataObject();
                var croppedBmp = BitmapSelectionToBmpSource(combinedBitmaps, selection);
                data.SetData(DataFormats.Bitmap, croppedBmp, true); //Bitmap, no transparency support

                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(croppedBmp));
                encoder.Save(pngStream);
                data.SetData("PNG", pngStream, false); //PNG, supports transparency

                Clipboard.SetImage(croppedBmp); //DIB format
                Clipboard.SetDataObject(data, true);
            }
        }

        /// <summary>
        ///     Pastes image from clipboard into new layer.
        /// </summary>
        public static void PasteFromClipboard()
        {
            var image = GetImageFromClipboard();
            if (image != null) AddImageToLayers(image);
        }

        /// <summary>
        ///     Gets image from clipboard, supported PNG, Dib and Bitmap
        /// </summary>
        /// <returns>WriteableBitmap</returns>
        public static WriteableBitmap GetImageFromClipboard()
        {
            var dao = (DataObject) Clipboard.GetDataObject();
            WriteableBitmap finalImage = null;
            if (dao.GetDataPresent("PNG"))
                using (var pngStream = dao.GetData("PNG") as MemoryStream)
                {
                    if (pngStream != null)
                    {
                        var decoder = new PngBitmapDecoder(pngStream, BitmapCreateOptions.IgnoreImageCache,
                            BitmapCacheOption.OnLoad);
                        finalImage = new WriteableBitmap(decoder.Frames[0].Clone());
                    }
                }
            else if (dao.GetDataPresent(DataFormats.Dib))
                finalImage = new WriteableBitmap(Clipboard.GetImage()!);
            else if (dao.GetDataPresent(DataFormats.Bitmap))
                finalImage = new WriteableBitmap((dao.GetData(DataFormats.Bitmap) as BitmapSource)!);

            return finalImage;
        }

        public static bool IsImageInClipboard()
        {
            var dao = (DataObject) Clipboard.GetDataObject();
            if (dao == null) return false;
            return dao.GetDataPresent("PNG") || dao.GetDataPresent(DataFormats.Dib) ||
                   dao.GetDataPresent(DataFormats.Bitmap);
        }

        private static void AddImageToLayers(WriteableBitmap image)
        {
            ViewModelMain.Current.BitmapManager.AddNewLayer("Image", image);
        }

        public static BitmapSource BitmapSelectionToBmpSource(WriteableBitmap bitmap, Coordinates[] selection)
        {
            var offsetX = selection.Min(x => x.X);
            var offsetY = selection.Min(x => x.Y);
            var width = selection.Max(x => x.X) - offsetX + 1;
            var height = selection.Max(x => x.Y) - offsetY + 1;
            return bitmap.Crop(offsetX, offsetY, width, height);
        }
    }
}