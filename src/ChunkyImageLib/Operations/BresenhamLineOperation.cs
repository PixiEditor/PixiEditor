using ChunkyImageLib.DataHolders;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;

namespace ChunkyImageLib.Operations;

internal class BresenhamLineOperation : IMirroredDrawOperation
{
    public bool IgnoreEmptyChunks => false;
    public bool NeedsDrawInSrgb => paintable is ISrgbPaintable;

    private readonly VecI from;
    private readonly VecI to;
    private readonly Paintable paintable;
    private readonly BlendMode blendMode;
    private readonly VecF[] points;
    private Paint paint;

    public BresenhamLineOperation(VecI from, VecI to, Paintable paintable, BlendMode blendMode)
    {
        this.from = from;
        this.to = to;
        this.paintable = paintable.Clone();
        if (this.paintable is IStartEndPaintable startEndPaintable)
        {
            startEndPaintable.Start = from;
            startEndPaintable.End = to;
            this.paintable.AbsoluteValues = true;
        }

        this.blendMode = blendMode;
        paint = new Paint() { BlendMode = blendMode, Paintable = paintable };
        points = BresenhamLineHelper.GetBresenhamLine(from, to).Select(v => new VecF(v)).ToArray();
    }

    public void DrawOnChunk(Chunk targetChunk, VecI chunkPos)
    {
        if (paintable is ColorPaintable colorPaintable)
        {
            // a hacky way to make the lines look slightly better on non full res chunks
            paint.Color = new Color(colorPaintable.Color.R, colorPaintable.Color.G, colorPaintable.Color.B,
                (byte)(colorPaintable.Color.A * targetChunk.Resolution.Multiplier()));
        }
        else
        {
            paint.SetPaintable(paintable);
        }

        var surf = targetChunk.Surface.DrawingSurface;
        surf.Canvas.Save();
        surf.Canvas.Scale((float)targetChunk.Resolution.Multiplier());
        surf.Canvas.Translate(-chunkPos * ChunkyImage.FullChunkSize);
        foreach (var point in points)
        {
            surf.Canvas.DrawRect(point.X, point.Y, 1, 1, paint);
        }
        // Draw Points and Draw Point does not work correctly on GPU surfaces
        //surf.Canvas.DrawPoints(PointMode.Polygon, points, paint);
        surf.Canvas.Restore();
    }

    public AffectedArea FindAffectedArea(VecI imageSize)
    {
        RectI bounds = RectI.FromTwoPixels(from, to);
        return new AffectedArea(OperationHelper.FindChunksTouchingRectangle(bounds, ChunkyImage.FullChunkSize), bounds);
    }

    public IDrawOperation AsMirrored(double? verAxisX, double? horAxisY)
    {
        RectI newFrom = new RectI(from, new VecI(1));
        RectI newTo = new RectI(to, new VecI(1));
        if (verAxisX is not null)
        {
            newFrom = (RectI)newFrom.ReflectX((double)verAxisX).Round();
            newTo = (RectI)newTo.ReflectX((double)verAxisX).Round();
        }

        if (horAxisY is not null)
        {
            newFrom = (RectI)newFrom.ReflectY((double)horAxisY).Round();
            newTo = (RectI)newTo.ReflectY((double)horAxisY).Round();
        }

        return new BresenhamLineOperation(newFrom.Pos, newTo.Pos, paintable, blendMode);
    }

    public void Dispose()
    {
        paint.Dispose();
        this.paintable.Dispose();
    }
}
