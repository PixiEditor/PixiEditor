using Drawie.Backend.Core.Surfaces.Vector;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces.Shapes;

public interface IReadOnlyPathData : IReadOnlyShapeVectorData
{
    public VectorPath Path { get; }
}
