using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces.Shapes;

public interface IReadOnlyRectangleData : IReadOnlyShapeVectorData // TODO: Add IReadOnlyStrokeJoinable
{
    public VecD Center { get; }
    public VecD Size { get; }
}
