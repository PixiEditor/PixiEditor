using PixiEditor.Models.Position;
using Xunit;

namespace PixiEditorTests.ModelsTests.PositionTests
{
    public class CoordinatesTests
    {
        [Fact]
        public void TestThatToStringReturnsCorrectFormat()
        {
            var cords = new Coordinates(5, 5);

            Assert.Equal("5, 5", cords.ToString());
        }

        [Fact]
        public void TestThatNotEqualOperatorWorks()
        {
            var cords = new Coordinates(5, 5);
            var cords2 = new Coordinates(6, 4);

            Assert.True(cords != cords2);
        }
    }
}