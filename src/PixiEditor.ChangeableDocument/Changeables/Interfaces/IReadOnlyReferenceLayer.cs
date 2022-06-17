using SkiaSharp;

namespace PixiEditor.ChangeableDocument.Changeables.Interfaces;

public interface IReadOnlyReferenceLayer
{
    public Surface Image { get; }
    
    public ShapeCorners Shape { get; }
}
