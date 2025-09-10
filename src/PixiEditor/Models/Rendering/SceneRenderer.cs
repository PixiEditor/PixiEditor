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

internal class SceneRenderer
{
    public const double ZoomDiffToRerender = 20;
    public const float OversizeFactor = 1.25f;
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
        await DrawingBackendApi.Current.RenderingDispatcher.InvokeInBackgroundAsync(() =>
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

        float oversizeFactor = 1;
        if (visibleDocumentRegion != null && viewport.IsScene && visibleDocumentRegion.Value != new RectI(0, 0, Document.Size.X, Document.Size.Y))
        {
            visibleDocumentRegion = (RectI)visibleDocumentRegion.Value.Scale(OversizeFactor,
                visibleDocumentRegion.Value.Center);
            oversizeFactor = OversizeFactor;
        }

        bool shouldRerender =
            ShouldRerender(renderTargetSize, targetMatrix, resolution, viewportId, targetOutput, finalGraph,
                previewTextures, visibleDocumentRegion, oversizeFactor);


        if (shouldRerender)
        {
            return RenderGraph(renderTargetSize, targetMatrix, viewportId, resolution, samplingOptions, affectedArea,
                visibleDocumentRegion, targetOutput, viewport.IsScene, oversizeFactor, finalGraph, previewTextures);
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
        float oversizeFactor,
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
            var bufferedSize = (VecI)(renderTargetSize * oversizeFactor);
            renderTexture = textureCache.RequestTexture(viewportId.GetHashCode(), bufferedSize,
                Document.ProcessingColorSpace);

            var bufferedMatrix = targetMatrix.PostConcat(Matrix3X3.CreateTranslation(
                (bufferedSize.X - renderTargetSize.X) / 2.0,
                (bufferedSize.Y - renderTargetSize.Y) / 2.0));

            renderTarget = renderTexture.DrawingSurface;
            renderTarget.Canvas.SetMatrix(bufferedMatrix);
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
                onionContext.VisibleDocumentRegion = visibleDocumentRegion;
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
                onionContext.VisibleDocumentRegion = visibleDocumentRegion;
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
        RectI? visibleDocumentRegion, float oversizeFactor)
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

        if (cachedTexture.Size != (VecI)(compareSize * oversizeFactor))
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
}

readonly struct RenderState
{
    public ChunkResolution ChunkResolution { get; init; }
    public bool HighResRendering { get; init; }
    public string TargetOutput { get; init; }
    public int GraphCacheHash { get; init; }
    public RectD VisibleDocumentRegion { get; init; }
    public double ZoomLevel { get; init; }
    public int OnionFrames { get; init; }
    public double OnionOpacity { get; init; }
    public bool OnionSkinning { get; init; }

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
