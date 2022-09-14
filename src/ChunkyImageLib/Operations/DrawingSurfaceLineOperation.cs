using ChunkyImageLib.DataHolders;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface;

namespace ChunkyImageLib.Operations;
internal class DrawingSurfaceLineOperation : IDrawOperation
{
    public bool IgnoreEmptyChunks => false;

    private Paint paint;
    private readonly VecI from;
    private readonly VecI to;

    public DrawingSurfaceLineOperation(VecI from, VecI to, StrokeCap strokeCap, float strokeWidth, Color color, BlendMode blendMode)
    {
        paint = new()
        {
            StrokeCap = strokeCap,
            StrokeWidth = strokeWidth,
            Color = color,
            Style = PaintStyle.Stroke,
            BlendMode = blendMode,
        };
        this.from = from;
        this.to = to;
    }

    public void DrawOnChunk(Chunk chunk, VecI chunkPos)
    {
        paint.IsAntiAliased = chunk.Resolution != ChunkResolution.Full;
        var surf = chunk.Surface.DrawingSurface;
        surf.Canvas.Save();
        surf.Canvas.Scale((float)chunk.Resolution.Multiplier());
        surf.Canvas.Translate(-chunkPos * ChunkyImage.FullChunkSize);
        surf.Canvas.DrawLine(from, to, paint);
        surf.Canvas.Restore();
    }

    public HashSet<VecI> FindAffectedChunks(VecI imageSize)
    {
        RectI bounds = RectI.FromTwoPoints(from, to).Inflate((int)Math.Ceiling(paint.StrokeWidth));
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
        return new DrawingSurfaceLineOperation(newFrom, newTo, paint.StrokeCap, paint.StrokeWidth, paint.Color, paint.BlendMode);
    }

    public void Dispose()
    {
        paint.Dispose();
    }
}
