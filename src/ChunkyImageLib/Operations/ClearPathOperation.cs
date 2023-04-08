using ChunkyImageLib.DataHolders;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface.Vector;

namespace ChunkyImageLib.Operations;
internal class ClearPathOperation : IMirroredDrawOperation
{
    private VectorPath path;
    private RectI pathTightBounds;

    public bool IgnoreEmptyChunks => true;

    public ClearPathOperation(VectorPath path, RectI? pathTightBounds = null)
    {
        this.path = new VectorPath(path);
        this.pathTightBounds = (pathTightBounds ?? (RectI)path.TightBounds);
    }

    public void DrawOnChunk(Chunk chunk, VecI chunkPos)
    {
        chunk.Surface.DrawingSurface.Canvas.Save();

        using VectorPath transformedPath = new(path);
        float scale = (float)chunk.Resolution.Multiplier();
        VecD trans = -chunkPos * ChunkyImage.FullChunkSize * scale;
        transformedPath.Transform(Matrix3X3.CreateScaleTranslation(scale, scale, (float)trans.X, (float)trans.Y));
        chunk.Surface.DrawingSurface.Canvas.ClipPath(transformedPath);
        chunk.Surface.DrawingSurface.Canvas.Clear();
        chunk.Surface.DrawingSurface.Canvas.Restore();
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
