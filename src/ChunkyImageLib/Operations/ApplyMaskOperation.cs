using ChunkyImageLib.DataHolders;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.PaintImpl;

namespace ChunkyImageLib.Operations;

internal class ApplyMaskOperation : IDrawOperation
{
    private ChunkyImage mask;
    private Paint clippingPaint = new Paint() { BlendMode = BlendMode.DstIn };

    public bool IgnoreEmptyChunks => true;

    public ApplyMaskOperation(ChunkyImage maskToApply)
    {
        mask = maskToApply;
    }
    
    public HashSet<VecI> FindAffectedChunks(VecI imageSize)
    {
        return mask.FindCommittedChunks();
    }
    
    public void DrawOnChunk(Chunk chunk, VecI chunkPos)
    {
        mask.DrawCommittedChunkOn(chunkPos, chunk.Resolution, chunk.Surface.DrawingSurface, VecI.Zero, clippingPaint);
    }

    public void Dispose()
    {
        clippingPaint.Dispose();
    }
}
