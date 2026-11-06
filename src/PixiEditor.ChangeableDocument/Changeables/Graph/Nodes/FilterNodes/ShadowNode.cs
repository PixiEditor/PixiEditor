using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.FilterNodes;

[NodeInfo("Shadow")]
public class ShadowNode : FilterNode
{
    public const string OffsetPropertyName = "Offset";
    public const string SigmaPropertyName = "Radius";
    public const string ColorPropertyName = "Color";
    public InputProperty<VecD> Offset { get; }
    public InputProperty<VecD> Sigma { get; }
    public InputProperty<Color> Color { get; }

    private ImageFilter? lastFilter;

    private VecD lastOffset;
    private VecD lastSigma;
    private Color lastColor;

    // Shadow is displayed at different position than the layer (adds pixels outside), this makes iterative rendering not possible
    public override bool SupportsIterativeRendering => false;

    public ShadowNode()
    {
        Offset = CreateInput(OffsetPropertyName, "OFFSET", new VecD(5, 5));
        Sigma = CreateInput(SigmaPropertyName, "RADIUS", new VecD(5, 5));
        Color = CreateInput(ColorPropertyName, "COLOR", Colors.Black);
    }

    protected override ImageFilter? GetImageFilter(RenderContext context)
    {
        if(lastColor == Color.Value && lastOffset.AlmostEquals(Offset.Value) && lastSigma.AlmostEquals(Sigma.Value))
            return lastFilter;

        lastColor = Color.Value;
        lastOffset = Offset.Value;
        lastSigma = Sigma.Value;

        lastFilter?.Dispose();
        lastFilter = ImageFilter.CreateDropShadow((float)Offset.Value.X, (float)Offset.Value.Y, (float)Sigma.Value.X, (float)Sigma.Value.Y, Color.Value, null);
        return lastFilter;
    }

    public override Node CreateCopy()
    {
        return new ShadowNode();
    }

    public override void Dispose()
    {
        base.Dispose();
        lastFilter?.Dispose();
    }
}
