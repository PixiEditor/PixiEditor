using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes;

[NodeInfo("Line")]
public class LineNode : ShapeNode<LineVectorData>
{
    public InputProperty<VecD> Start { get; }
    public InputProperty<VecD> End { get; }
    public InputProperty<Paintable> StrokeColor { get; }
    public InputProperty<double> StrokeWidth { get; }

    public LineNode()
    {
        Start = CreateInput<VecD>("LineStart", "LINE_START", VecD.Zero);
        End = CreateInput<VecD>("LineEnd", "LINE_END", new VecD(32, 32));
        StrokeColor = CreateInput<Paintable>("StrokeColor", "STROKE_COLOR", new Color(0, 0, 0, 255));
        StrokeWidth = CreateInput<double>("StrokeWidth", "STROKE_WIDTH", 1);
    }

    protected override LineVectorData? GetShapeData(RenderContext context)
    {
        return new LineVectorData(Start.Value, End.Value)
            { Stroke = StrokeColor.Value, StrokeWidth = (float)StrokeWidth.Value };
    }

    public override Node CreateCopy() => new LineNode();
}
