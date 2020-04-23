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
        private const int AcceptableMaringOfError = 1;

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


        //Acceptable margin of error is 1
        [TestCase(0,0,0,0,0,0)]
        [TestCase(255,255,255,0,0,100)]
        [TestCase(182,55,55,0,53.6f,46.5f)]
        [TestCase(20,47,255,233,100,53.9f)]
        [TestCase(137, 43,226, 271,75.9f,52.7f)]
        public void RgbToHslTest(int r, int g, int b, int h, float s, float l)
        {
            Tuple<int, float, float> hsl = ExColor.RgbToHsl(r, g, b);
            float marginOfErrorH = Math.Abs(hsl.Item1 - h);
            float marginOfErrorS = Math.Abs(hsl.Item2 - s);
            float marginOfErrorL = Math.Abs(hsl.Item3 - l);
            Assert.LessOrEqual(marginOfErrorH, AcceptableMaringOfError);
            Assert.LessOrEqual(marginOfErrorS, AcceptableMaringOfError);
            Assert.LessOrEqual(marginOfErrorL, AcceptableMaringOfError);

        }

        [TestCase(0, 0, 0, 0, 0, 0)]
        [TestCase(0, 0, 100, 255, 255, 255)]
        [TestCase(0, 53.6f, 46.5f, 182, 55, 55)]
        [TestCase(297, 100, 17.1f, 82, 0, 87)]
        [TestCase(271, 75.9f, 52.7f, 137, 43, 226)]
        public void HslToRgbTest(int h, float s, float l, int r, int g, int b)
        {
            Color rgb = ExColor.HslToRGB(h, s, l);
            int marginOfErrorR = Math.Abs(rgb.R - r);
            int marginOfErrorG = Math.Abs(rgb.G - g);
            int marginOfErrorB = Math.Abs(rgb.B - b);
            Assert.LessOrEqual(marginOfErrorR, AcceptableMaringOfError);
            Assert.LessOrEqual(marginOfErrorG, AcceptableMaringOfError);
            Assert.LessOrEqual(marginOfErrorB, AcceptableMaringOfError);

        }
    }
}
