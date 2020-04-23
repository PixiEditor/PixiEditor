using NUnit.Framework;
using PixiEditor.Models.Colors;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace PixiEditorTests.WorkspaceTests.ColorsTests
{
    [TestFixture]
    public class ExtendedColorTests
    {
        [TestCase()]
        public void ChangeColorBrightnessIsNotTheSameTest()
        {
            Color newColor = ExColor.ChangeColorBrightness(Colors.White, -1);
            Assert.AreNotEqual(Colors.White, newColor);
        }

        [TestCase()]
        public void ChangeColorBrightnessNewValueTest()
        {
            Color newColor = ExColor.ChangeColorBrightness(Colors.White, -100);
            Assert.AreEqual(Colors.Black, newColor);
        }

        [TestCase(0,0,0,0,0,0)]
        [TestCase(255,255,255,0,0,100)]
        [TestCase(182,55,55,0,53.6f,46.5f)]
        [TestCase(20,47,255,233,100,53.9f)]
        [TestCase(137, 43,226, 270,75.9f,52.7f)] //Theoretically 170 should be 171, but error margin of 1 is acceptable
        public void RgbToHslTest(int r, int g, int b, int h, float s, float l)
        {
            Tuple<int, float, float> hsl = ExColor.RgbToHsl(r, g, b);
            Assert.AreEqual(h, hsl.Item1);
            Assert.AreEqual(Math.Round(s), Math.Round(hsl.Item2));
            Assert.AreEqual(Math.Round(l), Math.Round(hsl.Item3));

        }

        [TestCase(0, 0, 0, 0, 0, 0)]
        [TestCase(0, 0, 100, 255, 255, 255)]
        [TestCase(0, 53.6f, 46.5f, 182, 55, 55)]
        [TestCase(297, 100, 17.1f, 82, 0, 87)]
        [TestCase(271, 75.9f, 52.7f, 137, 42, 226)] //Same as above, but with 43 instead of 42
        public void HslToRgbTest(int h, float s, float l, int r, int g, int b)
        {
            Color rgb = ExColor.HslToRGB(h, s, l);
            Assert.AreEqual(r, rgb.R);
            Assert.AreEqual(g, rgb.G);
            Assert.AreEqual(b, rgb.B);

        }
    }
}
