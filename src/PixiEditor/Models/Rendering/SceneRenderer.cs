using ChunkyImageLib.DataHolders;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Workspace;
using PixiEditor.Models.Handlers;

namespace PixiEditor.Models.Rendering;

internal class SceneRenderer : IDisposable
{
    public const double ZoomDiffToRerender = 20;
    public IReadOnlyDocument Document { get; }
    public IDocument DocumentViewModel { get; }
    public bool HighResRendering { get; set; } = true;

    private Dictionary<string, Texture> cachedTextures = new();
    private bool lastHighResRendering = true;
    private int lastGraphCacheHash = -1;
    private KeyFrameTime lastFrameTime;
    private Dictionary<Guid, bool> lastFramesVisibility = new();
    private PreviewRenderer previewRenderer;

    private ChunkResolution? lastResolution;

    public SceneRenderer(IReadOnlyDocument trackerDocument, IDocument documentViewModel, PreviewRenderer previewRenderer)
    {
        Document = trackerDocument;
        DocumentViewModel = documentViewModel;
        this.previewRenderer = previewRenderer;
    }

    public void RenderScene(DrawingSurface target, ChunkResolution resolution, SamplingOptions samplingOptions,
        string? targetOutput = null)
    {
        if (Document.Renderer.IsBusy || DocumentViewModel.Busy ||
            target.DeviceClipBounds.Size.ShortestAxis <= 0) return;
        RenderOnionSkin(target, resolution, samplingOptions, targetOutput);

        string adjustedTargetOutput = targetOutput ?? "";

        IReadOnlyNodeGraph finalGraph = RenderingUtils.SolveFinalNodeGraph(targetOutput, Document);
        bool shouldRerender = ShouldRerender(target, resolution, adjustedTargetOutput, finalGraph);

        // TODO: Check if clipping to visible area improves performance on full resolution
        // Meaning zoomed big textures

        if (shouldRerender)
        {
            if (cachedTextures.ContainsKey(adjustedTargetOutput))
            {
                cachedTextures[adjustedTargetOutput]?.Dispose();
            }

            var rendered = RenderGraph(target, resolution, samplingOptions, targetOutput, finalGraph);
            //previewRenderer.RenderPreviews(DocumentViewModel.AnimationHandler.ActiveFrameTime);
            cachedTextures[adjustedTargetOutput] = rendered;
            return;
        }

        var cachedTexture = cachedTextures[adjustedTargetOutput];
        Matrix3X3 matrixDiff = SolveMatrixDiff(target, cachedTexture);
        int saved = target.Canvas.Save();
        target.Canvas.SetMatrix(matrixDiff);
        if (samplingOptions == SamplingOptions.Default)
        {
            target.Canvas.DrawSurface(cachedTexture.DrawingSurface, 0, 0);
        }
        else
        {
            using var img = cachedTexture.DrawingSurface.Snapshot();
            target.Canvas.DrawImage(img, 0, 0, samplingOptions);
        }

        target.Canvas.RestoreToCount(saved);
    }

    private Texture RenderGraph(DrawingSurface target, ChunkResolution resolution, SamplingOptions samplingOptions,
        string? targetOutput,
        IReadOnlyNodeGraph finalGraph)
    {
        DrawingSurface renderTarget = target;
        Texture? renderTexture = null;
        int restoreCanvasTo;

        VecI finalSize = SolveRenderOutputSize(targetOutput, finalGraph, Document.Size);
        if (RenderInOutputSize(finalGraph))
        {
            finalSize = (VecI)(finalSize * resolution.Multiplier());

            renderTexture = Texture.ForProcessing(finalSize, Document.ProcessingColorSpace);
            renderTarget = renderTexture.DrawingSurface;
            renderTexture.DrawingSurface.Canvas.Save();
            renderTexture.DrawingSurface.Canvas.Scale((float)resolution.Multiplier());

            restoreCanvasTo = target.Canvas.Save();
            target.Canvas.Scale((float)resolution.InvertedMultiplier());
        }
        else
        {
            renderTexture = Texture.ForProcessing(renderTarget.DeviceClipBounds.Size, Document.ProcessingColorSpace);

            renderTarget = renderTexture.DrawingSurface;

            restoreCanvasTo = target.Canvas.Save();
            renderTarget.Canvas.Save();

            renderTarget.Canvas.SetMatrix(target.Canvas.TotalMatrix);
            target.Canvas.SetMatrix(Matrix3X3.Identity);
            renderTarget.Canvas.ClipRect(new RectD(0, 0, finalSize.X, finalSize.Y));
            resolution = ChunkResolution.Full;
        }

        RenderContext context = new(renderTarget, DocumentViewModel.AnimationHandler.ActiveFrameTime,
            resolution, finalSize, Document.Size, Document.ProcessingColorSpace, samplingOptions);
        context.TargetOutput = targetOutput;
        finalGraph.Execute(context);

        if (renderTexture != null)
        {
            if (samplingOptions == SamplingOptions.Default)
            {
                target.Canvas.DrawSurface(renderTexture.DrawingSurface, 0, 0);
            }
            else
            {
                using var snapshot = renderTexture.DrawingSurface.Snapshot();
                target.Canvas.DrawImage(snapshot, 0, 0, samplingOptions);
            }

            target.Canvas.RestoreToCount(restoreCanvasTo);
        }

        return renderTexture;
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

    private bool RenderInOutputSize(IReadOnlyNodeGraph finalGraph)
    {
        return !HighResRendering || !HighDpiRenderNodePresent(finalGraph);
    }

    private bool ShouldRerender(DrawingSurface target, ChunkResolution resolution, string? targetOutput,
        IReadOnlyNodeGraph finalGraph)
    {
        if (!cachedTextures.TryGetValue(targetOutput ?? "", out var cachedTexture) || cachedTexture == null ||
            cachedTexture.IsDisposed)
        {
            return true;
        }

        if (lastResolution != resolution)
        {
            lastResolution = resolution;
            return true;
        }

        if (lastHighResRendering != HighResRendering)
        {
            lastHighResRendering = HighResRendering;
            return true;
        }

        bool renderInDocumentSize = RenderInOutputSize(finalGraph);
        VecI compareSize = renderInDocumentSize
            ? (VecI)(Document.Size * resolution.Multiplier())
            : target.DeviceClipBounds.Size;

        if (cachedTexture.DrawingSurface.DeviceClipBounds.Size != compareSize)
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

        if (!renderInDocumentSize)
        {
            double lengthDiff = target.LocalClipBounds.Size.Length -
                                cachedTexture.DrawingSurface.LocalClipBounds.Size.Length;
            if (lengthDiff > 0 || target.LocalClipBounds.Pos != cachedTexture.DrawingSurface.LocalClipBounds.Pos ||
                lengthDiff < -ZoomDiffToRerender)
            {
                return true;
            }
        }

        int currentGraphCacheHash = finalGraph.GetCacheHash();
        if (lastGraphCacheHash != currentGraphCacheHash)
        {
            lastGraphCacheHash = currentGraphCacheHash;
            return true;
        }

        return false;
    }

    private Matrix3X3 SolveMatrixDiff(DrawingSurface target, Texture cachedTexture)
    {
        Matrix3X3 old = cachedTexture.DrawingSurface.Canvas.TotalMatrix;
        Matrix3X3 current = target.Canvas.TotalMatrix;

        Matrix3X3 solveMatrixDiff = current.Concat(old.Invert());
        return solveMatrixDiff;
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

    private void RenderOnionSkin(DrawingSurface target, ChunkResolution resolution, SamplingOptions sampling, string? targetOutput)
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
        foreach (var texture in cachedTextures)
        {
            texture.Value?.Dispose();
        }
    }
}
