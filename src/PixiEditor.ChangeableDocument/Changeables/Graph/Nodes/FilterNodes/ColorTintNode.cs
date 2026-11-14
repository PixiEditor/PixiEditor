using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.FilterNodes;

[NodeInfo("ColorTint")]
public class ColorOverlayFilterNode : FilterNode
{
    public InputProperty<Color> Color { get; }
    public InputProperty<BlendMode> BlendMode { get; }

    private ColorFilter? lastColorFilter;
    private Color lastColor;
    private BlendMode lastBlendMode;

    public ColorOverlayFilterNode()
    {
        Color = CreateInput("Color", "COLOR", Colors.White);
        BlendMode = CreateInput("BlendMode", "BLEND_MODE", Drawie.Backend.Core.Surfaces.BlendMode.SrcIn);
    }

    protected override ColorFilter? GetColorFilter(RenderContext context)
    {
        if (lastColor == Color.Value && lastBlendMode == BlendMode.Value)
        {
            return lastColorFilter;
        }

        lastColorFilter = ColorFilter.CreateBlendMode(Color.Value, BlendMode.Value);
        lastColor = Color.Value;
        lastBlendMode = BlendMode.Value;

        return lastColorFilter;
    }

    public override Node CreateCopy()
    {
        return new ColorOverlayFilterNode();
    }
}
