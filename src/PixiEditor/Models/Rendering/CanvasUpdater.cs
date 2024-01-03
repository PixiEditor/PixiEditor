using System.Collections.Generic;
using System.Printing;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.Xml;
using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using ChunkyImageLib.Operations;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.PaintImpl;
using PixiEditor.Models.DocumentModels;
using PixiEditor.Models.Rendering.RenderInfos;
using PixiEditor.ViewModels.SubViewModels.Document;

namespace PixiEditor.Models.Rendering;
#nullable enable
internal class CanvasUpdater
{
    private readonly DocumentViewModel doc;
    private readonly DocumentInternalParts internals;

    private static readonly Paint ReplacingPaint = new() { BlendMode = BlendMode.Src };
    private static readonly Paint ClearPaint = new() { BlendMode = BlendMode.Src, Color = PixiEditor.DrawingApi.Core.ColorsImpl.Colors.Transparent };

    /// <summary>
    /// Affected chunks that have not been rerendered yet.
    /// </summary>
    private readonly Dictionary<ChunkResolution, HashSet<VecI>> affectedAndNonRerenderedChunks = new() { [ChunkResolution.Full] = new(), [ChunkResolution.Half] = new(), [ChunkResolution.Quarter] = new(), [ChunkResolution.Eighth] = new() };

    /// <summary>
    /// Affected chunks that have not been rerendered yet.
    /// Doesn't include chunks that were affected after the last time rerenderDelayed was true.
    /// </summary>
    private readonly Dictionary<ChunkResolution, HashSet<VecI>> nonRerenderedChunksAffectedBeforeLastRerenderDelayed = new() { [ChunkResolution.Full] = new(), [ChunkResolution.Half] = new(), [ChunkResolution.Quarter] = new(), [ChunkResolution.Eighth] = new() };


    public CanvasUpdater(DocumentViewModel doc, DocumentInternalParts internals)
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

    private Dictionary<ChunkResolution, HashSet<VecI>> FindGlobalChunksToRerender(AffectedAreasGatherer areasGatherer, bool renderDelayed)
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

    private void UpdateAffectedNonRerenderedChunks(Dictionary<ChunkResolution, HashSet<VecI>> chunksToRerender, AffectedArea chunkGathererAffectedArea)
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
        Dictionary<ChunkResolution, HashSet<VecI>> chunksToRerender = FindGlobalChunksToRerender(chunkGatherer, rerenderDelayed);

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
        UpdateMainImage(chunksToRerender, updatingStoredChunks ? null : chunkGatherer.MainImageArea.GlobalArea.Value, infos);
        return infos;
    }

    private void UpdateMainImage(Dictionary<ChunkResolution, HashSet<VecI>> chunksToRerender, RectI? globalClippingRectangle, List<IRenderInfo> infos)
    {
        foreach (var (resolution, chunks) in chunksToRerender)
        {
            int chunkSize = resolution.PixelSize();
            RectI? globalScaledClippingRectangle = null;
            if (globalClippingRectangle is not null)
                globalScaledClippingRectangle = (RectI?)((RectI)globalClippingRectangle).Scale(resolution.Multiplier()).RoundOutwards();

            DrawingSurface screenSurface = doc.Surfaces[resolution];
            foreach (var chunkPos in chunks)
            {
                RenderChunk(chunkPos, screenSurface, resolution, globalClippingRectangle, globalScaledClippingRectangle);
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

    private void RenderChunk(VecI chunkPos, DrawingSurface screenSurface, ChunkResolution resolution, RectI? globalClippingRectangle, RectI? globalScaledClippingRectangle)
    {
        if (globalScaledClippingRectangle is not null)
        {
            screenSurface.Canvas.Save();
            screenSurface.Canvas.ClipRect((RectD)globalScaledClippingRectangle);
        }

        ChunkRenderer.MergeWholeStructure(chunkPos, resolution, internals.Tracker.Document.StructureRoot, globalClippingRectangle).Switch(
            (Chunk chunk) =>
            {
                screenSurface.Canvas.DrawSurface(chunk.Surface.DrawingSurface, chunkPos.Multiply(chunk.PixelSize), ReplacingPaint);
                chunk.Dispose();
            },
            (EmptyChunk _) =>
            {
                var pos = chunkPos * resolution.PixelSize();
                screenSurface.Canvas.DrawRect(pos.X, pos.Y, resolution.PixelSize(), resolution.PixelSize(), ClearPaint);
            });

        if (globalScaledClippingRectangle is not null)
            screenSurface.Canvas.Restore();
    }
}
