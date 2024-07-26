using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.ColorsImpl;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.CombineSeparate;

[NodeInfo("SeparateColor")]
public class SeparateColorNode : Node
{
    public FuncInputProperty<Color> Color { get; }
    
    public FuncOutputProperty<double> R { get; }
    
    public FuncOutputProperty<double> G { get; }
    
    public FuncOutputProperty<double> B { get; }
    
    public FuncOutputProperty<double> A { get; }
    
    public override string DisplayName { get; set; } = "SEPARATE_COLOR_NODE";

    public SeparateColorNode()
    {
        Color = CreateFuncInput(nameof(Color), "COLOR", new Color());
        R = CreateFuncOutput(nameof(R), "R", ctx => Color.Value(ctx).R / 255d);
        G = CreateFuncOutput(nameof(G), "G", ctx => Color.Value(ctx).G / 255d);
        B = CreateFuncOutput(nameof(B), "B", ctx => Color.Value(ctx).B / 255d);
        A = CreateFuncOutput(nameof(A), "A", ctx => Color.Value(ctx).A / 255d);
    }

    protected override Surface? OnExecute(RenderingContext context)
    {
        return null;
    }


    public override Node CreateCopy() => new SeparateColorNode();
}
