using PixiEditor.Models.Position;
using System.Linq;
using Xunit;

namespace PixiEditorTests.ModelsTests.PositionTests
{
    public class CoordinatesCalculatorTests
    {
        [Theory]
        [InlineData(0, 0, 2, 2, 9)]
        [InlineData(0, 0, 10, 10, 121)]
        public void TestThatRectangleToCoordinatesReturnsSameAmount(int x1, int y1, int x2, int y2, int expectedResult)
        {
            Assert.Equal(CoordinatesCalculator.RectangleToCoordinates(x1, y1, x2, y2).Count(), expectedResult);
        }

        [Fact]
        public void CalculateSquareEvenThicknessCenterTest()
        {
            DoubleCords cords = CoordinatesCalculator.CalculateThicknessCenter(new Coordinates(3, 3), 4);

            Assert.Equal(1, cords.Coords1.X);
            Assert.Equal(1, cords.Coords1.Y);
            Assert.Equal(4, cords.Coords2.X);
            Assert.Equal(4, cords.Coords2.Y);
        }

        [Fact]
        public void CalculateSquareOddThicknessCenterTest()
        {
            DoubleCords cords = CoordinatesCalculator.CalculateThicknessCenter(new Coordinates(3, 3), 3);

            Assert.Equal(2, cords.Coords1.X);
            Assert.Equal(2, cords.Coords1.Y);
            Assert.Equal(4, cords.Coords2.X);
            Assert.Equal(4, cords.Coords2.Y);
        }

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