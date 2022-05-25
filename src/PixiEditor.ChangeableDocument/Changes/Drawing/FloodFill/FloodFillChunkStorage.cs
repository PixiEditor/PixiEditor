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

    public void DrawOnImage()
    {
        foreach (var (pos, chunk) in acquiredChunks)
        {
            image.EnqueueDrawImage(pos * ChunkResolution.Full.PixelSize(), chunk.Surface, false);
        }
    }

    public void Dispose()
    {
        foreach (var chunk in acquiredChunks.Values)
            chunk.Dispose();
        ReplacingPaint.Dispose();
    }
}
