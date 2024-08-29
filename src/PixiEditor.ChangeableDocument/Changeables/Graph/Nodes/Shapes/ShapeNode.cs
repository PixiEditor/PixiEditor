using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.Numerics;
using ShapeData = PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data.ShapeData;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes;

public abstract class ShapeNode<T> : Node where T : ShapeData
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

    public Texture RasterizePreview(ShapeData data, VecI size)
    {
        Texture texture = RequestTexture(0, size);
        
        data.Rasterize(texture.DrawingSurface);
        
        return texture;
    }
}
