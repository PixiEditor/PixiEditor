using ChunkyImageLib.DataHolders;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Numerics;

namespace ChunkyImageLib.Operations;

internal class ClearRegionOperation : IMirroredDrawOperation
{
    private RectI rect;

    public bool IgnoreEmptyChunks => true;

    public ClearRegionOperation(RectI rect)
    {
        this.rect = rect;
    }

    public void DrawOnChunk(Chunk targetChunk, VecI chunkPos)
    {
        VecI convPos = OperationHelper.ConvertForResolution(rect.Pos, targetChunk.Resolution);
        VecI convSize = OperationHelper.ConvertForResolution(rect.Size, targetChunk.Resolution);

        targetChunk.Surface.Surface.Canvas.Save();
        targetChunk.Surface.Surface.Canvas.ClipRect(RectD.Create(convPos - chunkPos.Multiply(targetChunk.PixelSize), convSize));
        targetChunk.Surface.Surface.Canvas.Clear();
        targetChunk.Surface.Surface.Canvas.Restore();
    }

    public AffectedArea FindAffectedArea(VecI imageSize)
    {
        return new AffectedArea(OperationHelper.FindChunksTouchingRectangle(rect, ChunkPool.FullChunkSize), rect);
    }
    public void Dispose() { }

    public IDrawOperation AsMirrored(double? verAxisX, double? horAxisY)
    {
        var newRect = rect;
        if (verAxisX is not null)
            newRect = (RectI)newRect.ReflectX((double)verAxisX).Round();
        if (horAxisY is not null)
            newRect = (RectI)newRect.ReflectY((double)horAxisY).Round();
        return new ClearRegionOperation(newRect);
    }
}
