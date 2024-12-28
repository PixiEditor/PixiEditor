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

    private Texture renderTexture;
    
    public DocumentRenderer(IReadOnlyDocument document)
    {
        Document = document;
    }

    private IReadOnlyDocument Document { get; }
    public bool IsBusy { get; private set; }

    public void UpdateChunk(VecI chunkPos, ChunkResolution resolution, KeyFrameTime frameTime)
    {
        try
        {
            Document.NodeGraph.TryTraverse((node =>
            {
                if (node is IChunkRenderable imageNode)
                {
                    imageNode.RenderChunk(chunkPos, resolution, frameTime, Document.ProcessingColorSpace);
                }
            }));
        }
        catch (ObjectDisposedException)
        {
        }
    }

    public void RenderLayers(DrawingSurface toDrawOn, HashSet<Guid> layersToCombine, int frame,
        ChunkResolution resolution)
    {
        IsBusy = true;
        RenderContext context = new(toDrawOn, frame, resolution, Document.Size, Document.ProcessingColorSpace);
        context.FullRerender = true;
        IReadOnlyNodeGraph membersOnlyGraph = ConstructMembersOnlyGraph(layersToCombine, Document.NodeGraph);
        try
        {
            membersOnlyGraph.Execute(context);
        }
        catch (ObjectDisposedException)
        {
        }
        finally
        {
            IsBusy = false;
        }
    }


    public void RenderLayer(DrawingSurface renderOn, Guid layerId, ChunkResolution resolution, KeyFrameTime frameTime)
    {
        var node = Document.FindMember(layerId);

        if (node is null)
        {
            return;
        }
        
        IsBusy = true;

        RenderContext context = new(renderOn, frameTime, resolution, Document.Size, Document.ProcessingColorSpace);
        context.FullRerender = true;
        
        node.RenderForOutput(context, renderOn, null);
        IsBusy = false;
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

    public bool RenderPreview(DrawingSurface renderOn, RenderContext context,
        string elementToRenderName)
    {
        IsBusy = true;
        
        if(renderTexture == null || renderTexture.Size != Document.Size)
        {
            renderTexture?.Dispose();
            renderTexture = Texture.ForProcessing(Document.Size, Document.ProcessingColorSpace);
        }
        
        renderTexture.DrawingSurface.Canvas.Clear();
        context.RenderSurface = renderTexture.DrawingSurface;
        Document.NodeGraph.Execute(context);
        
        renderOn.Canvas.DrawSurface(renderTexture.DrawingSurface, 0, 0);
        
        IsBusy = false;

        return true;
    }

    public void RenderDocument(DrawingSurface toRenderOn, KeyFrameTime frameTime)
    {
        IsBusy = true;
        
        if(renderTexture == null || renderTexture.Size != Document.Size)
        {
            renderTexture?.Dispose();
            renderTexture = Texture.ForProcessing(Document.Size, Document.ProcessingColorSpace);
        }
        
        renderTexture.DrawingSurface.Canvas.Clear();
        RenderContext context = new(renderTexture.DrawingSurface, frameTime, ChunkResolution.Full, Document.Size, Document.ProcessingColorSpace) { FullRerender = true };
        Document.NodeGraph.Execute(context);
        
        toRenderOn.Canvas.DrawSurface(renderTexture.DrawingSurface, 0, 0);
        IsBusy = false;
    }
}
