using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes;

public abstract class ShapeNode<T> : Node where T : ShapeVectorData
{
    public OutputProperty<T> Output { get; }
    
    public ShapeNode()
    {
        Output = CreateOutput<T>("Output", "OUTPUT", null);
    }
    
    private static readonly Paint rasterizePreviewPaint = new Paint();

    protected override void OnExecute(RenderContext context)
    {
        var data = GetShapeData(context);

        Output.Value = data;
        
        /*if (data == null || !data.IsValid())
            return;

        return RasterizePreview(data, context.DocumentSize);*/
    }
    
    protected abstract T? GetShapeData(RenderContext context);

    public Texture RasterizePreview(ShapeVectorData vectorData, VecI size)
    {
        Texture texture = RequestTexture(0, size);
        
        vectorData.RasterizeTransformed(texture.DrawingSurface);
        
        return texture;
    }
}
