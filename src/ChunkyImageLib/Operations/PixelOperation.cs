using ChunkyImageLib.DataHolders;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.PaintImpl;

namespace ChunkyImageLib.Operations;

internal class PixelOperation : IDrawOperation
{
    public bool IgnoreEmptyChunks => false;
    private readonly VecI pixel;
    private readonly Color color;
    private readonly BlendMode blendMode;
    private readonly Paint paint;

    public PixelOperation(VecI pixel, Color color, BlendMode blendMode)
    {
        this.pixel = pixel;
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
        surf.Canvas.DrawPoint(pixel, paint);
        surf.Canvas.Restore();
    }

    public HashSet<VecI> FindAffectedChunks(VecI imageSize)
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
