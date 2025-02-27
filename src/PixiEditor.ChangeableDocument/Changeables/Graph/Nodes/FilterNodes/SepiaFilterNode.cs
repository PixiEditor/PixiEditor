using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.FilterNodes;

[NodeInfo("Sepia")]
public class SepiaFilterNode : FilterNode
{
    public InputProperty<double> Intensity { get; }

    private ColorMatrix sepiaMatrix;

    protected override bool ExecuteOnlyOnCacheChange => true;
    protected override CacheTriggerFlags CacheTrigger => CacheTriggerFlags.Inputs;

    private ColorFilter lastFilter;

    public SepiaFilterNode()
    {
        Intensity = CreateInput("Intensity", "INTENSITY", 1d)
            .WithRules(rules => rules.Min(0d).Max(1d));

        sepiaMatrix = new ColorMatrix(
            [
                0.393f, 0.769f, 0.189f, 0.0f, 0.0f,
                0.349f, 0.686f, 0.168f, 0.0f, 0.0f,
                0.272f, 0.534f, 0.131f, 0.0f, 0.0f,
                0.0f, 0.0f, 0.0f, 1.0f, 0.0f
            ]
        );
    }

    protected override ColorFilter? GetColorFilter()
    {
        lastFilter?.Dispose();

        var lerped = ColorMatrix.Lerp(ColorMatrix.Identity, sepiaMatrix, (float)Intensity.Value);
        lastFilter = ColorFilter.CreateColorMatrix(lerped);

        return lastFilter;
    }

    public override Node CreateCopy()
    {
        return new SepiaFilterNode();
    }
}
