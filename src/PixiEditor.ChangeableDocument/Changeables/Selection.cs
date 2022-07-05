using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using SkiaSharp;

namespace PixiEditor.ChangeableDocument.Changeables;

internal class Selection : IReadOnlySelection, IDisposable
{
    public static SKColor SelectionColor { get; } = SKColors.CornflowerBlue;
    public SKPath SelectionPath { get; set; } = new();
    SKPath IReadOnlySelection.SelectionPath 
    {
        get {
            try
            {
                // I think this might throw if another thread disposes SelectionPath at the wrong time?
                return new SKPath(SelectionPath);
            }
            catch (Exception)
            {
                return new SKPath();
            }
        }
    }

    public void Dispose()
    {
        SelectionPath.Dispose();
    }
}
