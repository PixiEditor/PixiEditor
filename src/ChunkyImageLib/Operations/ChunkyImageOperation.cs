using ChunkyImageLib.DataHolders;
using PixiEditor.DrawingApi.Core.Numerics;

namespace ChunkyImageLib.Operations;
internal class ChunkyImageOperation : IDrawOperation
{
    private readonly ChunkyImage imageToDraw;
    private readonly VecI pos;
    private readonly bool mirrorHorizontal;
    private readonly bool mirrorVertical;

    public bool IgnoreEmptyChunks => false;

    public ChunkyImageOperation(ChunkyImage imageToDraw, VecI pos, bool mirrorHorizontal, bool mirrorVertical)
    {
        this.imageToDraw = imageToDraw;
        this.pos = pos;
        this.mirrorHorizontal = mirrorHorizontal;
        this.mirrorVertical = mirrorVertical;
    }

    public void DrawOnChunk(Chunk chunk, VecI chunkPos)
    {
        chunk.Surface.DrawingSurface.Canvas.Save();

        {
            VecI pixelPos = chunkPos * ChunkyImage.FullChunkSize;
            VecI topLeftImageCorner = GetTopLeft();
            RectD clippingRect = RectD.Create(
                OperationHelper.ConvertForResolution(topLeftImageCorner - pixelPos, chunk.Resolution),
                OperationHelper.ConvertForResolution(imageToDraw.CommittedSize, chunk.Resolution));
            chunk.Surface.DrawingSurface.Canvas.ClipRect(clippingRect);
        }

        VecI chunkPixelCenter = chunkPos * ChunkyImage.FullChunkSize;
        chunkPixelCenter.X += ChunkyImage.FullChunkSize / 2;
        chunkPixelCenter.Y += ChunkyImage.FullChunkSize / 2;

        VecI chunkCenterOnImage = chunkPixelCenter - pos;
        VecI chunkSize = chunk.PixelSize;
        if (mirrorHorizontal)
        {
            chunk.Surface.DrawingSurface.Canvas.Scale(-1, 1, chunkSize.X / 2f, chunkSize.Y / 2f);
            chunkCenterOnImage.X = -chunkCenterOnImage.X;
        }
        if (mirrorVertical)
        {
            chunk.Surface.DrawingSurface.Canvas.Scale(1, -1, chunkSize.X / 2f, chunkSize.Y / 2f);
            chunkCenterOnImage.Y = -chunkCenterOnImage.Y;
        }

        VecI halfChunk = new(ChunkyImage.FullChunkSize / 2, ChunkyImage.FullChunkSize / 2);

        VecI topLeft = OperationHelper.GetChunkPos(chunkCenterOnImage - halfChunk, ChunkyImage.FullChunkSize);
        VecI topRight = OperationHelper.GetChunkPos(
            new VecI(chunkCenterOnImage.X + halfChunk.X, chunkCenterOnImage.Y - halfChunk.Y), ChunkyImage.FullChunkSize);
        VecI bottomRight = OperationHelper.GetChunkPos(chunkCenterOnImage + halfChunk, ChunkyImage.FullChunkSize);
        VecI bottomLeft = OperationHelper.GetChunkPos(
            new VecI(chunkCenterOnImage.X - halfChunk.X, chunkCenterOnImage.Y + halfChunk.Y), ChunkyImage.FullChunkSize);

        imageToDraw.DrawCommittedChunkOn(
            topLeft,
            chunk.Resolution,
            chunk.Surface.DrawingSurface,
            (VecI)((topLeft * ChunkyImage.FullChunkSize - chunkCenterOnImage).Add(ChunkyImage.FullChunkSize / 2) * chunk.Resolution.Multiplier()));

        VecI gridShift = pos % ChunkyImage.FullChunkSize;
        if (gridShift.X != 0)
        {
            imageToDraw.DrawCommittedChunkOn(
            topRight,
            chunk.Resolution,
            chunk.Surface.DrawingSurface,
            (VecI)((topRight * ChunkyImage.FullChunkSize - chunkCenterOnImage).Add(ChunkyImage.FullChunkSize / 2) * chunk.Resolution.Multiplier()));
        }
        if (gridShift.Y != 0)
        {
            imageToDraw.DrawCommittedChunkOn(
            bottomLeft,
            chunk.Resolution,
            chunk.Surface.DrawingSurface,
            (VecI)((bottomLeft * ChunkyImage.FullChunkSize - chunkCenterOnImage).Add(ChunkyImage.FullChunkSize / 2) * chunk.Resolution.Multiplier()));
        }
        if (gridShift.X != 0 && gridShift.Y != 0)
        {
            imageToDraw.DrawCommittedChunkOn(
            bottomRight,
            chunk.Resolution,
            chunk.Surface.DrawingSurface,
            (VecI)((bottomRight * ChunkyImage.FullChunkSize - chunkCenterOnImage).Add(ChunkyImage.FullChunkSize / 2) * chunk.Resolution.Multiplier()));
        }

        chunk.Surface.DrawingSurface.Canvas.Restore();
    }

    public HashSet<VecI> FindAffectedChunks(VecI imageSize)
    {
        return OperationHelper.FindChunksTouchingRectangle(new(GetTopLeft(), imageToDraw.CommittedSize), ChunkyImage.FullChunkSize);
    }

    private VecI GetTopLeft()
    {
        VecI topLeft = pos;
        if (mirrorHorizontal)
            topLeft.X -= imageToDraw.CommittedSize.X;
        if (mirrorVertical)
            topLeft.Y -= imageToDraw.CommittedSize.Y;
        return topLeft;
    }

    public IDrawOperation AsMirrored(int? verAxisX, int? horAxisY)
    {
        var newPos = pos;
        if (verAxisX is not null)
            newPos = newPos.ReflectX((int)verAxisX);
        if (horAxisY is not null)
            newPos = newPos.ReflectY((int)horAxisY);
        return new ChunkyImageOperation(imageToDraw, newPos, mirrorHorizontal ^ (verAxisX is not null), mirrorVertical ^ (horAxisY is not null));
    }

    public void Dispose() { }
}
