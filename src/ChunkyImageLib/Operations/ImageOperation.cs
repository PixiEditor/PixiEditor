using ChunkyImageLib.DataHolders;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;

namespace ChunkyImageLib.Operations;

internal class ImageOperation : IMirroredDrawOperation
{
    private Matrix3X3 transformMatrix;
    private ShapeCorners corners;
    private Surface toPaint;
    private bool imageWasCopied = false;
    private SamplingOptions samplingOptions = SamplingOptions.Default;
    private readonly Paint? customPaint;

    public bool IgnoreEmptyChunks => false;
    public bool NeedsDrawInSrgb => false;

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
        this.samplingOptions = samplingOptions;

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

    public ImageOperation(Matrix3X3 transformMatrix, Surface image, SamplingOptions samplingOptions, Paint? paint = null, bool copyImage = true)
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
        this.samplingOptions = samplingOptions;

        // copying is needed for thread safety
        if (copyImage)
            toPaint = new Surface(image);
        else
            toPaint = image;
        imageWasCopied = copyImage;
    }

    public void DrawOnChunk(Chunk targetChunk, VecI chunkPos)
    {
        //customPaint.FilterQuality = targetChunk.Resolution != ChunkResolution.Full ? FilterQuality.High : FilterQuality.None;
        var sampling = samplingOptions;
        if (samplingOptions == SamplingOptions.Default && targetChunk.Resolution != ChunkResolution.Full)
        {
            sampling = SamplingOptions.Bilinear;
        }

        float scaleMult = (float)targetChunk.Resolution.Multiplier();
        VecD trans = -chunkPos * ChunkPool.FullChunkSize;

        var scaleTrans = Matrix3X3.CreateScaleTranslation(scaleMult, scaleMult, (float)trans.X * scaleMult,
            (float)trans.Y * scaleMult);
        var finalMatrix = Matrix3X3.Concat(scaleTrans, transformMatrix);

        using var snapshot = toPaint.DrawingSurface.Snapshot();
        targetChunk.Surface.DrawingSurface.Canvas.Save();
        targetChunk.Surface.DrawingSurface.Canvas.SetMatrix(finalMatrix);

        bool hasPerspective = Math.Abs(finalMatrix.Persp0) > 0.0001 || Math.Abs(finalMatrix.Persp1) > 0.0001;

        // More optimized, but works badly with perspective transformation
        if (!hasPerspective)
        {
            ShapeCorners chunkCorners = new ShapeCorners(new RectD(VecD.Zero, targetChunk.PixelSize));
            RectD rect = chunkCorners.WithMatrix(finalMatrix.Invert()).AABBBounds;

            targetChunk.Surface.DrawingSurface.Canvas.DrawImage(snapshot, rect, rect, customPaint, sampling);
        }
        else
        {
            // Slower, but works with perspective transformation
            targetChunk.Surface.DrawingSurface.Canvas.DrawImage(snapshot, 0, 0, sampling, customPaint);
        }

        targetChunk.Surface.DrawingSurface.Canvas.Restore();
    }

    public AffectedArea FindAffectedArea(VecI imageSize)
    {
        return new AffectedArea(OperationHelper.FindChunksTouchingQuadrilateral(corners, ChunkPool.FullChunkSize),
            (RectI)corners.AABBBounds.RoundOutwards());
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
            return new ImageOperation
            (corners.AsMirroredAcrossVerAxis((double)verAxisX).AsMirroredAcrossHorAxis((double)horAxisY), toPaint,
                customPaint, imageWasCopied);
        }

        if (verAxisX is not null)
        {
            return new ImageOperation
                (corners.AsMirroredAcrossVerAxis((double)verAxisX), toPaint, customPaint, imageWasCopied);
        }

        if (horAxisY is not null)
        {
            return new ImageOperation
                (corners.AsMirroredAcrossHorAxis((double)horAxisY), toPaint, customPaint, imageWasCopied);
        }

        return new ImageOperation(corners, toPaint, customPaint, imageWasCopied);
    }
}
