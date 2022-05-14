using ChunkyImageLib;
using SkiaSharp;

namespace PixiEditor.ChangeableDocument.Changeables.Interfaces;

public interface IReadOnlySelection
{
    public IReadOnlyChunkyImage ReadOnlySelectionImage { get; }
    public bool ReadOnlyIsEmptyAndInactive { get; }
    public SKPath ReadOnlySelectionPath { get; }
}
