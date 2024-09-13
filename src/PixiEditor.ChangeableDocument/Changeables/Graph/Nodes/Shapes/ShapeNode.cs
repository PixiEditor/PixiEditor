using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes;

public abstract class ShapeNode<T> : Node where T : ShapeVectorData
{
    public OutputProperty<T> Output { get; }
    
    public ShapeNode()
    {
        Output = CreateOutput<T>("Output", "OUTPUT", null);
    }

    protected override Texture? OnExecute(RenderingContext context)
    {
        var data = GetShapeData(context);

        Output.Value = data;
        
        if (data == null || !data.IsValid())
            return null;

        return RasterizePreview(data, context.DocumentSize);
    }
    
    protected abstract T? GetShapeData(RenderingContext context);

    public Texture RasterizePreview(ShapeVectorData vectorData, VecI size)
    {
        Texture texture = RequestTexture(0, size);
        
        vectorData.Rasterize(texture.DrawingSurface, ChunkResolution.Full, null);
        
        return texture;
    }
}
