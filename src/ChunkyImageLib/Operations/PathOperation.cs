using ChunkyImageLib.DataHolders;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;

namespace ChunkyImageLib.Operations;
internal class PathOperation : IMirroredDrawOperation
{
    private readonly VectorPath path;

    private readonly Paint paint;
    private readonly RectI bounds;

    public bool IgnoreEmptyChunks => false;

    public PathOperation(VectorPath path, Color color, float strokeWidth, StrokeCap cap, BlendMode blendMode, RectI? customBounds = null)
    {
        this.path = new VectorPath(path);
        paint = new() { Color = color, Style = PaintStyle.Stroke, StrokeWidth = strokeWidth, StrokeCap = cap, BlendMode = blendMode };

        RectI floatBounds = customBounds ?? (RectI)(path.TightBounds).RoundOutwards();
        bounds = floatBounds.Inflate((int)Math.Ceiling(strokeWidth) + 1);
    }

    public void DrawOnChunk(Chunk targetChunk, VecI chunkPos)
    {
        paint.IsAntiAliased = targetChunk.Resolution != ChunkResolution.Full;
        var surf = targetChunk.Surface.DrawingSurface;
        surf.Canvas.Save();
        surf.Canvas.Scale((float)targetChunk.Resolution.Multiplier());
        surf.Canvas.Translate(-chunkPos * ChunkyImage.FullChunkSize);
        surf.Canvas.DrawPath(path, paint);
        surf.Canvas.Restore();
    }

    public AffectedArea FindAffectedArea(VecI imageSize)
    {
        return new AffectedArea(OperationHelper.FindChunksTouchingRectangle(bounds, ChunkyImage.FullChunkSize), bounds);
    }

    public IDrawOperation AsMirrored(double? verAxisX, double? horAxisY)
    {
        var matrix = Matrix3X3.CreateScale(verAxisX is not null ? -1 : 1, horAxisY is not null ? -1 : 1, (float?)verAxisX ?? 0, (float?)horAxisY ?? 0);
        using var copy = new VectorPath(path);
        copy.Transform(matrix);

        RectI newBounds = bounds;
        if (verAxisX is not null)
            newBounds = (RectI)newBounds.ReflectX((double)verAxisX).Round();
        if (horAxisY is not null)
            newBounds = (RectI)newBounds.ReflectY((double)horAxisY).Round();
        return new PathOperation(copy, paint.Color, paint.StrokeWidth, paint.StrokeCap, paint.BlendMode, newBounds);
    }

    public void Dispose()
    {
        path.Dispose();
        paint.Dispose();
    }
}
