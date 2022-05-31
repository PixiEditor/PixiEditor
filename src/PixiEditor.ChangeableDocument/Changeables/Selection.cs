using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using SkiaSharp;

namespace PixiEditor.ChangeableDocument.Changeables;

internal class Selection : IReadOnlySelection, IDisposable
{
    public static SKColor SelectionColor { get; } = SKColors.CornflowerBlue;
    public SKPath SelectionPath { get; set; } = new();
    SKPath IReadOnlySelection.SelectionPath => new SKPath(SelectionPath);

    public void Dispose()
    {
        SelectionPath.Dispose();
    }
}
