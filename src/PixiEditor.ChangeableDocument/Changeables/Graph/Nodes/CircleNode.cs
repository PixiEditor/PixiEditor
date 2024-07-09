using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.DrawingApi.Core.ColorsImpl;
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
    public OutputProperty<ChunkyImage> Output { get; }
    
    public CircleNode() 
    {
        Radius = CreateInput<int>("Radius", "RADIUS", 10);
        X = CreateInput<int>("X", "X", 0);
        Y = CreateInput<int>("Y", "Y", 0);
        StrokeColor = CreateInput<Color>("StrokeColor", "STROKE_COLOR", new Color(0, 0, 0, 255));
        FillColor = CreateInput<Color>("FillColor", "FILL_COLOR", new Color(0, 0, 0, 255));
        StrokeWidth = CreateInput<int>("StrokeWidth", "STROKE_WIDTH", 1);
        Output = CreateOutput<ChunkyImage>("Output", "OUTPUT", null);
    }
    
    public override ChunkyImage? OnExecute(KeyFrameTime frameTime)
    {
        Output.Value = new ChunkyImage(new VecI(Radius.Value * 2, Radius.Value * 2));
        
        Output.Value.EnqueueDrawEllipse(
            RectI.Create(X.Value, Y.Value, Radius.Value * 2, Radius.Value * 2), 
            FillColor.Value, StrokeColor.Value, StrokeWidth.Value);
        
        Output.Value.CommitChanges();
        
        return Output.Value;
    }

    public override bool Validate()
    {
        return Radius.Value > 0 && StrokeWidth.Value > 0;
    }
}
