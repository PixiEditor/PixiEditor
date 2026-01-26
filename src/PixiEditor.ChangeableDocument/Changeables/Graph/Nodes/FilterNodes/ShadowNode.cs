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

    public ShadowNode()
    {
        Offset = CreateInput(OffsetPropertyName, "OFFSET", new VecD(5, 5));
        Sigma = CreateInput(SigmaPropertyName, "RADIUS", new VecD(5, 5));
        Color = CreateInput(ColorPropertyName, "COLOR", Colors.Black);
    }

    protected override ImageFilter? GetImageFilter(RenderContext context)
    {
        return ImageFilter.CreateDropShadow((float)Offset.Value.X, (float)Offset.Value.Y, (float)Sigma.Value.X, (float)Sigma.Value.Y, Color.Value, null);
    }

    public override Node CreateCopy()
    {
        return new ShadowNode();
    }
}
