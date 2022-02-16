using PixiEditor.Helpers.Extensions;
using SkiaSharp;
using System.Windows;
using Xunit;

namespace PixiEditorTests.ModelsTests.PositionTests
{
    public class RectTests
    {
        [Fact]
        public void TestThatInt32RectToSKRectIWorks()
        {
            Int32Rect rect = new Int32Rect(5, 2, 8, 10);
            SKRectI converted = rect.ToSKRectI();
            Assert.Equal(rect.X, converted.Left);
            Assert.Equal(rect.Y, converted.Top);
            Assert.Equal(rect.Width, converted.Width);
            Assert.Equal(rect.Height, converted.Height);
        }

        [Fact]
        public void TestThatSKRectIToInt32RectWorks()
        {
            SKRectI rect = new SKRectI(5, 2, 8, 10);
            Int32Rect converted = rect.ToInt32Rect();
            Assert.Equal(rect.Left, converted.X);
            Assert.Equal(rect.Top, converted.Y);
            Assert.Equal(rect.Width, converted.Width);
            Assert.Equal(rect.Height, converted.Height);
        }
    }
}
