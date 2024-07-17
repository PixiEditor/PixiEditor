using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core.ColorsImpl;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.CombineSeparate;

public class CombineColorNode : Node
{
    public FieldOutputProperty<Color> Color { get; }
    
    public FieldInputProperty<double> R { get; }
    
    public FieldInputProperty<double> G { get; }
    
    public FieldInputProperty<double> B { get; }
    
    public FieldInputProperty<double> A { get; }

    public CombineColorNode()
    {
        Color = CreateFieldOutput(nameof(Color), "COLOR", GetColor);
        
        R = CreateFieldInput("R", "R", 0d);
        G = CreateFieldInput("G", "G", 0d);
        B = CreateFieldInput("B", "B", 0d);
        A = CreateFieldInput("A", "A", 0d);
    }

    private Color GetColor(FieldContext ctx)
    {
        var r = R.Value(ctx) * 255;
        var g = G.Value(ctx) * 255;
        var b = B.Value(ctx) * 255;
        var a = A.Value(ctx) * 255;

        return new Color((byte)r, (byte)g, (byte)b, (byte)a);
    }
    
    protected override Surface? OnExecute(RenderingContext context)
    {
        return null;
    }

    public override bool Validate() => true;

    public override Node CreateCopy() => new CombineColorNode();
}
