using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Shaders.Generation.Expressions;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.CombineSeparate;

[NodeInfo("SeparateVecI")]
public class SeparateVecINode : Node
{
    public FuncInputProperty<Int2> Vector { get; }
    
    public FuncOutputProperty<Int1> X { get; }
    
    public FuncOutputProperty<Int1> Y { get; }
    
    public SeparateVecINode()
    {
        X = CreateFuncOutput<Int1>("X", "X", ctx => ctx.GetValue(Vector).X);
        Y = CreateFuncOutput<Int1>("Y", "Y", ctx => ctx.GetValue(Vector).Y);
        Vector = CreateFuncInput<Int2>("Vector", "VECTOR", new VecI(0, 0));
    }

    protected override void OnExecute(RenderContext context)
    {
    }


    public override Node CreateCopy() => new SeparateVecINode();
}
