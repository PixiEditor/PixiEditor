using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Rendering;

public class DocumentRenderer : IPreviewRenderable
{
    private Paint ClearPaint { get; } = new Paint()
    {
        BlendMode = BlendMode.Src, Color = Drawie.Backend.Core.ColorsImpl.Colors.Transparent
    };

    public DocumentRenderer(IReadOnlyDocument document)
    {
        Document = document;
    }

    private IReadOnlyDocument Document { get; }

    public void UpdateChunk(VecI chunkPos, ChunkResolution resolution, KeyFrameTime frameTime)
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

    private static RectI? TransformClipRect(RectI? globalClippingRect, ChunkResolution resolution, VecI chunkPos)
    {
        if (globalClippingRect is not RectI rect)
            return null;

        double multiplier = resolution.Multiplier();
        VecI pixelChunkPos = chunkPos * (int)(ChunkyImage.FullChunkSize * multiplier);
        return (RectI?)rect.Scale(multiplier).Translate(-pixelChunkPos).RoundOutwards();
    }

    public void RenderLayers(DrawingSurface toDrawOn, HashSet<Guid> layersToCombine, int frame,
        ChunkResolution resolution)
    {
        RenderContext context = new(toDrawOn, frame, resolution, Document.Size);
        context.FullRerender = true;
        IReadOnlyNodeGraph membersOnlyGraph = ConstructMembersOnlyGraph(layersToCombine, Document.NodeGraph);
        try
        {
            membersOnlyGraph.Execute(context);
        }
        catch (ObjectDisposedException)
        {
        }
    }


    public void RenderLayer(DrawingSurface renderOn, Guid layerId, ChunkResolution resolution, KeyFrameTime frameTime)
    {
        var node = Document.FindMember(layerId);

        if (node is null)
        {
            return;
        }

        RenderContext context = new(renderOn, frameTime, resolution, Document.Size);
        context.FullRerender = true;
        
        node.RenderForOutput(context, renderOn, null);
    }

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

        IInputProperty<Painter> lastInput = outputNode.Input;

        foreach (var layer in layersInOrder)
        {
            var clone = (LayerNode)layer.Clone();
            membersOnlyGraph.AddNode(clone);

            clone.Output.ConnectTo(lastInput);
            lastInput = clone.Background;
        }

        return membersOnlyGraph;
    }

    public RectD? GetPreviewBounds(int frame, string elementNameToRender = "") =>
        new(0, 0, Document.Size.X, Document.Size.Y);

    public bool RenderPreview(DrawingSurface renderOn, ChunkResolution resolution, int frame,
        string elementToRenderName)
    {
        RenderContext context = new(renderOn, frame, resolution, Document.Size);
        Document.NodeGraph.Execute(context);

        return true;
    }

    public void RenderDocument(DrawingSurface toRenderOn, KeyFrameTime frameTime)
    {
        RenderContext context = new(toRenderOn, frameTime, ChunkResolution.Full, Document.Size) { FullRerender = true };
        Document.NodeGraph.Execute(context);
    }
}
