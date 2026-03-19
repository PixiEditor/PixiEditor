using ChunkyImageLib.DataHolders;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;

namespace ChunkyImageLib.Operations;

internal class PixelOperation : IMirroredDrawOperation
{
    public bool IgnoreEmptyChunks => false;
    public bool NeedsDrawInSrgb => false;

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

    public void DrawOnChunk(Chunk targetChunk, VecI chunkPos)
    {
        // a hacky way to make the lines look slightly better on non full res chunks
        paint.Color = GetColor(targetChunk, chunkPos);

        DrawingSurface surf = targetChunk.Surface.DrawingSurface;
        surf.Canvas.Save();
        surf.Canvas.Scale((float)targetChunk.Resolution.Multiplier());
        surf.Canvas.Translate(-chunkPos * ChunkyImage.FullChunkSize);

        // Drawing points with GPU chunks doesn't work well, that's why we draw rects instead
        surf.Canvas.DrawRect(new RectD(pixel, new VecD(1)), paint);
        surf.Canvas.Restore();
    }

    private Color GetColor(Chunk chunk, VecI chunkPos)
    {
        Color pixelColor = color;
        return new Color(pixelColor.R, pixelColor.G, pixelColor.B, (byte)(pixelColor.A * chunk.Resolution.Multiplier()));
    }

    public AffectedArea FindAffectedArea(VecI imageSize)
    {
        return new AffectedArea(new HashSet<VecI>() { OperationHelper.GetChunkPos(pixel, ChunkyImage.FullChunkSize) }, new RectI(pixel, VecI.One));
    }

    public IDrawOperation AsMirrored(double? verAxisX, double? horAxisY)
    {
        RectI pixelRect = new RectI(pixel, new VecI(1, 1));
        if (verAxisX is not null)
            pixelRect = (RectI)pixelRect.ReflectX((double)verAxisX).Round();
        if (horAxisY is not null)
            pixelRect = (RectI)pixelRect.ReflectY((double)horAxisY);

        return new PixelOperation(pixelRect.Pos, color, blendMode);
    }

    public void Dispose()
    {
        paint.Dispose();
    }
}
