using ChunkyImageLib.DataHolders;
using SkiaSharp;

namespace ChunkyImageLib.Operations;

internal class ClearRegionOperation : IDrawOperation
{
    RectI rect;

    public bool IgnoreEmptyChunks => true;

    public ClearRegionOperation(RectI rect)
    {
        this.rect = rect;
    }

    public void DrawOnChunk(Chunk chunk, VecI chunkPos)
    {
        VecI convPos = OperationHelper.ConvertForResolution(rect.Pos, chunk.Resolution);
        VecI convSize = OperationHelper.ConvertForResolution(rect.Size, chunk.Resolution);

        chunk.Surface.SkiaSurface.Canvas.Save();
        chunk.Surface.SkiaSurface.Canvas.ClipRect(SKRect.Create(convPos - chunkPos.Multiply(chunk.PixelSize), convSize));
        chunk.Surface.SkiaSurface.Canvas.Clear();
        chunk.Surface.SkiaSurface.Canvas.Restore();
    }

    public HashSet<VecI> FindAffectedChunks()
    {
        return OperationHelper.FindChunksTouchingRectangle(rect, ChunkPool.FullChunkSize);
    }
    public void Dispose() { }

    public IDrawOperation AsMirrored(int? verAxisX, int? horAxisY)
    {
        var newRect = rect;
        if (verAxisX is not null)
            newRect = newRect.ReflectX((int)verAxisX);
        if (horAxisY is not null)
            newRect = newRect.ReflectY((int)horAxisY);
        return new ClearRegionOperation(newRect);
    }
}
