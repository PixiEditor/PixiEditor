using PixiEditor.Parser;

namespace PixiEditor.Models.IO
{
    internal static class PixiFileMaxSizeChecker
    {
        // Result of 1080 (Width) * 1080 (Height) * 5 (Layers count).
        private const int MaxBitCountAllowed = 5832000;

        public static bool IsFileUnderMaxSize(SerializableDocument fileToCheck)
        {
            return fileToCheck.Height * fileToCheck.Width * fileToCheck.Layers.Count < MaxBitCountAllowed;
        }
    }
}