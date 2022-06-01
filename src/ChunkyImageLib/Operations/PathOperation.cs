using ChunkyImageLib.DataHolders;
using SkiaSharp;

namespace ChunkyImageLib.Operations;
internal class PathOperation : IDrawOperation
{
    private readonly SKPath path;

    private readonly SKPaint paint;
    private readonly VecI boundsTopLeft;
    private readonly VecI boundsSize;

    public bool IgnoreEmptyChunks => false;

    public PathOperation(SKPath path, SKColor color, float strokeWidth, SKStrokeCap cap, SKRect? customBounds = null)
    {
        this.path = new SKPath(path);
        paint = new() { Color = color, Style = SKPaintStyle.Stroke, StrokeWidth = strokeWidth, StrokeCap = cap };

        var floatBounds = customBounds ?? path.TightBounds;
        floatBounds.Inflate(strokeWidth + 1, strokeWidth + 1);
        boundsTopLeft = (VecI)floatBounds.Location;
        boundsSize = (VecI)floatBounds.Size;
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
        return OperationHelper.FindChunksTouchingRectangle(boundsTopLeft, boundsSize, ChunkyImage.FullChunkSize);
    }

    public IDrawOperation AsMirrored(int? verAxisX, int? horAxisY)
    {
        var matrix = SKMatrix.CreateScale(verAxisX is not null ? -1 : 1, horAxisY is not null ? -1 : 1, verAxisX ?? 0, horAxisY ?? 0);
        using var copy = new SKPath(path);
        copy.Transform(matrix);

        VecI p1 = (VecI)matrix.MapPoint(boundsTopLeft);
        VecI p2 = (VecI)matrix.MapPoint(boundsTopLeft + boundsSize);
        VecI topLeft = new(Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y));

        return new PathOperation(copy, paint.Color, paint.StrokeWidth, paint.StrokeCap, SKRect.Create(topLeft, boundsSize));
    }

    public void Dispose()
    {
        path.Dispose();
        paint.Dispose();
    }
}
