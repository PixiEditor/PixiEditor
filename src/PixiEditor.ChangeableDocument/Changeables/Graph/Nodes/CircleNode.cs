using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.ImageData;
using PixiEditor.DrawingApi.Core.Surface.PaintImpl;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

public class CircleNode : Node
{
    public InputProperty<int> Radius { get; }
    public InputProperty<int> X { get; }
    public InputProperty<int> Y { get; }
    public InputProperty<Color> StrokeColor { get; }
    public InputProperty<Color> FillColor { get; }
    public InputProperty<int> StrokeWidth { get; }
    public OutputProperty<Image> Output { get; }
    
    public CircleNode() 
    {
        Radius = CreateInput<int>("Radius", "RADIUS", 10);
        X = CreateInput<int>("X", "X", 0);
        Y = CreateInput<int>("Y", "Y", 0);
        StrokeColor = CreateInput<Color>("StrokeColor", "STROKE_COLOR", new Color(0, 0, 0, 255));
        FillColor = CreateInput<Color>("FillColor", "FILL_COLOR", new Color(0, 0, 0, 255));
        StrokeWidth = CreateInput<int>("StrokeWidth", "STROKE_WIDTH", 1);
        Output = CreateOutput<Image?>("Output", "OUTPUT", null);
    }
    
    protected override Image? OnExecute(KeyFrameTime frameTime)
    {
        Surface workingSurface = new Surface(new VecI(Radius.Value * 2, Radius.Value * 2));
        
        using Paint paint = new Paint();
        paint.StrokeWidth = StrokeWidth.Value;
        paint.Color = StrokeColor.Value;
        
        workingSurface.DrawingSurface.Canvas.DrawCircle(Radius.Value, Radius.Value, Radius.Value, paint);
        
        Output.Value = workingSurface.DrawingSurface.Snapshot();
        
        workingSurface.Dispose();
        
        return Output.Value;
    }

    public override bool Validate()
    {
        return Radius.Value > 0 && StrokeWidth.Value > 0;
    }

    public override Node CreateCopy() => new CircleNode();
}
