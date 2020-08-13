using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Avalonia;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Media.Imaging;
using AvaloniaWriteableBitmapEx;
using PixiEditor.Models.ImageManipulation;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using SharpDX.WIC;

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
            var clipboard = (IClipboard)AvaloniaLocator.Current.GetService(typeof(IClipboard));
            WriteableBitmap combinedBitmaps = BitmapUtils.CombineLayers(layers, originalImageWidth, originalImageHeight);
            using (var pngStream = new MemoryStream())
            {
                DataObject data = new ClipboardDataObject();
                var croppedBmp = BitmapSelectionToBmpSource(combinedBitmaps, selection);
                data.SetData(DataFormats.Bitmap, croppedBmp, true); //Bitmap, no transparency support

                using (var bmpData = croppedBmp.Lock())
                {
                    var format = PixelFormat.FormatDontCare;
                    PngBitmapEncoder encoder = new PngBitmapEncoder(new ImagingFactory(bmpData.Address), pngStream);
                    BitmapFrameEncode frameEncode = new BitmapFrameEncode(encoder);
                    frameEncode.Initialize();
                    frameEncode.SetSize(bmpData.Size.Width, bmpData.Size.Height);
                    frameEncode.SetPixelFormat(ref format);
                    frameEncode.WritePixels(bmpData.Size.Height, new SharpDX.DataRectangle(bmpData.Address, 4 * sizeof(ushort)));
                    frameEncode.Commit();
                    encoder.Commit();
                    data.SetData("PNG", pngStream, false); //PNG, supports transparency

                    Clipboard.SetImage(croppedBmp); //DIB format
                    Clipboard.SetDataObject(data, true);
                }
            }
        }

        /// <summary>
        ///     Pastes image from clipboard into new layer.
        /// </summary>
        public static void PasteFromClipboard()
        {
            WriteableBitmap image = GetImageFromClipboard();
            if (image != null) AddImageToLayers(image);
        }

        /// <summary>
        ///     Gets image from clipboard, supported PNG, Dib and Bitmap
        /// </summary>
        /// <returns>WriteableBitmap</returns>
        public static WriteableBitmap GetImageFromClipboard()
        {
            DataObject dao = (DataObject)Clipboard.GetDataObject();
            WriteableBitmap finalImage = null;
            if (dao.GetDataPresent("PNG"))
                using (MemoryStream pngStream = dao.GetData("PNG") as MemoryStream)
                {
                    if (pngStream != null)
                    {
                        var imagingFactory = new ImagingFactory2();
                        PngBitmapDecoder decoder = new PngBitmapDecoder(imagingFactory);
                        decoder.Initialize(new WICStream(new ImagingFactory2(), pngStream), DecodeOptions.CacheOnLoad);
                        var frame = decoder.GetFrame(0);
                        byte[] output = new byte[4 * frame.Size.Width * frame.Size.Height];
                        frame.CopyPixels(output, 4);
                        return BitmapFactory.New(frame.Size.Width, frame.Size.Height).FromByteArray(output);
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
            DataObject dao = (DataObject)Application.Current.Clipboard.GetDataObject();
            if (dao == null) return false;
            return dao.Contains("PNG") || dao.Contains("Dib") ||
                   dao.Contains("Bitmap");
        }

        private static void AddImageToLayers(WriteableBitmap image)
        {
            ViewModelMain.Current.BitmapManager.AddNewLayer("Image", image);
        }

        public static WriteableBitmap BitmapSelectionToBmpSource(WriteableBitmap bitmap, Coordinates[] selection)
        {
            int offsetX = selection.Min(x => x.X);
            int offsetY = selection.Min(x => x.Y);
            int width = selection.Max(x => x.X) - offsetX + 1;
            int height = selection.Max(x => x.Y) - offsetY + 1;
            return bitmap.Crop(offsetX, offsetY, width, height);
        }
    }
}