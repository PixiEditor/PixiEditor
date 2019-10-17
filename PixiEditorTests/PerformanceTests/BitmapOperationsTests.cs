using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media.Imaging;

namespace PixiEditorTests.PerformanceTests
{
    [TestFixture]
    public class BitmapOperationsTests
    {
        [TestCase(16,16)]
        [TestCase(128, 128)]
        [TestCase(512, 512)]
        [TestCase(1024, 1024)]
        [TestCase(2046, 2046)]
        [TestCase(4096, 4096)]
        public void FillBitmapWithPixelsTest(int width, int height)
        {
            WriteableBitmap bitmap = BitmapFactory.New(width, height);
            bitmap.Lock();

            for (int i = 0; i < width * height; i++)
            {
                bitmap.SetPixeli(i, 0xFFFFF);

            }
            bitmap.Unlock();

            Assert.Pass();
        }
    }
}
