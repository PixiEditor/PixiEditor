using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Helpers;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.FilterNodes;

[NodeInfo("ApplyFilter")]
public class ApplyFilterNode : RenderNode, IRenderInput
{
    private Paint _paint = new();
    public InputProperty<Filter?> Filter { get; }

    public RenderInputProperty Background { get; }

    public ApplyFilterNode()
    {
        Background = CreateRenderInput("Input", "IMAGE");
        Filter = CreateInput<Filter>("Filter", "FILTER", null);
    }

    protected override void OnPaint(RenderContext context, DrawingSurface surface)
    {
        _paint.SetFilters(Filter.Value);
        var layer = surface.Canvas.SaveLayer(_paint);
        
        Background.Value.Paint(context, surface);
        
        surface.Canvas.RestoreToCount(layer);
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

        using RenderContext context = new(renderOn, frame, ChunkResolution.Full, VecI.One);
        
        int layer = renderOn.Canvas.SaveLayer(_paint);
        Background.Value.Paint(context, renderOn);
        renderOn.Canvas.RestoreToCount(layer);

        return true;
    }

    public override Node CreateCopy() => new ApplyFilterNode();
}
