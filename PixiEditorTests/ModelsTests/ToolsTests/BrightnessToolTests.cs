using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools.Tools;
using SkiaSharp;
using Xunit;

namespace PixiEditorTests.ModelsTests.ToolsTests
{
    [Collection("Application collection")]
    public class BrightnessToolTests
    {
        //[StaTheory]
        //[InlineData(5, 12, 12, 12)]
        //[InlineData(-5, 242, 242, 242)]

        //// If correction factor is negative, testing color will be white, otherwise black
        //public void TestThatBrightnessToolChangesPixelBrightness(float correctionFactor, byte expectedR, byte expectedG, byte expectedB)
        //{
        //    SKColor expectedColor = new SKColor(expectedR, expectedG, expectedB);

        //    BrightnessTool tool = new BrightnessTool();

        //    Layer layer = new Layer("test", 1, 1);
        //    layer.SetPixel(new Coordinates(0, 0), correctionFactor < 0 ? SKColors.White : SKColors.Black);

        //    PixiEditor.Models.DataHolders.BitmapPixelChanges changes = tool.ChangeBrightness(layer, new Coordinates(0, 0), 1, correctionFactor);
        //    layer.SetPixels(changes);

        //    Assert.Equal(expectedColor, layer.GetPixel(0, 0));
        //}
    }
}
