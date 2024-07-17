using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.CombineSeparate;

public class CombineVecI : Node
{
    public FieldOutputProperty<VecI> Vector { get; }
    
    public FieldInputProperty<int> X { get; }
    
    public FieldInputProperty<int> Y { get; }

    public CombineVecI()
    {
        Vector = CreateFieldOutput(nameof(Vector), "VECTOR", GetVector);

        X = CreateFieldInput(nameof(X), "X", 0);
        Y = CreateFieldInput(nameof(Y), "Y", 0);
    }

    private VecI GetVector(FieldContext ctx)
    {
        var r = X.Value(ctx);
        var g = Y.Value(ctx);

        return new VecI(r, g);
    }

    protected override Surface? OnExecute(RenderingContext context)
    {
        return null;
    }

    public override bool Validate() => true;

    public override Node CreateCopy() => new CombineVecI();
}
