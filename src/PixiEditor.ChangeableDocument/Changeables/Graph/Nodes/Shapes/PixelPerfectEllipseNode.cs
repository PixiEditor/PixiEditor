using ChunkyImageLib.Operations;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes;

[NodeInfo("PixelPerfectEllipse")]
public class PixelPerfectEllipseNode : ShapeNode<PathVectorData>
{
    public InputProperty<VecD> Center { get; }
    public InputProperty<VecI> Size { get; }
    public InputProperty<Paintable> StrokeColor { get; }
    public InputProperty<Paintable> FillColor { get; }
    public InputProperty<double> StrokeWidth { get; }

    protected override bool ExecuteOnlyOnCacheChange => true;

    private VectorPath cachedPath = new();
    private VecI cachedSize = VecI.Zero;
    private VecI lastCenter = VecI.Zero;

    public PixelPerfectEllipseNode()
    {
        Center = CreateInput<VecD>("Position", "POSITION", VecI.Zero);
        Size = CreateInput<VecI>("Size", "SIZE", new VecI(32, 32)).WithRules(
            v => v.Min(new VecI(0)));
        StrokeColor = CreateInput<Paintable>("StrokeColor", "STROKE_COLOR", new Color(0, 0, 0, 255));
        FillColor = CreateInput<Paintable>("FillColor", "FILL_COLOR", new Color(0, 0, 0, 255));
        StrokeWidth = CreateInput<double>("StrokeWidth", "STROKE_WIDTH", 1);
    }
    protected override PathVectorData? GetShapeData(RenderContext context)
    {
        if(cachedSize != Size.Value)
        {
            cachedSize = Size.Value;
            cachedPath?.Dispose();
            cachedPath = EllipseHelper.ConstructEllipseOutline(new RectI(VecI.Zero, Size.Value));
        }

        var path = new VectorPath(cachedPath);
        path.Offset((Center.Value - new VecD(Size.Value.X / 2.0, Size.Value.Y / 2.0)));

        return new PathVectorData(path) { Stroke = StrokeColor.Value, FillPaintable = FillColor.Value, StrokeWidth = (float)StrokeWidth.Value };
    }

    public override Node CreateCopy()
    {
        return new PixelPerfectEllipseNode();
    }
}
