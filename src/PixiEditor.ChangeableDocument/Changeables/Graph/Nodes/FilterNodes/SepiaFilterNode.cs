using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.FilterNodes;

[NodeInfo("Sepia")]
public class SepiaFilterNode : FilterNode
{
    public static ColorMatrix SepiaMatrix { get; } = new ColorMatrix(
    [
        0.393f, 0.769f, 0.189f, 0.0f, 0.0f,
        0.349f, 0.686f, 0.168f, 0.0f, 0.0f,
        0.272f, 0.534f, 0.131f, 0.0f, 0.0f,
        0.0f, 0.0f, 0.0f, 1.0f, 0.0f
    ]);

    public InputProperty<double> Intensity { get; }

    protected override bool ExecuteOnlyOnCacheChange => true;
    protected override CacheTriggerFlags CacheTrigger => CacheTriggerFlags.Inputs;

    private ColorFilter lastFilter;

    public SepiaFilterNode()
    {
        Intensity = CreateInput("Intensity", "INTENSITY", 1d)
            .WithRules(rules => rules.Min(0d).Max(1d));
    }

    protected override ColorFilter? GetColorFilter(RenderContext context)
    {
        lastFilter?.Dispose();

        var lerped = ColorMatrix.Lerp(ColorMatrix.Identity, SepiaMatrix, (float)Intensity.Value);
        lastFilter = ColorFilter.CreateColorMatrix(lerped);

        return lastFilter;
    }

    public override Node CreateCopy()
    {
        return new SepiaFilterNode();
    }

    public override void Dispose()
    {
        base.Dispose();
        lastFilter?.Dispose();
    }
}
