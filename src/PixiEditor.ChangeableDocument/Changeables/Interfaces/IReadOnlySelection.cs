using SkiaSharp;

namespace PixiEditor.ChangeableDocument.Changeables.Interfaces;

public interface IReadOnlySelection
{
    public SKPath SelectionPath { get; }
}
