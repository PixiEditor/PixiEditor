using ChunkyImageLib.DataHolders;
using PixiEditor.DrawingApi.Core.Surface.PaintImpl;
using PixiEditor.Numerics;

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
