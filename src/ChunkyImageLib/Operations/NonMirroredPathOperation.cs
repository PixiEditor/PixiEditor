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
internal class NonMirroredPathOperation : IDrawOperation
{
    private readonly VectorPath path;

    private readonly Paint paint;
    private readonly RectI bounds;
    private readonly Matrix3X3? paintableMatrix;

    private bool antiAliasing;

    public bool IgnoreEmptyChunks => false;

    public NonMirroredPathOperation(VectorPath path, Paintable paintable, float strokeWidth, StrokeCap cap, BlendMode blendMode,
        PaintStyle style, bool antiAliasing, RectI? customBounds = null, Matrix3X3? paintableMatrix = null)
    {
        this.antiAliasing = antiAliasing;
        this.path = new VectorPath(path);
        this.paintableMatrix = paintableMatrix ??  Matrix3X3.Identity;
        paint = new() { Paintable = paintable, Style = style, StrokeWidth = strokeWidth, StrokeCap = cap, BlendMode = blendMode };

        RectI floatBounds = customBounds ?? (RectI)(path.Bounds).RoundOutwards();
        bounds = floatBounds.Inflate((int)Math.Ceiling(strokeWidth) + 1);
    }

    public NonMirroredPathOperation(VectorPath path, Paintable paintable, float strokeWidth, StrokeCap cap, Blender blender,
        PaintStyle style, bool antiAliasing, RectI? customBounds, Matrix3X3? paintableMatrix = null)
    {
        this.antiAliasing = antiAliasing;
        this.path = new VectorPath(path);
        paint = new() { Paintable = paintable, Style = style, StrokeWidth = strokeWidth, StrokeCap = cap, Blender = blender };

        RectI floatBounds = customBounds ?? (RectI)(path.Bounds).RoundOutwards();
        this.paintableMatrix = paintableMatrix ?? Matrix3X3.Identity;
        bounds = floatBounds.Inflate((int)Math.Ceiling(strokeWidth) + 1);
    }

    public void DrawOnChunk(Chunk targetChunk, VecI chunkPos)
    {
        paint.IsAntiAliased = antiAliasing || targetChunk.Resolution != ChunkResolution.Full;
        var surf = targetChunk.Surface.DrawingSurface;
        surf.Canvas.Save();
        surf.Canvas.Scale((float)targetChunk.Resolution.Multiplier());
        surf.Canvas.Translate(-chunkPos * ChunkyImage.FullChunkSize);
        var oldMtx = paint.Paintable.Transform;
        var final = paintableMatrix;
        if(oldMtx != null)
        {
            final = oldMtx.Value.PostConcat(final ?? Matrix3X3.Identity);
        }
        paint.Paintable.Transform = final ?? Matrix3X3.Identity;
        surf.Canvas.DrawPath(path, paint);
        paint.Paintable.Transform = oldMtx;
        surf.Canvas.Restore();
    }

    public AffectedArea FindAffectedArea(VecI imageSize)
    {
        return new AffectedArea(OperationHelper.FindChunksTouchingRectangle(bounds, ChunkyImage.FullChunkSize), bounds);
    }

    public void Dispose()
    {
        path.Dispose();
        paint.Dispose();
    }
}
