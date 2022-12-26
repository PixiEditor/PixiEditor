using ChunkyImageLib.DataHolders;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.PaintImpl;

namespace ChunkyImageLib.Operations;

internal class PixelsOperation : IMirroredDrawOperation
{
    public bool IgnoreEmptyChunks => false;
    private readonly Point[] pixels;
    private readonly Color color;
    private readonly BlendMode blendMode;
    private readonly Paint paint;

    public PixelsOperation(IEnumerable<VecI> pixels, Color color, BlendMode blendMode)
    {
        this.pixels = pixels.Select(pixel => new Point(pixel.X, pixel.Y)).ToArray();
        this.color = color;
        this.blendMode = blendMode;
        paint = new Paint() { BlendMode = blendMode };
    }

    public void DrawOnChunk(Chunk chunk, VecI chunkPos)
    {
        // a hacky way to make the lines look slightly better on non full res chunks
        paint.Color = new Color(color.R, color.G, color.B, (byte)(color.A * chunk.Resolution.Multiplier()));

        DrawingSurface surf = chunk.Surface.DrawingSurface;
        surf.Canvas.Save();
        surf.Canvas.Scale((float)chunk.Resolution.Multiplier());
        surf.Canvas.Translate(-chunkPos * ChunkyImage.FullChunkSize);
        surf.Canvas.DrawPoints(PointMode.Points, pixels, paint);
        surf.Canvas.Restore();
    }

    public HashSet<VecI> FindAffectedChunks(VecI imageSize)
    {
        return pixels.Select(static pixel => OperationHelper.GetChunkPos(pixel, ChunkyImage.FullChunkSize)).ToHashSet();
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
