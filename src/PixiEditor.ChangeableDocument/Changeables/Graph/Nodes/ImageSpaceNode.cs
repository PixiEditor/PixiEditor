using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

public class ImageSpaceNode : Node
{
    public FieldOutputProperty<VecD> Position { get; }
    
    public FieldOutputProperty<VecI> Size { get; }

    public ImageSpaceNode()
    {
        Position = CreateFieldOutput(nameof(Position), "PIXEL_COORDINATE", ctx => ctx.Position);
        Size = CreateFieldOutput(nameof(Size), "SIZE", ctx => ctx.Size);
    }
    
    protected override ChunkyImage? OnExecute(KeyFrameTime frameTime)
    {
        return null;
    }

    public override bool Validate() => true;

    public override Node CreateCopy() => new ImageSpaceNode();
}
