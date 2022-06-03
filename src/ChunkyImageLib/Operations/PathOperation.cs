using ChunkyImageLib.DataHolders;
using SkiaSharp;

namespace ChunkyImageLib.Operations;
internal class PathOperation : IDrawOperation
{
    private readonly SKPath path;

    private readonly SKPaint paint;
    private readonly RectI bounds;

    public bool IgnoreEmptyChunks => false;

    public PathOperation(SKPath path, SKColor color, float strokeWidth, SKStrokeCap cap, RectI? customBounds = null)
    {
        this.path = new SKPath(path);
        paint = new() { Color = color, Style = SKPaintStyle.Stroke, StrokeWidth = strokeWidth, StrokeCap = cap };

        RectI floatBounds = customBounds ?? (RectI)((RectD)path.TightBounds).RoundOutwards();
        bounds = floatBounds.Inflate((int)Math.Ceiling(strokeWidth) + 1);
    }

    public void DrawOnChunk(Chunk chunk, VecI chunkPos)
    {
        var surf = chunk.Surface.SkiaSurface;
        surf.Canvas.Save();
        surf.Canvas.Scale((float)chunk.Resolution.Multiplier());
        surf.Canvas.Translate(-chunkPos * ChunkyImage.FullChunkSize);
        surf.Canvas.DrawPath(path, paint);
        surf.Canvas.Restore();
    }

    public HashSet<VecI> FindAffectedChunks()
    {
        return OperationHelper.FindChunksTouchingRectangle(bounds, ChunkyImage.FullChunkSize);
    }

    public IDrawOperation AsMirrored(int? verAxisX, int? horAxisY)
    {
        var matrix = SKMatrix.CreateScale(verAxisX is not null ? -1 : 1, horAxisY is not null ? -1 : 1, verAxisX ?? 0, horAxisY ?? 0);
        using var copy = new SKPath(path);
        copy.Transform(matrix);

        RectI newBounds = bounds;
        if (verAxisX is not null)
            newBounds = newBounds.ReflectX((int)verAxisX);
        if (horAxisY is not null)
            newBounds = newBounds.ReflectY((int)horAxisY);
        return new PathOperation(copy, paint.Color, paint.StrokeWidth, paint.StrokeCap, newBounds);
    }

    public void Dispose()
    {
        path.Dispose();
        paint.Dispose();
    }
}
