using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Vector;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces.Shapes;

public interface IReadOnlyStrokeJoinable
{
    public StrokeJoin StrokeLineJoin { get; }
    public StrokeCap StrokeLineCap { get; }
}
