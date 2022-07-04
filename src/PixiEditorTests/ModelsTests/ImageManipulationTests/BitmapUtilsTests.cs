using PixiEditor.Models.DataHolders;
using PixiEditor.Models.ImageManipulation;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace PixiEditorTests.ModelsTests.ImageManipulationTests
{
    public class BitmapUtilsTests
    {

        //[Fact]
        //public void TestThatCombineLayersReturnsCorrectBitmap()
        //{
        //    Coordinates[] cords = { new Coordinates(0, 0), new Coordinates(1, 1) };
        //    Layer[] layers = { new Layer("test", 2, 2), new Layer("test2", 2, 2) };

        //    layers[0].SetPixels(BitmapPixelChanges.FromSingleColoredArray(new[] { cords[0] }, SKColors.Lime));

        //    layers[1].SetPixels(BitmapPixelChanges.FromSingleColoredArray(new[] { cords[1] }, SKColors.Red));

        //    Surface outputBitmap = BitmapUtils.CombineLayers(2, 2, layers);

        //    Assert.Equal(SKColors.Lime, outputBitmap.GetSRGBPixel(0, 0));
        //    Assert.Equal(SKColors.Red, outputBitmap.GetSRGBPixel(1, 1));
        //}

        //[Fact]
        //public void TestThatCombineLayersReturnsCorrectBitmapWithSamePixels()
        //{
        //    Coordinates[] cords = { new Coordinates(0, 0) };
        //    Layer[] layers = { new Layer("test", 2, 2), new Layer("test2", 2, 2) };

        //    layers[0].SetPixels(BitmapPixelChanges.FromSingleColoredArray(cords, SKColors.Lime));

        //    layers[1].SetPixels(BitmapPixelChanges.FromSingleColoredArray(cords, SKColors.Red));

        //    Surface outputBitmap = BitmapUtils.CombineLayers(2, 2, layers);

        //    Assert.Equal(SKColors.Red, outputBitmap.GetSRGBPixel(0, 0));
        //}

        //[Fact]
        //public void TestThatGetPixelsForSelectionReturnsCorrectPixels()
        //{
        //    Coordinates[] cords =
        //    {
        //        new Coordinates(0, 0),
        //        new Coordinates(1, 1), new Coordinates(0, 1), new Coordinates(1, 0)
        //    };
        //    Layer[] layers = { new Layer("test", 2, 2), new Layer("test2", 2, 2) };

        //    layers[0].SetPixels(BitmapPixelChanges.FromSingleColoredArray(new[] { cords[0] }, SKColors.Lime));
        //    layers[1].SetPixels(BitmapPixelChanges.FromSingleColoredArray(new[] { cords[1] }, SKColors.Red));

        //    Dictionary<Guid, SKColor[]> output = BitmapUtils.GetPixelsForSelection(layers, cords);

        //    List<SKColor> colors = new List<SKColor>();

        //    foreach (KeyValuePair<Guid, SKColor[]> layerColor in output.ToArray())
        //    {
        //        foreach (SKColor color in layerColor.Value)
        //        {
        //            colors.Add(color);
        //        }
        //    }

        //    Assert.Single(colors.Where(x => x == SKColors.Lime));
        //    Assert.Single(colors.Where(x => x == SKColors.Red));
        //    Assert.Equal(6, colors.Count(x => x.Alpha == 0)); // 6 because layer is 4 pixels,

        //    // 2 * 4 = 8, 2 other color pixels, so 8 - 2 = 6
        //}
    }
}
