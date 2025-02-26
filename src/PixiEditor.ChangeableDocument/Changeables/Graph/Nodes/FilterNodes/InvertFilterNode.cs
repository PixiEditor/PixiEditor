using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.FilterNodes;

[NodeInfo("InvertFilter")]
public class InvertFilterNode : FilterNode
{
    public InputProperty<double> Intensity { get; }
    private ColorFilter? filter;
    private ColorMatrix invertedMatrix;

    public InvertFilterNode()
    {
        Intensity = CreateInput("Intensity", "INTENSITY", 1.0)
            .WithRules(rules => rules.Min(0d).Max(1d));
        invertedMatrix = new ColorMatrix(new float[] { -1, 0, 0, 0, 1, 0, -1, 0, 0, 1, 0, 0, -1, 0, 1, 0, 0, 0, 1, 0 });

        filter = ColorFilter.CreateColorMatrix(invertedMatrix);
    }

    protected override ColorFilter? GetColorFilter()
    {
        filter?.Dispose();

        var lerped = ColorMatrix.Lerp(ColorMatrix.Identity, invertedMatrix, (float)Intensity.Value);
        filter = ColorFilter.CreateColorMatrix(lerped);

        return filter;
    }

    public override Node CreateCopy()
    {
        return new InvertFilterNode();
    }
}
