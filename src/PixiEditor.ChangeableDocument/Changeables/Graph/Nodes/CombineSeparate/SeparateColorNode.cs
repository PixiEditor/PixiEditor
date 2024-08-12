using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Shaders.Generation.Expressions;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.CombineSeparate;

[NodeInfo("SeparateColor", "SEPARATE_COLOR_NODE")]
public class SeparateColorNode : Node
{
    public FuncInputProperty<Half4> Color { get; }
    
    public FuncOutputProperty<Float1> R { get; }
    
    public FuncOutputProperty<Float1> G { get; }
    
    public FuncOutputProperty<Float1> B { get; }
    
    public FuncOutputProperty<Float1> A { get; }
    
    
    private FuncContext lastContext;
    private Half4 lastColor;

    public SeparateColorNode()
    {
        Color = CreateFuncInput<Half4>(nameof(Color), "COLOR", new Color());
        R = CreateFuncOutput<Float1>(nameof(R), "R", ctx => GetColor(ctx).R);
        G = CreateFuncOutput<Float1>(nameof(G), "G", ctx => GetColor(ctx).G);
        B = CreateFuncOutput<Float1>(nameof(B), "B", ctx => GetColor(ctx).B);
        A = CreateFuncOutput<Float1>(nameof(A), "A", ctx => GetColor(ctx).A);
    }

    protected override Texture? OnExecute(RenderingContext context)
    {
        return null;
    }
    
    private Half4 GetColor(FuncContext ctx)
    {
        Half4 target = null;
        if (lastContext == ctx)
        {
            target = lastColor;
        }
        else
        {
            target = Color.Value(ctx);
        }
        
        lastColor = target;
        lastContext = ctx;
        return lastColor;
    }

    public override Node CreateCopy() => new SeparateColorNode();
}
