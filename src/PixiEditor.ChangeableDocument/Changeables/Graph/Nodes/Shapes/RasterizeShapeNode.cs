using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes;

[NodeInfo("RasterizeShape")]
public class RasterizeShapeNode : RenderNode
{
    public InputProperty<ShapeVectorData> Data { get; }


    public RasterizeShapeNode()
    {
        Data = CreateInput<ShapeVectorData>("Points", "SHAPE", null);
    }

    protected override void OnPaint(RenderContext context, DrawingSurface surface)
    {
        var shape = Data.Value;

        if (shape == null || !shape.IsValid())
            return;
        
        shape.RasterizeTransformed(surface);
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
