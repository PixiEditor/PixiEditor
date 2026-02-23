using ChunkyImageLib.DataHolders;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Shaders;
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

    private bool antiAliasing;

    public bool IgnoreEmptyChunks => false;
    public bool NeedsDrawInSrgb => paint?.Paintable is ISrgbPaintable;

    public PathOperation(VectorPath path, Color color, float strokeWidth, StrokeCap cap, BlendMode blendMode, RectI? customBounds = null)
    {
        this.path = new VectorPath(path);
        paint = new() { Color = color, Style = PaintStyle.Stroke, StrokeWidth = strokeWidth, StrokeCap = cap, BlendMode = blendMode };

        RectI floatBounds = customBounds ?? (RectI)(path.TightBounds).RoundOutwards();
        bounds = floatBounds.Inflate((int)Math.Ceiling(strokeWidth) + 1);
    }

    public PathOperation(VectorPath path, Color color, float strokeWidth, StrokeCap cap, Blender blender, RectI? customBounds = null)
    {
        this.path = new VectorPath(path);
        paint = new() { Color = color, Style = PaintStyle.Stroke, StrokeWidth = strokeWidth, StrokeCap = cap, Blender = blender };

        RectI floatBounds = customBounds ?? (RectI)(path.TightBounds).RoundOutwards();
        bounds = floatBounds.Inflate((int)Math.Ceiling(strokeWidth) + 1);
    }

    public PathOperation(VectorPath path, Paintable paintable, float strokeWidth, StrokeCap cap, BlendMode blendMode,
        PaintStyle style, bool antiAliasing, RectI? customBounds = null)
    {
        this.antiAliasing = antiAliasing;
        this.path = new VectorPath(path);
        paint = new() { Paintable = paintable, Style = style, StrokeWidth = strokeWidth, StrokeCap = cap, BlendMode = blendMode };

        RectI floatBounds = customBounds ?? (RectI)(path.Bounds).RoundOutwards();
        bounds = floatBounds.Inflate((int)Math.Ceiling(strokeWidth) + 1);
    }

    public PathOperation(VectorPath path, Paintable paintable, float strokeWidth, StrokeCap cap, Blender blender, PaintStyle style, bool antiAliasing, RectI? customBounds)
    {
        this.antiAliasing = antiAliasing;
        this.path = new VectorPath(path);
        paint = new() { Paintable = paintable, Style = style, StrokeWidth = strokeWidth, StrokeCap = cap, Blender = blender };

        RectI floatBounds = customBounds ?? (RectI)(path.Bounds).RoundOutwards();
        bounds = floatBounds.Inflate((int)Math.Ceiling(strokeWidth) + 1);
    }

    public void DrawOnChunk(Chunk targetChunk, VecI chunkPos)
    {
        paint.IsAntiAliased = antiAliasing || targetChunk.Resolution != ChunkResolution.Full;
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
        if (paint.Paintable != null)
        {
            if( paint.Blender != null)
            {
                return new PathOperation(copy, paint.Paintable, paint.StrokeWidth, paint.StrokeCap, paint.Blender, paint.Style, antiAliasing, newBounds);
            }
            else
            {
                return new PathOperation(copy, paint.Paintable, paint.StrokeWidth, paint.StrokeCap, paint.BlendMode,
                    paint.Style, antiAliasing, newBounds);
            }
        }

        if (paint.Blender != null)
        {
            return new PathOperation(copy, paint.Color, paint.StrokeWidth, paint.StrokeCap, paint.Blender, newBounds);
        }

        return new PathOperation(copy, paint.Color, paint.StrokeWidth, paint.StrokeCap, paint.BlendMode, newBounds);
    }

    public void Dispose()
    {
        path.Dispose();
        paint.Dispose();
    }
}
