using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes;

[NodeInfo("RasterizeShape")]
public class RasterizeShapeNode : RenderNode
{
    public InputProperty<ShapeVectorData> Data { get; }


    private Paint rasterizePaint = new Paint();

    public RasterizeShapeNode()
    {
        Data = CreateInput<ShapeVectorData>("Points", "SHAPE", null);
    }

    protected override DrawingSurface? ExecuteRender(RenderContext context)
    {
        var shape = Data.Value;

        if (shape == null || !shape.IsValid())
            return null;

        var surface = context.RenderSurface;
        
        shape.RasterizeTransformed(surface);
        return surface;
    }

    public override Node CreateCopy() => new RasterizeShapeNode();
    public override RectD? GetPreviewBounds(int frame, string elementToRenderName = "")
    {
        return Data?.Value?.TransformedAABB;
    }

    public override bool RenderPreview(DrawingSurface renderOn, ChunkResolution resolution, int frame, string elementToRenderName)
    {
        var shape = Data.Value;

        if (shape == null || !shape.IsValid())
            return false;

        shape.RasterizeTransformed(renderOn);

        return true;
    }
}
