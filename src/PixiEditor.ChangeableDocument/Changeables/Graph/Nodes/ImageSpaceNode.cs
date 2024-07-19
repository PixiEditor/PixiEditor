using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core.Surface.ImageData;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

public class ImageSpaceNode : Node
{
    public FuncOutputProperty<VecD> SpacePosition { get; }
    
    public FuncOutputProperty<VecI> Size { get; }

    public override string DisplayName { get; set; } = "IMAGE_SPACE_NODE";
    public ImageSpaceNode()
    {
        SpacePosition = CreateFieldOutput(nameof(SpacePosition), "PIXEL_COORDINATE", ctx => ctx.Position);
        Size = CreateFieldOutput(nameof(Size), "SIZE", ctx => ctx.Size);
    }

    protected override string NodeUniqueName => "ImageSpace";

    protected override Surface? OnExecute(RenderingContext context)
    {
        return null;
    }

    public override bool Validate() => true;

    public override Node CreateCopy() => new ImageSpaceNode();
}
