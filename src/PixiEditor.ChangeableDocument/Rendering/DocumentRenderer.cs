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

public class DocumentRenderer : IDisposable
{
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
            Document.ProcessingColorSpace, SamplingOptions.Default, Document.Blackboard);
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
            Document.ProcessingColorSpace, SamplingOptions.Default, Document.Blackboard);
        context.FullRerender = true;

        node.RenderForOutput(context, toRenderOn, null);

        renderTexture.DrawingSurface.Canvas.Restore();
        toRenderOn.Canvas.Restore();

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
                Document.Size, Document.ProcessingColorSpace, SamplingOptions.Default, Document.Blackboard) { FullRerender = true };

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

        IsBusy = false;
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
    }
}
