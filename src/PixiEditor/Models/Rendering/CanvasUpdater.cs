using ChunkyImageLib.DataHolders;
using ChunkyImageLib.Operations;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;
using PixiEditor.Models.DocumentModels;
using PixiEditor.Models.Handlers;
using PixiEditor.Numerics;

namespace PixiEditor.Models.Rendering;
#nullable enable
internal class CanvasUpdater
{
    private readonly IDocument doc;
    private readonly DocumentInternalParts internals;

    private Dictionary<int, Texture> renderedFramesCache = new();
    private int lastRenderedFrameNumber = -1;
    private int lastOnionKeyFrames = -1;
    private double lastOnionOpacity = -1;

    private static readonly Paint ReplacingPaint = new() { BlendMode = BlendMode.Src };

    private static readonly Paint ClearPaint = new()
    {
        BlendMode = BlendMode.Src, Color = PixiEditor.DrawingApi.Core.ColorsImpl.Colors.Transparent
    };

    /// <summary>
    /// Affected chunks that have not been rerendered yet.
    /// </summary>
    private readonly Dictionary<ChunkResolution, HashSet<VecI>> affectedAndNonRerenderedChunks = new()
    {
        [ChunkResolution.Full] = new(),
        [ChunkResolution.Half] = new(),
        [ChunkResolution.Quarter] = new(),
        [ChunkResolution.Eighth] = new()
    };

    /// <summary>
    /// Affected chunks that have not been rerendered yet.
    /// Doesn't include chunks that were affected after the last time rerenderDelayed was true.
    /// </summary>
    private readonly Dictionary<ChunkResolution, HashSet<VecI>> nonRerenderedChunksAffectedBeforeLastRerenderDelayed =
        new()
        {
            [ChunkResolution.Full] = new(),
            [ChunkResolution.Half] = new(),
            [ChunkResolution.Quarter] = new(),
            [ChunkResolution.Eighth] = new()
        };


    public CanvasUpdater(IDocument doc, DocumentInternalParts internals)
    {
        this.doc = doc;
        this.internals = internals;
    }

    /// <summary>
    /// Don't call this outside ActionAccumulator
    /// </summary>
    public async Task UpdateGatheredChunks
        (AffectedAreasGatherer chunkGatherer, bool rerenderDelayed)
    { 
        await Task.Run(() => Render(chunkGatherer, rerenderDelayed)).ConfigureAwait(true);
    }

    /// <summary>
    /// Don't call this outside ActionAccumulator
    /// </summary>
    public void UpdateGatheredChunksSync
        (AffectedAreasGatherer chunkGatherer, bool rerenderDelayed)
    {
        Render(chunkGatherer, rerenderDelayed);
    }

    private Dictionary<ChunkResolution, HashSet<VecI>> FindChunksVisibleOnViewports(bool onDelayed, bool all)
    {
        Dictionary<ChunkResolution, HashSet<VecI>> chunks = new()
        {
            [ChunkResolution.Full] = new(),
            [ChunkResolution.Half] = new(),
            [ChunkResolution.Quarter] = new(),
            [ChunkResolution.Eighth] = new()
        };
        foreach (var (_, viewport) in internals.State.Viewports)
        {
            if (onDelayed != viewport.Delayed && !all)
                continue;

            var viewportChunks = OperationHelper.FindChunksTouchingRectangle(
                viewport.Center,
                viewport.Dimensions,
                -viewport.Angle,
                ChunkResolution.Full.PixelSize());
            chunks[viewport.Resolution].UnionWith(viewportChunks);
        }

        return chunks;
    }

    private Dictionary<ChunkResolution, HashSet<VecI>> FindGlobalChunksToRerender(AffectedAreasGatherer areasGatherer,
        bool renderDelayed)
    {
        // find all affected non rerendered chunks
        var chunksToRerender = new Dictionary<ChunkResolution, HashSet<VecI>>();
        foreach (var (res, stored) in affectedAndNonRerenderedChunks)
        {
            chunksToRerender[res] = new HashSet<VecI>(stored);
            chunksToRerender[res].UnionWith(areasGatherer.MainImageArea.Chunks);
        }

        // find all chunks that would need to be rerendered if affected
        var chunksToMaybeRerender = FindChunksVisibleOnViewports(false, renderDelayed);
        if (!renderDelayed)
        {
            var chunksOnDelayedViewports = FindChunksVisibleOnViewports(true, false);
            foreach (var (res, stored) in nonRerenderedChunksAffectedBeforeLastRerenderDelayed)
            {
                chunksOnDelayedViewports[res].IntersectWith(stored);
                chunksToMaybeRerender[res].UnionWith(chunksOnDelayedViewports[res]);
            }
        }

        // find affected chunks that need to be rerendered right now
        foreach (var (res, toRerender) in chunksToRerender)
        {
            toRerender.IntersectWith(chunksToMaybeRerender[res]);
        }

        return chunksToRerender;
    }

    private void UpdateAffectedNonRerenderedChunks(Dictionary<ChunkResolution, HashSet<VecI>> chunksToRerender,
        AffectedArea chunkGathererAffectedArea)
    {
        if (chunkGathererAffectedArea.Chunks.Count > 0)
        {
            foreach (var (res, chunks) in chunksToRerender)
            {
                affectedAndNonRerenderedChunks[res].UnionWith(chunkGathererAffectedArea.Chunks);
            }
        }

        foreach (var (res, chunks) in chunksToRerender)
        {
            affectedAndNonRerenderedChunks[res].ExceptWith(chunks);
            nonRerenderedChunksAffectedBeforeLastRerenderDelayed[res].ExceptWith(chunks);
        }
    }

    private void Render(AffectedAreasGatherer chunkGatherer, bool rerenderDelayed)
    {
        Dictionary<ChunkResolution, HashSet<VecI>> chunksToRerender =
            FindGlobalChunksToRerender(chunkGatherer, rerenderDelayed);
        
        ChunkResolution onionSkinResolution = chunksToRerender.Min(x => x.Key);

        bool updatingStoredChunks = false;
        foreach (var (res, stored) in affectedAndNonRerenderedChunks)
        {
            HashSet<VecI> storedCopy = new HashSet<VecI>(stored);
            storedCopy.IntersectWith(chunksToRerender[res]);
            if (storedCopy.Count > 0)
            {
                updatingStoredChunks = true;
                break;
            }
        }

        UpdateAffectedNonRerenderedChunks(chunksToRerender, chunkGatherer.MainImageArea);

        bool anythingToUpdate = false;
        foreach (var (_, chunks) in chunksToRerender)
        {
            anythingToUpdate |= chunks.Count > 0;
        }

        if (!anythingToUpdate)
            return;

        UpdateMainImage(chunksToRerender, updatingStoredChunks ? null : chunkGatherer.MainImageArea.GlobalArea.Value);
    }

    private void UpdateMainImage(Dictionary<ChunkResolution, HashSet<VecI>> chunksToRerender,
        RectI? globalClippingRectangle)
    {
        if (chunksToRerender.Count == 0)
            return;

        foreach (var (resolution, chunks) in chunksToRerender)
        {
            int chunkSize = resolution.PixelSize();
            RectI? globalScaledClippingRectangle = null;
            if (globalClippingRectangle is not null)
                globalScaledClippingRectangle =
                    (RectI?)((RectI)globalClippingRectangle).Scale(resolution.Multiplier()).RoundOutwards();
            
            foreach (var chunkPos in chunks)
            {
                RenderChunk(chunkPos, resolution);
                RectI chunkRect = new(chunkPos * chunkSize, new(chunkSize, chunkSize));
                if (globalScaledClippingRectangle is RectI rect)
                    chunkRect = chunkRect.Intersect(rect);
            }
        }
    }

    /*
    private void UpdateOnionSkinning(Texture lastRendered)
    {
        if (doc.AnimationHandler.OnionSkinningEnabledBindable)
        {
            if (lastRenderedFrameNumber > 0)
            {
                UpdateLastRenderedFrame(lastRendered, lastRenderedFrameNumber);
            }

            if (lastRenderedFrameNumber != doc.AnimationHandler.ActiveFrameBindable 
                || doc.AnimationHandler.OnionFramesBindable != lastOnionKeyFrames
                || Math.Abs(doc.AnimationHandler.OnionOpacityBindable - lastOnionOpacity) > 0.01)
            {
                int framesToRender = doc.AnimationHandler.OnionFramesBindable;
                using Paint onionPaint = new Paint();
                byte opacity = (byte)((doc.AnimationHandler.OnionOpacityBindable / 100f) * 255);
                onionPaint.Color = new Color(0, 0, 0, opacity); 
                onionPaint.BlendMode = BlendMode.SrcOver;

                if (doc.Renderer.OnionSkinTexture == null || doc.Renderer.OnionSkinTexture.Size != doc.SizeBindable)
                {
                    doc.Renderer.OnionSkinTexture?.Dispose();
                    doc.Renderer.OnionSkinTexture = new Texture(doc.SizeBindable);
                }

                doc.Renderer.OnionSkinTexture.DrawingSurface.Canvas.Clear();

                float alphaFalloffMultiplier = 1f / framesToRender;

                for (int i = 1; i <= framesToRender; i++)
                {
                    int previousFrameIndex = doc.AnimationHandler.ActiveFrameBindable - i;
                    int nextFrameIndex = doc.AnimationHandler.ActiveFrameBindable + i;

                    if (!renderedFramesCache.ContainsKey(previousFrameIndex))
                    {
                        RenderNextOnionSkinningFrame(previousFrameIndex);
                    }

                    if (!renderedFramesCache.ContainsKey(nextFrameIndex))
                    {
                        RenderNextOnionSkinningFrame(nextFrameIndex);
                    }

                    DrawOnionSkinningFrame(previousFrameIndex, doc.Renderer.OnionSkinTexture, onionPaint);
                    DrawOnionSkinningFrame(nextFrameIndex, doc.Renderer.OnionSkinTexture, onionPaint);

                    onionPaint.Color = onionPaint.Color.WithAlpha((byte)(opacity -
                                                                         (opacity *
                                                                          alphaFalloffMultiplier * i)));
                }
            }

            lastRenderedFrameNumber = doc.AnimationHandler.ActiveFrameBindable;
            lastOnionKeyFrames = doc.AnimationHandler.OnionFramesBindable;
            lastOnionOpacity = doc.AnimationHandler.OnionOpacityBindable;
        }
    }

    private void RenderNextOnionSkinningFrame(int frameIndex)
    {
        int firstFrame = doc.AnimationHandler.FirstFrame;
        int lastFrame = doc.AnimationHandler.LastFrame;
        if (frameIndex < firstFrame || frameIndex >= lastFrame)
            return;

        double newNormalizedTime = (double)(frameIndex - firstFrame) / (lastFrame - firstFrame);

        KeyFrameTime newFrameTime = new(frameIndex, newNormalizedTime);

        using Texture rendered = doc.Renderer.RenderDocument(newFrameTime, ChunkResolution.Full);
        UpdateLastRenderedFrame(rendered, frameIndex);
    }*/

    private void UpdateLastRenderedFrame(Texture rendered, int index)
    {
        if (renderedFramesCache.ContainsKey(index))
        {
            renderedFramesCache[index].Dispose();
            renderedFramesCache[index] = new Texture(rendered);
        }
        else
        {
            renderedFramesCache.Add(index, new Texture(rendered));
        }
    }

    private void DrawOnionSkinningFrame(int frameIndex, Texture onionSkinTexture, Paint paint)
    {
        if (frameIndex < 1 || frameIndex >= doc.AnimationHandler.LastFrame)
            return;

        if (renderedFramesCache.TryGetValue(frameIndex, out var frame))
        {
            onionSkinTexture.DrawingSurface.Canvas.DrawSurface(frame.DrawingSurface, 0, 0, paint);
        }
    }

    private void RenderChunk(VecI chunkPos, ChunkResolution resolution)
    {
        doc.Renderer.RenderChunk(chunkPos, resolution, doc.AnimationHandler.ActiveFrameTime);
    }
}
