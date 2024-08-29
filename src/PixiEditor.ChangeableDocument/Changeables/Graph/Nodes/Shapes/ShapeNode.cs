using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.Numerics;
using ShapeData = PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data.ShapeData;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes;

public abstract class ShapeNode : Node
{
    public OutputProperty<ShapeData> Output { get; }
    private const int PreviewSize = 150;
    
    public ShapeNode()
    {
        Output = CreateOutput<ShapeData>("Output", "OUTPUT", null);
    }

    protected override Texture? OnExecute(RenderingContext context)
    {
        var data = GetShapeData(context);

        Output.Value = data;
        
        if (data == null || !data.IsValid())
            return null;

        return RasterizePreview(data);
    }
    
    protected abstract ShapeData? GetShapeData(RenderingContext context);

    public Texture RasterizePreview(ShapeData data)
    {
        Texture texture = RequestTexture(0, new VecI(PreviewSize, PreviewSize));
        
        data.Rasterize(texture.DrawingSurface);
        
        return texture;
    }
}
