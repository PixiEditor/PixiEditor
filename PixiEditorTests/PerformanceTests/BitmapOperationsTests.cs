using NUnit.Framework;
using System.Diagnostics;
using System.Windows.Media.Imaging;

namespace PixiEditorTests.PerformanceTests
{
    [TestFixture]
    public class BitmapOperationsTests
    {
        [TestCase(16,16, 100)]
        [TestCase(128, 128, 200)]
        [TestCase(512, 512, 300)]
        [TestCase(1024, 1024, 500)]
        [TestCase(2046, 2046, 1500)]
        [TestCase(4096, 4096, 5000)]
        public void FillBitmapWithPixelsTest(int width, int height, float maxExecutionTime)
        {
            WriteableBitmap bitmap = BitmapFactory.New(width, height);
            bitmap.Lock();

            Stopwatch timer = new Stopwatch(); //Timer starts here, because we don't want to include creating new bitmap in "benchmark"
            timer.Start();
            for (int i = 0; i < width * height; i++)
            {
                bitmap.SetPixeli(i, 0xFFFFF);

            }
            bitmap.Unlock();
            timer.Stop();
            System.Console.WriteLine("Execution time: " + timer.ElapsedMilliseconds + "ms");
            Assert.IsTrue(timer.ElapsedMilliseconds <= maxExecutionTime);
        }
    }
}
