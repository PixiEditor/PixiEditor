using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces.Shapes;

public interface IReadOnlyRectangleData : IReadOnlyShapeVectorData, IReadOnlyStrokeJoinable
{
    public VecD Center { get; }
    public VecD Size { get; }
    public double CornerRadius { get; }
}
