using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Shaders.Generation.Expressions;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.CombineSeparate;

[NodeInfo("CombineVecI")]
public class CombineVecINode : Node
{
    public FuncOutputProperty<Int2, ShaderFuncContext> Vector { get; }
    
    public FuncInputProperty<Int1, ShaderFuncContext> X { get; }
    
    public FuncInputProperty<Int1, ShaderFuncContext> Y { get; }

    public CombineVecINode()
    {
        Vector = CreateFuncOutput<Int2, ShaderFuncContext>(nameof(Vector), "VECTOR", GetVector);

        X = CreateFuncInput<Int1, ShaderFuncContext>(nameof(X), "X", 0);
        Y = CreateFuncInput<Int1, ShaderFuncContext>(nameof(Y), "Y", 0);
    }

    private Int2 GetVector(ShaderFuncContext ctx)
    {
        var x = ctx.GetValue(X);
        var y = ctx.GetValue(Y);

        return ctx.NewInt2(x, y);
    }


    protected override void OnExecute(RenderContext context)
    {
    }


    public override Node CreateCopy() => new CombineVecINode();
}
