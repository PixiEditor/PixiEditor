using NUnit.Framework;
using PixiEditor.Models.Position;

namespace PixiEditorTests.WorkspaceTests.ToolsTests
{
    [TestFixture]
    public class CoordinatesCalculatorTests
    {
        [TestCase(0, 0, 2, 2, ExpectedResult = 9)]
        [TestCase(0, 0, 10, 10, ExpectedResult = 121)]
        public int RectangleToCoordinatesAmountTest(int x1, int y1, int x2, int y2)
        {
            return CoordinatesCalculator.RectangleToCoordinates(x1, y1, x2, y2).Length;
        }

        [TestCase()]
        public void CalculateSquareEvenThicknessCenterTest()
        {
            DoubleCords cords = CoordinatesCalculator.CalculateThicknessCenter(new Coordinates(3, 3), 4);

            Assert.AreEqual(1, cords.Coords1.X);
            Assert.AreEqual(1, cords.Coords1.Y);
            Assert.AreEqual(4, cords.Coords2.X);
            Assert.AreEqual(4, cords.Coords2.Y);
        }

        [TestCase()]
        public void CalculateSquareOddThicknessCenterTest()
        {
            DoubleCords cords = CoordinatesCalculator.CalculateThicknessCenter(new Coordinates(3, 3), 3);

            Assert.AreEqual(2, cords.Coords1.X);
            Assert.AreEqual(2, cords.Coords1.Y);
            Assert.AreEqual(4, cords.Coords2.X);
            Assert.AreEqual(4, cords.Coords2.Y);
        }
    }
}
