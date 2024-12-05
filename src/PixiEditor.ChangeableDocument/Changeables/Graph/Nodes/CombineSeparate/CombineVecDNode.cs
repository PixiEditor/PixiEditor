using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Shaders.Generation.Expressions;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.CombineSeparate;

[NodeInfo("CombineVecD")]
public class CombineVecDNode : Node
{
    public FuncOutputProperty<Float2> Vector { get; }
    
    public FuncInputProperty<Float1> X { get; }
    
    public FuncInputProperty<Float1> Y { get; }
    
    public CombineVecDNode()
    {
        Vector = CreateFuncOutput(nameof(Vector), "VECTOR", GetVector);

        X = CreateFuncInput<Float1>(nameof(X), "X", 0);
        Y = CreateFuncInput<Float1>(nameof(Y), "Y", 0);
    }
    
    private Float2 GetVector(FuncContext ctx)
    {
        var x = ctx.GetValue(X); 
        var y = ctx.GetValue(Y);

        return ctx.NewFloat2(x, y); 
    }

    protected override void OnExecute(RenderContext context)
    {
        
    }


    public override Node CreateCopy() => new CombineVecDNode();
}
