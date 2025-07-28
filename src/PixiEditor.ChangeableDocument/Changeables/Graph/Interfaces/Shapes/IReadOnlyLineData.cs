using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces.Shapes;

public interface IReadOnlyLineData : IReadOnlyShapeVectorData
{
    public VecD Start { get; }
    public VecD End { get; }
    public VecD TransformedStart { get; set; }
    public VecD TransformedEnd { get; set; }
}
