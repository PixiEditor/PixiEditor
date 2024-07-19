using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.PaintImpl;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

public class CircleNode : Node
{
    public InputProperty<VecI> Radius { get; }
    public InputProperty<Color> StrokeColor { get; }
    public InputProperty<Color> FillColor { get; }
    public InputProperty<int> StrokeWidth { get; }
    public OutputProperty<Surface> Output { get; }
    
    public CircleNode()
    {
        Radius = CreateInput<VecI>("Radius", "RADIUS", new VecI(32, 32));
        StrokeColor = CreateInput<Color>("StrokeColor", "STROKE_COLOR", new Color(0, 0, 0, 255));
        FillColor = CreateInput<Color>("FillColor", "FILL_COLOR", new Color(0, 0, 0, 255));
        StrokeWidth = CreateInput<int>("StrokeWidth", "STROKE_WIDTH", 1);
        Output = CreateOutput<Surface?>("Output", "OUTPUT", null);
    }

    protected override string NodeUniqueName => "Circle";

    protected override Surface? OnExecute(RenderingContext context)
    {
        /*TODO: ChunkyImage*/
        var radius = Radius.Value / 2;
        var strokeWidth = StrokeWidth.Value;
        var strokeOffset = new VecI(strokeWidth / 2);
        
        Surface workingSurface = new Surface(Radius.Value + strokeOffset * 2);
        
        using Paint paint = new Paint();
        paint.Color = FillColor.Value;
        paint.Style = PaintStyle.Fill;
        
        workingSurface.DrawingSurface.Canvas.DrawOval(radius + strokeOffset, radius, paint);
        
        paint.Color = StrokeColor.Value;
        paint.StrokeWidth = strokeWidth;
        paint.Style = PaintStyle.Stroke;

        workingSurface.DrawingSurface.Canvas.DrawOval(radius + strokeOffset, radius, paint);

        Output.Value = workingSurface;
        
        return Output.Value;
    }

    public override string DisplayName { get; set; } = "CIRCLE_NODE";

    public override bool AreInputsLegal()
    {
        return Radius.Value is { X: > 0, Y: > 0 } && StrokeWidth.Value > 0;
    }

    public override Node CreateCopy() => new CircleNode();
}
