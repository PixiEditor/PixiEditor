using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using Xunit;

namespace PixiEditorTests.ModelsTests.ControllersTests
{
    public class ClipboardControllerTests
    {
        private Color testColor = Colors.Coral;

        [StaFact]
        public void TestThatClipboardControllerIgnoresNonImageDataInClipboard()
        {
            Clipboard.Clear();
            Clipboard.SetText("Text data");
            var img = ClipboardController.GetImageFromClipboard();
            Assert.Null(img);
        }

        [StaFact]
        public void TestThatIsImageInClipboardWorksForDib()
        {
            Clipboard.Clear();
            Clipboard.SetImage(BitmapFactory.New(10, 10));
            Assert.True(ClipboardController.IsImageInClipboard());
        }

        [StaFact]
        public void TestThatClipboardControllerSavesImageToClipboard()
        {
            Layer testLayer = new Layer("test layer", 10, 10);
            ClipboardController.CopyToClipboard(new []{testLayer}, CoordinatesCalculator.RectangleToCoordinates(0,0, 9,9), 10, 10);
            Assert.True(ClipboardController.IsImageInClipboard());
        }

        [StaFact]
        public void TestThatCopyToClipboardWithSelectionSavesCorrectBitmap()
        {
            Clipboard.Clear();

            Layer testLayer = new Layer("test layer", 10, 10);
            Layer testLayer2 = new Layer("test layer", 10, 10);
            testLayer.ApplyPixel(new Coordinates(4,4), testColor);
            testLayer2.ApplyPixel(new Coordinates(5,5), testColor);

            ClipboardController.CopyToClipboard(new []{testLayer, testLayer2}, 
                new []{new Coordinates(4,4), new Coordinates(5,5)}, 10, 10);

            var img = Clipboard.GetImage(); // Using default Clipboard get image to avoid false positives from faulty ClipboardController GetImage

            Assert.True(ClipboardController.IsImageInClipboard());
            Assert.NotNull(img);
            Assert.Equal(2, img.PixelWidth);
            Assert.Equal(2, img.PixelHeight);

            var bmp = new WriteableBitmap(img);
            Assert.Equal(testColor, bmp.GetPixel(0, 0));
            Assert.Equal(testColor, bmp.GetPixel(1, 1));
        }

        [StaFact]
        public void TestThatClipboardControllerGetsCorrectImageInDibFormatFromClipboard()
        {
            Clipboard.Clear();
            var bmp = BitmapFactory.New(10, 10);
            bmp.SetPixel(4, 4, testColor);
            Clipboard.SetImage(bmp);

            var img = ClipboardController.GetImageFromClipboard();
            Assert.NotNull(img);
            Assert.Equal(10, img.PixelWidth);
            Assert.Equal(10, img.PixelHeight);
            Assert.Equal(testColor, bmp.GetPixel(4, 4));
        }

        [StaFact]
        public void TestThatClipboardControllerGetsCorrectImageInPngFormatFromClipboard()
        {
            Clipboard.Clear();
            var bmp = BitmapFactory.New(10, 10);
            bmp.SetPixel(4,4, testColor);
            using (var pngStream = new MemoryStream())
            {
                DataObject data = new DataObject();

                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bmp));
                encoder.Save(pngStream);
                data.SetData("PNG", pngStream, false); //PNG, supports transparency
                Clipboard.SetDataObject(data, true);
            }

            var img = ClipboardController.GetImageFromClipboard();
            Assert.NotNull(img);
            Assert.Equal(10, img.PixelWidth);
            Assert.Equal(10, img.PixelHeight);
            Assert.Equal(testColor, bmp.GetPixel(4,4));
        }

        [StaFact]
        public void TestThatClipboardControllerGetsCorrectImageInBitmapFormatFromClipboard()
        {
            Clipboard.Clear();
            var bmp = BitmapFactory.New(10, 10);
            bmp.SetPixel(4, 4, testColor);

            DataObject data = new DataObject();
            data.SetData(DataFormats.Bitmap, bmp, false); //PNG, supports transparency
            Clipboard.SetDataObject(data, true);

            var img = ClipboardController.GetImageFromClipboard();
            Assert.NotNull(img);
            Assert.Equal(10, img.PixelWidth);
            Assert.Equal(10, img.PixelHeight);
            Assert.Equal(testColor, bmp.GetPixel(4, 4));
        }

        
    }
}
