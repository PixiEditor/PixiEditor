using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Surface.ImageData;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

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
    
    protected override Image? OnExecute(KeyFrameTime frameTime)
    {
        return null;
    }

    public override bool Validate() => true;

    public override Node CreateCopy() => new SeparateVecDNode();
}
