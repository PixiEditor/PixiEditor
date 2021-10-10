using System.Windows.Media.Imaging;

namespace PixiEditor.Models.IO
{
    internal static class ImageFileMaxSizeChecker
    {
        // Result of 2048 (Width) * 2048 (Height).
        private const int MaxBitCountAllowed = 4194304;

        public static bool IsFileUnderMaxSize(WriteableBitmap fileToCheck)
        {
            return fileToCheck.PixelHeight * fileToCheck.PixelWidth < MaxBitCountAllowed;
        }
    }
}