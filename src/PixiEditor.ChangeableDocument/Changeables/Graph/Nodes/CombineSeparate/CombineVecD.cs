using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.CombineSeparate;

[NodeInfo("CombineVecD")]
public class CombineVecD : Node
{
    public FuncOutputProperty<VecD> Vector { get; }
    
    public FuncInputProperty<double> X { get; }
    
    public FuncInputProperty<double> Y { get; }
    
    
    public override string DisplayName { get; set; } = "COMBINE_VECD_NODE";

    public CombineVecD()
    {
        Vector = CreateFuncOutput(nameof(Vector), "VECTOR", GetVector);

        X = CreateFuncInput(nameof(X), "X", 0d);
        Y = CreateFuncInput(nameof(Y), "Y", 0d);
    }
    
    private VecD GetVector(FuncContext ctx)
    {
        var r = X.Value(ctx);
        var g = Y.Value(ctx);

        return new VecD(r, g);
    }

    protected override Surface? OnExecute(RenderingContext context)
    {
        return null;
    }


    public override Node CreateCopy() => new CombineVecD();
}
