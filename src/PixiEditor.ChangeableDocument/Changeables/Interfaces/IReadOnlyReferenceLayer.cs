using System.Collections.Immutable;
using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Interfaces;
public interface IReadOnlyReferenceLayer
{
    public ImmutableArray<byte> ImagePbgra32Bytes { get; }
    public VecI ImageSize { get; }
    public ShapeCorners Shape { get; }
    public bool IsVisible { get; }
}
