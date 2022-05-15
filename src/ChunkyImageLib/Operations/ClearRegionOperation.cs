using ChunkyImageLib.DataHolders;
using SkiaSharp;

namespace ChunkyImageLib.Operations;

internal class ClearRegionOperation : IDrawOperation
{
    VecI pos;
    VecI size;

    public bool IgnoreEmptyChunks => true;

    public ClearRegionOperation(VecI pos, VecI size)
    {
        this.pos = pos;
        this.size = size;
    }

    public void DrawOnChunk(Chunk chunk, VecI chunkPos)
    {
        VecI convPos = OperationHelper.ConvertForResolution(pos, chunk.Resolution);
        VecI convSize = OperationHelper.ConvertForResolution(size, chunk.Resolution);

        chunk.Surface.SkiaSurface.Canvas.Save();
        chunk.Surface.SkiaSurface.Canvas.ClipRect(SKRect.Create(convPos - chunkPos.Multiply(chunk.PixelSize), convSize));
        chunk.Surface.SkiaSurface.Canvas.Clear();
        chunk.Surface.SkiaSurface.Canvas.Restore();
    }

    public HashSet<VecI> FindAffectedChunks()
    {
        return OperationHelper.FindChunksFullyInsideRectangle(pos, size, ChunkPool.FullChunkSize);
    }
    public void Dispose() { }

    public IDrawOperation AsMirrored(int? verAxisX, int? horAxisY)
    {
        if (verAxisX is not null && horAxisY is not null)
            return new ClearRegionOperation((pos + size).ReflectX((int)verAxisX).ReflectY((int)horAxisY), size);
        if (verAxisX is not null)
            return new ClearRegionOperation(new VecI(pos.X + size.X, pos.Y).ReflectX((int)verAxisX), size);
        if (horAxisY is not null)
            return new ClearRegionOperation(new VecI(pos.X, pos.Y + size.Y).ReflectY((int)horAxisY), size);
        return new ClearRegionOperation(pos, size);
    }
}
