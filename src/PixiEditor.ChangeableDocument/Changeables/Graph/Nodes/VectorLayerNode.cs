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
public class VectorLayerNode : LayerNode, ITransformableObject, IReadOnlyVectorNode
{
    public Matrix3X3 TransformationMatrix
    {
        get => ShapeData?.TransformationMatrix ?? Matrix3X3.Identity;
        set
        {
            if (ShapeData == null)
            {
                return;
            }
            
            ShapeData.TransformationMatrix = value;
        }
    }

    public ShapeVectorData? ShapeData { get; set; }
    IReadOnlyShapeVectorData IReadOnlyVectorNode.ShapeData => ShapeData;

    protected override bool AffectedByChunkResolution => true;

    private int lastCacheHash;
    
    protected override Texture? OnExecute(RenderingContext context)
    {
        if (ShapeData == null)
        {
            Output.Value = null;
            return null;
        }
        
        Texture texture = RequestTexture(0, context.DocumentSize);

        ShapeData.Rasterize(texture.DrawingSurface, context.ChunkResolution);
        
        Output.Value = texture;
        
        return texture;
    }

    protected override bool CacheChanged(RenderingContext context)
    {
        return base.CacheChanged(context) || (ShapeData?.GetCacheHash() ?? -1) != lastCacheHash;
    }

    protected override void UpdateCache(RenderingContext context)
    {
        base.UpdateCache(context);
        lastCacheHash = ShapeData?.GetCacheHash() ?? -1;
    }

    public override RectD? GetTightBounds(KeyFrameTime frameTime)
    {
        return ShapeData?.TransformedAABB ?? null;
    }

    public override ShapeCorners GetTransformationCorners(KeyFrameTime frameTime)
    {
        return ShapeData?.TransformationCorners ?? new ShapeCorners();
    }

    public override Node CreateCopy()
    {
        return new VectorLayerNode();
    }

}
