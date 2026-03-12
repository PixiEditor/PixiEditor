using ChunkyImageLib.DataHolders;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;

namespace ChunkyImageLib.Operations;
internal class ClearPathOperation : IMirroredDrawOperation
{
    private VectorPath path;
    private RectI pathTightBounds;

    public bool IgnoreEmptyChunks => true;
    public bool NeedsDrawInSrgb => false;

    public ClearPathOperation(VectorPath path, RectI? pathTightBounds = null)
    {
        this.path = new VectorPath(path);
        this.pathTightBounds = (pathTightBounds ?? (RectI)path.TightBounds);
    }

    public void DrawOnChunk(Chunk targetChunk, VecI chunkPos)
    {
        targetChunk.Surface.DrawingSurface.Canvas.Save();

        using VectorPath transformedPath = new(path);
        float scale = (float)targetChunk.Resolution.Multiplier();
        VecD trans = -chunkPos * ChunkyImage.FullChunkSize * scale;
        transformedPath.Transform(Matrix3X3.CreateScaleTranslation(scale, scale, (float)trans.X, (float)trans.Y));
        targetChunk.Surface.DrawingSurface.Canvas.ClipPath(transformedPath);
        targetChunk.Surface.DrawingSurface.Canvas.Clear();
        targetChunk.Surface.DrawingSurface.Canvas.Restore();
    }

    public AffectedArea FindAffectedArea(VecI imageSize)
    {
        return new AffectedArea(OperationHelper.FindChunksTouchingRectangle(pathTightBounds, ChunkPool.FullChunkSize), pathTightBounds);
    }
    public void Dispose()
    {
        path.Dispose();
    }

    public IDrawOperation AsMirrored(double? verAxisX, double? horAxisY)
    {
        var matrix = Matrix3X3.CreateScale(verAxisX is not null ? -1 : 1, horAxisY is not null ? -1 : 1, (float?)verAxisX ?? 0, (float?)horAxisY ?? 0);
        using var copy = new VectorPath(path);
        copy.Transform(matrix);

        var newRect = pathTightBounds;
        if (verAxisX is not null)
            newRect = (RectI)newRect.ReflectX((double)verAxisX).Round();
        if (horAxisY is not null)
            newRect = (RectI)newRect.ReflectY((double)horAxisY).Round();
        return new ClearPathOperation(copy, newRect);
    }
}
