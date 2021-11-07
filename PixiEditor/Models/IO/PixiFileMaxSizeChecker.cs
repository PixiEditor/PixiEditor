using PixiEditor.Parser;

namespace PixiEditor.Models.IO
{
    internal class PixiFileMaxSizeChecker
    {
        private readonly int maxPixelCountAllowed;

        public PixiFileMaxSizeChecker(int maxPixelCountAllowed)
        {
            this.maxPixelCountAllowed = maxPixelCountAllowed;
        }

        public bool IsFileUnderMaxSize(SerializableDocument fileToCheck)
        {
            return fileToCheck.Height * fileToCheck.Width * fileToCheck.Layers.Count < maxPixelCountAllowed;
        }
    }
}