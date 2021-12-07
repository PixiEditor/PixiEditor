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

        public override LayerChange[] Use(Layer layer, List<Coordinates> mouseMove, SKColor color)
        {
            return Only(BitmapPixelChanges.FromSingleColoredArray(new[] { mouseMove[0] }, color), layer.LayerGuid);
        }
    }
}
