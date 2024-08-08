using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.Shaders.Generation.Expressions;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.CombineSeparate;

[NodeInfo("SeparateVecD")]
public class SeparateVecDNode : Node
{
    public FuncInputProperty<Float2> Vector { get; }
    
    public FuncOutputProperty<Float1> X { get; }
    
    public FuncOutputProperty<Float1> Y { get; }
    
    public override string DisplayName { get; set; } = "SEPARATE_VECD_NODE";

    public SeparateVecDNode()
    {
        X = CreateFuncOutput("X", "X", ctx => Vector.Value(ctx).X);
        Y = CreateFuncOutput("Y", "Y", ctx => Vector.Value(ctx).Y);
        Vector = CreateFuncInput<Float2>("Vector", "VECTOR", VecD.Zero);
    }

    protected override Texture? OnExecute(RenderingContext context)
    {
        return null;
    }


    public override Node CreateCopy() => new SeparateVecDNode();
}
