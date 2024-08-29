using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Shaders.Generation.Expressions;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.CombineSeparate;

[NodeInfo("CombineColor", "COMBINE_COLOR_NODE", Category = "COLOR")]
public class CombineColorNode : Node
{
    public FuncOutputProperty<Half4> Color { get; }

    public InputProperty<CombineSeparateColorMode> Mode { get; }

    public FuncInputProperty<Float1> R { get; }

    public FuncInputProperty<Float1> G { get; }

    public FuncInputProperty<Float1> B { get; }

    public FuncInputProperty<Float1> A { get; }

    public CombineColorNode()
    {
        Color = CreateFuncOutput(nameof(Color), "COLOR", GetColor);
        Mode = CreateInput("Mode", "MODE", CombineSeparateColorMode.RGB);

        R = CreateFuncInput<Float1>("R", "R", 0d);
        G = CreateFuncInput<Float1>("G", "G", 0d);
        B = CreateFuncInput<Float1>("B", "B", 0d);
        A = CreateFuncInput<Float1>("A", "A", 0d);
    }

    private Half4 GetColor(FuncContext ctx) =>
        Mode.Value switch
        {
            CombineSeparateColorMode.RGB => GetRgb(ctx),
            CombineSeparateColorMode.HSV => GetHsv(ctx),
            CombineSeparateColorMode.HSL => GetHsl(ctx)
        };

    private Half4 GetRgb(FuncContext ctx)
    {
        var r = ctx.GetValue(R);
        var g = ctx.GetValue(G);
        var b = ctx.GetValue(B);
        var a = ctx.GetValue(A);

        return ctx.NewHalf4(r, g, b, a); 
    }

    private Half4 GetHsv(FuncContext ctx)
    {
        var h = ctx.GetValue(R);
        var s = ctx.GetValue(G);
        var v = ctx.GetValue(B);
        var a = ctx.GetValue(A);

        return ctx.HsvaToRgba(h, s, v, a);
    }
    
    private Half4 GetHsl(FuncContext ctx)
    {
        var h = ctx.GetValue(R);
        var s = ctx.GetValue(G);
        var l = ctx.GetValue(B);
        var a = ctx.GetValue(A);

        return ctx.HslaToRgba(h, s, l, a);
    }

    protected override Texture? OnExecute(RenderingContext context)
    {
        return null;
    }


    public override Node CreateCopy() => new CombineColorNode();
}
