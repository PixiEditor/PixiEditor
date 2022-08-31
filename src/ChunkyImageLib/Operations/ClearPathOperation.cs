using ChunkyImageLib.DataHolders;
using SkiaSharp;

namespace ChunkyImageLib.Operations;
internal class ClearPathOperation : IDrawOperation
{
    private SKPath path;
    private RectI pathTightBounds;

    public bool IgnoreEmptyChunks => true;

    public ClearPathOperation(SKPath path, RectI? pathTightBounds = null)
    {
        this.path = new SKPath(path);
        this.pathTightBounds = (RectI)(pathTightBounds ?? path.TightBounds);
    }

    public void DrawOnChunk(Chunk chunk, VecI chunkPos)
    {
        chunk.Surface.DrawingSurface.Canvas.Save();

        using SKPath transformedPath = new(path);
        float scale = (float)chunk.Resolution.Multiplier();
        VecD trans = -chunkPos * ChunkyImage.FullChunkSize * scale;
        transformedPath.Transform(SKMatrix.CreateScaleTranslation(scale, scale, (float)trans.X, (float)trans.Y));
        chunk.Surface.DrawingSurface.Canvas.ClipPath(transformedPath);
        chunk.Surface.DrawingSurface.Canvas.Clear();
        chunk.Surface.DrawingSurface.Canvas.Restore();
    }

    public HashSet<VecI> FindAffectedChunks(VecI imageSize)
    {
        return OperationHelper.FindChunksTouchingRectangle(pathTightBounds, ChunkPool.FullChunkSize);
    }
    public void Dispose()
    {
        path.Dispose();
    }

    public IDrawOperation AsMirrored(int? verAxisX, int? horAxisY)
    {
        var matrix = SKMatrix.CreateScale(verAxisX is not null ? -1 : 1, horAxisY is not null ? -1 : 1, verAxisX ?? 0, horAxisY ?? 0);
        using var copy = new SKPath(path);
        copy.Transform(matrix);

        var newRect = pathTightBounds;
        if (verAxisX is not null)
            newRect = newRect.ReflectX((int)verAxisX);
        if (horAxisY is not null)
            newRect = newRect.ReflectY((int)horAxisY);
        return new ClearPathOperation(copy, newRect);
    }
}
