using System.Windows.Media.Imaging;

namespace PixiEditor.Models.IO
{
    internal class ImageFileMaxSizeChecker
    {
        public int MaxAllowedWidthInPixels { get; init; } = Constants.MaxPreviewWidth;
        public int MaxAllowedHeightInPixels { get; init; } = Constants.MaxPreviewHeight;

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