using System.Collections.Generic;
using System.Windows.Media;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools;
using SkiaSharp;

namespace PixiEditorTests.ModelsTests.ControllersTests
{
    public class MockedSinglePixelPenTool : BitmapOperationTool
    {
        public override string Tooltip => "";

        public override void Use(Layer layer, List<Coordinates> mouseMove, SKColor color)
        {
            layer.LayerBitmap.SkiaSurface.Canvas.DrawPoint(mouseMove[0].ToSKPoint(), color);
        }
    }
}
