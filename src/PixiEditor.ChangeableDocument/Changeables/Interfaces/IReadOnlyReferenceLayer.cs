using System.Collections.Immutable;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Interfaces;
public interface IReadOnlyReferenceLayer
{
    public ImmutableArray<byte> ImageBgra8888Bytes { get; }
    public VecI ImageSize { get; }
    public ShapeCorners Shape { get; }
    public bool IsVisible { get; }
    public bool IsTopMost { get; }
}
