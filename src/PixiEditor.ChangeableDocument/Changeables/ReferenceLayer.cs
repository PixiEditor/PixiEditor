using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using SkiaSharp;

namespace PixiEditor.ChangeableDocument.Changeables;

public class ReferenceLayer : IReadOnlyReferenceLayer
{
    public Surface Image { get; }
    
    public ShapeCorners Shape { get; set; }

    public ReferenceLayer(Surface image, ShapeCorners shape)
    {
        Image = image;
        Shape = shape;
    }
}
