using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using ChunkyImageLib.Operations;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.PaintImpl;

namespace SfmlUi.Rendering;
#nullable enable
internal class WriteableBitmapUpdater
{
    private static readonly Paint ReplacingPaint = new() { BlendMode = BlendMode.Src };
    private static readonly Paint SmoothReplacingPaint = new() { BlendMode = BlendMode.Src, FilterQuality = FilterQuality.Medium, IsAntiAliased = true };
    private static readonly Paint ClearPaint = new() { BlendMode = BlendMode.Src, Color = PixiEditor.DrawingApi.Core.ColorsImpl.Colors.Transparent };

    /// <summary>
    /// Chunks that have been updated but don't need to be re-rendered because they are out of view
    /// </summary>
    private readonly Dictionary<ChunkResolution, HashSet<VecI>> globalPostponedChunks = new()
    {
        [ChunkResolution.Full] = new(),
        [ChunkResolution.Half] = new(),
        [ChunkResolution.Quarter] = new(),
        [ChunkResolution.Eighth] = new()
    };
    private readonly Dictionary<ChunkResolution, DrawingSurface> surfaces;
    private readonly DocumentViewModel document;

    public WriteableBitmapUpdater(Dictionary<ChunkResolution, DrawingSurface> images, DocumentViewModel document)
    {
        this.surfaces = images;
        this.document = document;
    }

    private Dictionary<ChunkResolution, HashSet<VecI>> FindGlobalChunksToRerender(AffectedChunkGatherer chunkGatherer, RectD viewportRect, ChunkResolution viewportResolution)
    {
        foreach (var (_, postponed) in globalPostponedChunks)
        {
            postponed.UnionWith(chunkGatherer.MainImageChunks);
        }

        var chunksToUpdate = new Dictionary<ChunkResolution, HashSet<VecI>>() 
        { 
            [ChunkResolution.Full] = new(), 
            [ChunkResolution.Half] = new(), 
            [ChunkResolution.Quarter] = new(), 
            [ChunkResolution.Eighth] = new() 
        };

        var viewportChunks = OperationHelper.FindChunksTouchingRectangle(
            viewportRect.Center,
            viewportRect.Size,
            0,
            ChunkResolution.Full.PixelSize());

        chunksToUpdate[viewportResolution].UnionWith(viewportChunks);

        // exclude the chunks that don't need to be updated, remove chunks that will be updated from postponed
        foreach (var (res, postponed) in globalPostponedChunks)
        {
            chunksToUpdate[res].IntersectWith(postponed);
            postponed.ExceptWith(chunksToUpdate[res]);
        }

        return chunksToUpdate;
    }

    public List<DirtyRect_RenderInfo> Render(AffectedChunkGatherer chunkGatherer, RectD viewportRect, ChunkResolution viewportResolution)
    {
        Dictionary<ChunkResolution, HashSet<VecI>> chunksToRerender = FindGlobalChunksToRerender(chunkGatherer, viewportRect, viewportResolution);

        List<DirtyRect_RenderInfo> infos = new();
        UpdateMainImage(chunksToRerender, infos);

        return infos;
    }

    private void UpdateMainImage(Dictionary<ChunkResolution, HashSet<VecI>> chunksToRerender, List<DirtyRect_RenderInfo> infos)
    {
        foreach (var (resolution, chunks) in chunksToRerender)
        {
            int chunkSize = resolution.PixelSize();
            DrawingSurface screenSurface = surfaces[resolution];
            foreach (var chunkPos in chunks)
            {
                RenderChunk(chunkPos, screenSurface, resolution);
                infos.Add(new DirtyRect_RenderInfo(
                    chunkPos * chunkSize,
                    new(chunkSize, chunkSize),
                    resolution
                ));
            }
        }
    }

    private void RenderChunk(VecI chunkPos, DrawingSurface screenSurface, ChunkResolution resolution)
    {
        ChunkRenderer.MergeWholeStructure(chunkPos, resolution, document.Tracker.Document.StructureRoot).Switch(
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
    }
}
