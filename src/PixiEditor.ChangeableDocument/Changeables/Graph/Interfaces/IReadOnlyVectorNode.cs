using PixiEditor.ChangeableDocument.Changeables.Animations;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;

public interface IReadOnlyVectorNode : IReadOnlyLayerNode
{
    public IReadOnlyShapeVectorData? ShapeData { get; }

    public IReadOnlyShapeVectorData? GetShapeData(KeyFrameTime frame);
}
