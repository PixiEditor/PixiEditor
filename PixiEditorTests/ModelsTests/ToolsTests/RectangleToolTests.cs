using System.Linq;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools.Tools;
using Xunit;

namespace PixiEditorTests.ModelsTests.ToolsTests
{
    [Collection("Application collection")]
    public class RectangleToolTests
    {
        [StaTheory]
        [InlineData(0, 0, 2, 2)]
        [InlineData(0, 0, 9, 9)]
        [InlineData(5, 5, 6, 6)]
        [InlineData(0, 0, 15, 15)]
        public void TestThatCreateRectangleCalculatesCorrectOutlineWithOneThickness(int startX, int startY, int endX, int endY)
        {
            RectangleTool tool = new RectangleTool();

            System.Collections.Generic.IEnumerable<Coordinates> outline = tool.CreateRectangle(
                new Coordinates(startX, startY),
                new Coordinates(endX, endY),
                1);

            int expectedBorderPoints = ((endX - startX) * 2) + ((endY - startX) * 2);

            Assert.Equal(expectedBorderPoints, outline.Count());
        }
    }
}