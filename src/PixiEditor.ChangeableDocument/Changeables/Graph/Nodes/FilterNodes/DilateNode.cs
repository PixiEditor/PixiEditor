using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.FilterNodes;

[NodeInfo("Dilate")]
public class DilateNode : FilterNode
{
    public const string RadiusPropertyName = "Radius";
    public InputProperty<VecD> Radius { get; }

    private ImageFilter? lastFilter;
    private VecD lastRadius;

    public DilateNode()
    {
        Radius = CreateInput(RadiusPropertyName, "RADIUS", new VecD(1f, 1f))
            .WithRules(rules => rules.Min(VecD.Zero));
    }

    protected override ImageFilter? GetImageFilter(RenderContext context)
    {
        if (lastRadius.AlmostEquals(Radius.Value))
        {
            return lastFilter;
        }

        lastFilter?.Dispose();
        lastFilter = ImageFilter.CreateDilate((float)Radius.Value.X, (float)Radius.Value.Y);
        lastRadius = Radius.Value;
        return lastFilter;
    }

    public override Node CreateCopy()
    {
        return new DilateNode();
    }

    public override void Dispose()
    {
        base.Dispose();
        lastFilter?.Dispose();
    }
}
