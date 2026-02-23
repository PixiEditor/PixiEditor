using ChunkyImageLib.DataHolders;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;

namespace ChunkyImageLib.Operations;
internal class ChunkyImageOperation : IMirroredDrawOperation
{
    private readonly ChunkyImage imageToDraw;
    private readonly VecI targetPos;
    private readonly bool mirrorHorizontal;
    private readonly bool mirrorVertical;
    private readonly bool drawUpToDate;

    public bool IgnoreEmptyChunks => false;
    public bool NeedsDrawInSrgb => false;

    public ChunkyImageOperation(ChunkyImage imageToDraw, VecI targetPos, bool mirrorHorizontal, bool mirrorVertical,
        bool drawUpToDate)
    {
        this.imageToDraw = imageToDraw;
        this.targetPos = targetPos;
        this.mirrorHorizontal = mirrorHorizontal;
        this.mirrorVertical = mirrorVertical;
        this.drawUpToDate = drawUpToDate;
    }

    public void DrawOnChunk(Chunk targetChunk, VecI chunkPos)
    {
        targetChunk.Surface.DrawingSurface.Canvas.Save();
        {
            VecI pixelPos = chunkPos * ChunkyImage.FullChunkSize;
            VecI topLeftImageCorner = GetTopLeft();
            RectD clippingRect = RectD.Create(
                OperationHelper.ConvertForResolution(topLeftImageCorner - pixelPos, targetChunk.Resolution),
                OperationHelper.ConvertForResolution(drawUpToDate ? imageToDraw.LatestSize : imageToDraw.CommittedSize, targetChunk.Resolution));
            targetChunk.Surface.DrawingSurface.Canvas.ClipRect(clippingRect);
        }

        VecI chunkPixelCenter = chunkPos * ChunkyImage.FullChunkSize;
        chunkPixelCenter.X += ChunkyImage.FullChunkSize / 2;
        chunkPixelCenter.Y += ChunkyImage.FullChunkSize / 2;

        VecI chunkCenterOnImage = chunkPixelCenter - targetPos;
        VecI chunkSize = targetChunk.PixelSize;
        if (mirrorHorizontal)
        {
            targetChunk.Surface.DrawingSurface.Canvas.Scale(-1, 1, chunkSize.X / 2f, chunkSize.Y / 2f);
            chunkCenterOnImage.X = -chunkCenterOnImage.X;
        }
        if (mirrorVertical)
        {
            targetChunk.Surface.DrawingSurface.Canvas.Scale(1, -1, chunkSize.X / 2f, chunkSize.Y / 2f);
            chunkCenterOnImage.Y = -chunkCenterOnImage.Y;
        }

        VecI halfChunk = new(ChunkyImage.FullChunkSize / 2, ChunkyImage.FullChunkSize / 2);

        VecI topLeft = OperationHelper.GetChunkPos(chunkCenterOnImage - halfChunk, ChunkyImage.FullChunkSize);
        VecI topRight = OperationHelper.GetChunkPos(
            new VecI(chunkCenterOnImage.X + halfChunk.X, chunkCenterOnImage.Y - halfChunk.Y), ChunkyImage.FullChunkSize);
        VecI bottomRight = OperationHelper.GetChunkPos(chunkCenterOnImage + halfChunk, ChunkyImage.FullChunkSize);
        VecI bottomLeft = OperationHelper.GetChunkPos(
            new VecI(chunkCenterOnImage.X - halfChunk.X, chunkCenterOnImage.Y + halfChunk.Y), ChunkyImage.FullChunkSize);

        Func<VecI, ChunkResolution, Canvas, VecD, Paint?, SamplingOptions?, bool> drawMethod = drawUpToDate ? imageToDraw.DrawMostUpToDateChunkOn : imageToDraw.DrawCommittedChunkOn;
        
        drawMethod(
            topLeft,
            targetChunk.Resolution,
            targetChunk.Surface.DrawingSurface.Canvas,
            (VecI)((topLeft * ChunkyImage.FullChunkSize - chunkCenterOnImage).Add(ChunkyImage.FullChunkSize / 2) * targetChunk.Resolution.Multiplier()), null, null);

        VecI gridShift = targetPos % ChunkyImage.FullChunkSize;
        if (gridShift.X != 0)
        {
            drawMethod(
            topRight,
            targetChunk.Resolution,
            targetChunk.Surface.DrawingSurface.Canvas,
            (VecI)((topRight * ChunkyImage.FullChunkSize - chunkCenterOnImage).Add(ChunkyImage.FullChunkSize / 2) * targetChunk.Resolution.Multiplier()),
            null, null);
        }
        if (gridShift.Y != 0)
        {
            drawMethod(
            bottomLeft,
            targetChunk.Resolution,
            targetChunk.Surface.DrawingSurface.Canvas,
            (VecI)((bottomLeft * ChunkyImage.FullChunkSize - chunkCenterOnImage).Add(ChunkyImage.FullChunkSize / 2) * targetChunk.Resolution.Multiplier()),
            null, null);
        }
        if (gridShift.X != 0 && gridShift.Y != 0)
        {
            drawMethod(
            bottomRight,
            targetChunk.Resolution,
            targetChunk.Surface.DrawingSurface.Canvas,
            (VecI)((bottomRight * ChunkyImage.FullChunkSize - chunkCenterOnImage).Add(ChunkyImage.FullChunkSize / 2) * targetChunk.Resolution.Multiplier()),
            null, null);
        }

        targetChunk.Surface.DrawingSurface.Canvas.Restore();
    }

    public AffectedArea FindAffectedArea(VecI imageSize)
    {
        RectI rect = new(GetTopLeft(), imageToDraw.CommittedSize);
        return new AffectedArea(OperationHelper.FindChunksTouchingRectangle(rect, ChunkyImage.FullChunkSize), rect);
    }

    private VecI GetTopLeft()
    {
        VecI topLeft = targetPos;
        if (mirrorHorizontal)
            topLeft.X -= imageToDraw.CommittedSize.X;
        if (mirrorVertical)
            topLeft.Y -= imageToDraw.CommittedSize.Y;
        return topLeft;
    }

    public IDrawOperation AsMirrored(double? verAxisX, double? horAxisY)
    {
        var newPos = targetPos;
        if (verAxisX is not null)
            newPos = (VecI)newPos.ReflectX((double)verAxisX).Round();
        if (horAxisY is not null)
            newPos = (VecI)newPos.ReflectY((double)horAxisY).Round();
        return new ChunkyImageOperation(imageToDraw, newPos, mirrorHorizontal ^ (verAxisX is not null), mirrorVertical ^ (horAxisY is not null),
            drawUpToDate);
    }

    public void Dispose() { }
}
