using ChunkyImageLib;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using SkiaSharp;

namespace PixiEditor.ChangeableDocument.Changeables;

internal class Selection : IReadOnlySelection, IDisposable
{
    public static SKColor SelectionColor { get; } = SKColors.CornflowerBlue;
    public bool IsEmptyAndInactive { get; set; } = true;
    public ChunkyImage SelectionImage { get; set; } = new(new(64, 64));
    public SKPath SelectionPath { get; set; } = new();
    public SKPath ReadOnlySelectionPath => new SKPath(SelectionPath);

    public IReadOnlyChunkyImage ReadOnlySelectionImage => SelectionImage;
    public bool ReadOnlyIsEmptyAndInactive => IsEmptyAndInactive;

    public void Dispose()
    {
        SelectionImage.Dispose();
        SelectionPath.Dispose();
    }
}
