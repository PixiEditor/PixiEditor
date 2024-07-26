using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.ColorsImpl;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.CombineSeparate;

[NodeInfo("CombineColor")]
public class CombineColorNode : Node
{
    public FuncOutputProperty<Color> Color { get; }

    public FuncInputProperty<double> R { get; }

    public FuncInputProperty<double> G { get; }

    public FuncInputProperty<double> B { get; }

    public FuncInputProperty<double> A { get; }

    public override string DisplayName { get; set; } = "COMBINE_COLOR_NODE";

    public CombineColorNode()
    {
        Color = CreateFuncOutput(nameof(Color), "COLOR", GetColor);

        R = CreateFuncInput("R", "R", 0d);
        G = CreateFuncInput("G", "G", 0d);
        B = CreateFuncInput("B", "B", 0d);
        A = CreateFuncInput("A", "A", 0d);
    }

    private Color GetColor(FuncContext ctx)
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


    public override Node CreateCopy() => new CombineColorNode();
}
