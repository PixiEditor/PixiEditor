using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Shaders.Generation.Expressions;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("Lerp")]
public class LerpColorNode : Node // TODO: ILerpable as inputs? 
{
    public FuncOutputProperty<Half4> Result { get; } 
    public FuncInputProperty<Half4> From { get; }
    public FuncInputProperty<Half4> To { get; }
    public FuncInputProperty<Float1> Time { get; }
    
    public LerpColorNode()
    {
        Result = CreateFuncOutput<Half4>("Result", "RESULT", Lerp);
        From = CreateFuncInput<Half4>("From", "FROM", Colors.Black);
        To = CreateFuncInput<Half4>("To", "TO", Colors.White);
        Time = CreateFuncInput<Float1>("Time", "TIME", 0.5);
    }

    private Half4 Lerp(FuncContext arg)
    {
        var from = arg.GetValue(From);
        var to = arg.GetValue(To);
        var time = arg.GetValue(Time);
        
        return arg.NewHalf4(ShaderMath.Lerp(from, to, time)); 
    }

    protected override void OnExecute(RenderContext context)
    {
    }

    public override Node CreateCopy()
    {
        return new LerpColorNode();
    }
}
