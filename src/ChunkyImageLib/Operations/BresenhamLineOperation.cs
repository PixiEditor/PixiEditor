using ChunkyImageLib.DataHolders;
using SkiaSharp;

namespace ChunkyImageLib.Operations;
internal class BresenhamLineOperation : IDrawOperation
{
    public bool IgnoreEmptyChunks => false;
    private readonly VecI from;
    private readonly VecI to;
    private readonly SKColor color;
    private readonly SKBlendMode blendMode;
    private readonly SKPoint[] points;
    private SKPaint paint;

    public BresenhamLineOperation(VecI from, VecI to, SKColor color, SKBlendMode blendMode)
    {
        this.from = from;
        this.to = to;
        this.color = color;
        this.blendMode = blendMode;
        paint = new SKPaint() { BlendMode = blendMode };
        points = BresenhamLineHelper.GetBresenhamLine(from, to);
    }

    public void DrawOnChunk(Chunk chunk, VecI chunkPos)
    {
        // a hacky way to make the lines look slightly better on non full res chunks
        paint.Color = new SKColor(color.Red, color.Green, color.Blue, (byte)(color.Alpha * chunk.Resolution.Multiplier()));

        var surf = chunk.Surface.SkiaSurface;
        surf.Canvas.Save();
        surf.Canvas.Scale((float)chunk.Resolution.Multiplier());
        surf.Canvas.Translate(-chunkPos * ChunkyImage.FullChunkSize);
        surf.Canvas.DrawPoints(SKPointMode.Points, points, paint);
        surf.Canvas.Restore();
    }

    public HashSet<VecI> FindAffectedChunks()
    {
        RectI bounds = RectI.FromTwoPoints(from, to);
        return OperationHelper.FindChunksTouchingRectangle(bounds, ChunkyImage.FullChunkSize);
    }

    public IDrawOperation AsMirrored(int? verAxisX, int? horAxisY)
    {
        VecI newFrom = from;
        VecI newTo = to;
        if (verAxisX is not null)
        {
            newFrom = newFrom.ReflectX((int)verAxisX);
            newTo = newTo.ReflectX((int)verAxisX);
        }
        if (horAxisY is not null)
        {
            newFrom = newFrom.ReflectY((int)horAxisY);
            newTo = newTo.ReflectY((int)horAxisY);
        }
        return new BresenhamLineOperation(newFrom, newTo, color, blendMode);
    }

    public void Dispose()
    {
        paint.Dispose();
    }
}
