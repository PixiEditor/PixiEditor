using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;

public interface IReadOnlyVectorNode : IReadOnlyLayerNode
{
    public IReadOnlyShapeVectorData? ShapeData { get; }
}
