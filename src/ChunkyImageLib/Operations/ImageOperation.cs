using ChunkyImageLib.DataHolders;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface;
using SkiaSharp;

namespace ChunkyImageLib.Operations;

internal class ImageOperation : IDrawOperation
{
    private Matrix3X3 transformMatrix;
    private ShapeCorners corners;
    private Surface toPaint;
    private bool imageWasCopied = false;
    private readonly Paint? customPaint;

    public bool IgnoreEmptyChunks => false;

    public ImageOperation(VecI pos, Surface image, Paint? paint = null, bool copyImage = true)
    {
        if (paint is not null)
            customPaint = paint.Clone();

        corners = new()
        {
            TopLeft = pos,
            TopRight = new(pos.X + image.Size.X, pos.Y),
            BottomRight = pos + image.Size,
            BottomLeft = new VecD(pos.X, pos.Y + image.Size.Y)
        };
        transformMatrix = Matrix3X3.CreateIdentity();
        transformMatrix.TransX = pos.X;
        transformMatrix.TransY = pos.Y;

        // copying is needed for thread safety
        if (copyImage)
            toPaint = new Surface(image);
        else
            toPaint = image;
        imageWasCopied = copyImage;
    }

    public ImageOperation(ShapeCorners corners, Surface image, Paint? paint = null, bool copyImage = true)
    {
        if (paint is not null)
            customPaint = paint.Clone();

        this.corners = corners;
        transformMatrix = OperationHelper.CreateMatrixFromPoints(corners, image.Size);

        // copying is needed for thread safety
        if (copyImage)
            toPaint = new Surface(image);
        else
            toPaint = image;
        imageWasCopied = copyImage;
    }

    public ImageOperation(Matrix3X3 transformMatrix, Surface image, Paint? paint = null, bool copyImage = true)
    {
        if (paint is not null)
            customPaint = paint.Clone();

        this.corners = new ShapeCorners()
        {
            TopLeft = transformMatrix.MapPoint(0, 0),
            TopRight = transformMatrix.MapPoint(image.Size.X, 0),
            BottomLeft = transformMatrix.MapPoint(0, image.Size.Y),
            BottomRight = transformMatrix.MapPoint(image.Size),
        };
        this.transformMatrix = transformMatrix;

        // copying is needed for thread safety
        if (copyImage)
            toPaint = new Surface(image);
        else
            toPaint = image;
        imageWasCopied = copyImage;
    }



    public void DrawOnChunk(Chunk chunk, VecI chunkPos)
    {
        //customPaint.FilterQuality = chunk.Resolution != ChunkResolution.Full;
        float scaleMult = (float)chunk.Resolution.Multiplier();
        VecD trans = -chunkPos * ChunkPool.FullChunkSize;

        var scaleTrans = Matrix3X3.CreateScaleTranslation(scaleMult, scaleMult, (float)trans.X * scaleMult, (float)trans.Y * scaleMult);
        var finalMatrix = Matrix3X3.Concat(scaleTrans, transformMatrix);

        chunk.Surface.DrawingSurface.Canvas.Save();
        chunk.Surface.DrawingSurface.Canvas.SetMatrix(finalMatrix);
        chunk.Surface.DrawingSurface.Canvas.DrawSurface(toPaint.DrawingSurface, 0, 0, customPaint);
        chunk.Surface.DrawingSurface.Canvas.Restore();
    }

    public HashSet<VecI> FindAffectedChunks(VecI imageSize)
    {
        return OperationHelper.FindChunksTouchingQuadrilateral(corners, ChunkPool.FullChunkSize);
    }

    public void Dispose()
    {
        if (imageWasCopied)
            toPaint.Dispose();
        customPaint?.Dispose();
    }

    public IDrawOperation AsMirrored(int? verAxisX, int? horAxisY)
    {
        if (verAxisX is not null && horAxisY is not null)
            return new ImageOperation
                (corners.AsMirroredAcrossVerAxis((int)verAxisX).AsMirroredAcrossHorAxis((int)horAxisY), toPaint, customPaint, imageWasCopied);
        if (verAxisX is not null)
            return new ImageOperation
                (corners.AsMirroredAcrossVerAxis((int)verAxisX), toPaint, customPaint, imageWasCopied);
        if (horAxisY is not null)
            return new ImageOperation
                (corners.AsMirroredAcrossHorAxis((int)horAxisY), toPaint, customPaint, imageWasCopied);
        return new ImageOperation(corners, toPaint, customPaint, imageWasCopied);
    }
}
