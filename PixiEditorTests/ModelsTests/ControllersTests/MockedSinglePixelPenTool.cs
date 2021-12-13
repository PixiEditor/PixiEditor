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

        public override void Use(Layer activeLayer, Layer previewLayer, IEnumerable<Layer> allLayers, IReadOnlyList<Coordinates> recordedMouseMovement,
            SKColor color)
        {
            activeLayer.LayerBitmap.SkiaSurface.Canvas.DrawPoint(recordedMouseMovement[0].ToSKPoint(), color);

        }
    }
}
