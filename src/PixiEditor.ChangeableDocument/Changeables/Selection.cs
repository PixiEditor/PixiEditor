using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using SkiaSharp;

namespace PixiEditor.ChangeableDocument.Changeables;

internal class Selection : IReadOnlySelection, IDisposable
{
    public static SKColor SelectionColor { get; } = SKColors.CornflowerBlue;
    public bool IsEmptyAndInactive { get; set; } = true;
    public ChunkyImage SelectionImage { get; set; } = new(new(64, 64));
    public SKPath SelectionPath { get; set; } = new();
    SKPath IReadOnlySelection.SelectionPath => new SKPath(SelectionPath);

    IReadOnlyChunkyImage IReadOnlySelection.SelectionImage => SelectionImage;
    bool IReadOnlySelection.IsEmptyAndInactive => IsEmptyAndInactive;

    public void Dispose()
    {
        SelectionImage.Dispose();
        SelectionPath.Dispose();
    }
}
