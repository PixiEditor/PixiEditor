using ChunkyImageLib.DataHolders;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.PaintImpl;

namespace ChunkyImageLib.Operations;
internal class DrawingSurfaceLineOperation : IMirroredDrawOperation
{
    public bool IgnoreEmptyChunks => false;

    private Paint paint;
    private readonly VecI from;
    private readonly VecI to;

    public DrawingSurfaceLineOperation(VecI from, VecI to, StrokeCap strokeCap, float strokeWidth, Color color, BlendMode blendMode)
    {
        paint = new()
        {
            StrokeCap = strokeCap,
            StrokeWidth = strokeWidth,
            Color = color,
            Style = PaintStyle.Stroke,
            BlendMode = blendMode,
        };
        this.from = from;
        this.to = to;
    }

    public void DrawOnChunk(Chunk chunk, VecI chunkPos)
    {
        paint.IsAntiAliased = chunk.Resolution != ChunkResolution.Full;
        var surf = chunk.Surface.DrawingSurface;
        surf.Canvas.Save();
        surf.Canvas.Scale((float)chunk.Resolution.Multiplier());
        surf.Canvas.Translate(-chunkPos * ChunkyImage.FullChunkSize);
        surf.Canvas.DrawLine(from, to, paint);
        surf.Canvas.Restore();
    }

    public AffectedArea FindAffectedArea(VecI imageSize)
    {
        RectI bounds = RectI.FromTwoPoints(from, to).Inflate((int)Math.Ceiling(paint.StrokeWidth));
        return new AffectedArea(OperationHelper.FindChunksTouchingRectangle(bounds, ChunkyImage.FullChunkSize), bounds);
    }

    public IDrawOperation AsMirrored(double? verAxisX, double? horAxisY)
    {
        VecI newFrom = from;
        VecI newTo = to;
        if (verAxisX is not null)
        {
            newFrom = (VecI)newFrom.ReflectX((double)verAxisX).Round();
            newTo = (VecI)newTo.ReflectX((double)verAxisX).Round();
        }
        if (horAxisY is not null)
        {
            newFrom = (VecI)newFrom.ReflectY((double)horAxisY).Round();
            newTo = (VecI)newTo.ReflectY((double)horAxisY).Round();
        }
        return new DrawingSurfaceLineOperation(newFrom, newTo, paint.StrokeCap, paint.StrokeWidth, paint.Color, paint.BlendMode);
    }

    public void Dispose()
    {
        paint.Dispose();
    }
}
