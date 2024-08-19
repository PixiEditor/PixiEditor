using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Threading;
using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using ChunkyImageLib.Operations;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.DrawingApi.Core.Surfaces.ImageData;
using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;
using PixiEditor.Models.DocumentModels;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Rendering.RenderInfos;
using PixiEditor.Numerics;

namespace PixiEditor.Models.Rendering;
#nullable enable
internal class CanvasUpdater
{
    private readonly IDocument doc;
    private readonly DocumentInternalParts internals;

    private Dictionary<int, Texture> renderedFramesCache = new();
    private int lastRenderedFrameNumber = -1;

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
    public async Task<List<IRenderInfo>> UpdateGatheredChunks
        (AffectedAreasGatherer chunkGatherer, bool rerenderDelayed)
    {
        return await Task.Run(() => Render(chunkGatherer, rerenderDelayed)).ConfigureAwait(true);
    }

    /// <summary>
    /// Don't call this outside ActionAccumulator
    /// </summary>
    public List<IRenderInfo> UpdateGatheredChunksSync
        (AffectedAreasGatherer chunkGatherer, bool rerenderDelayed)
    {
        return Render(chunkGatherer, rerenderDelayed);
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

    private List<IRenderInfo> Render(AffectedAreasGatherer chunkGatherer, bool rerenderDelayed)
    {
        Dictionary<ChunkResolution, HashSet<VecI>> chunksToRerender =
            FindGlobalChunksToRerender(chunkGatherer, rerenderDelayed);

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
            return new();

        List<IRenderInfo> infos = new();
        UpdateMainImage(chunksToRerender, updatingStoredChunks ? null : chunkGatherer.MainImageArea.GlobalArea.Value,
            infos);
        return infos;
    }

    private void UpdateMainImage(Dictionary<ChunkResolution, HashSet<VecI>> chunksToRerender,
        RectI? globalClippingRectangle, List<IRenderInfo> infos)
    {
        if (chunksToRerender.Count == 0)
            return;

        ChunkResolution onionSkinResolution = chunksToRerender.Min(x => x.Key);
        UpdateOnionSkinning(doc.Surfaces[onionSkinResolution]);

        foreach (var (resolution, chunks) in chunksToRerender)
        {
            int chunkSize = resolution.PixelSize();
            RectI? globalScaledClippingRectangle = null;
            if (globalClippingRectangle is not null)
                globalScaledClippingRectangle =
                    (RectI?)((RectI)globalClippingRectangle).Scale(resolution.Multiplier()).RoundOutwards();

            Texture screenSurface = doc.Surfaces[resolution];
            foreach (var chunkPos in chunks)
            {
                RenderChunk(chunkPos, screenSurface, resolution, globalClippingRectangle,
                    globalScaledClippingRectangle);
                RectI chunkRect = new(chunkPos * chunkSize, new(chunkSize, chunkSize));
                if (globalScaledClippingRectangle is RectI rect)
                    chunkRect = chunkRect.Intersect(rect);

                infos.Add(new DirtyRect_RenderInfo(
                    chunkRect.Pos,
                    chunkRect.Size,
                    resolution
                ));
            }
        }
    }

    private void UpdateOnionSkinning(Texture lastRendered)
    {
        if (doc.AnimationHandler.OnionSkinningEnabledBindable)
        {
            if (lastRenderedFrameNumber > 0)
            {
                UpdateLastRenderedFrame(lastRendered, lastRenderedFrameNumber);
            }

            if (lastRenderedFrameNumber != doc.AnimationHandler.ActiveFrameBindable)
            {
                int previousFrameIndex = doc.AnimationHandler.ActiveFrameBindable - 1;
                int nextFrameIndex = doc.AnimationHandler.ActiveFrameBindable + 1;

                if (doc.Renderer.OnionSkinTexture == null || doc.Renderer.OnionSkinTexture.Size != doc.SizeBindable)
                {
                    doc.Renderer.OnionSkinTexture?.Dispose();
                    doc.Renderer.OnionSkinTexture = new Texture(doc.SizeBindable);
                }

                doc.Renderer.OnionSkinTexture.DrawingSurface.Canvas.Clear();

                if (!renderedFramesCache.ContainsKey(previousFrameIndex))
                {
                    RenderNextOnionSkinningFrame(previousFrameIndex);
                }

                if (!renderedFramesCache.ContainsKey(nextFrameIndex))
                {
                    RenderNextOnionSkinningFrame(nextFrameIndex);
                }

                DrawOnionSkinningFrame(previousFrameIndex, doc.Renderer.OnionSkinTexture);
                DrawOnionSkinningFrame(nextFrameIndex, doc.Renderer.OnionSkinTexture);
            }

            lastRenderedFrameNumber = doc.AnimationHandler.ActiveFrameBindable;
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
    }

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

    private void DrawOnionSkinningFrame(int frameIndex, Texture onionSkinTexture)
    {
        if (frameIndex < 1 || frameIndex >= doc.AnimationHandler.LastFrame)
            return;
        
        if (renderedFramesCache.TryGetValue(frameIndex, out var frame))
        {
            onionSkinTexture.DrawingSurface.Canvas.DrawSurface(frame.DrawingSurface, 0, 0);
        }
    }

    private void RenderChunk(VecI chunkPos, Texture screenSurface, ChunkResolution resolution,
        RectI? globalClippingRectangle, RectI? globalScaledClippingRectangle)
    {
        if (screenSurface is null || screenSurface.IsDisposed)
            return;


        doc.Renderer.RenderChunk(chunkPos, resolution, doc.AnimationHandler.ActiveFrameTime, globalClippingRectangle)
            .Switch(
                (Chunk chunk) =>
                {
                    if (screenSurface.IsDisposed) return;

                    if (globalScaledClippingRectangle is not null)
                    {
                        screenSurface.DrawingSurface.Canvas.Save();
                        screenSurface.DrawingSurface.Canvas.ClipRect((RectD)globalScaledClippingRectangle);
                    }

                    screenSurface.DrawingSurface.Canvas.DrawSurface(
                        chunk.Surface.DrawingSurface,
                        chunkPos.Multiply(new VecI(resolution.PixelSize())), ReplacingPaint);
                    chunk.Dispose();


                    if (globalScaledClippingRectangle is not null)
                        screenSurface.DrawingSurface.Canvas.Restore();
                },
                (EmptyChunk _) =>
                {
                    if (screenSurface.IsDisposed) return;

                    if (globalScaledClippingRectangle is not null)
                    {
                        screenSurface.DrawingSurface.Canvas.Save();
                        screenSurface.DrawingSurface.Canvas.ClipRect((RectD)globalScaledClippingRectangle);
                    }

                    var pos = chunkPos * resolution.PixelSize();
                    screenSurface.DrawingSurface.Canvas.DrawRect(pos.X, pos.Y, resolution.PixelSize(),
                        resolution.PixelSize(), ClearPaint);

                    if (globalScaledClippingRectangle is not null)
                        screenSurface.DrawingSurface.Canvas.Restore();
                });
    }
}
