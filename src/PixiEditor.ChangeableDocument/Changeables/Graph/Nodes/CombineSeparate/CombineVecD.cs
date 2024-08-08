using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Shaders.Generation.Expressions;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.CombineSeparate;

[NodeInfo("CombineVecD")]
public class CombineVecD : Node
{
    public FuncOutputProperty<Float2> Vector { get; }
    
    public FuncInputProperty<Float1> X { get; }
    
    public FuncInputProperty<Float1> Y { get; }
    
    
    public override string DisplayName { get; set; } = "COMBINE_VECD_NODE";

    public CombineVecD()
    {
        Vector = CreateFuncOutput(nameof(Vector), "VECTOR", GetVector);

        X = CreateFuncInput<Float1>(nameof(X), "X", 0);
        Y = CreateFuncInput<Float1>(nameof(Y), "Y", 0);
    }
    
    private Float2 GetVector(FuncContext ctx)
    {
        var x = ctx.GetValue(X); 
        var y = ctx.GetValue(Y);

        return ctx.NewFloat2(x, y); 
    }

    protected override Texture? OnExecute(RenderingContext context)
    {
        return null;
    }


    public override Node CreateCopy() => new CombineVecD();
}
