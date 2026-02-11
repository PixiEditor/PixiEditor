using Avalonia.Input;
using ChunkyImageLib.DataHolders;
﻿using Avalonia.Threading;
using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using ChunkyImageLib.Operations;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core.Surfaces;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Brushes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Workspace;
using PixiEditor.ChangeableDocument.Rendering.ContextData;
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

    public IReadOnlyDictionary<Guid, RenderState> LastRenderedStates => lastRenderedStates;
    private Dictionary<Guid, RenderState> lastRenderedStates = new();
    private int lastGraphCacheHash = -1;
    private Dictionary<Guid, KeyFrameTime> lastFrameTimes = new();
    private Dictionary<Guid, bool> lastFramesVisibility = new();

    private TextureCache textureCache = new();

    public SceneRenderer(IReadOnlyDocument trackerDocument, IDocument documentViewModel)
    {
        Document = trackerDocument;
        DocumentViewModel = documentViewModel;
    }

    public async Task RecordRender(Dictionary<Guid, ViewportInfo> stateViewports, AffectedArea affectedArea,
        bool updateDelayed, Dictionary<Guid, List<PreviewRenderRequest>>? previewTextures, bool immediateRender)
    {
        Render(stateViewports, affectedArea, updateDelayed, true, previewTextures);
    }

    public async Task RenderAsync(Dictionary<Guid, ViewportInfo> stateViewports, AffectedArea affectedArea,
        bool updateDelayed, Dictionary<Guid, List<PreviewRenderRequest>>? previewTextures, bool immediateRender)
    {
        if (immediateRender)
        {
            Render(stateViewports, affectedArea, updateDelayed, false, previewTextures);
            return;
        }

        await DrawingBackendApi.Current.RenderingDispatcher.InvokeInBackgroundAsync(() =>
        {
            Render(stateViewports, affectedArea, updateDelayed, false, previewTextures);
        });
    }

    public void RenderSync(Dictionary<Guid, ViewportInfo> stateViewports, AffectedArea affectedAreasMainImageArea,
        bool updateDelayed, Dictionary<Guid, List<PreviewRenderRequest>>? previewTextures)
    {
        Render(stateViewports, affectedAreasMainImageArea, updateDelayed, false, previewTextures);
    }

    private void Render(Dictionary<Guid, ViewportInfo> stateViewports, AffectedArea affectedArea, bool updateDelayed,
        bool debugRecord,
        Dictionary<Guid, List<PreviewRenderRequest>>? previewTextures)
    {
        using var ctx = DrawingBackendApi.Current.RenderingDispatcher.EnsureContext();
        int renderedCount = 0;
        int graphHash = Document.NodeGraph.GetCacheHash();
        foreach (var viewport in stateViewports)
        {
            if (viewport.Value.Delayed && !updateDelayed)
            {
                continue;
            }

            if (viewport.Value.RealDimensions.ShortestAxis <= 0 ||
                Math.Abs(viewport.Value.RealDimensions.LongestAxis - double.MaxValue) < double.Epsilon) continue;

            var rendered = RenderScene(viewport.Value, affectedArea, graphHash, debugRecord && viewport.Value.IsScene,
                previewTextures);
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
            RenderOnlyPreviews(affectedArea, previewTextures, graphHash);
        }

        lastGraphCacheHash = Document.NodeGraph.GetCacheHash(); // Update the graph hash after rendering, in case it changed during rendering
    }

    private void RenderOnlyPreviews(AffectedArea affectedArea,
        Dictionary<Guid, List<PreviewRenderRequest>> previewTextures, int graphCacheHash)
    {
        ViewportInfo previewGenerationViewport = new()
        {
            RealDimensions = new VecD(1, 1),
            ViewportData = new ViewportData(Matrix3X3.Identity, new VecD(1, 1), 0, false, false),
            Id = Guid.NewGuid(),
            Resolution = ChunkResolution.Full,
            Sampling = SamplingOptions.Bilinear,
            EditorData = new EditorData(Colors.White, Colors.Black),
            VisibleDocumentRegion = null,
            RenderOutput = "DEFAULT",
            Delayed = false
        };

        var rendered = RenderScene(previewGenerationViewport, affectedArea, graphCacheHash, false, previewTextures);
        rendered.Dispose();
    }

    public Texture? RenderScene(ViewportInfo viewport, AffectedArea affectedArea,
        int graphCacheHash,
        bool debugRecord = false,
        Dictionary<Guid, List<PreviewRenderRequest>>? previewTextures = null)
    {
        /*if (Document.Renderer.IsBusy || DocumentViewModel.Busy ||
            target.DeviceClipBounds.Size.ShortestAxis <= 0) return;*/

        /*TODO:
         - [ ] Rendering optimizer
         - [?] Render thread and proper locking/synchronization - check render-thread branch (both drawie and pixieditor)
               but be aware, this is a nightmare and good luck
         */

        VecI renderTargetSize = (VecI)viewport.RealDimensions;

        Matrix3X3 targetMatrix = viewport.ViewportData.Transform;
        Guid viewportId = viewport.Id;
        ChunkResolution resolution = viewport.Resolution;
        SamplingOptions samplingOptions = viewport.Sampling;
        RectI? visibleDocumentRegion = viewport.VisibleDocumentRegion;
        PointerInfo pointerInfo = viewport.PointerInfo;
        KeyboardInfo keyboardInfo = viewport.KeyboardInfo;
        EditorData editorData = viewport.EditorData;
        string? targetOutput = viewport.RenderOutput.Equals("DEFAULT", StringComparison.InvariantCultureIgnoreCase)
            ? null
            : viewport.RenderOutput;
        bool isFullViewportRender = false;

        IReadOnlyNodeGraph finalGraph = RenderingUtils.SolveFinalNodeGraph(targetOutput, Document);

        if (targetOutput != null)
        {
            var outputNode = finalGraph.AllNodes.FirstOrDefault(n =>
                n is CustomOutputNode outputNode && outputNode.OutputName.Value == targetOutput);

            if (outputNode is CustomOutputNode customOutputNode)
            {
                isFullViewportRender = customOutputNode.FullViewportRender.Value;
                visibleDocumentRegion = isFullViewportRender
                    ? null
                    : viewport.VisibleDocumentRegion;
                resolution = ChunkResolution.Full;
            }
        }

        float oversizeFactor = 1;
        if (visibleDocumentRegion != null && viewport.IsScene &&
            visibleDocumentRegion.Value != new RectI(0, 0, Document.Size.X, Document.Size.Y))
        {
            visibleDocumentRegion = (RectI)visibleDocumentRegion.Value.Scale(OversizeFactor,
                visibleDocumentRegion.Value.Center);
            oversizeFactor = OversizeFactor;
        }

        bool shouldRerender =
            ShouldRerender(renderTargetSize, isFullViewportRender ? Matrix3X3.Identity : targetMatrix, resolution,
                viewportId, targetOutput, finalGraph,
                previewTextures, visibleDocumentRegion, oversizeFactor, out bool fullAffectedArea, out RenderState renderState) ||
            debugRecord;

        shouldRerender |= lastGraphCacheHash != graphCacheHash;

        if (shouldRerender)
        {
            affectedArea = fullAffectedArea && viewport.VisibleDocumentRegion.HasValue
                ? new AffectedArea(OperationHelper.FindChunksTouchingRectangle(viewport.VisibleDocumentRegion.Value,
                    ChunkyImage.FullChunkSize))
                : affectedArea;
            var tex = RenderGraph(renderTargetSize, targetMatrix, viewportId, resolution, samplingOptions, affectedArea,
                visibleDocumentRegion, targetOutput, viewport.IsScene, oversizeFactor,
                pointerInfo, keyboardInfo, editorData, viewport.ViewportData, debugRecord, finalGraph, previewTextures);

            lastRenderedStates[viewportId] = renderState;
            return tex;
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
        PointerInfo pointerInfo,
        KeyboardInfo keyboardInfo,
        EditorData editorData,
        ViewportData viewportData,
        bool debugRecord,
        IReadOnlyNodeGraph finalGraph, Dictionary<Guid, List<PreviewRenderRequest>>? previewTextures)
    {
        DrawingSurface renderTarget = null;
        Texture? renderTexture = null;
        int restoreCanvasTo;

        VecI finalSize = SolveRenderOutputSize(targetOutput, finalGraph, Document.Size, renderTargetSize,
            out bool isFullViewportRender);
        if (isFullViewportRender)
        {
            renderTexture =
                textureCache.RequestTexture(viewportId.GetHashCode(), renderTargetSize, Document.ProcessingColorSpace);
            renderTarget = renderTexture.DrawingSurface;
            renderTarget.Canvas.Save();
        }
        else
        {
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
                RenderContext onionContext = new(renderTarget.Canvas, frame, resolution, finalSize, Document.Size,
                    Document.ProcessingColorSpace, samplingOptions, Document.NodeGraph, finalOpacity);
                onionContext.TargetOutput = targetOutput;
                onionContext.VisibleDocumentRegion = visibleDocumentRegion;
                onionContext.ViewportData = viewportData;
                finalGraph.Execute(onionContext);
            }
        }

        RenderContext context = new(renderTarget.Canvas, DocumentViewModel.AnimationHandler.ActiveFrameTime,
            resolution, finalSize, Document.Size, Document.ProcessingColorSpace, samplingOptions, Document.NodeGraph);
        context.PointerInfo = pointerInfo;
        context.KeyboardInfo = keyboardInfo;
        context.EditorData = editorData;

        context.TargetOutput = targetOutput;
        context.AffectedArea = area;
        context.VisibleDocumentRegion = visibleDocumentRegion;
        context.PreviewTextures = previewTextures;
        context.ViewportData = viewportData;
        if (debugRecord)
        {
            using DrawingRecorder recorder = new DrawingRecorder();
            var recordingCanvas = recorder.BeginRecording(new RectD(0, 0, renderTargetSize.X, renderTargetSize.Y));
            recordingCanvas.SetMatrix(context.RenderSurface.TotalMatrix);
            context.RenderSurface = recordingCanvas;
            finalGraph.Execute(context);
            var picture = recorder.EndRecordingImmutable();
            using FileStream fs = new FileStream("data.skp", FileMode.Create, FileAccess.Write);
            picture.Serialize(fs);
        }
        else
        {
            finalGraph.Execute(context);
        }

        ExecuteBrushOutputPreviews(finalGraph, previewTextures, context);

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
                RenderContext onionContext = new(renderTarget.Canvas, frame, resolution, finalSize, Document.Size,
                    Document.ProcessingColorSpace, samplingOptions, Document.NodeGraph, finalOpacity);
                onionContext.TargetOutput = targetOutput;
                onionContext.VisibleDocumentRegion = visibleDocumentRegion;
                onionContext.ViewportData = viewportData;
                finalGraph.Execute(onionContext);
            }
        }

        renderTarget.Canvas.Restore();

        return renderTexture;
    }

    private static void ExecuteBrushOutputPreviews(IReadOnlyNodeGraph finalGraph,
        Dictionary<Guid, List<PreviewRenderRequest>>? previewTextures, RenderContext ctx)
    {
        if (previewTextures == null || previewTextures.Count == 0)
            return;

        BrushOutputNode? brushOutputNode = null;
        HashSet<BrushOutputNode> allBrushOutputNodes = finalGraph.AllNodes.OfType<BrushOutputNode>().ToHashSet();
        do
        {
            if (previewTextures.Count > 0)
            {
                brushOutputNode = allBrushOutputNodes.FirstOrDefault(n => previewTextures.ContainsKey(n.Id));
                if (brushOutputNode == null)
                    break;

                finalGraph.Execute(brushOutputNode, ctx);
                allBrushOutputNodes.Remove(brushOutputNode);
            }
        } while (brushOutputNode != null && previewTextures.Count > 0);
    }

    private static VecI SolveRenderOutputSize(string? targetOutput, IReadOnlyNodeGraph finalGraph,
        VecI documentSize, VecI viewportSize, out bool isFullViewportRender)
    {
        VecI finalSize = documentSize;
        isFullViewportRender = false;
        if (targetOutput != null)
        {
            var outputNode = finalGraph.AllNodes.FirstOrDefault(n =>
                n is CustomOutputNode outputNode && outputNode.OutputName.Value == targetOutput);

            if (outputNode is CustomOutputNode customOutputNode)
            {
                if (customOutputNode.FullViewportRender.Value)
                {
                    finalSize = viewportSize;
                    isFullViewportRender = true;
                }
                else if (customOutputNode.Size.Value.ShortestAxis > 0)
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
        RectI? visibleDocumentRegion, float oversizeFactor, out bool fullAffectedArea, out RenderState renderState)
    {
        renderState = new RenderState
        {
            ChunkResolution = resolution,
            HighResRendering = HighResRendering,
            TargetOutput = targetOutput,
            OnionFrames = Document.AnimationData.OnionFrames,
            OnionOpacity = Document.AnimationData.OnionOpacity,
            OnionSkinning = DocumentViewModel.AnimationHandler.OnionSkinningEnabledBindable,
            ZoomLevel = matrix.ScaleX,
            FallbackFramesToLayer = Document.AnimationData.FallbackAnimationToLayerImage,
            VisibleDocumentRegion =
                (RectD?)visibleDocumentRegion ?? new RectD(0, 0, Document.Size.X, Document.Size.Y)
        };

        fullAffectedArea = false;
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

        if (lastRenderedStates.TryGetValue(viewportId, out var lastState))
        {
            if (lastState.ShouldRerender(renderState))
            {
                fullAffectedArea = lastState.ZoomLevel > renderState.ZoomLevel;
                return true;
            }
        }

        VecI finalSize = SolveRenderOutputSize(targetOutput, finalGraph, Document.Size, targetSize, out _);
        bool renderInDocumentSize = RenderInOutputSize(finalGraph, targetSize, finalSize);
        VecI compareSize = renderInDocumentSize
            ? (VecI)(Document.Size * resolution.Multiplier())
            : targetSize;

        if (cachedTexture.Size != (VecI)(compareSize * oversizeFactor))
        {
            return true;
        }

        if (!lastFrameTimes.TryGetValue(viewportId, out var frameTime) ||
            frameTime.Frame != DocumentViewModel.AnimationHandler.ActiveFrameTime.Frame)
        {
            lastFrameTimes[viewportId] = DocumentViewModel.AnimationHandler.ActiveFrameTime;
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
    public RectD VisibleDocumentRegion { get; init; }
    public double ZoomLevel { get; init; }
    public int OnionFrames { get; init; }
    public double OnionOpacity { get; init; }
    public bool OnionSkinning { get; init; }
    public bool FallbackFramesToLayer { get; init; }

    public bool ShouldRerender(RenderState other)
    {
        return ChunkResolution > other.ChunkResolution || HighResRendering != other.HighResRendering ||
               TargetOutput != other.TargetOutput ||
               OnionFrames != other.OnionFrames || Math.Abs(OnionOpacity - other.OnionOpacity) > 0.05 ||
               FallbackFramesToLayer != other.FallbackFramesToLayer ||
               OnionSkinning != other.OnionSkinning ||
               VisibleRegionChanged(other) || ZoomDiffRequiresRender(other);
    }

    private bool VisibleRegionChanged(RenderState other)
    {
        return !other.VisibleDocumentRegion.IsFullyInside(VisibleDocumentRegion);
    }

    private bool ZoomDiffRequiresRender(RenderState other)
    {
        bool fullyVisible = !VisibleRegionChanged(other);
        double diff = ZoomLevel - other.ZoomLevel;
        if (!fullyVisible)
        {
            return Math.Abs(diff) > 0;
        }

        return diff < 0;
    }
}
