using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core.Surface.ImageData;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

public class ImageSpaceNode : Node
{
    public FieldOutputProperty<VecD> PositionField { get; }
    
    public FieldOutputProperty<VecI> Size { get; }

    public ImageSpaceNode()
    {
        PositionField = CreateFieldOutput(nameof(PositionField), "PIXEL_COORDINATE", ctx => ctx.Position);
        Size = CreateFieldOutput(nameof(Size), "SIZE", ctx => ctx.Size);
    }
    
    protected override Chunk? OnExecute(RenderingContext context)
    {
        return null;
    }

    public override bool Validate() => true;

    public override Node CreateCopy() => new ImageSpaceNode();
}
