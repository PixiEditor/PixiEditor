using System.Windows.Media;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools;

namespace PixiEditorTests.ModelsTests.ControllersTests
{
    public class MockedSinglePixelPenTool : BitmapOperationTool
    {
        public override LayerChange[] Use(Layer layer, Coordinates[] mouseMove, Color color)
        {
            return Only(BitmapPixelChanges.FromSingleColoredArray(new[] { mouseMove[0] }, color), layer.LayerGuid);
        }
    }
}