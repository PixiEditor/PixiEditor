using System.Windows.Media;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools;
using Xunit;

namespace PixiEditorTests.ModelsTests.ControllersTests
{
    public class MockedSinglePixelPen : BitmapOperationTool
    {
        public override ToolType ToolType { get; } = ToolType.Pen;

        public override LayerChange[] Use(Layer layer, Coordinates[] mouseMove, Color color)
        {
            return Only(
                BitmapPixelChanges.FromSingleColoredArray(new[] { mouseMove[0] }, color), 0);
        }
    }
}