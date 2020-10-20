using System.Linq;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools.Tools;
using Xunit;

namespace PixiEditorTests.ModelsTests.ToolsTests
{
    [Collection("Application collection")]
    public class LineToolTests
    {
        [StaTheory]
        [InlineData(2)]
        [InlineData(10)]
        [InlineData(100)]
        public void TestThatCreateLineCreatesDiagonalLine(int length)
        {
            var lineTool = new LineTool();

            var line = lineTool.CreateLine(new Coordinates(0, 0), new Coordinates(length - 1, length - 1), 1);

            Assert.Equal(length, line.Count());

            for (var i = 0; i < length; i++) Assert.Contains(new Coordinates(i, i), line);
        }
    }
}