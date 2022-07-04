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
        public void TestCalculationOfAbsoluteFromPercentageWorks(int percent, int currentWidth, int currentHeight, int expectedWidth, int expectedHeight)
        {
            var newSize = SizeCalculator.CalcAbsoluteFromPercentage(percent, new System.Drawing.Size(currentWidth, currentHeight));
            Assert.Equal(expectedWidth, newSize.Width);
            Assert.Equal(expectedHeight, newSize.Height);
        }

        [Theory]
        [InlineData(32, 64, 50)]
        [InlineData(32, 32, 100)]
        [InlineData(64, 32, 200)]
        public void TestCalculationOfPercentageFromAbsoluteWorks(int currentSize, int initSize, int expectedPerc)
        {
            var perc = SizeCalculator.CalcPercentageFromAbsolute(initSize, currentSize);
            Assert.Equal(perc, expectedPerc);
        }
    }
}
