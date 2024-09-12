using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces.Shapes;

public interface IReadOnlyLineData : IReadOnlyShapeVectorData
{
    public VecD Start { get; }
    public VecD End { get; }
}
