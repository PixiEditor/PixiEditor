using ChunkyImageLib.DataHolders;
using SkiaSharp;

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
        chunk.Surface.SkiaSurface.Canvas.Save();

        {
            VecI pixelPos = chunkPos * ChunkyImage.FullChunkSize;
            VecI topLeft = GetTopLeft();
            SKRect clippingRect = SKRect.Create(
                OperationHelper.ConvertForResolution(topLeft - pixelPos, chunk.Resolution),
                OperationHelper.ConvertForResolution(imageToDraw.CommittedSize, chunk.Resolution));
            chunk.Surface.SkiaSurface.Canvas.ClipRect(clippingRect);
        }

        if (mirrorHorizontal)
        {
            chunkPos.X = (-((chunkPos.X * ChunkyImage.FullChunkSize) - pos.X) + pos.X) / ChunkyImage.FullChunkSize - 1;
            chunk.Surface.SkiaSurface.Canvas.Translate(chunk.PixelSize.X, 0);
            chunk.Surface.SkiaSurface.Canvas.Scale(-1, 0);
        }
        if (mirrorVertical)
        {
            chunkPos.Y = (-((chunkPos.Y * ChunkyImage.FullChunkSize) - pos.Y) + pos.Y) / ChunkyImage.FullChunkSize - 1;
            chunk.Surface.SkiaSurface.Canvas.Translate(0, chunk.PixelSize.Y);
            chunk.Surface.SkiaSurface.Canvas.Scale(0, -1);
        }

        VecD posOnImage = chunkPos - (pos / (double)ChunkyImage.FullChunkSize);
        int topY = (int)Math.Floor(posOnImage.Y);
        int bottomY = (int)Math.Ceiling(posOnImage.Y);
        int leftX = (int)Math.Floor(posOnImage.X);
        int rightX = (int)Math.Ceiling(posOnImage.X);


        int chunkPixelSize = chunk.Resolution.PixelSize();

        // this is kinda dumb
        if (pos % ChunkyImage.FullChunkSize == VecI.Zero)
        {
            imageToDraw.DrawCommittedChunkOn((VecI)posOnImage, chunk.Resolution, chunk.Surface.SkiaSurface, VecI.Zero);
        }
        else if (pos.X % ChunkyImage.FullChunkSize == 0)
        {
            imageToDraw.DrawCommittedChunkOn(new VecI((int)posOnImage.X, topY), chunk.Resolution, chunk.Surface.SkiaSurface, new VecI(0, (int)((topY - posOnImage.Y) * chunkPixelSize)));
            imageToDraw.DrawCommittedChunkOn(new VecI((int)posOnImage.X, bottomY), chunk.Resolution, chunk.Surface.SkiaSurface, new VecI(0, (int)((bottomY - posOnImage.Y) * chunkPixelSize)));
        }
        else if (pos.Y % ChunkyImage.FullChunkSize == 0)
        {
            imageToDraw.DrawCommittedChunkOn(new VecI(leftX, (int)posOnImage.Y), chunk.Resolution, chunk.Surface.SkiaSurface, new VecI((int)((leftX - posOnImage.X) * chunkPixelSize), 0));
            imageToDraw.DrawCommittedChunkOn(new VecI(rightX, (int)posOnImage.Y), chunk.Resolution, chunk.Surface.SkiaSurface, new VecI((int)((rightX - posOnImage.X) * chunkPixelSize), 0));
        }
        else
        {
            imageToDraw.DrawCommittedChunkOn(new VecI(leftX, topY), chunk.Resolution, chunk.Surface.SkiaSurface, new VecI((int)((leftX - posOnImage.X) * chunkPixelSize), (int)((topY - posOnImage.Y) * chunkPixelSize)));
            imageToDraw.DrawCommittedChunkOn(new VecI(rightX, topY), chunk.Resolution, chunk.Surface.SkiaSurface, new VecI((int)((rightX - posOnImage.X) * chunkPixelSize), (int)((topY - posOnImage.Y) * chunkPixelSize)));
            imageToDraw.DrawCommittedChunkOn(new VecI(leftX, bottomY), chunk.Resolution, chunk.Surface.SkiaSurface, new VecI((int)((leftX - posOnImage.X) * chunkPixelSize), (int)((bottomY - posOnImage.Y) * chunkPixelSize)));
            imageToDraw.DrawCommittedChunkOn(new VecI(rightX, bottomY), chunk.Resolution, chunk.Surface.SkiaSurface, new VecI((int)((rightX - posOnImage.X) * chunkPixelSize), (int)((bottomY - posOnImage.Y) * chunkPixelSize)));
        }

        chunk.Surface.SkiaSurface.Canvas.Restore();
    }

    public HashSet<VecI> FindAffectedChunks()
    {
        return OperationHelper.FindChunksFullyInsideRectangle(GetTopLeft(), imageToDraw.CommittedSize, ChunkyImage.FullChunkSize);
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
