using Drawie.Backend.Core.Vector;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces.Shapes;

public interface IReadOnlyPathData : IReadOnlyShapeVectorData
{
    public VectorPath Path { get; }
}
