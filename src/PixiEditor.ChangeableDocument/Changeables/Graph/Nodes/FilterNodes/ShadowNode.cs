using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.FilterNodes;

[NodeInfo("Shadow")]
public class ShadowNode : FilterNode
{
    public InputProperty<VecD> Offset { get; }
    public InputProperty<VecD> Sigma { get; }
    public InputProperty<Color> Color { get; }

    public ShadowNode()
    {
        Offset = CreateInput("Offset", "OFFSET", new VecD(5, 5));
        Sigma = CreateInput("Radius", "RADIUS", new VecD(5, 5));
        Color = CreateInput("Color", "COLOR", Colors.Black);
    }

    protected override ImageFilter? GetImageFilter()
    {
        return ImageFilter.CreateDropShadow((float)Offset.Value.X, (float)Offset.Value.Y, (float)Sigma.Value.X, (float)Sigma.Value.Y, Color.Value, null);
    }

    public override Node CreateCopy()
    {
        return new ShadowNode();
    }
}
