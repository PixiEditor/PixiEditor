using PixiEditor.Models.Position;
using Xunit;

namespace PixiEditorTests.ModelsTests.PositionTests
{
    public class CoordinatesCalculatorTests
    {
        [Theory]
        [InlineData(0, 0, 3, 3, 1, 1)]
        [InlineData(0, 0, 2, 2, 1, 1)]
        [InlineData(5, 5, 7, 7, 6, 6)]
        [InlineData(5, 5, 9, 9, 7, 7)]
        public void TestGetCenter(int x1, int y1, int x2, int y2, int expectedX, int expectedY)
        {
            Coordinates center = CoordinatesCalculator.GetCenterPoint(new Coordinates(x1, y1), new Coordinates(x2, y2));
            Assert.Equal(new Coordinates(expectedX, expectedY), center);
        }

    }
}
