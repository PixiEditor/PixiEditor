using PixiEditor.Helpers;
using Xunit;

namespace PixiEditorTests.HelpersTests
{
    public class SizeCalculatorTest
    {
        [Theory]
        [InlineData(50, 64, 64, 32, 32)]
        [InlineData(100, 64, 64, 64, 64)]
        [InlineData(200, 128, 128, 256, 256)]
        public void TestCalculationOfPercentsWorks(int percent, int currentWidth, int currentHeight, int expectedWidth, int expectedHeight)
        {
            var newSize = SizeCalculator.CalcAbsoluteFromPercentage(percent, new System.Drawing.Size(currentWidth, currentHeight));
            Assert.Equal(expectedWidth, newSize.Width);
            Assert.Equal(expectedHeight, newSize.Height);
        }
    }
}
