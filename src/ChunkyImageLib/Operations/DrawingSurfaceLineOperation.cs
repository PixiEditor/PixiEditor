using ChunkyImageLib.DataHolders;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;

namespace ChunkyImageLib.Operations;
internal class DrawingSurfaceLineOperation : IMirroredDrawOperation
{
    public bool IgnoreEmptyChunks => false;
    public bool NeedsDrawInSrgb => paint?.Paintable is ISrgbPaintable;

    private Paint paint;
    private readonly VecD from;
    private readonly VecD to;
    private bool isAntiAliased;

    public DrawingSurfaceLineOperation(VecD from, VecD to, StrokeCap strokeCap, float strokeWidth, Color color, BlendMode blendMode)
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
    
    public DrawingSurfaceLineOperation(VecD from, VecD to, Paint paint)
    {
        this.paint = paint.Clone();
        this.from = from;
        this.to = to;
        isAntiAliased = paint.IsAntiAliased;
    }

    public void DrawOnChunk(Chunk targetChunk, VecI chunkPos)
    {
        paint.IsAntiAliased = isAntiAliased || targetChunk.Resolution != ChunkResolution.Full;
        var surf = targetChunk.Surface.DrawingSurface;
        surf.Canvas.Save();
        surf.Canvas.Scale((float)targetChunk.Resolution.Multiplier());
        surf.Canvas.Translate(-chunkPos * ChunkyImage.FullChunkSize);
        surf.Canvas.DrawLine(from, to, paint);
        surf.Canvas.Restore();
    }

    public AffectedArea FindAffectedArea(VecI imageSize)
    {
        RectI bounds = (RectI)RectD.FromTwoPoints(from, to).Inflate((int)Math.Ceiling(paint.StrokeWidth));
        return new AffectedArea(OperationHelper.FindChunksTouchingRectangle(bounds, ChunkyImage.FullChunkSize), bounds);
    }

    public IDrawOperation AsMirrored(double? verAxisX, double? horAxisY)
    {
        VecD newFrom = from;
        VecD newTo = to;
        if (verAxisX is not null)
        {
            newFrom = (VecI)newFrom.ReflectX((double)verAxisX).Round();
            newTo = (VecI)newTo.ReflectX((double)verAxisX).Round();
        }
        if (horAxisY is not null)
        {
            newFrom = (VecI)newFrom.ReflectY((double)horAxisY).Round();
            newTo = (VecI)newTo.ReflectY((double)horAxisY).Round();
        }


        return new DrawingSurfaceLineOperation(newFrom, newTo, paint);
    }

    public void Dispose()
    {
        paint.Dispose();
    }
}
