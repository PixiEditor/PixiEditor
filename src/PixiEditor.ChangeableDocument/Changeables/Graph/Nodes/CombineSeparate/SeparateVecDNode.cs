using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Shaders.Generation.Expressions;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.CombineSeparate;

[NodeInfo("SeparateVecD")]
public class SeparateVecDNode : Node
{
    public FuncInputProperty<Float2> Vector { get; }
    
    public FuncOutputProperty<Float1> X { get; }
    
    public FuncOutputProperty<Float1> Y { get; }
    
    public SeparateVecDNode()
    {
        X = CreateFuncOutput<Float1>("X", "X", ctx => ctx.GetValue(Vector).X);
        Y = CreateFuncOutput("Y", "Y", ctx => ctx.GetValue(Vector).Y);
        Vector = CreateFuncInput<Float2>("Vector", "VECTOR", VecD.Zero);
    }

    protected override void OnExecute(RenderContext context)
    {
    }


    public override Node CreateCopy() => new SeparateVecDNode();
}
