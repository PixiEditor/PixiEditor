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

    public bool Latest { get; }

    public ApplyMaskOperation(ChunkyImage maskToApply, bool latest)
    {
        mask = maskToApply;
        Latest = latest;
    }

    public AffectedArea FindAffectedArea(VecI imageSize)
    {
        return Latest ? mask.FindAffectedArea() : new AffectedArea(mask.FindCommittedChunks());
    }
    
    public void DrawOnChunk(Chunk targetChunk, VecI chunkPos)
    {
        if (Latest)
        {
            mask.DrawMostUpToDateChunkOn(chunkPos, targetChunk.Resolution, targetChunk.Surface.DrawingSurface.Canvas,
                VecI.Zero, clippingPaint);
        }
        else
        {
            mask.DrawCommittedChunkOn(chunkPos, targetChunk.Resolution, targetChunk.Surface.DrawingSurface.Canvas,
                VecI.Zero, clippingPaint);
        }
    }

    public void Dispose()
    {
        clippingPaint.Dispose();
    }
}
