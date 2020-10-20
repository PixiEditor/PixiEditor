using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.ImageManipulation;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using Xunit;

namespace PixiEditorTests.ModelsTests.ImageManipulationTests
{
    public class BitmapUtilsTests
    {
        [Fact]
        public void TestBytesToWriteableBitmap()
        {
            int width = 10;
            int height = 10;
            Coordinates[] coloredPoints = { new Coordinates(0, 0), new Coordinates(3, 6), new Coordinates(9, 9) };
            WriteableBitmap bmp = BitmapFactory.New(width, height);
            for (int i = 0; i < coloredPoints.Length; i++)
            {
                bmp.SetPixel(coloredPoints[i].X, coloredPoints[i].Y, Colors.Green);
            }

            byte[] byteArray = bmp.ToByteArray();

            WriteableBitmap convertedBitmap = BitmapUtils.BytesToWriteableBitmap(width, height, byteArray);

            for (int i = 0; i < coloredPoints.Length; i++)
            {
                Assert.Equal(Colors.Green, convertedBitmap.GetPixel(coloredPoints[i].X, coloredPoints[i].Y));
            }
        }

        [Fact]
        public void TestThatCombineLayersReturnsCorrectBitmap()
        {
            Coordinates[] cords = { new Coordinates(0, 0), new Coordinates(1, 1) };
            Layer[] layers = { new Layer("test", 2, 2), new Layer("test2", 2, 2) };

            layers[0].SetPixels(BitmapPixelChanges.FromSingleColoredArray(new[] { cords[0] }, Colors.Green));

            layers[1].SetPixels(BitmapPixelChanges.FromSingleColoredArray(new[] { cords[1] }, Colors.Red));

            WriteableBitmap outputBitmap = BitmapUtils.CombineLayers(layers, 2, 2);

            Assert.Equal(Colors.Green, outputBitmap.GetPixel(0, 0));
            Assert.Equal(Colors.Red, outputBitmap.GetPixel(1, 1));
        }

        [Fact]
        public void TestThatCombineLayersReturnsCorrectBitmapWithSamePixels()
        {
            Coordinates[] cords = { new Coordinates(0, 0) };
            Layer[] layers = { new Layer("test", 2, 2), new Layer("test2", 2, 2) };

            layers[0].SetPixels(BitmapPixelChanges.FromSingleColoredArray(cords, Colors.Green));

            layers[1].SetPixels(BitmapPixelChanges.FromSingleColoredArray(cords, Colors.Red));

            WriteableBitmap outputBitmap = BitmapUtils.CombineLayers(layers, 2, 2);

            Assert.Equal(Colors.Red, outputBitmap.GetPixel(0, 0));
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

            layers[0].SetPixels(BitmapPixelChanges.FromSingleColoredArray(new[] { cords[0] }, Colors.Green));
            layers[1].SetPixels(BitmapPixelChanges.FromSingleColoredArray(new[] { cords[1] }, Colors.Red));

            Dictionary<Layer, Color[]> output = BitmapUtils.GetPixelsForSelection(layers, cords);

            List<Color> colors = new List<Color>();

            foreach (KeyValuePair<Layer, Color[]> layerColor in output.ToArray())
            {
                foreach (Color color in layerColor.Value)
                {
                    colors.Add(color);
                }
            }

            Assert.Single(colors.Where(x => x == Colors.Green));
            Assert.Single(colors.Where(x => x == Colors.Red));
            Assert.Equal(6, colors.Count(x => x.A == 0)); // 6 because layer is 4 pixels,
            // 2 * 4 = 8, 2 other color pixels, so 8 - 2 = 6
        }
    }
}