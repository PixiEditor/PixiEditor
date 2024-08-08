using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Shaders.Generation.Expressions;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.CombineSeparate;

[NodeInfo("CombineColor")]
public class CombineColorNode : Node
{
    public FuncOutputProperty<Half4> Color { get; }

    public FuncInputProperty<Float1> R { get; }

    public FuncInputProperty<Float1> G { get; }

    public FuncInputProperty<Float1> B { get; }

    public FuncInputProperty<Float1> A { get; }

    public override string DisplayName { get; set; } = "COMBINE_COLOR_NODE";

    public CombineColorNode()
    {
        Color = CreateFuncOutput(nameof(Color), "COLOR", GetColor);

        R = CreateFuncInput<Float1>("R", "R", 0d);
        G = CreateFuncInput<Float1>("G", "G", 0d);
        B = CreateFuncInput<Float1>("B", "B", 0d);
        A = CreateFuncInput<Float1>("A", "A", 0d);
    }

    private Half4 GetColor(FuncContext ctx)
    {
        var r = R.Value(ctx);
        var g = G.Value(ctx);
        var b = B.Value(ctx);
        var a = A.Value(ctx);

        return ctx.NewHalf4(r, g, b, a); 
    }


    protected override Texture? OnExecute(RenderingContext context)
    {
        return null;
    }


    public override Node CreateCopy() => new CombineColorNode();
}
