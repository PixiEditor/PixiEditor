using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.FilterNodes;

[NodeInfo("InvertFilter")]
public class InvertFilterNode : FilterNode
{
    public static ColorMatrix InvertedMatrix { get; } = new ColorMatrix([-1, 0, 0, 0, 1, 0, -1, 0, 0, 1, 0, 0, -1, 0, 1, 0, 0, 0, 1, 0]);
    public InputProperty<double> Intensity { get; }
    private ColorFilter? filter;

    public InvertFilterNode()
    {
        Intensity = CreateInput("Intensity", "INTENSITY", 1.0)
            .WithRules(rules => rules.Min(0d).Max(1d));

        filter = ColorFilter.CreateColorMatrix(InvertedMatrix);
    }

    protected override ColorFilter? GetColorFilter(RenderContext context)
    {
        filter?.Dispose();

        var lerped = ColorMatrix.Lerp(ColorMatrix.Identity, InvertedMatrix, (float)Intensity.Value);
        filter = ColorFilter.CreateColorMatrix(lerped);

        return filter;
    }

    public override Node CreateCopy()
    {
        return new InvertFilterNode();
    }

    public override void Dispose()
    {
        base.Dispose();
        filter?.Dispose();
    }
}
