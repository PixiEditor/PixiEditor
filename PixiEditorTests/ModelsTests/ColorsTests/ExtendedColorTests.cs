using System;
using System.Windows.Media;
using PixiEditor.Models.Colors;
using Xunit;

namespace PixiEditorTests.ModelsTests.ColorsTests
{
    public class ExtendedColorTests
    {
        private const int AcceptableMaringOfError = 1;

        [Fact]
        public void ChangeColorBrightnessIsNotTheSameTest()
        {
            Color newColor = ExColor.ChangeColorBrightness(Colors.White, -1);
            Assert.NotEqual(Colors.White, newColor);
        }

        [Fact]
        public void ChangeColorBrightnessNewValueTest()
        {
            Color newColor = ExColor.ChangeColorBrightness(Colors.White, -100);
            Assert.Equal(Colors.Black, newColor);
        }


        //Acceptable margin of error is 1
        [Theory]
        [InlineData(0, 0, 0, 0, 0, 0)]
        [InlineData(255, 255, 255, 0, 0, 100)]
        [InlineData(182, 55, 55, 0, 53.6f, 46.5f)]
        [InlineData(20, 47, 255, 233, 100, 53.9f)]
        [InlineData(137, 43, 226, 271, 75.9f, 52.7f)]
        public void RgbToHslTest(int r, int g, int b, int h, float s, float l)
        {
            Tuple<int, float, float> hsl = ExColor.RgbToHsl(r, g, b);
            float marginOfErrorH = Math.Abs(hsl.Item1 - h);
            float marginOfErrorS = Math.Abs(hsl.Item2 - s);
            float marginOfErrorL = Math.Abs(hsl.Item3 - l);
            Assert.True(marginOfErrorH <= AcceptableMaringOfError);
            Assert.True(marginOfErrorS <= AcceptableMaringOfError);
            Assert.True(marginOfErrorL <= AcceptableMaringOfError);

        }

        [Theory]
        [InlineData(0, 0, 0, 0, 0, 0)]
        [InlineData(0, 0, 100, 255, 255, 255)]
        [InlineData(0, 53.6f, 46.5f, 182, 55, 55)]
        [InlineData(297, 100, 17.1f, 82, 0, 87)]
        [InlineData(271, 75.9f, 52.7f, 137, 43, 226)]
        public void HslToRgbTest(int h, float s, float l, int r, int g, int b)
        {
            Color rgb = ExColor.HslToRGB(h, s, l);
            int marginOfErrorR = Math.Abs(rgb.R - r);
            int marginOfErrorG = Math.Abs(rgb.G - g);
            int marginOfErrorB = Math.Abs(rgb.B - b);
            Assert.True(marginOfErrorR <= AcceptableMaringOfError);
            Assert.True(marginOfErrorG <= AcceptableMaringOfError);
            Assert.True(marginOfErrorB <= AcceptableMaringOfError);

        }
    }
}
