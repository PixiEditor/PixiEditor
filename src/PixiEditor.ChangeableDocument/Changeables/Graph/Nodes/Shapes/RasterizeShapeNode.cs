using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes;

[NodeInfo("RasterizeShape")]
public class RasterizeShapeNode : Node
{
    public OutputProperty<Texture> Image { get; }

    public InputProperty<ShapeVectorData> Data { get; }


    protected override bool AffectedByChunkResolution => true;

    private Paint rasterizePaint = new Paint();

    public RasterizeShapeNode()
    {
        Image = CreateOutput<Texture>("Image", "IMAGE", null);
        Data = CreateInput<ShapeVectorData>("Points", "SHAPE", null);
    }

    protected override void OnExecute(RenderContext context)
    {
        var shape = Data.Value;

        if (shape == null || !shape.IsValid())
            return;

        var size = context.DocumentSize;
        var image = RequestTexture(0, size);

        image.DrawingSurface.Canvas.Save();

        shape.RasterizeTransformed(image.DrawingSurface, context.ChunkResolution, rasterizePaint);
        
        image.DrawingSurface.Canvas.Restore();

        Image.Value = image;
        
        return;
    }

    public override Node CreateCopy() => new RasterizeShapeNode();
}
