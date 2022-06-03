using ChunkyImageLib.DataHolders;
using SkiaSharp;

namespace ChunkyImageLib.Operations;
internal class SkiaLineOperation : IDrawOperation
{
    public bool IgnoreEmptyChunks => false;

    private SKPaint paint;
    private readonly VecI from;
    private readonly VecI to;

    public SkiaLineOperation(VecI from, VecI to, SKStrokeCap strokeCap, float strokeWidth, SKColor color)
    {
        paint = new()
        {
            StrokeCap = strokeCap,
            StrokeWidth = strokeWidth,
            Color = color,
            Style = SKPaintStyle.Stroke,
        };
        this.from = from;
        this.to = to;
    }

    public void DrawOnChunk(Chunk chunk, VecI chunkPos)
    {
        var surf = chunk.Surface.SkiaSurface;
        surf.Canvas.Save();
        surf.Canvas.Scale((float)chunk.Resolution.Multiplier());
        surf.Canvas.Translate(-chunkPos * ChunkyImage.FullChunkSize);
        surf.Canvas.DrawLine(from, to, paint);
        surf.Canvas.Restore();
    }

    public HashSet<VecI> FindAffectedChunks()
    {
        RectI bounds = RectI.FromTwoPoints(from, to).Inflate((int)Math.Ceiling(paint.StrokeWidth));
        return OperationHelper.FindChunksTouchingRectangle(bounds, ChunkyImage.FullChunkSize);
    }

    public IDrawOperation AsMirrored(int? verAxisX, int? horAxisY)
    {
        VecI newFrom = from;
        VecI newTo = to;
        if (verAxisX is not null)
        {
            newFrom = newFrom.ReflectX((int)verAxisX);
            newTo = newFrom.ReflectX((int)verAxisX);
        }
        if (horAxisY is not null)
        {
            newFrom = newFrom.ReflectY((int)horAxisY);
            newTo = newFrom.ReflectY((int)horAxisY);
        }
        return new SkiaLineOperation(newFrom, newTo, paint.StrokeCap, paint.StrokeWidth, paint.Color);
    }

    public void Dispose()
    {
        paint.Dispose();
    }
}
