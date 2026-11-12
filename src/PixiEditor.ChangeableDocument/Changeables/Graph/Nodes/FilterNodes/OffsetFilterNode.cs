using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Matrix;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.FilterNodes;

[NodeInfo("OffsetFilter")]
public class OffsetFilterNode : FilterNode
{
    public const string OffsetPropertyName = "Offset";
    public InputProperty<VecD> Offset { get; }

    private ImageFilter? lastFilter;
    private VecD lastOffset;

    public override bool SupportsIterativeRendering => false;

    public OffsetFilterNode()
    {
        Offset = CreateInput(OffsetPropertyName, "OFFSET", VecD.Zero);
    }

    protected override ImageFilter? GetImageFilter(RenderContext context)
    {
        if (lastOffset.AlmostEquals(Offset.Value))
            return lastFilter;

        lastFilter?.Dispose();

        lastOffset = Offset.Value;
        lastFilter = ImageFilter.CreateOffset((float)Offset.Value.X, (float)Offset.Value.Y);

        return lastFilter;
    }

    public override Node CreateCopy() => new OffsetFilterNode();

    public override void Dispose()
    {
        base.Dispose();
        lastFilter?.Dispose();
    }
}
