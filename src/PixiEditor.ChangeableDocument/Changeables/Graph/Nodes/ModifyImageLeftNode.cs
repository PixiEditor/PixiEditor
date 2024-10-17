using System.Collections.Concurrent;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Shaders;
using PixiEditor.DrawingApi.Core.Shaders.Generation;
using PixiEditor.DrawingApi.Core.Shaders.Generation.Expressions;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("ModifyImageLeft")]
[PairNode(typeof(ModifyImageRightNode), "ModifyImageZone", true)]
public class ModifyImageLeftNode : Node, IPairNode
{
    public RenderInputProperty Image { get; }

    public FuncOutputProperty<Float2> Coordinate { get; }

    public FuncOutputProperty<Half4> Color { get; }

    public Guid OtherNode { get; set; }
    
    public Texture InputTexture { get; private set; }

    public ModifyImageLeftNode()
    {
        Image = CreateRenderInput("Surface", "IMAGE");
        Coordinate = CreateFuncOutput("Coordinate", "UV", ctx => ctx.OriginalPosition);
        Color = CreateFuncOutput("Color", "COLOR", GetColor);
    }
    
    private Half4 GetColor(FuncContext context)
    {
        context.ThrowOnMissingContext();

        return context.SampleSurface(InputTexture.DrawingSurface, context.SamplePosition);
    }

    protected override void OnExecute(RenderContext context)
    {
        InputTexture = RequestTexture(0, context.DocumentSize);
        Image.Value.Paint(context, InputTexture.DrawingSurface);
    }

    public override Node CreateCopy() => new ModifyImageLeftNode();
}
