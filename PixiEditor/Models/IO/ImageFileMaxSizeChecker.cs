using System.Windows.Media.Imaging;

namespace PixiEditor.Models.IO
{
    internal class ImageFileMaxSizeChecker
    {
        private readonly int maxPixelCountAllowed;

        public ImageFileMaxSizeChecker(int maxPixelCountAllowed)
        {
            this.maxPixelCountAllowed = maxPixelCountAllowed;
        }

        public bool IsFileUnderMaxSize(WriteableBitmap fileToCheck)
        {
            return fileToCheck.PixelHeight * fileToCheck.PixelWidth < maxPixelCountAllowed;
        }
    }
}