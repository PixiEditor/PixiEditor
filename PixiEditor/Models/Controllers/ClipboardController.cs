using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;

namespace PixiEditor.Models.Controllers
{
    public class ClipboardController
    {
        public void CopyToClipboard(WriteableBitmap bitmap, Coordinates[] selection)
        {
            Clipboard.Clear();
            using (var pngStream = new MemoryStream())
            {
                DataObject data = new DataObject();
                var croppedBmp = BitmapSelectionToBmpSource(bitmap.Clone(), selection);
                data.SetData(DataFormats.Bitmap, croppedBmp, true); //Bitmap, no transparency support

                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(croppedBmp));
                encoder.Save(pngStream);
                data.SetData("PNG", pngStream, false); //PNG, supports transparency

                Clipboard.SetDataObject(data, true);
            }
        }

        public BitmapSource BitmapSelectionToBmpSource(WriteableBitmap bitmap, Coordinates[] selection)
        {
            int offsetX = selection.Min(x => x.X);
            int offsetY = selection.Min(x => x.Y);
            int width = selection.Max(x => x.X) - offsetX + 1;
            int height = selection.Max(x => x.Y) - offsetY + 1;
            return bitmap.Crop(offsetX, offsetY, width, height);
        }

        private Bitmap BitmapFromBitmapSource(BitmapSource bitmap)
        {
            Bitmap bmp;
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmap));
                enc.Save(outStream);
                bmp = new Bitmap(outStream);
            }
            return bmp;
        }

        private Stream StreamFromBitmapSource(BitmapSource writeBmp)
        {
            Stream bmp = new MemoryStream();

            BitmapEncoder enc = new BmpBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(writeBmp));
            enc.Save(bmp);

            return bmp;
        }
    }
}
