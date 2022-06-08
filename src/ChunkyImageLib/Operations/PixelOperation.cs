using ChunkyImageLib.DataHolders;
using SkiaSharp;

namespace ChunkyImageLib.Operations;

internal class PixelOperation : IDrawOperation
{
    public bool IgnoreEmptyChunks => false;
    private readonly VecI pixel;
    private readonly SKColor color;
    private readonly SKBlendMode blendMode;
    private readonly SKPaint paint;

    public PixelOperation(VecI pixel, SKColor color, SKBlendMode blendMode)
    {
        this.pixel = pixel;
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
        surf.Canvas.DrawPoint(pixel, paint);
        surf.Canvas.Restore();
    }

    public HashSet<VecI> FindAffectedChunks()
    {
        return new HashSet<VecI>() { OperationHelper.GetChunkPos(pixel, ChunkyImage.FullChunkSize) };
    }

    public IDrawOperation AsMirrored(int? verAxisX, int? horAxisY)
    {
        RectI pixelRect = new RectI(pixel, new VecI(1, 1));
        if (verAxisX is not null)
            pixelRect = pixelRect.ReflectX((int)verAxisX);
        if (horAxisY is not null)
            pixelRect = pixelRect.ReflectY((int)horAxisY);
        return new PixelOperation(pixelRect.Pos, color, blendMode);
    }

    public void Dispose()
    {
        paint.Dispose();
    }
}
