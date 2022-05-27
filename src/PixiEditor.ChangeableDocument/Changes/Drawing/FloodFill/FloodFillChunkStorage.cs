using SkiaSharp;

namespace PixiEditor.ChangeableDocument.Changes.Drawing.FloodFill;
internal class FloodFillChunkStorage : IDisposable
{
    private readonly ChunkyImage image;
    private Dictionary<VecI, Chunk> acquiredChunks = new();
    private SKPaint ReplacingPaint { get; } = new SKPaint() { BlendMode = SKBlendMode.Src };

    public FloodFillChunkStorage(ChunkyImage image)
    {
        this.image = image;
    }

    public bool ChunkExistsInStorageOrInImage(VecI pos)
    {
        return acquiredChunks.ContainsKey(pos) || image.LatestOrCommittedChunkExists(pos);
    }

    public Chunk GetChunk(VecI pos)
    {
        if (acquiredChunks.ContainsKey(pos))
            return acquiredChunks[pos];
        Chunk chunk = Chunk.Create(ChunkResolution.Full);
        if (!image.DrawMostUpToDateChunkOn(pos, ChunkResolution.Full, chunk.Surface.SkiaSurface, new(0, 0), ReplacingPaint))
            chunk.Surface.SkiaSurface.Canvas.Clear();
        acquiredChunks[pos] = chunk;
        return chunk;
    }

    public (CommittedChunkStorage, HashSet<VecI>) DrawOnChunkyImage(ChunkyImage chunkyImage)
    {
        foreach (var (pos, chunk) in acquiredChunks)
        {
            chunkyImage.EnqueueDrawImage(pos * ChunkResolution.Full.PixelSize(), chunk.Surface, false);
        }
        var affected = chunkyImage.FindAffectedChunks();
        var affectedChunkStorage = new CommittedChunkStorage(chunkyImage, affected);
        chunkyImage.CommitChanges();
        return (affectedChunkStorage, affected);
    }

    public void Dispose()
    {
        foreach (var chunk in acquiredChunks.Values)
            chunk.Dispose();
        ReplacingPaint.Dispose();
    }
}
