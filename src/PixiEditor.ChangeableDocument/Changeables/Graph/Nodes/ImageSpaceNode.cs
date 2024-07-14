using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

public class ImageSpaceNode : Node
{
    public OutputProperty<Func<IFieldContext, VecD>> Position { get; }

    public ImageSpaceNode()
    {
        Position = CreateOutput<Func<IFieldContext, VecD>>(nameof(Position), "PIXEL_COORDINATE", ctx => ctx.Position);
    }
    
    protected override ChunkyImage? OnExecute(KeyFrameTime frameTime)
    {
        return null;
    }

    public override bool Validate() => true;

    public override Node CreateCopy() => new ImageSpaceNode();
}
