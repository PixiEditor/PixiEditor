using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Shaders.Generation.Expressions;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.CombineSeparate;

[NodeInfo("SeparateVecI")]
public class SeparateVecINode : Node
{
    public FuncInputProperty<Int2, ShaderFuncContext> Vector { get; }
    
    public FuncOutputProperty<Int1, ShaderFuncContext> X { get; }
    
    public FuncOutputProperty<Int1, ShaderFuncContext> Y { get; }
    
    public SeparateVecINode()
    {
        X = CreateFuncOutput<Int1, ShaderFuncContext>("X", "X", ctx => ctx.GetValue(Vector).X);
        Y = CreateFuncOutput<Int1, ShaderFuncContext>("Y", "Y", ctx => ctx.GetValue(Vector).Y);
        Vector = CreateFuncInput<Int2, ShaderFuncContext>("Vector", "VECTOR", new VecI(0, 0));
    }

    protected override void OnExecute(RenderContext context)
    {
    }


    public override Node CreateCopy() => new SeparateVecINode();
}
