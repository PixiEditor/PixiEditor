using System.Collections.Concurrent;
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

public class DocumentRenderer/* : IPreviewRenderable*/
{
    Dictionary<Guid, List<PreviewRequest>> queuedPreviewSizes = new();
    private Texture renderTexture;
    
    public Dictionary<Guid, List<PreviewRequest>> QueuedPreviewSizes => queuedPreviewSizes;
    
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

    public int QueueRenderPreview(VecI sizeToRequest, Guid nodeId, string elementName, KeyFrameTime frame,
        Action onRendered)
    {
        if (!queuedPreviewSizes.ContainsKey(nodeId))
        {
            queuedPreviewSizes[nodeId] = new List<PreviewRequest>();
        }
        
        PreviewRequest request = new(queuedPreviewSizes.Count, sizeToRequest, frame, nodeId, elementName, onRendered);
        
        TryMergeRequests(request);
        
        return request.Id;
    }
    
    public void RenderNodePreview(IPreviewRenderable previewRenderable, DrawingSurface renderOn, RenderContext context, string elementToRenderName)
    {
        if (IsBusy)
        {
            return;
        }
        
        IsBusy = true;
        
        /*if(previewRenderable is Node { IsDisposed: true }) return;
        
        previewRenderable.RenderPreview(renderOn, context, elementToRenderName);*/
        
        
        
        IsBusy = false;
    }

    public static IReadOnlyNodeGraph ConstructMembersOnlyGraph(IReadOnlyNodeGraph fullGraph)
    {
        return ConstructMembersOnlyGraph(null, fullGraph);
    }

    public static IReadOnlyNodeGraph ConstructMembersOnlyGraph(
        HashSet<Guid>? membersToCombine,
        IReadOnlyNodeGraph fullGraph)
    {
        RenderNodeGraph membersOnlyGraph = new();

        OutputNode outputNode = new();

        membersOnlyGraph.AddNode(outputNode);

        Dictionary<Guid, Guid> nodeMapping = new();

        fullGraph.OutputNode.TraverseBackwards((node, input) =>
        {
            if (node is StructureNode structureNode && membersToCombine != null &&
                !membersToCombine.Contains(structureNode.Id))
            {
                return true;
            }

            if (node is LayerNode layer)
            {
                LayerNode clone = (LayerNode)layer.Clone();
                membersOnlyGraph.AddNode(clone);

                
                IInputProperty targetInput = GetTargetInput(input, fullGraph, membersOnlyGraph, nodeMapping);
                
                clone.Output.ConnectTo(targetInput);
                nodeMapping[layer.Id] = clone.Id;
            }
            else if (node is FolderNode folder)
            {
                FolderNode clone = (FolderNode)folder.Clone();
                membersOnlyGraph.AddNode(clone);

                var targetInput = GetTargetInput(input, fullGraph, membersOnlyGraph, nodeMapping);
                
                clone.Output.ConnectTo(targetInput);
                nodeMapping[folder.Id] = clone.Id;
            }

            return true;
        });

        return membersOnlyGraph;
    }

    public RectD? GetPreviewBounds(int frame, string elementNameToRender = "") =>
        new(0, 0, Document.Size.X, Document.Size.Y);

    public void RenderDocument(DrawingSurface toRenderOn, KeyFrameTime frameTime)
    {
        IsBusy = true;

        if (renderTexture == null || renderTexture.Size != Document.Size)
        {
            renderTexture?.Dispose();
            renderTexture = Texture.ForProcessing(Document.Size, Document.ProcessingColorSpace);
        }

        renderTexture.DrawingSurface.Canvas.Clear();
        RenderContext context =
            new(renderTexture.DrawingSurface, frameTime, ChunkResolution.Full, Document.Size,
                Document.ProcessingColorSpace) { FullRerender = true };
        Document.NodeGraph.Execute(context);

        toRenderOn.Canvas.DrawSurface(renderTexture.DrawingSurface, 0, 0);
        IsBusy = false;
    }
    
    private static IInputProperty GetTargetInput(IInputProperty? input, 
        IReadOnlyNodeGraph sourceGraph,
        RenderNodeGraph membersOnlyGraph,
        Dictionary<Guid, Guid> nodeMapping)
    {
        if (input == null)
        {
            if(membersOnlyGraph.OutputNode is IRenderInput inputNode) return inputNode.Background;

            return null;
        }
        
        if (nodeMapping.ContainsKey(input.Node?.Id ?? Guid.Empty))
        {
            return membersOnlyGraph.Nodes.First(x => x.Id == nodeMapping[input.Node.Id])
                .GetInputProperty(input.InternalPropertyName);
        }
        
        var sourceNode = sourceGraph.AllNodes.First(x => x.Id == input.Node.Id);

        IInputProperty? found = null;
        sourceNode.TraverseForwards((n, input) =>
        {
            if (n is StructureNode structureNode)
            {
                if(nodeMapping.TryGetValue(structureNode.Id, out var value))
                {
                    Node mappedNode = membersOnlyGraph.Nodes.First(x => x.Id == value);
                    found = mappedNode.GetInputProperty(input.InternalPropertyName);
                    return false;
                }
            }
            
            return true;
        });
        
        return found ?? (membersOnlyGraph.OutputNode as IRenderInput)?.Background;
    }
    
    private void TryMergeRequests(PreviewRequest request)
    {
        if (queuedPreviewSizes[request.NodeId].Count == 0)
        {
            queuedPreviewSizes[request.NodeId].Add(request);
            return;
        }

        for (var i = 0; i < queuedPreviewSizes[request.NodeId].Count; i++)
        {
            var queuedRequest = queuedPreviewSizes[request.NodeId][i];
            bool targetMatches = queuedRequest.ElementName == request.ElementName &&
                                 queuedRequest.Frame.Frame == request.Frame.Frame;

            VecI targetSize = new(Math.Max(queuedRequest.Size.X, request.Size.X),
                Math.Max(queuedRequest.Size.Y, request.Size.Y));
            if (targetMatches)
            {
                queuedPreviewSizes[request.NodeId][i] = new PreviewRequest(request.Id, targetSize, request.Frame, request.NodeId,
                    request.ElementName, () =>
                    {
                        queuedRequest.OnRendered?.Invoke();
                        request.OnRendered?.Invoke();
                    });
                return;
            }
        }
    }

    public void NotifyPreviewRendered()
    {
        foreach (var request in queuedPreviewSizes.Values.SelectMany(x => x))
        {
            request.OnRendered?.Invoke();
        }
    }
}
