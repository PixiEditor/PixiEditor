using PixiEditor.Models.Position;
using PixiEditor.Models.Tools.Tools;
using Xunit;

namespace PixiEditorTests.ModelsTests.ToolsTests
{
    [Collection("Application collection")]
    public class PenToolTests
    {
        [StaFact]
        public void TestThatPixelPerfectPenReturnsShapeWithoutLShapePixels()
        {
            PenTool pen = new PenTool();

            Coordinates start = new Coordinates(0, 0);
            Coordinates end = new Coordinates(0, 0);
            Coordinates end2 = new Coordinates(1, 0);
            Coordinates start2 = new Coordinates(1, 1);

            pen.Draw(start, end, System.Windows.Media.Colors.Black, 1, true);
            pen.Draw(end, end2, System.Windows.Media.Colors.Black, 1, true);
            var points = pen.Draw(end2, start2, System.Windows.Media.Colors.Black, 1, true);

            Assert.Contains(points.ChangedPixels, x => x.Value.A == 0);
        }
    }
}