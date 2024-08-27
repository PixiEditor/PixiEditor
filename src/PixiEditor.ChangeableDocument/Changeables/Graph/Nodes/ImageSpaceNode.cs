using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("ImageSpace", "IMAGE_SPACE_NODE", Category = "IMAGE")]
public class ImageSpaceNode : Node
{
    public FuncOutputProperty<VecD> SpacePosition { get; }
    
    public FuncOutputProperty<VecI> Size { get; }

    public ImageSpaceNode()
    {
        // TODO: Implement this
        //SpacePosition = CreateFuncOutput(nameof(SpacePosition), "UV", ctx => ctx.Position);
        Size = CreateFuncOutput(nameof(Size), "SIZE", ctx => ctx.Size);
    }


    protected override Texture? OnExecute(RenderingContext context)
    {
        return null;
    }


    public override Node CreateCopy() => new ImageSpaceNode();
}
