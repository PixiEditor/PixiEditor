using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.Surfaces.ImageData;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Rendering;

public class DocumentRenderer
{
    public DocumentRenderer(IReadOnlyDocument document)
    {
        Document = document;
    }

    private IReadOnlyDocument Document { get; }

    public OneOf<Chunk, EmptyChunk> RenderChunk(VecI chunkPos, ChunkResolution resolution, KeyFrameTime frameTime,
        RectI? globalClippingRect = null)
    {
        RenderingContext context = new(frameTime, chunkPos, resolution, Document.Size);
        try
        {
            return RenderChunkOnGraph(chunkPos, resolution, globalClippingRect, Document.NodeGraph, context);
        }
        catch (ObjectDisposedException)
        {
            return new EmptyChunk();
        }
        finally
        {
            context.Dispose();
        }
    }

    public OneOf<Chunk, EmptyChunk> RenderChunk(VecI chunkPos, ChunkResolution resolution,
        IReadOnlyNode node, KeyFrameTime frameTime, RectI? globalClippingRect = null)
    {
        using RenderingContext context = new(frameTime, chunkPos, resolution, Document.Size);
        try
        {
            RectI? transformedClippingRect = TransformClipRect(globalClippingRect, resolution, chunkPos);

            Texture? evaluated = node.Execute(context);
            if (evaluated is null)
            {
                return new EmptyChunk();
            }

            var result = ChunkFromResult(resolution, transformedClippingRect, evaluated.DrawingSurface.Snapshot(), context);
            evaluated.Dispose();
            
            return result;
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
        using RenderingContext context = new(frame, chunkPos, resolution, Document.Size);
        NodeGraph membersOnlyGraph = ConstructMembersOnlyGraph(layersToCombine, Document.NodeGraph);
        try
        {
            return RenderChunkOnGraph(chunkPos, resolution, globalClippingRect, membersOnlyGraph, context);
        }
        catch (ObjectDisposedException)
        {
            return new EmptyChunk();
        }
    }

    private static OneOf<Chunk, EmptyChunk> RenderChunkOnGraph(VecI chunkPos, ChunkResolution resolution, RectI? globalClippingRect,
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
            
        if(context.IsDisposed) return new EmptyChunk();

        chunk.Surface.DrawingSurface.Canvas.DrawImage(chunkSnapshot, 0, 0, context.ReplacingPaintWithOpacity);

        chunk.Surface.DrawingSurface.Canvas.Restore();

        return chunk;
    }

    private NodeGraph ConstructMembersOnlyGraph(HashSet<Guid> layersToCombine, IReadOnlyNodeGraph fullGraph)
    {
        NodeGraph membersOnlyGraph = new();

        OutputNode outputNode = new();
        
        membersOnlyGraph.AddNode(outputNode);

        List<LayerNode> layersInOrder = new();

        fullGraph.TryTraverse(node =>
        {
            if (node is LayerNode layer && layersToCombine.Contains(layer.Id))
            {
                layersInOrder.Insert(0, layer);
            }
        });

        IInputProperty<Texture> lastInput = outputNode.Input;

        foreach (var layer in layersInOrder)
        {
            var clone = (LayerNode)layer.Clone();
            membersOnlyGraph.AddNode(clone);
            
            clone.Output.ConnectTo(lastInput);
            lastInput = clone.Background;
        }
        
        return membersOnlyGraph;
    }

    private static OneOf<Chunk, EmptyChunk> ChunkFromResult(
        ChunkResolution resolution,
        RectI? transformedClippingRect, Image evaluated,
        RenderingContext context)
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
}
