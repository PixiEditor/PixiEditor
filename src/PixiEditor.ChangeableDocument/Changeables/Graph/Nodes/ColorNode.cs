using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Shaders.Generation.Expressions;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("Color")]
public class ColorNode : Node
{
    public FuncInputProperty<Half4, ShaderFuncContext> InputColor { get; }
    public FuncOutputProperty<Half4, ShaderFuncContext> Color { get; }
    
    public ColorNode()
    {
        InputColor = CreateFuncInput<Half4, ShaderFuncContext>("InputColor", "COLOR", Colors.White);
        Color = CreateFuncOutput<Half4, ShaderFuncContext>("OutputColor", "COLOR", ctx => ctx.GetValue(InputColor));
    }
    
    protected override void OnExecute(RenderContext context)
    {
        
    }

    public override Node CreateCopy()
    {
        return new ColorNode();
    }
}
