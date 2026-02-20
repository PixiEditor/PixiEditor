using ChunkyImageLib.DataHolders;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;

namespace ChunkyImageLib.Operations;

public class PaintOperation : IDrawOperation
{
    private Paint paint;

    public PaintOperation(Paint paint)
    {
        this.paint = paint;
    }
    
    public void Dispose()
    {
        
    }

    public bool IgnoreEmptyChunks => false;
    public bool NeedsDrawInSrgb => paint?.Paintable is ISrgbPaintable;

    public void DrawOnChunk(Chunk targetChunk, VecI chunkPos)
    {
        targetChunk.Surface.DrawingSurface.Canvas.DrawPaint(paint);
    }

    public AffectedArea FindAffectedArea(VecI imageSize)
    {
        return new AffectedArea(OperationHelper.FindChunksTouchingRectangle(
            new RectI(0, 0, imageSize.X, imageSize.Y), 
            ChunkyImage.FullChunkSize));
    }
}
