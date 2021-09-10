using System;
using System.Windows.Media;
using PixiEditor.Models.Colors;
using SkiaSharp;
using Xunit;

namespace PixiEditorTests.ModelsTests.ColorsTests
{
    public class ExtendedColorTests
    {

        public static readonly SKColor white = new SKColor(255, 255, 255);
        public static readonly SKColor black = new SKColor(0, 0, 0);
        public static readonly SKColor transparent = new SKColor(0, 0, 0, 0);
        public static readonly SKColor red = new SKColor(255, 0, 0);
        public static readonly SKColor green = new SKColor(0, 255, 0);
        public static readonly SKColor blue = new SKColor(0, 0, 255);

        private const int AcceptableMaringOfError = 1;


        [Fact]
        public void ChangeColorBrightnessIsNotTheSameTest()
        {
            SKColor newColor = ExColor.ChangeColorBrightness(white, -1);
            Assert.NotEqual(white, newColor);
        }

        [Fact]
        public void ChangeColorBrightnessNewValueTest()
        {
            SKColor newColor = ExColor.ChangeColorBrightness(white, -100);
            Assert.Equal(black, newColor);
        }

        // Acceptable margin of error is 1
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
            SKColor rgb = ExColor.HslToRgb(h, s, l);
            int marginOfErrorR = Math.Abs(rgb.Red - r);
            int marginOfErrorG = Math.Abs(rgb.Green - g);
            int marginOfErrorB = Math.Abs(rgb.Blue - b);
            Assert.True(marginOfErrorR <= AcceptableMaringOfError);
            Assert.True(marginOfErrorG <= AcceptableMaringOfError);
            Assert.True(marginOfErrorB <= AcceptableMaringOfError);
        }
    }
}