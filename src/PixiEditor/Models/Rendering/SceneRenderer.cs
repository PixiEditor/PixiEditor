using Avalonia.Threading;
using ChunkyImageLib.DataHolders;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core.Surfaces;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Workspace;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Position;

namespace PixiEditor.Models.Rendering;

internal class SceneRenderer : IDisposable
{
    public const double ZoomDiffToRerender = 20;
    public IReadOnlyDocument Document { get; }
    public IDocument DocumentViewModel { get; }
    public bool HighResRendering { get; set; } = true;

    private Dictionary<Guid, RenderState> lastRenderedStates = new();
    private int lastGraphCacheHash = -1;
    private KeyFrameTime lastFrameTime;
    private Dictionary<Guid, bool> lastFramesVisibility = new();

    private TextureCache textureCache = new();

    public SceneRenderer(IReadOnlyDocument trackerDocument, IDocument documentViewModel)
    {
        Document = trackerDocument;
        DocumentViewModel = documentViewModel;
    }

    public async Task RenderAsync(Dictionary<Guid, ViewportInfo> stateViewports, AffectedArea affectedArea,
        bool updateDelayed, Dictionary<Guid, List<PreviewRenderRequest>>? previewTextures)
    {
        await DrawingBackendApi.Current.RenderingDispatcher.InvokeAsync(() =>
        {
            using var ctx = DrawingBackendApi.Current.RenderingDispatcher.EnsureContext();
            int renderedCount = 0;
            foreach (var viewport in stateViewports)
            {
                if (viewport.Value.Delayed && !updateDelayed)
                {
                    continue;
                }

                if (viewport.Value.RealDimensions.ShortestAxis <= 0) continue;

                var rendered = RenderScene(viewport.Value, affectedArea, previewTextures);
                if (DocumentViewModel.SceneTextures.TryGetValue(viewport.Key, out var texture) && texture != rendered)
                {
                    texture.Dispose();
                }

                DocumentViewModel.SceneTextures[viewport.Key] = rendered;
                viewport.Value.InvalidateVisual();
                renderedCount++;
            }

            if (renderedCount == 0 && previewTextures is { Count: > 0 })
            {
                RenderOnlyPreviews(affectedArea, previewTextures);
            }
        });
    }

    private void RenderOnlyPreviews(AffectedArea affectedArea,
        Dictionary<Guid, List<PreviewRenderRequest>> previewTextures)
    {
        ViewportInfo previewGenerationViewport = new()
        {
            RealDimensions = new VecD(1, 1),
            Transform = Matrix3X3.Identity,
            Id = Guid.NewGuid(),
            Resolution = ChunkResolution.Full,
            Sampling = SamplingOptions.Bilinear,
            VisibleDocumentRegion = null,
            RenderOutput = "DEFAULT",
            Delayed = false
        };
        var rendered = RenderScene(previewGenerationViewport, affectedArea, previewTextures);
        rendered.Dispose();
    }

    public Texture? RenderScene(ViewportInfo viewport, AffectedArea affectedArea,
        Dictionary<Guid, List<PreviewRenderRequest>>? previewTextures = null)
    {
        /*if (Document.Renderer.IsBusy || DocumentViewModel.Busy ||
            target.DeviceClipBounds.Size.ShortestAxis <= 0) return;*/

        /*TODO:
         - [ ] Rendering optimizer
         - [?] Render thread and proper locking/synchronization
         */

        VecI renderTargetSize = (VecI)viewport.RealDimensions;
        Matrix3X3 targetMatrix = viewport.Transform;
        Guid viewportId = viewport.Id;
        ChunkResolution resolution = viewport.Resolution;
        SamplingOptions samplingOptions = viewport.Sampling;
        RectI? visibleDocumentRegion = viewport.VisibleDocumentRegion;
        string? targetOutput = viewport.RenderOutput.Equals("DEFAULT", StringComparison.InvariantCultureIgnoreCase)
            ? null
            : viewport.RenderOutput;

        IReadOnlyNodeGraph finalGraph = RenderingUtils.SolveFinalNodeGraph(targetOutput, Document);
        bool shouldRerender =
            ShouldRerender(renderTargetSize, targetMatrix, resolution, viewportId, targetOutput, finalGraph,
                previewTextures, visibleDocumentRegion);

        if (shouldRerender)
        {
            return RenderGraph(renderTargetSize, targetMatrix, viewportId, resolution, samplingOptions, affectedArea,
                visibleDocumentRegion, targetOutput, viewport.IsScene, finalGraph, previewTextures);
        }

        var cachedTexture = DocumentViewModel.SceneTextures[viewportId];
        return cachedTexture;
    }

    private Texture RenderGraph(VecI renderTargetSize, Matrix3X3 targetMatrix, Guid viewportId,
        ChunkResolution resolution,
        SamplingOptions samplingOptions,
        AffectedArea area,
        RectI? visibleDocumentRegion,
        string? targetOutput,
        bool canRenderOnionSkinning,
        IReadOnlyNodeGraph finalGraph, Dictionary<Guid, List<PreviewRenderRequest>>? previewTextures)
    {
        DrawingSurface renderTarget = null;
        Texture? renderTexture = null;
        int restoreCanvasTo;

        VecI finalSize = SolveRenderOutputSize(targetOutput, finalGraph, Document.Size);
        if (RenderInOutputSize(finalGraph, renderTargetSize, finalSize))
        {
            finalSize = (VecI)(finalSize * resolution.Multiplier());

            renderTexture =
                textureCache.RequestTexture(viewportId.GetHashCode(), finalSize, Document.ProcessingColorSpace);
            renderTarget = renderTexture.DrawingSurface;
            renderTarget.Canvas.Save();
            renderTexture.DrawingSurface.Canvas.Save();
            renderTexture.DrawingSurface.Canvas.Scale((float)resolution.Multiplier());
        }
        else
        {
            renderTexture = textureCache.RequestTexture(viewportId.GetHashCode(), renderTargetSize,
                Document.ProcessingColorSpace);

            renderTarget = renderTexture.DrawingSurface;
            renderTarget.Canvas.SetMatrix(targetMatrix);
        }

        bool renderOnionSkinning = canRenderOnionSkinning &&
                                   DocumentViewModel.AnimationHandler.OnionSkinningEnabledBindable;

        var animationData = Document.AnimationData;
        double onionOpacity = animationData.OnionOpacity / 100.0;
        double alphaFalloffMultiplier = 1.0 / animationData.OnionFrames;
        if (renderOnionSkinning)
        {
            for (int i = 1; i <= animationData.OnionFrames; i++)
            {
                int frame = DocumentViewModel.AnimationHandler.ActiveFrameTime.Frame - i;
                if (frame < DocumentViewModel.AnimationHandler.FirstVisibleFrame)
                {
                    break;
                }

                double finalOpacity = onionOpacity * alphaFalloffMultiplier * (animationData.OnionFrames - i + 1);
                RenderContext onionContext = new(renderTarget, frame, resolution, finalSize, Document.Size,
                    Document.ProcessingColorSpace, samplingOptions, finalOpacity);
                onionContext.TargetOutput = targetOutput;
                finalGraph.Execute(onionContext);
            }
        }

        RenderContext context = new(renderTarget, DocumentViewModel.AnimationHandler.ActiveFrameTime,
            resolution, finalSize, Document.Size, Document.ProcessingColorSpace, samplingOptions);
        context.TargetOutput = targetOutput;
        context.AffectedArea = area;
        context.VisibleDocumentRegion = visibleDocumentRegion;
        context.PreviewTextures = previewTextures;
        finalGraph.Execute(context);

        if (renderOnionSkinning)
        {
            for (int i = 1; i <= animationData.OnionFrames; i++)
            {
                int frame = DocumentViewModel.AnimationHandler.ActiveFrameTime.Frame + i;
                if (frame >= DocumentViewModel.AnimationHandler.LastFrame)
                {
                    break;
                }

                double finalOpacity = onionOpacity * alphaFalloffMultiplier * (animationData.OnionFrames - i + 1);
                RenderContext onionContext = new(renderTarget, frame, resolution, finalSize, Document.Size,
                    Document.ProcessingColorSpace, samplingOptions, finalOpacity);
                onionContext.TargetOutput = targetOutput;
                finalGraph.Execute(onionContext);
            }
        }

        renderTarget.Canvas.Restore();

        return renderTexture;
    }

    private static VecI SolveRenderOutputSize(string? targetOutput, IReadOnlyNodeGraph finalGraph,
        VecI documentSize)
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

    private bool RenderInOutputSize(IReadOnlyNodeGraph finalGraph, VecI renderTargetSize, VecI finalSize)
    {
        return !HighResRendering ||
               (!HighDpiRenderNodePresent(finalGraph) && renderTargetSize.Length > finalSize.Length);
    }

    private bool ShouldRerender(VecI targetSize, Matrix3X3 matrix, ChunkResolution resolution,
        Guid viewportId,
        string targetOutput,
        IReadOnlyNodeGraph finalGraph, Dictionary<Guid, List<PreviewRenderRequest>>? previewTextures,
        RectI? visibleDocumentRegion)
    {
        if (!DocumentViewModel.SceneTextures.TryGetValue(viewportId, out var cachedTexture) ||
            cachedTexture == null ||
            cachedTexture.IsDisposed)
        {
            return true;
        }

        if (previewTextures is { Count: > 0 })
        {
            return true;
        }

        var renderState = new RenderState
        {
            ChunkResolution = resolution,
            HighResRendering = HighResRendering,
            TargetOutput = targetOutput,
            OnionFrames = Document.AnimationData.OnionFrames,
            OnionOpacity = Document.AnimationData.OnionOpacity,
            OnionSkinning = DocumentViewModel.AnimationHandler.OnionSkinningEnabledBindable,
            GraphCacheHash = finalGraph.GetCacheHash(),
            ZoomLevel = matrix.ScaleX,
            VisibleDocumentRegion =
                (RectD?)visibleDocumentRegion ?? new RectD(0, 0, Document.Size.X, Document.Size.Y)
        };

        if (lastRenderedStates.TryGetValue(viewportId, out var lastState))
        {
            if (lastState.ShouldRerender(renderState))
            {
                lastRenderedStates[viewportId] = renderState;
                return true;
            }
        }
        else
        {
            lastRenderedStates[viewportId] = renderState;
            return true;
        }

        VecI finalSize = SolveRenderOutputSize(targetOutput, finalGraph, Document.Size);
        bool renderInDocumentSize = RenderInOutputSize(finalGraph, targetSize, finalSize);
        VecI compareSize = renderInDocumentSize
            ? (VecI)(Document.Size * resolution.Multiplier())
            : targetSize;

        if (cachedTexture.Size != compareSize)
        {
            return true;
        }

        if (lastFrameTime.Frame != DocumentViewModel.AnimationHandler.ActiveFrameTime.Frame)
        {
            lastFrameTime = DocumentViewModel.AnimationHandler.ActiveFrameTime;
            return true;
        }

        foreach (var frame in DocumentViewModel.AnimationHandler.KeyFrames)
        {
            if (lastFramesVisibility.TryGetValue(frame.Id, out var lastVisibility))
            {
                if (frame.IsVisible != lastVisibility)
                {
                    lastFramesVisibility[frame.Id] = frame.IsVisible;
                    return true;
                }
            }
            else
            {
                lastFramesVisibility[frame.Id] = frame.IsVisible;
                return true;
            }
        }


        return false;
    }

    private bool HighDpiRenderNodePresent(IReadOnlyNodeGraph documentNodeGraph)
    {
        bool highDpiRenderNodePresent = false;
        documentNodeGraph.TryTraverse(n =>
        {
            if (n is IHighDpiRenderNode { AllowHighDpiRendering: true })
            {
                highDpiRenderNodePresent = true;
            }
        });

        return highDpiRenderNodePresent;
    }

    private void RenderOnionSkin(DrawingSurface target, ChunkResolution resolution, SamplingOptions sampling,
        string? targetOutput)
    {
        var animationData = Document.AnimationData;
        if (!DocumentViewModel.AnimationHandler.OnionSkinningEnabledBindable)
        {
            return;
        }

        double onionOpacity = animationData.OnionOpacity / 100.0;
        double alphaFalloffMultiplier = 1.0 / animationData.OnionFrames;

        var finalGraph = RenderingUtils.SolveFinalNodeGraph(targetOutput, Document);
        var renderOutputSize = SolveRenderOutputSize(targetOutput, finalGraph, Document.Size);

        // Render previous frames'
        for (int i = 1; i <= animationData.OnionFrames; i++)
        {
            int frame = DocumentViewModel.AnimationHandler.ActiveFrameTime.Frame - i;
            if (frame < DocumentViewModel.AnimationHandler.FirstVisibleFrame)
            {
                break;
            }

            double finalOpacity = onionOpacity * alphaFalloffMultiplier * (animationData.OnionFrames - i + 1);


            RenderContext onionContext = new(target, frame, resolution, renderOutputSize, Document.Size,
                Document.ProcessingColorSpace, sampling, finalOpacity);
            onionContext.TargetOutput = targetOutput;
            finalGraph.Execute(onionContext);
        }

        // Render next frames
        for (int i = 1; i <= animationData.OnionFrames; i++)
        {
            int frame = DocumentViewModel.AnimationHandler.ActiveFrameTime.Frame + i;
            if (frame >= DocumentViewModel.AnimationHandler.LastFrame)
            {
                break;
            }

            double finalOpacity = onionOpacity * alphaFalloffMultiplier * (animationData.OnionFrames - i + 1);
            RenderContext onionContext = new(target, frame, resolution, renderOutputSize, Document.Size,
                Document.ProcessingColorSpace, sampling, finalOpacity);
            onionContext.TargetOutput = targetOutput;
            finalGraph.Execute(onionContext);
        }
    }

    public void Dispose()
    {
        /*foreach (var texture in cachedTextures)
        {
            texture.Value?.Dispose();
        }*/
    }
}

struct RenderState
{
    public ChunkResolution ChunkResolution { get; set; }
    public bool HighResRendering { get; set; }
    public string TargetOutput { get; set; }
    public int GraphCacheHash { get; set; }
    public RectD VisibleDocumentRegion { get; set; }
    public double ZoomLevel { get; set; }
    public int OnionFrames { get; set; }
    public double OnionOpacity { get; set; }
    public bool OnionSkinning { get; set; }

    public bool ShouldRerender(RenderState other)
    {
        return !ChunkResolution.Equals(other.ChunkResolution) || HighResRendering != other.HighResRendering ||
               TargetOutput != other.TargetOutput || GraphCacheHash != other.GraphCacheHash ||
                OnionFrames != other.OnionFrames || Math.Abs(OnionOpacity - other.OnionOpacity) > 0.05 ||
               OnionSkinning != other.OnionSkinning ||
               VisibleRegionChanged(other) || ZoomDiff(other) > 0;
    }

    private bool VisibleRegionChanged(RenderState other)
    {
        return !other.VisibleDocumentRegion.IsFullyInside(VisibleDocumentRegion);
    }

    private double ZoomDiff(RenderState other)
    {
        return Math.Abs(ZoomLevel - other.ZoomLevel);
    }
}
