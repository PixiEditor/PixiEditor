using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.CombineSeparate;

public class CombineVecI : Node
{
    public FuncOutputProperty<VecI> Vector { get; }
    
    public FuncInputProperty<int> X { get; }
    
    public FuncInputProperty<int> Y { get; }

    public override string DisplayName { get; set; } = "COMBINE_VECI_NODE";

    public CombineVecI()
    {
        Vector = CreateFieldOutput(nameof(Vector), "VECTOR", GetVector);

        X = CreateFuncInput(nameof(X), "X", 0);
        Y = CreateFuncInput(nameof(Y), "Y", 0);
    }

    private VecI GetVector(FuncContext ctx)
    {
        var r = X.Value(ctx);
        var g = Y.Value(ctx);

        return new VecI(r, g);
    }

    protected override string NodeUniqueName => "CombineVecI";

    protected override Surface? OnExecute(RenderingContext context)
    {
        return null;
    }


    public override Node CreateCopy() => new CombineVecI();
}
