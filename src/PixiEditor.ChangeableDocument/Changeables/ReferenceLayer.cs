using System.Collections.Immutable;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using Drawie.Backend.Core.Numerics;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables;

public class ReferenceLayer : IReadOnlyReferenceLayer
{
    public ImmutableArray<byte> ImageBgra8888Bytes { get; }
    public VecI ImageSize { get; }
    public ShapeCorners Shape { get; set; }
    public bool IsVisible { get; set; } = true;
    public bool IsTopMost { get; set; }
    
    public ReferenceLayer(ImmutableArray<byte> imageBgra8888Bytes, VecI imageSize, ShapeCorners shape)
    {
        ImageBgra8888Bytes = imageBgra8888Bytes;
        ImageSize = imageSize;
        Shape = shape;
    }

    public ReferenceLayer Clone()
    {
        return new ReferenceLayer(ImageBgra8888Bytes, ImageSize, Shape);
    }
}
