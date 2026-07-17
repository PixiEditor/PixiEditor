using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Shaders.Generation.Expressions;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo(UniqueName)]
public class ColorNode : Node
{
    public const string UniqueName = "Color";
    public const string InputColorPropertyName = "InputColor";
    public FuncInputProperty<Half4> InputColor { get; }
    public FuncOutputProperty<Half4> Color { get; }

    public ColorNode()
    {
        InputColor = CreateFuncInput<Half4>(InputColorPropertyName, "COLOR", new Half4(new Vec4D(1)));
        Color = CreateFuncOutput<Half4>("OutputColor", "COLOR", ctx => ctx.GetValue(InputColor));
    }
    
    protected override void OnExecute(RenderContext context)
    {
        
    }

    public override Node CreateCopy()
    {
        return new ColorNode();
    }
}
