using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.CombineSeparate;

public class SeparateVecINode : Node
{
    public FuncInputProperty<VecI> Vector { get; }
    
    public FuncOutputProperty<int> X { get; }
    
    public FuncOutputProperty<int> Y { get; }
    
    public override string DisplayName { get; set; } = "SEPARATE_VECI_NODE";

    public SeparateVecINode()
    {
        X = CreateFieldOutput("X", "X", ctx => Vector.Value(ctx).X);
        Y = CreateFieldOutput("Y", "Y", ctx => Vector.Value(ctx).Y);
        Vector = CreateFuncInput("Vector", "VECTOR", new VecI(0, 0));
    }

    protected override string NodeUniqueName => "SeparateVecI";

    protected override Surface? OnExecute(RenderingContext context)
    {
        return null;
    }


    public override Node CreateCopy() => new SeparateVecINode();
}
