using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.CombineSeparate;

[NodeInfo("SeparateVecI", "SEPARATE_VECI_NODE")]
public class SeparateVecINode : Node
{
    public FuncInputProperty<VecI> Vector { get; }
    
    public FuncOutputProperty<int> X { get; }
    
    public FuncOutputProperty<int> Y { get; }
    
    public SeparateVecINode()
    {
        X = CreateFuncOutput("X", "X", ctx => Vector.Value(ctx).X);
        Y = CreateFuncOutput("Y", "Y", ctx => Vector.Value(ctx).Y);
        Vector = CreateFuncInput("Vector", "VECTOR", new VecI(0, 0));
    }

    protected override Surface? OnExecute(RenderingContext context)
    {
        return null;
    }


    public override Node CreateCopy() => new SeparateVecINode();
}
