using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.Shaders.Generation.Expressions;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.CombineSeparate;

[NodeInfo("CombineVecI")]
public class CombineVecI : Node
{
    public FuncOutputProperty<Int2> Vector { get; }
    
    public FuncInputProperty<Int1> X { get; }
    
    public FuncInputProperty<Int1> Y { get; }

    public override string DisplayName { get; set; } = "COMBINE_VECI_NODE";

    public CombineVecI()
    {
        Vector = CreateFuncOutput(nameof(Vector), "VECTOR", GetVector);

        X = CreateFuncInput<Int1>(nameof(X), "X", 0);
        Y = CreateFuncInput<Int1>(nameof(Y), "Y", 0);
    }

    private Int2 GetVector(FuncContext ctx)
    {
        var x = ctx.GetValue(X);
        var y = ctx.GetValue(Y);

        return ctx.NewInt2(x, y);
    }


    protected override Texture? OnExecute(RenderingContext context)
    {
        return null;
    }


    public override Node CreateCopy() => new CombineVecI();
}
