using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Shaders.Generation.Expressions;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.CombineSeparate;

[NodeInfo("CombineVecI")]
public class CombineVecINode : Node
{
    public FuncOutputProperty<Int2> Vector { get; }
    
    public FuncInputProperty<Int1> X { get; }
    
    public FuncInputProperty<Int1> Y { get; }

    public CombineVecINode()
    {
        Vector = CreateFuncOutput(nameof(Vector), "VECTOR", GetVector);

        X = CreateFuncInput<Int1>(nameof(X), "X", 0);
        Y = CreateFuncInput<Int1>(nameof(Y), "Y", 0);
    }

    private Int2 GetVector(FuncContext ctx)
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
