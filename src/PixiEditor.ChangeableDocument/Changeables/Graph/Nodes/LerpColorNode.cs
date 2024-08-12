using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Shaders.Generation.Expressions;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("Lerp", "LERP_NODE")]
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
        var from = From.Value(arg);
        var to = To.Value(arg);
        var time = Time.Value(arg);
        
        return arg.NewHalf4(ShaderMath.Lerp(from, to, time)); 
    }

    protected override Texture? OnExecute(RenderingContext context)
    {
        return null;
    }

    public override Node CreateCopy()
    {
        return new LerpColorNode();
    }
}
