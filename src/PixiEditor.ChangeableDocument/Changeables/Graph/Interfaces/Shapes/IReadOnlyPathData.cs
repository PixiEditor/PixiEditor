using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Vector;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces.Shapes;

public interface IReadOnlyPathData : IReadOnlyShapeVectorData
{
    public VectorPath Path { get; }
    public StrokeCap StrokeLineCap { get; }
    public StrokeJoin StrokeLineJoin { get; }
}
