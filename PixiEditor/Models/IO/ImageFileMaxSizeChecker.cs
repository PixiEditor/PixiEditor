using System.Windows.Media.Imaging;

namespace PixiEditor.Models.IO
{
    internal class ImageFileMaxSizeChecker
    {
        public int MaxAllowedWidthInPixels { get; init; } = 2048;
        public int MaxAllowedHeightInPixels { get; init; } = 2048;

        public ImageFileMaxSizeChecker()
        {
        }

        public bool IsFileUnderMaxSize(WriteableBitmap fileToCheck)
        {
            return fileToCheck.PixelWidth <= MaxAllowedWidthInPixels
                && fileToCheck.PixelHeight <= MaxAllowedHeightInPixels;
        }
    }
}