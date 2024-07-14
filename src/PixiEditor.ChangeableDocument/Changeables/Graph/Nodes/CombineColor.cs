using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.DrawingApi.Core.ColorsImpl;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

public class CombineColor : Node
{
    public OutputProperty<Func<IFieldContext, Color>> Color { get; }
    
    public InputProperty<Func<IFieldContext, double>> R { get; }
    
    public InputProperty<Func<IFieldContext, double>> G { get; }
    
    public InputProperty<Func<IFieldContext, double>> B { get; }
    
    public InputProperty<Func<IFieldContext, double>> A { get; }

    public CombineColor()
    {
        Color = CreateOutput<Func<IFieldContext, Color>>(nameof(Color), "COLOR", GetColor);
        
        R = CreateInput<Func<IFieldContext, double>>("R", "R", _ => 0);
        G = CreateInput<Func<IFieldContext, double>>("G", "G", _ => 0);
        B = CreateInput<Func<IFieldContext, double>>("B", "B", _ => 0);
        A = CreateInput<Func<IFieldContext, double>>("A", "A", _ => 1);
    }

    private Color GetColor(IFieldContext ctx)
    {
        var r = R.Value(ctx) * 255;
        var g = G.Value(ctx) * 255;
        var b = B.Value(ctx) * 255;
        var a = A.Value(ctx) * 255;

        return new Color((byte)r, (byte)g, (byte)b, (byte)a);
    }
    
    protected override ChunkyImage? OnExecute(KeyFrameTime frameTime)
    {
        return null;
    }

    public override bool Validate() => true;

    public override Node CreateCopy() => new SeparateVecI();
}
