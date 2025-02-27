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

    protected Paint maskPaint = new Paint()
    {
        BlendMode = Drawie.Backend.Core.Surfaces.BlendMode.DstIn, ColorFilter = Nodes.Filters.MaskFilter
    };

    public MaskNode()
    {
        Background = CreateRenderInput("Background", "INPUT");
        Mask = CreateRenderInput("Mask", "MASK");
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

        int layer = surface.Canvas.SaveLayer(maskPaint);
        Mask.Value.Paint(context, surface);
        surface.Canvas.RestoreToCount(layer);
    }


    public override Node CreateCopy()
    {
        return new MaskNode();
    }
}
