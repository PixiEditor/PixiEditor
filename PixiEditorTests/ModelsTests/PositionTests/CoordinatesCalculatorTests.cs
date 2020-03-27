using NUnit.Framework;
using PixiEditor.Models.Position;

namespace PixiEditorTests.ModelsTests.PositionTests
{
    [TestFixture]
    public class CoordinatesCalculatorTests
    {
        [TestCase(0, 0, 3, 3, 2, 2)]
        [TestCase(0, 0, 2, 2, 1, 1)]
        [TestCase(5, 5, 7, 7, 6, 6)]
        [TestCase(5, 5, 9, 9, 7, 7)]
        public void TestGetCenter(int x1, int y1, int x2, int y2, int expectedX, int expectedY)
        {
            Coordinates center = CoordinatesCalculator.GetCenterPoint(new Coordinates(x1, y1), new Coordinates(x2, y2));
            Assert.AreEqual(center, new Coordinates(expectedX, expectedY));
        }

    }
}
