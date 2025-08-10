using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Image;

[NodeInfo("Mask")]
public class MaskNode : RenderNode, IRenderInput
{
    public RenderInputProperty Background { get; }
    public RenderInputProperty Mask { get; }
    public InputProperty<bool> Invert { get; }

    protected Paint maskPaint = new()
    {
        BlendMode = BlendMode.DstIn,
        ColorFilter = Filters.MaskFilter
    };

    public MaskNode()
    {
        Background = CreateRenderInput("Background", "INPUT");
        Mask = CreateRenderInput("Mask", "MASK");
        Invert = CreateInput("Invert", "INVERT", false);
        AllowHighDpiRendering = true;
        Output.FirstInChain = null;
    }

    protected override void OnPaint(RenderContext context, DrawingSurface surface)
    {
        if (Background.Value == null)
        {
            return;
        }

        Background.Value.Paint(context, surface);

        if (Mask.Value == null)
        {
            return;
        }

        maskPaint.BlendMode = !Invert.Value ? BlendMode.DstIn : BlendMode.DstOut;

        int layer = surface.Canvas.SaveLayer(maskPaint);
        Mask.Value.Paint(context, surface);
        surface.Canvas.RestoreToCount(layer);
    }

    public override Node CreateCopy()
    {
        return new MaskNode();
    }
}
