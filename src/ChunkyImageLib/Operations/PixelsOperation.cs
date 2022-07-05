using ChunkyImageLib.DataHolders;
using SkiaSharp;

namespace ChunkyImageLib.Operations;

internal class PixelsOperation : IDrawOperation
{
    public bool IgnoreEmptyChunks => false;
    private readonly SKPoint[] pixels;
    private readonly SKColor color;
    private readonly SKBlendMode blendMode;
    private readonly SKPaint paint;

    public PixelsOperation(IEnumerable<VecI> pixels, SKColor color, SKBlendMode blendMode)
    {
        this.pixels = pixels.Select(pixel => (SKPoint)pixel).ToArray();
        this.color = color;
        this.blendMode = blendMode;
        paint = new SKPaint() { BlendMode = blendMode };
    }

    public void DrawOnChunk(Chunk chunk, VecI chunkPos)
    {
        // a hacky way to make the lines look slightly better on non full res chunks
        paint.Color = new SKColor(color.Red, color.Green, color.Blue, (byte)(color.Alpha * chunk.Resolution.Multiplier()));

        SKSurface surf = chunk.Surface.SkiaSurface;
        surf.Canvas.Save();
        surf.Canvas.Scale((float)chunk.Resolution.Multiplier());
        surf.Canvas.Translate(-chunkPos * ChunkyImage.FullChunkSize);
        surf.Canvas.DrawPoints(SKPointMode.Points, pixels, paint);
        surf.Canvas.Restore();
    }

    public HashSet<VecI> FindAffectedChunks()
    {
        return pixels.Select(static pixel => OperationHelper.GetChunkPos((VecI)pixel, ChunkyImage.FullChunkSize)).ToHashSet();
    }

    public IDrawOperation AsMirrored(int? verAxisX, int? horAxisY)
    {
        var arr = pixels.Select(pixel => new VecI(
            verAxisX is not null ? 2 * (int)verAxisX - (int)pixel.X - 1 : (int)pixel.X,
            horAxisY is not null ? 2 * (int)horAxisY - (int)pixel.Y - 1 : (int)pixel.Y
        ));
        return new PixelsOperation(arr, color, blendMode);
    }

    public void Dispose()
    {
        paint.Dispose();
    }
}
