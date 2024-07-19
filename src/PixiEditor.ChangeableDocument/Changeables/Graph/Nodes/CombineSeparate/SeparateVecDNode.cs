using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.CombineSeparate;

public class SeparateVecDNode : Node
{
    public FuncInputProperty<VecD> Vector { get; }
    
    public FuncOutputProperty<double> X { get; }
    
    public FuncOutputProperty<double> Y { get; }
    
    public override string DisplayName { get; set; } = "SEPARATE_VECD_NODE";

    public SeparateVecDNode()
    {
        X = CreateFieldOutput("X", "X", ctx => Vector.Value(ctx).X);
        Y = CreateFieldOutput("Y", "Y", ctx => Vector.Value(ctx).Y);
        Vector = CreateFuncInput("Vector", "VECTOR", new VecD(0, 0));
    }

    protected override string NodeUniqueName => "SeparateVecD";

    protected override Surface? OnExecute(RenderingContext context)
    {
        return null;
    }

    public override bool AreInputsLegal() => true;

    public override Node CreateCopy() => new SeparateVecDNode();
}
