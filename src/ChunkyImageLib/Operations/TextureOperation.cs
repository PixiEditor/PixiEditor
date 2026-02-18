using ChunkyImageLib.DataHolders;
using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;

namespace ChunkyImageLib.Operations;

internal class TextureOperation : IMirroredDrawOperation
{
    private Matrix3X3 transformMatrix;
    private ShapeCorners corners;
    private Texture toPaint;
    private bool imageWasCopied = false;
    private readonly Paint? customPaint;

    public bool IgnoreEmptyChunks => false;
    public bool NeedsDrawInSrgb => false;

    public TextureOperation(VecI pos, Texture image, Paint? paint = null, bool copyImage = true)
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
            toPaint = new Texture(image);
        else
            toPaint = image;
        imageWasCopied = copyImage;
    }

    public TextureOperation(ShapeCorners corners, Texture image, Paint? paint = null, bool copyImage = true)
    {
        if (paint is not null)
            customPaint = paint.Clone();

        this.corners = corners;
        transformMatrix = OperationHelper.CreateMatrixFromPoints(corners, image.Size);

        // copying is needed for thread safety
        if (copyImage)
            toPaint = new Texture(image);
        else
            toPaint = image;
        imageWasCopied = copyImage;
    }

    public TextureOperation(Matrix3X3 transformMatrix, Texture image, Paint? paint = null, bool copyImage = true)
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
            toPaint = new Texture(image);
        else
            toPaint = image;
        imageWasCopied = copyImage;
    }

    public void DrawOnChunk(Chunk targetChunk, VecI chunkPos)
    {
        //customPaint.FilterQuality = chunk.Resolution != ChunkResolution.Full;
        float scaleMult = (float)targetChunk.Resolution.Multiplier();
        VecD trans = -chunkPos * ChunkPool.FullChunkSize;

        var scaleTrans = Matrix3X3.CreateScaleTranslation(scaleMult, scaleMult, (float)trans.X * scaleMult, (float)trans.Y * scaleMult);
        var finalMatrix = Matrix3X3.Concat(scaleTrans, transformMatrix);

        targetChunk.Surface.DrawingSurface.Canvas.Save();
        targetChunk.Surface.DrawingSurface.Canvas.SetMatrix(finalMatrix);
        using var snap = toPaint.DrawingSurface.Snapshot();
        targetChunk.Surface.DrawingSurface.Canvas.DrawImage(snap, 0, 0, customPaint);
        targetChunk.Surface.DrawingSurface.Canvas.Restore();
    }

    public AffectedArea FindAffectedArea(VecI imageSize)
    {
        return new AffectedArea(OperationHelper.FindChunksTouchingQuadrilateral(corners, ChunkPool.FullChunkSize), (RectI)corners.AABBBounds.RoundOutwards());
    }

    public void Dispose()
    {
        if (imageWasCopied)
            toPaint.Dispose();
        customPaint?.Dispose();
    }

    public IDrawOperation AsMirrored(double? verAxisX, double? horAxisY)
    {
        if (verAxisX is not null && horAxisY is not null)
        {
            return new TextureOperation
                (corners.AsMirroredAcrossVerAxis((double)verAxisX).AsMirroredAcrossHorAxis((double)horAxisY), toPaint, customPaint, imageWasCopied);
        }
        if (verAxisX is not null)
        {
            return new TextureOperation
                (corners.AsMirroredAcrossVerAxis((double)verAxisX), toPaint, customPaint, imageWasCopied);
        }
        if (horAxisY is not null)
        {
            return new TextureOperation
                (corners.AsMirroredAcrossHorAxis((double)horAxisY), toPaint, customPaint, imageWasCopied);
        }
        return new TextureOperation(corners, toPaint, customPaint, imageWasCopied);
    }
}
