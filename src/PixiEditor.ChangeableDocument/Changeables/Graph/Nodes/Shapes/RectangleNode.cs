using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes;

[NodeInfo("Rectangle")]
public class RectangleNode : ShapeNode<RectangleVectorData>
{
    public InputProperty<VecD> Center { get; }
    public InputProperty<VecD> Size { get; }
    public InputProperty<double> CornerRadius { get; }
    public InputProperty<Paintable> StrokeColor { get; }
    public InputProperty<Paintable> FillColor { get; }
    public InputProperty<double> StrokeWidth { get; }

    public RectangleNode()
    {
        Center = CreateInput<VecD>("Position", "CENTER", VecI.Zero);
        Size = CreateInput<VecD>("Size", "SIZE", new VecD(32, 32)).WithRules(
            v => v.Min(new VecD(0)));
        CornerRadius = CreateInput<double>("CornerRadius", "RADIUS", 0);
        StrokeColor = CreateInput<Paintable>("StrokeColor", "STROKE_COLOR", new Color(0, 0, 0, 255));
        FillColor = CreateInput<Paintable>("FillColor", "FILL_COLOR", new Color(0, 0, 0, 255));
        StrokeWidth = CreateInput<double>("StrokeWidth", "STROKE_WIDTH", 1).WithRules(v => v.Min(0d));
    }

    protected override RectangleVectorData? GetShapeData(RenderContext context)
    {
        return new RectangleVectorData(Center.Value, Size.Value)
            { CornerRadius = CornerRadius.Value, Stroke = StrokeColor.Value, FillPaintable = FillColor.Value, StrokeWidth = (float)StrokeWidth.Value };
    }

    public override Node CreateCopy() => new RectangleNode();
}
