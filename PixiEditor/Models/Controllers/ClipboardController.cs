using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Images;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.ViewModels;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

namespace PixiEditor.Models.Controllers
{
    public class ClipboardController
    {

        /// <summary>
        /// Copies selection to clipboard in PNG, Bitmap and DIB formats.
        /// </summary>
        /// <param name="bitmap">Bitmap where selection is</param>
        /// <param name="selection"></param>
        public void CopyToClipboard(Layer[] layers, Coordinates[] selection)
        {
            Clipboard.Clear();
            WriteableBitmap combinedBitmaps = BitmapUtils.CombineBitmaps(layers);
            using (var pngStream = new MemoryStream())
            {
                DataObject data = new DataObject();
                var croppedBmp = BitmapSelectionToBmpSource(combinedBitmaps, selection);
                data.SetData(DataFormats.Bitmap, croppedBmp, true); //Bitmap, no transparency support

                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(croppedBmp));
                encoder.Save(pngStream);
                data.SetData("PNG", pngStream, false); //PNG, supports transparency

                Clipboard.SetImage(croppedBmp); //DIB format
                Clipboard.SetDataObject(data, true);
            }
        }

        public void PasteFromClipboard()
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
                finalImage = new WriteableBitmap(Clipboard.GetImage());
            }
            else if (dao.GetDataPresent(DataFormats.Bitmap))
            {
                finalImage = new WriteableBitmap(dao.GetData(DataFormats.Bitmap) as BitmapSource);
            }
            if (finalImage != null)
            {
                AddImageToLayers(finalImage);
            }
        }

        private void AddImageToLayers(WriteableBitmap image)
        {
            Document doc = ViewModelMain.Current.BitmapManager.ActiveDocument;
            Rect imgRect = new Rect(0, 0, image.PixelWidth, image.PixelHeight);
            ViewModelMain.Current.BitmapManager.AddNewLayer("Image", doc.Width, doc.Height, true);
            ViewModelMain.Current.BitmapManager.ActiveLayer.
                LayerBitmap.Blit(imgRect, image, imgRect);
        }

        public BitmapSource BitmapSelectionToBmpSource(WriteableBitmap bitmap, Coordinates[] selection)
        {
            int offsetX = selection.Min(x => x.X);
            int offsetY = selection.Min(x => x.Y);
            int width = selection.Max(x => x.X) - offsetX + 1;
            int height = selection.Max(x => x.Y) - offsetY + 1;
            return bitmap.Crop(offsetX, offsetY, width, height);
        }
    }
}
