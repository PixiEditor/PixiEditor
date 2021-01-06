using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools;
using System.Windows.Media;

namespace PixiEditorTests.ModelsTests.ControllersTests
{
    public class MockedSinglePixelPenTool : BitmapOperationTool
    {
        public override LayerChange[] Use(Layer layer, Coordinates[] mouseMove, Color color)
        {
            return Only(BitmapPixelChanges.FromSingleColoredArray(new[] { mouseMove[0] }, color), 0);
        }
    }
}