using ChunkyImageLib.DataHolders;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;

namespace ChunkyImageLib.Operations;

internal class ApplyMaskOperation : IDrawOperation
{
    private ChunkyImage mask;
    private Paint clippingPaint = new Paint() { BlendMode = BlendMode.DstIn };

    public bool IgnoreEmptyChunks => true;
    public bool NeedsDrawInSrgb => false;

    public ApplyMaskOperation(ChunkyImage maskToApply)
    {
        mask = maskToApply;
    }

    public AffectedArea FindAffectedArea(VecI imageSize)
    {
        return new AffectedArea(mask.FindCommittedChunks());
    }
    
    public void DrawOnChunk(Chunk targetChunk, VecI chunkPos)
    {
        mask.DrawCommittedChunkOn(chunkPos, targetChunk.Resolution, targetChunk.Surface.DrawingSurface.Canvas, VecI.Zero, clippingPaint);
    }

    public void Dispose()
    {
        clippingPaint.Dispose();
    }
}
