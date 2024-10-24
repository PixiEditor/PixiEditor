using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces.Shapes;

public interface IReadOnlyEllipseData : IReadOnlyShapeVectorData
{
    public VecD Center { get; }
    public VecD Radius { get; }
}
