using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("Lerp")]
public class LerpColorNode : Node // TODO: ILerpable as inputs? 
{
    public FuncOutputProperty<Color> Result { get; } 
    public FuncInputProperty<Color> From { get; }
    public FuncInputProperty<Color> To { get; }
    public FuncInputProperty<double> Time { get; }
    
    public override string DisplayName { get; set; } = "LERP_NODE";
    
    
    public LerpColorNode()
    {
        Result = CreateFuncOutput("Result", "RESULT", Lerp);
        From = CreateFuncInput("From", "FROM", Colors.Black);
        To = CreateFuncInput("To", "TO", Colors.White);
        Time = CreateFuncInput("Time", "TIME", 0.5);
    }

    private Color Lerp(FuncContext arg)
    {
        var from = From.Value(arg);
        var to = To.Value(arg);
        var time = Time.Value(arg);
        
        return Color.Lerp(from, to, time); 
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
