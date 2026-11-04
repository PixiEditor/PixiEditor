using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.FilterNodes;

[NodeInfo("Erode")]
public class ErodeNode : FilterNode
{
    public InputProperty<VecD> Radius { get; }

    public ErodeNode()
    {
        Radius = CreateInput("Radius", "RADIUS", new VecD(1f, 1f))
            .WithRules(rules => rules.Min(VecD.Zero));
    }

    protected override ImageFilter? GetImageFilter(RenderContext context)
    {
        return ImageFilter.CreateErode((float)Radius.Value.X, (float)Radius.Value.Y);
    }

    public override Node CreateCopy()
    {
        return new ErodeNode();
    }
}
