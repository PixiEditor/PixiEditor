using PixiEditor.Parser;

namespace PixiEditor.Models.IO
{
    internal class PixiFileMaxSizeChecker
    {
        public int MaxAllowedWidthInPixels { get; init; } = 1080;
        public int MaxAllowedHeightInPixels { get; init; } = 1080;
        public int MaxAllowedLayerCount { get; init; } = 5;

        public PixiFileMaxSizeChecker()
        {
        }

        public bool IsFileUnderMaxSize(SerializableDocument fileToCheck)
        {
            return fileToCheck.Width <= MaxAllowedWidthInPixels
                && fileToCheck.Height <= MaxAllowedHeightInPixels
                && fileToCheck.Layers.Count <= MaxAllowedLayerCount;
        }
    }
}