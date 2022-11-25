using System.Collections.Immutable;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables;

public class ReferenceLayer : IReadOnlyReferenceLayer
{
    public ImmutableArray<byte> ImagePbgra32Bytes { get; }
    public VecI ImageSize { get; }
    public ShapeCorners Shape { get; set; }
    public bool IsVisible { get; set; } = true;
    
    public ReferenceLayer(ImmutableArray<byte> imagePbgra32Bytes, VecI imageSize, ShapeCorners shape)
    {
        ImagePbgra32Bytes = imagePbgra32Bytes;
        ImageSize = imageSize;
        Shape = shape;
    }

    public ReferenceLayer Clone()
    {
        return new ReferenceLayer(ImagePbgra32Bytes, ImageSize, Shape);
    }
}
