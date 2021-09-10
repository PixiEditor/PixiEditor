using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.ImageManipulation;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditorTests.ModelsTests.ColorsTests;
using Xunit;

namespace PixiEditorTests.ModelsTests.ImageManipulationTests
{
    public class BitmapUtilsTests
    {
        [Fact]
        public void TestBytesToSurface()
        {
            int width = 10;
            int height = 10;
            Coordinates[] coloredPoints = { new Coordinates(0, 0), new Coordinates(3, 6), new Coordinates(9, 9) };
            Surface bmp = BitmapFactory.New(width, height);
            for (int i = 0; i < coloredPoints.Length; i++)
            {
                bmp.SetPixel(coloredPoints[i].X, coloredPoints[i].Y, ExtendedColorTests.green);
            }

            byte[] byteArray = bmp.ToByteArray();

            Surface convertedBitmap = BitmapUtils.BytesToSurface(width, height, byteArray);

            for (int i = 0; i < coloredPoints.Length; i++)
            {
                Assert.Equal(ExtendedColorTests.green, convertedBitmap.GetPixel(coloredPoints[i].X, coloredPoints[i].Y));
            }
        }

        [Fact]
        public void TestThatCombineLayersReturnsCorrectBitmap()
        {
            Coordinates[] cords = { new Coordinates(0, 0), new Coordinates(1, 1) };
            Layer[] layers = { new Layer("test", 2, 2), new Layer("test2", 2, 2) };

            layers[0].SetPixels(BitmapPixelChanges.FromSingleColoredArray(new[] { cords[0] }, ExtendedColorTests.green));

            layers[1].SetPixels(BitmapPixelChanges.FromSingleColoredArray(new[] { cords[1] }, ExtendedColorTests.red));

            Surface outputBitmap = BitmapUtils.CombineLayers(2, 2, layers);

            Assert.Equal(ExtendedColorTests.green, outputBitmap.GetPixel(0, 0));
            Assert.Equal(ExtendedColorTests.red, outputBitmap.GetPixel(1, 1));
        }

        [Fact]
        public void TestThatCombineLayersReturnsCorrectBitmapWithSamePixels()
        {
            Coordinates[] cords = { new Coordinates(0, 0) };
            Layer[] layers = { new Layer("test", 2, 2), new Layer("test2", 2, 2) };

            layers[0].SetPixels(BitmapPixelChanges.FromSingleColoredArray(cords, ExtendedColorTests.green));

            layers[1].SetPixels(BitmapPixelChanges.FromSingleColoredArray(cords, ExtendedColorTests.red));

            Surface outputBitmap = BitmapUtils.CombineLayers(2, 2, layers);

            Assert.Equal(ExtendedColorTests.red, outputBitmap.GetSRGBPixel(0, 0));
        }

        [Fact]
        public void TestThatGetPixelsForSelectionReturnsCorrectPixels()
        {
            Coordinates[] cords =
            {
                new Coordinates(0, 0),
                new Coordinates(1, 1), new Coordinates(0, 1), new Coordinates(1, 0)
            };
            Layer[] layers = { new Layer("test", 2, 2), new Layer("test2", 2, 2) };

            layers[0].SetPixels(BitmapPixelChanges.FromSingleColoredArray(new[] { cords[0] }, ExtendedColorTests.green));
            layers[1].SetPixels(BitmapPixelChanges.FromSingleColoredArray(new[] { cords[1] }, ExtendedColorTests.red));

            Dictionary<Guid, Color[]> output = BitmapUtils.GetPixelsForSelection(layers, cords);

            List<Color> colors = new List<Color>();

            foreach (KeyValuePair<Guid, Color[]> layerColor in output.ToArray())
            {
                foreach (Color color in layerColor.Value)
                {
                    colors.Add(color);
                }
            }

            Assert.Single(colors.Where(x => x == ExtendedColorTests.green));
            Assert.Single(colors.Where(x => x == ExtendedColorTests.red));
            Assert.Equal(6, colors.Count(x => x.A == 0)); // 6 because layer is 4 pixels,

            // 2 * 4 = 8, 2 other color pixels, so 8 - 2 = 6
        }
    }
}