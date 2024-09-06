using ChunkyImageLib.Operations;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("VectorLayer")]
public class VectorLayerNode : LayerNode, ITransformableObject
{
    public Matrix3X3 TransformationMatrix
    {
        get => ShapeData.TransformationMatrix;
        set => ShapeData.TransformationMatrix = value;
    }
    
    public ShapeVectorData ShapeData { get; } = new EllipseVectorData(new VecI(32), new VecI(32));
    
    private int lastCacheHash;
    
    protected override Texture? OnExecute(RenderingContext context)
    {
        Texture texture = RequestTexture(0, context.DocumentSize);
        ShapeData.Rasterize(texture.DrawingSurface);
        
        Output.Value = texture;
        
        return texture;
    }

    protected override bool CacheChanged(RenderingContext context)
    {
        return base.CacheChanged(context) || ShapeData.GetCacheHash() != lastCacheHash;
    }

    protected override void UpdateCache(RenderingContext context)
    {
        base.UpdateCache(context);
        lastCacheHash = ShapeData.GetCacheHash();
    }

    public override RectI? GetTightBounds(KeyFrameTime frameTime)
    {
        return (RectI)ShapeData.GeometryAABB;
    }

    public override ShapeCorners GetTransformationCorners(KeyFrameTime frameTime)
    {
        return ShapeData.TransformationCorners;
    }

    public override Node CreateCopy()
    {
        return new VectorLayerNode();
    }

}
