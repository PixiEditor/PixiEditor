using System.Collections.Immutable;
using Drawie.Backend.Core.Numerics;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Interfaces;
public interface IReadOnlyReferenceLayer
{
    public ImmutableArray<byte> ImageBgra8888Bytes { get; }
    public VecI ImageSize { get; }
    public ShapeCorners Shape { get; }
    public bool IsVisible { get; }
    public bool IsTopMost { get; }
}
