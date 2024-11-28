using ChunkyImageLib.DataHolders;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;

namespace ChunkyImageLib.Operations;
internal class BresenhamLineOperation : IMirroredDrawOperation
{
    public bool IgnoreEmptyChunks => false;
    private readonly VecI from;
    private readonly VecI to;
    private readonly Color color;
    private readonly BlendMode blendMode;
    private readonly VecF[] points;
    private Paint paint;

    public BresenhamLineOperation(VecI from, VecI to, Color color, BlendMode blendMode)
    {
        this.from = from;
        this.to = to;
        this.color = color;
        this.blendMode = blendMode;
        paint = new Paint() { BlendMode = blendMode };
        points = BresenhamLineHelper.GetBresenhamLine(from, to);
    }

    public void DrawOnChunk(Chunk targetChunk, VecI chunkPos)
    {
        // a hacky way to make the lines look slightly better on non full res chunks
        paint.Color = new Color(color.R, color.G, color.B, (byte)(color.A * targetChunk.Resolution.Multiplier()));

        var surf = targetChunk.Surface.DrawingSurface;
        surf.Canvas.Save();
        surf.Canvas.Scale((float)targetChunk.Resolution.Multiplier());
        surf.Canvas.Translate(-chunkPos * ChunkyImage.FullChunkSize);
        surf.Canvas.DrawPoints(PointMode.Points, points, paint);
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
        return new BresenhamLineOperation(newFrom.Pos, newTo.Pos, color, blendMode);
    }

    public void Dispose()
    {
        paint.Dispose();
    }
}
