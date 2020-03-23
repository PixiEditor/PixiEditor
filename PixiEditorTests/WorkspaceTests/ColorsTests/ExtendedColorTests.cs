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
            Color newColor = ExColor.ChangeColorBrightness(Colors.White, -0.1f);
            Assert.AreNotEqual(Colors.White, newColor);
        }

        [TestCase()]
        public void ChangeColorBrightnessNewValueTest()
        {
            Color newColor = ExColor.ChangeColorBrightness(Colors.White, -1f);
            Assert.AreEqual(Colors.Black, newColor);
        }
    }
}
