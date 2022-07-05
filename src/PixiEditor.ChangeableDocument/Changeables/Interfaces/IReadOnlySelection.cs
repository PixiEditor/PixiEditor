using SkiaSharp;

namespace PixiEditor.ChangeableDocument.Changeables.Interfaces;

public interface IReadOnlySelection
{
    /// <summary>
    /// The path of the selection
    /// </summary>
    public SKPath SelectionPath { get; }
}
