using ChunkyImageLib;
using SkiaSharp;

namespace PixiEditor.ChangeableDocument.Changeables.Interfaces;

public interface IReadOnlySelection
{
    public IReadOnlyChunkyImage SelectionImage { get; }
    public bool IsEmptyAndInactive { get; }
    public SKPath SelectionPath { get; }
}
