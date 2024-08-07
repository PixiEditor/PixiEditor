using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.CombineSeparate;

[NodeInfo("SeparateVecD", "SEPARATE_VECD_NODE")]
public class SeparateVecDNode : Node
{
    public FuncInputProperty<VecD> Vector { get; }
    
    public FuncOutputProperty<double> X { get; }
    
    public FuncOutputProperty<double> Y { get; }
    
    public SeparateVecDNode()
    {
        X = CreateFuncOutput("X", "X", ctx => Vector.Value(ctx).X);
        Y = CreateFuncOutput("Y", "Y", ctx => Vector.Value(ctx).Y);
        Vector = CreateFuncInput("Vector", "VECTOR", new VecD(0, 0));
    }


    protected override Surface? OnExecute(RenderingContext context)
    {
        return null;
    }


    public override Node CreateCopy() => new SeparateVecDNode();
}
