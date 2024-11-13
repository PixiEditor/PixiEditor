using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Helpers;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.FilterNodes;

[NodeInfo("ApplyFilter")]
public class ApplyFilterNode : RenderNode, IRenderInput
{
    public InputProperty<Filter?> Filter { get; }

    public RenderInputProperty Background { get; }

    public ApplyFilterNode()
    {
        Background = CreateRenderInput("Input", "IMAGE");
        Filter = CreateInput<Filter>("Filter", "FILTER", null);
        Output.FirstInChain = null;
    }

    protected override void OnPaint(RenderContext context, DrawingSurface surface)
    {
        if (Background.Value == null)
            return;

        Background.Value.Paint(context, surface);
        Filter.Value?.Apply(surface);
    }

    public override RectD? GetPreviewBounds(int frame, string elementToRenderName = "")
    {
        return PreviewUtils.FindPreviewBounds(Background.Connection, frame, elementToRenderName); 
    }

    public override bool RenderPreview(DrawingSurface renderOn, ChunkResolution resolution, int frame,
        string elementToRenderName)
    {
        if(Background.Value == null)
            return false;

        RenderContext context = new(renderOn, frame, ChunkResolution.Full, VecI.One);
        
        int layer = renderOn.Canvas.SaveLayer();
        Background.Value.Paint(context, renderOn);
        renderOn.Canvas.RestoreToCount(layer);

        return true;
    }

    public override Node CreateCopy() => new ApplyFilterNode();
}
