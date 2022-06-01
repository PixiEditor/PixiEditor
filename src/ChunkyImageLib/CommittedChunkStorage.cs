using ChunkyImageLib.DataHolders;
using SkiaSharp;

namespace ChunkyImageLib;

public class CommittedChunkStorage : IDisposable
{
    private bool disposed = false;
    private List<(VecI, Chunk?)> savedChunks = new();
    private static SKPaint ReplacingPaint { get; } = new SKPaint() { BlendMode = SKBlendMode.Src };

    public CommittedChunkStorage(ChunkyImage image, HashSet<VecI> committedChunksToSave)
    {
        foreach (var chunkPos in committedChunksToSave)
        {
            Chunk copy = Chunk.Create();
            if (!image.DrawCommittedChunkOn(chunkPos, ChunkResolution.Full, copy.Surface.SkiaSurface, VecI.Zero, ReplacingPaint))
            {
                copy.Dispose();
                savedChunks.Add((chunkPos, null));
                continue;
            }
            savedChunks.Add((chunkPos, copy));
        }
    }

    public void ApplyChunksToImage(ChunkyImage image)
    {
        if (disposed)
            throw new ObjectDisposedException(nameof(CommittedChunkStorage));
        foreach (var (pos, chunk) in savedChunks)
        {
            if (chunk is null)
                image.EnqueueClearRegion(pos * ChunkPool.FullChunkSize, new(ChunkPool.FullChunkSize, ChunkPool.FullChunkSize));
            else
                image.EnqueueDrawImage(pos * ChunkPool.FullChunkSize, chunk.Surface, ReplacingPaint);
        }
    }

    public void Dispose()
    {
        if (disposed)
            return;
        disposed = true;
        foreach (var (_, chunk) in savedChunks)
        {
            if (chunk is not null)
                chunk.Dispose();
        }
    }
}
