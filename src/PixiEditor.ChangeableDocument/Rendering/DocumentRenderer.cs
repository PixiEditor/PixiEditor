using System.Collections.Concurrent;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Text;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Workspace;

namespace PixiEditor.ChangeableDocument.Rendering;

public class DocumentRenderer : IPreviewRenderable, IDisposable
{
    private Queue<RenderRequest> renderRequests = new();
    private Texture renderTexture;
    private int lastExecutedGraphFrame = -1;

    public DocumentRenderer(IReadOnlyDocument document)
    {
        Document = document;
    }

    private IReadOnlyDocument Document { get; }
    public bool IsBusy { get; private set; }

    private bool isExecuting = false;

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

    public void RenderLayers(DrawingSurface toRenderOn, HashSet<Guid> layersToCombine, int frame,
        ChunkResolution resolution, VecI renderSize)
    {
        IsBusy = true;

        if (renderTexture == null || renderTexture.Size != renderSize)
        {
            renderTexture?.Dispose();
            renderTexture = Texture.ForProcessing(renderSize, Document.ProcessingColorSpace);
        }

        renderTexture.DrawingSurface.Canvas.Save();
        renderTexture.DrawingSurface.Canvas.Clear();

        renderTexture.DrawingSurface.Canvas.SetMatrix(toRenderOn.Canvas.TotalMatrix);
        toRenderOn.Canvas.Save();
        toRenderOn.Canvas.SetMatrix(Matrix3X3.Identity);

        RenderContext context = new(renderTexture.DrawingSurface, frame, resolution, Document.Size, Document.Size,
            Document.ProcessingColorSpace, SamplingOptions.Default);
        context.FullRerender = true;
        IReadOnlyNodeGraph membersOnlyGraph = ConstructMembersOnlyGraph(layersToCombine, Document.NodeGraph);
        try
        {
            membersOnlyGraph.Execute(context);
            toRenderOn.Canvas.DrawSurface(renderTexture.DrawingSurface, 0, 0);
        }
        catch (ObjectDisposedException)
        {
        }
        finally
        {
            renderTexture.DrawingSurface.Canvas.Restore();
            toRenderOn.Canvas.Restore();
            IsBusy = false;
        }
    }


    public void RenderLayer(DrawingSurface toRenderOn, Guid layerId, ChunkResolution resolution, KeyFrameTime frameTime,
        VecI renderSize)
    {
        var node = Document.FindMember(layerId);

        if (node is null)
        {
            return;
        }

        IsBusy = true;

        if (renderTexture == null || renderTexture.Size != renderSize)
        {
            renderTexture?.Dispose();
            renderTexture = Texture.ForProcessing(renderSize, Document.ProcessingColorSpace);
        }

        renderTexture.DrawingSurface.Canvas.Save();
        renderTexture.DrawingSurface.Canvas.Clear();

        renderTexture.DrawingSurface.Canvas.SetMatrix(toRenderOn.Canvas.TotalMatrix);
        toRenderOn.Canvas.Save();
        toRenderOn.Canvas.SetMatrix(Matrix3X3.Identity);

        RenderContext context = new(renderTexture.DrawingSurface, frameTime, resolution, Document.Size, Document.Size,
            Document.ProcessingColorSpace, SamplingOptions.Default);
        context.FullRerender = true;

        node.RenderForOutput(context, toRenderOn, null);

        renderTexture.DrawingSurface.Canvas.Restore();
        toRenderOn.Canvas.Restore();

        IsBusy = false;
    }

    public async Task<bool> RenderNodePreview(IPreviewRenderable previewRenderable, DrawingSurface renderOn,
        RenderContext context,
        string elementToRenderName)
    {
        if (previewRenderable is Node { IsDisposed: true }) return false;
        TaskCompletionSource<bool> tcs = new();
        RenderRequest request = new(tcs, context, renderOn, previewRenderable, elementToRenderName);

        renderRequests.Enqueue(request);
        ExecuteRenderRequests(context.FrameTime);

        return await tcs.Task;
    }

    public static IReadOnlyNodeGraph ConstructMembersOnlyGraph(IReadOnlyNodeGraph fullGraph)
    {
        return ConstructMembersOnlyGraph(null, fullGraph);
    }

    public static IReadOnlyNodeGraph ConstructMembersOnlyGraph(
        HashSet<Guid>? membersToCombine,
        IReadOnlyNodeGraph fullGraph)
    {
        NodeGraph membersOnlyGraph = new();

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

    RectD? IPreviewRenderable.GetPreviewBounds(int frame, string elementNameToRender = "") =>
        new(0, 0, Document.Size.X, Document.Size.Y);

    bool IPreviewRenderable.RenderPreview(DrawingSurface renderOn, RenderContext context,
        string elementToRenderName)
    {
        IsBusy = true;

        renderOn.Canvas.Clear();
        int savedCount = renderOn.Canvas.Save();
        renderOn.Canvas.Scale((float)context.ChunkResolution.Multiplier());
        context.RenderSurface = renderOn;
        Document.NodeGraph.Execute(context);
        lastExecutedGraphFrame = context.FrameTime.Frame;
        renderOn.Canvas.RestoreToCount(savedCount);

        IsBusy = false;

        return true;
    }

    public void RenderDocument(DrawingSurface toRenderOn, KeyFrameTime frameTime, VecI renderSize,
        string? customOutput = null)
    {
        using var ctx = DrawingBackendApi.Current.RenderingDispatcher.EnsureContext();
        IsBusy = true;

        if (renderTexture == null || renderTexture.Size != renderSize)
        {
            renderTexture?.Dispose();
            renderTexture = Texture.ForProcessing(renderSize, Document.ProcessingColorSpace);
        }

        renderTexture.DrawingSurface.Canvas.Save();
        renderTexture.DrawingSurface.Canvas.Clear();

        renderTexture.DrawingSurface.Canvas.SetMatrix(toRenderOn.Canvas.TotalMatrix);
        toRenderOn.Canvas.Save();
        toRenderOn.Canvas.SetMatrix(Matrix3X3.Identity);


        bool hasCustomOutput = !string.IsNullOrEmpty(customOutput) && customOutput != "DEFAULT";

        var graph = hasCustomOutput
            ? RenderingUtils.SolveFinalNodeGraph(customOutput, Document)
            : Document.NodeGraph;

        RenderContext context =
            new(renderTexture.DrawingSurface, frameTime, ChunkResolution.Full,
                SolveRenderOutputSize(customOutput, graph, Document.Size),
                Document.Size, Document.ProcessingColorSpace, SamplingOptions.Default) { FullRerender = true };

        if (hasCustomOutput)
        {
            context.TargetOutput = customOutput;
        }

        try
        {
            graph.Execute(context);
            toRenderOn.Canvas.DrawSurface(renderTexture.DrawingSurface, 0, 0);
        }
        catch (Exception e)
        {
            renderTexture.DrawingSurface.Canvas.Clear();
            using Paint paint = new Paint { Color = Colors.White, IsAntiAliased = true };

            using Font defaultSizedFont = Font.CreateDefault();
            defaultSizedFont.Size = 24;

            renderTexture.DrawingSurface.Canvas.DrawText("Graph Setup produced an error. Fix it the node graph",
                renderTexture.Size / 2f, TextAlign.Center, defaultSizedFont, paint);
        }

        renderTexture.DrawingSurface.Canvas.Restore();
        toRenderOn.Canvas.Restore();

        lastExecutedGraphFrame = frameTime.Frame;

        IsBusy = false;
    }

    private void ExecuteRenderRequests(KeyFrameTime frameTime)
    {
        if (isExecuting) return;

        isExecuting = true;
        using var ctx = DrawingBackendApi.Current?.RenderingDispatcher.EnsureContext();

        while (renderRequests.Count > 0)
        {
            RenderRequest request = renderRequests.Dequeue();

            if (frameTime.Frame != lastExecutedGraphFrame && request.PreviewRenderable != this)
            {
                using Texture executeSurface = Texture.ForDisplay(new VecI(1));
                RenderDocument(executeSurface.DrawingSurface, frameTime, VecI.One);
            }

            try
            {
                bool result = true;
                if (request.PreviewRenderable != null)
                {
                    result = request.PreviewRenderable.RenderPreview(request.RenderOn, request.Context,
                        request.ElementToRenderName);
                }
                else if (request.NodeGraph != null)
                {
                    request.NodeGraph.Execute(request.Context);
                }

                request.TaskCompletionSource.SetResult(result);
            }
            catch (Exception e)
            {
                request.TaskCompletionSource.SetException(e);
            }
        }

        isExecuting = false;
    }

    private static IInputProperty GetTargetInput(IInputProperty? input,
        IReadOnlyNodeGraph sourceGraph,
        NodeGraph membersOnlyGraph,
        Dictionary<Guid, Guid> nodeMapping)
    {
        if (input == null)
        {
            if (membersOnlyGraph.OutputNode is IRenderInput inputNode) return inputNode.Background;

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
                if (nodeMapping.TryGetValue(structureNode.Id, out var value))
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

    private static VecI SolveRenderOutputSize(string? targetOutput, IReadOnlyNodeGraph finalGraph, VecI documentSize)
    {
        VecI finalSize = documentSize;
        if (targetOutput != null)
        {
            var outputNode = finalGraph.AllNodes.FirstOrDefault(n =>
                n is CustomOutputNode outputNode && outputNode.OutputName.Value == targetOutput);

            if (outputNode is CustomOutputNode customOutputNode)
            {
                if (customOutputNode.Size.Value.ShortestAxis > 0)
                {
                    finalSize = customOutputNode.Size.Value;
                }
            }
            else
            {
                finalSize = documentSize;
            }
        }

        return finalSize;
    }

    public void Dispose()
    {
        renderTexture?.Dispose();
        renderTexture = null;

        foreach (var request in renderRequests)
        {
            if (request.TaskCompletionSource == null) continue;

            request.TaskCompletionSource.TrySetCanceled();
        }
    }
}

public struct RenderRequest
{
    public RenderContext Context { get; set; }
    public DrawingSurface RenderOn { get; set; }
    public IReadOnlyNodeGraph? NodeGraph { get; set; } // TODO: Implement async rendering for stuff other than previews
    public IPreviewRenderable? PreviewRenderable { get; set; }
    public string ElementToRenderName { get; set; }
    public TaskCompletionSource<bool> TaskCompletionSource { get; set; }

    public RenderRequest(TaskCompletionSource<bool> completionSource, RenderContext context, DrawingSurface renderOn,
        IReadOnlyNodeGraph nodeGraph)
    {
        TaskCompletionSource = completionSource;
        Context = context;
        RenderOn = renderOn;
        NodeGraph = nodeGraph;
    }

    public RenderRequest(TaskCompletionSource<bool> completionSource, RenderContext context, DrawingSurface renderOn,
        IPreviewRenderable previewRenderable, string elementToRenderName)
    {
        TaskCompletionSource = completionSource;
        Context = context;
        RenderOn = renderOn;
        PreviewRenderable = previewRenderable;
        ElementToRenderName = elementToRenderName;
    }
}
