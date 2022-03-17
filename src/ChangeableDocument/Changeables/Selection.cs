using ChangeableDocument.Changeables.Interfaces;
using ChunkyImageLib;
using SkiaSharp;

namespace ChangeableDocument.Changeables
{
    internal class Selection : IReadOnlySelection
    {
        public static SKColor SelectionColor { get; } = SKColors.CornflowerBlue;
        public bool IsEmptyAndInactive { get; set; } = true;
        public ChunkyImage SelectionImage { get; set; } = new(new(64, 64));

        public IReadOnlyChunkyImage ReadOnlySelectionImage => SelectionImage;
        public bool ReadOnlyIsEmptyAndInactive => IsEmptyAndInactive;
    }
}
