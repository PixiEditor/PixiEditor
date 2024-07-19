using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.CombineSeparate;

public class CombineVecD : Node
{
    public FieldOutputProperty<VecD> Vector { get; }
    
    public FieldInputProperty<double> X { get; }
    
    public FieldInputProperty<double> Y { get; }
    
    
    public override string DisplayName { get; set; } = "COMBINE_VECD_NODE";

    public CombineVecD()
    {
        Vector = CreateFieldOutput(nameof(Vector), "VECTOR", GetVector);

        X = CreateFieldInput(nameof(X), "X", 0d);
        Y = CreateFieldInput(nameof(Y), "Y", 0d);
    }
    
    private VecD GetVector(FieldContext ctx)
    {
        var r = X.Value(ctx);
        var g = Y.Value(ctx);

        return new VecD(r, g);
    }

    protected override string NodeUniqueName => "CombineVecD";

    protected override Surface? OnExecute(RenderingContext context)
    {
        return null;
    }

    public override bool Validate() => true;

    public override Node CreateCopy() => new CombineVecD();
}
