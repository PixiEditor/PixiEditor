using ChunkyImageLib.DataHolders;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;

namespace ChunkyImageLib.Operations;

public delegate Color PixelProcessor(Color commited, Color upToDate);
internal class PixelOperation : IMirroredDrawOperation
{
    public bool IgnoreEmptyChunks => false;
    private readonly VecI pixel;
    private readonly Color color;
    private readonly BlendMode blendMode;
    private readonly Paint paint;
    private readonly Func<VecI, Color>? getCommitedPixelFunc = null;

    private readonly PixelProcessor? colorProcessor = null;

    public PixelOperation(VecI pixel, Color color, BlendMode blendMode)
    {
        this.pixel = pixel;
        this.color = color;
        this.blendMode = blendMode;
        paint = new Paint() { BlendMode = blendMode };
    }

    public PixelOperation(VecI pixel, PixelProcessor colorProcessor, Func<VecI, Color> getCommitedPixelFunc, BlendMode blendMode)
    {
        this.pixel = pixel;
        this.colorProcessor = colorProcessor;
        this.blendMode = blendMode;
        this.getCommitedPixelFunc = getCommitedPixelFunc;
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
        surf.Canvas.DrawPoint(pixel, paint);
        surf.Canvas.Restore();
    }

    private Color GetColor(Chunk chunk, VecI chunkPos)
    {
        Color pixelColor = color;
        if (colorProcessor != null && getCommitedPixelFunc != null)
        {
            var pos = pixel - chunkPos * ChunkyImage.FullChunkSize;
            pixelColor = colorProcessor(getCommitedPixelFunc(pixel), chunk.Surface.GetSrgbPixel(pos));
        }

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
        if (colorProcessor != null && getCommitedPixelFunc != null)
        {
            return new PixelOperation(pixelRect.Pos, colorProcessor, getCommitedPixelFunc, blendMode);
        }

        return new PixelOperation(pixelRect.Pos, color, blendMode);
    }

    public void Dispose()
    {
        paint.Dispose();
    }
}
