using ChunkyImageLib.DataHolders;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.PaintImpl;

namespace ChunkyImageLib.Operations;

public delegate Color PixelProcessor(Color input);
internal class PixelOperation : IMirroredDrawOperation
{
    public bool IgnoreEmptyChunks => false;
    private readonly VecI pixel;
    private readonly Color color;
    private readonly BlendMode blendMode;
    private readonly Paint paint;

    private readonly PixelProcessor? _colorProcessor = null;

    public PixelOperation(VecI pixel, Color color, BlendMode blendMode)
    {
        this.pixel = pixel;
        this.color = color;
        this.blendMode = blendMode;
        paint = new Paint() { BlendMode = blendMode };
    }

    public PixelOperation(VecI pixel, PixelProcessor colorProcessor, BlendMode blendMode)
    {
        this.pixel = pixel;
        this._colorProcessor = colorProcessor;
        this.blendMode = blendMode;
        paint = new Paint() { BlendMode = blendMode };
    }

    public void DrawOnChunk(Chunk chunk, VecI chunkPos)
    {
        // a hacky way to make the lines look slightly better on non full res chunks
        paint.Color = GetColor(chunk, chunkPos);

        DrawingSurface surf = chunk.Surface.DrawingSurface;
        surf.Canvas.Save();
        surf.Canvas.Scale((float)chunk.Resolution.Multiplier());
        surf.Canvas.Translate(-chunkPos * ChunkyImage.FullChunkSize);
        surf.Canvas.DrawPoint(pixel, paint);
        surf.Canvas.Restore();
    }

    private Color GetColor(Chunk chunk, VecI chunkPos)
    {
        Color pixelColor = color;
        if (_colorProcessor != null)
        {
            pixelColor = _colorProcessor(chunk.Surface.GetSRGBPixel(pixel - chunkPos * ChunkyImage.FullChunkSize));
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
        if (_colorProcessor != null)
        {
            return new PixelOperation(pixelRect.Pos, _colorProcessor, blendMode);
        }

        return new PixelOperation(pixelRect.Pos, color, blendMode);
    }

    public void Dispose()
    {
        paint.Dispose();
    }
}
