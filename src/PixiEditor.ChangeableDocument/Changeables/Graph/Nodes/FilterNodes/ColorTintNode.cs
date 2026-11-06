using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.FilterNodes;

[NodeInfo("ColorTint")]
public class ColorTintFilterNode : FilterNode
{
    public InputProperty<Color> Tint { get; }

    private ColorFilter? lastColorFilter;
    private Color lastTint;

    public ColorTintFilterNode()
    {
        Tint = CreateInput("Tint", "TINT_PROPERTY", Colors.White);
    }

    protected override ColorFilter? GetColorFilter(RenderContext context)
    {
        if (lastTint == Tint.Value)
        {
            return lastColorFilter;
        }

        lastColorFilter = ColorFilter.CreateBlendMode(Tint.Value, BlendMode.SrcIn);

        return lastColorFilter;
    }

    public override Node CreateCopy()
    {
        return new ColorTintFilterNode();
    }
}
