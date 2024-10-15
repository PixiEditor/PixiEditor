using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.DrawingApi.Core.Surfaces.ImageData;
using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Rendering;

public class DocumentRenderer : IPreviewRenderable
{
    
    private Paint ClearPaint { get; } = new Paint()
    {
        BlendMode = BlendMode.Src, Color = PixiEditor.DrawingApi.Core.ColorsImpl.Colors.Transparent
    };

    public DocumentRenderer(IReadOnlyDocument document)
    {
        Document = document;
    }

    private IReadOnlyDocument Document { get; }
    //public Texture OnionSkinTexture { get; set; }

    /*public Texture RenderDocument(KeyFrameTime frameTime, ChunkResolution resolution, Texture toDrawOn = null,
        Paint paint = null)
    {
        VecI sizeInChunks = Document.Size / resolution.PixelSize();

        sizeInChunks = new VecI(
            Math.Max(1, sizeInChunks.X),
            Math.Max(1, sizeInChunks.Y));

        if (toDrawOn is null)
        {
            VecI size = new VecI(
                Math.Min(Document.Size.X, resolution.PixelSize() * sizeInChunks.X),
                Math.Min(Document.Size.Y, resolution.PixelSize() * sizeInChunks.Y));
            toDrawOn = new Texture(size);
        }

        for (int x = 0; x < sizeInChunks.X; x++)
        {
            for (int y = 0; y < sizeInChunks.Y; y++)
            {
                VecI chunkPos = new(x, y);
                OneOf<Chunk, EmptyChunk> chunk = RenderChunk(chunkPos, resolution, frameTime);
                if (chunk.IsT0)
                {
                    toDrawOn.DrawingSurface.Canvas.DrawSurface(
                        chunk.AsT0.Surface.DrawingSurface,
                        chunkPos.Multiply(new VecI(resolution.PixelSize())), paint);
                }
                else
                {
                    var pos = chunkPos * resolution.PixelSize();
                    toDrawOn.DrawingSurface.Canvas.DrawRect(pos.X, pos.Y, resolution.PixelSize(),
                        resolution.PixelSize(), ClearPaint);
                }
            }
        }

        return toDrawOn;
    }*/
    
    public void RenderChunk(VecI chunkPos, ChunkResolution resolution, KeyFrameTime frameTime)
    {
        try
        {
            Document.NodeGraph.TryTraverse((node =>
            {
                if (node is IChunkRenderable imageNode)
                {
                    imageNode.RenderChunk(chunkPos, resolution, frameTime);
                }
            }));
        }
        catch (ObjectDisposedException)
        {
        }
    }

    public OneOf<Chunk, EmptyChunk> RenderChunk(VecI chunkPos, ChunkResolution resolution,
        IReadOnlyNode node, KeyFrameTime frameTime, RectI? globalClippingRect = null)
    {
        //using RenderingContext context = new(frameTime, chunkPos, resolution, Document.Size);
        try
        {
            /*RectI? transformedClippingRect = TransformClipRect(globalClippingRect, resolution, chunkPos);

            Texture? evaluated = node.Execute(context);
            if (evaluated is null)
            {
                return new EmptyChunk();
            }

            var result = ChunkFromResult(resolution, transformedClippingRect, evaluated.DrawingSurface.Snapshot(),
                context);
            evaluated.Dispose();

            return result;*/
            return new EmptyChunk();
        }
        catch (ObjectDisposedException)
        {
            return new EmptyChunk();
        }
    }

    
    private static RectI? TransformClipRect(RectI? globalClippingRect, ChunkResolution resolution, VecI chunkPos)
    {
        if (globalClippingRect is not RectI rect)
            return null;

        double multiplier = resolution.Multiplier();
        VecI pixelChunkPos = chunkPos * (int)(ChunkyImage.FullChunkSize * multiplier);
        return (RectI?)rect.Scale(multiplier).Translate(-pixelChunkPos).RoundOutwards();
    }

    public OneOf<Chunk, EmptyChunk> RenderLayersChunk(VecI chunkPos, ChunkResolution resolution, KeyFrameTime frame,
        HashSet<Guid> layersToCombine, RectI? globalClippingRect)
    {
        //using RenderingContext context = new(frame, chunkPos, resolution, Document.Size);
        IReadOnlyNodeGraph membersOnlyGraph = ConstructMembersOnlyGraph(layersToCombine, Document.NodeGraph);
        try
        {
            //return RenderChunkOnGraph(chunkPos, resolution, globalClippingRect, membersOnlyGraph, context);
            return new EmptyChunk();
        }
        catch (ObjectDisposedException)
        {
            return new EmptyChunk();
        }
    }
    
    
    public Texture? RenderLayer(Guid nodeId, ChunkResolution resolution, KeyFrameTime frameTime)
    {
        var node = Document.FindNode(nodeId);
        
        if (node is null)
        {
            return null;
        }
        
        VecI sizeInChunks = Document.Size / resolution.PixelSize();
        
        sizeInChunks = new VecI(
            Math.Max(1, sizeInChunks.X),
            Math.Max(1, sizeInChunks.Y));
        
        VecI size = new VecI(
            Math.Min(Document.Size.X, resolution.PixelSize() * sizeInChunks.X),
            Math.Min(Document.Size.Y, resolution.PixelSize() * sizeInChunks.Y));
        
        Texture texture = new(size);
        
        for (int x = 0; x < sizeInChunks.X; x++)
        {
            for (int y = 0; y < sizeInChunks.Y; y++)
            {
                VecI chunkPos = new(x, y);
                RectI globalClippingRect = new(0, 0, Document.Size.X, Document.Size.Y);
                OneOf<Chunk, EmptyChunk> chunk = RenderChunk(chunkPos, resolution, node, frameTime, globalClippingRect);
                if (chunk.IsT0)
                {
                    VecI pos = chunkPos * resolution.PixelSize(); 
                    texture.DrawingSurface.Canvas.DrawSurface(
                        chunk.AsT0.Surface.DrawingSurface,
                        pos.X, pos.Y, null);
                }
            }
        }
        
        return texture;
    }

    /*
    private static OneOf<Chunk, EmptyChunk> RenderChunkOnGraph(VecI chunkPos, ChunkResolution resolution,
        RectI? globalClippingRect,
        IReadOnlyNodeGraph graph, RenderingContext context)
    {
        RectI? transformedClippingRect = TransformClipRect(globalClippingRect, resolution, chunkPos);

        Texture? evaluated = graph.Execute(context);
        if (evaluated is null)
        {
            return new EmptyChunk();
        }

        Chunk chunk = Chunk.Create(resolution);

        chunk.Surface.DrawingSurface.Canvas.Save();
        chunk.Surface.DrawingSurface.Canvas.Clear();

        if (transformedClippingRect is not null)
        {
            chunk.Surface.DrawingSurface.Canvas.ClipRect((RectD)transformedClippingRect);
        }

        VecD pos = chunkPos;
        int x = (int)(pos.X * ChunkyImage.FullChunkSize * resolution.Multiplier());
        int y = (int)(pos.Y * ChunkyImage.FullChunkSize * resolution.Multiplier());
        int width = (int)(ChunkyImage.FullChunkSize * resolution.Multiplier());
        int height = (int)(ChunkyImage.FullChunkSize * resolution.Multiplier());

        RectD sourceRect = new(x, y, width, height);

        RectD availableRect = new(0, 0, evaluated.Size.X, evaluated.Size.Y);

        sourceRect = sourceRect.Intersect(availableRect);

        if (sourceRect.IsZeroOrNegativeArea)
        {
            chunk.Dispose();
            return new EmptyChunk();
        }

        using var chunkSnapshot = evaluated.DrawingSurface.Snapshot((RectI)sourceRect);

        if (context.IsDisposed) return new EmptyChunk();

        chunk.Surface.DrawingSurface.Canvas.DrawImage(chunkSnapshot, 0, 0, context.ReplacingPaintWithOpacity);

        chunk.Surface.DrawingSurface.Canvas.Restore();

        return chunk;
    }
    */

    public static IReadOnlyNodeGraph ConstructMembersOnlyGraph(IReadOnlyNodeGraph fullGraph)
    {
        return ConstructMembersOnlyGraph(null, fullGraph); 
    }

    public static IReadOnlyNodeGraph ConstructMembersOnlyGraph(HashSet<Guid>? layersToCombine,
        IReadOnlyNodeGraph fullGraph)
    {
        NodeGraph membersOnlyGraph = new();

        OutputNode outputNode = new();

        membersOnlyGraph.AddNode(outputNode);

        List<LayerNode> layersInOrder = new();

        fullGraph.TryTraverse(node =>
        {
            if (node is LayerNode layer && (layersToCombine == null || layersToCombine.Contains(layer.Id)))
            {
                layersInOrder.Insert(0, layer);
            }
        });

        IInputProperty<DrawingSurface> lastInput = outputNode.Input;

        foreach (var layer in layersInOrder)
        {
            var clone = (LayerNode)layer.Clone();
            membersOnlyGraph.AddNode(clone);

            clone.Output.ConnectTo(lastInput);
            lastInput = clone.RenderTarget;
        }

        return membersOnlyGraph;
    }

    private static OneOf<Chunk, EmptyChunk> ChunkFromResult(
        ChunkResolution resolution,
        RectI? transformedClippingRect, Image evaluated,
        RenderContext context)
    {
        Chunk chunk = Chunk.Create(resolution);

        chunk.Surface.DrawingSurface.Canvas.Save();
        chunk.Surface.DrawingSurface.Canvas.Clear();

        int x = 0;
        int y = 0;

        if (transformedClippingRect is not null)
        {
            chunk.Surface.DrawingSurface.Canvas.ClipRect((RectD)transformedClippingRect);
            x = transformedClippingRect.Value.X;
            y = transformedClippingRect.Value.Y;
        }

        chunk.Surface.DrawingSurface.Canvas.DrawImage(evaluated, x, y,
            context.ReplacingPaintWithOpacity);

        chunk.Surface.DrawingSurface.Canvas.Restore();

        return chunk;
    }

    public RectD? GetPreviewBounds(int frame, string elementNameToRender = "") => 
        new(0, 0, Document.Size.X, Document.Size.Y); 

    public bool RenderPreview(DrawingSurface renderOn, ChunkResolution resolution, int frame, string elementToRenderName)
    {
        using RenderContext context = new(renderOn, frame, resolution, Document.Size);
        Document.NodeGraph.Execute(context);
        
        return true;
    }

    public void RenderDocument(DrawingSurface toRenderOn, KeyFrameTime frameTime)
    {
        using RenderContext context = new(toRenderOn, frameTime, ChunkResolution.Full, Document.Size) { IsExportRender = true };
        Document.NodeGraph.Execute(context);
    }
}
