using PixiEditor.Models.ImageManipulation;
using PixiEditor.Models.Position;
using Xunit;

namespace PixiEditorTests.ModelsTests.ImageManipulationTests
{
    public class TransformTests
    {
        [Theory]
        [InlineData(0, 0, 1, 1, 1, 1)]
        [InlineData(1, 1, 0, 0, -1, -1)]
        [InlineData(5, 5, 4, 6, -1, 1)]
        [InlineData(-15, -15, -16, -16, -1, -1)]
        [InlineData(150, 150, 1150, 1150, 1000, 1000)]
        public void TestGetTranslation(int x1, int y1, int x2, int y2, int expectedX, int expectedY)
        {
            Coordinates translation = Transform.GetTranslation(new Coordinates(x1, y1), new Coordinates(x2, y2));
            Assert.Equal(new Coordinates(expectedX, expectedY), translation);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 1)]
        [InlineData(5, 2)]
        [InlineData(50, 150)]
        [InlineData(-5, -52)]
        public void TestTranslate(int vectorX, int vectorY)
        {
            Coordinates[] points = { new Coordinates(0, 0), new Coordinates(5, 5), new Coordinates(15, 2) };
            Coordinates[] translatedCords = Transform.Translate(points, new Coordinates(vectorX, vectorY));

            for (int i = 0; i < points.Length; i++)
            {
                Assert.Equal(points[i].X + vectorX, translatedCords[i].X);
                Assert.Equal(points[i].Y + vectorY, translatedCords[i].Y);
            }
        }
    }
}