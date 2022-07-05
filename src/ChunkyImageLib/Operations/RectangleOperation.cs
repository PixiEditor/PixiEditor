using ChunkyImageLib.DataHolders;
using SkiaSharp;

namespace ChunkyImageLib.Operations;

internal class RectangleOperation : IDrawOperation
{
    public RectangleOperation(ShapeData rect)
    {
        Data = rect;
    }

    public ShapeData Data { get; }

    public bool IgnoreEmptyChunks => false;

    public void DrawOnChunk(Chunk chunk, VecI chunkPos)
    {
        var skiaSurf = chunk.Surface.SkiaSurface;

        var surf = chunk.Surface.SkiaSurface;

        var rect = RectD.FromCenterAndSize(Data.Center, Data.Size.Abs());
        var innerRect = rect.Inflate(-Data.StrokeWidth);
        if (innerRect.IsZeroOrNegativeArea)
            innerRect = RectD.Empty;

        surf.Canvas.Save();
        surf.Canvas.Scale((float)chunk.Resolution.Multiplier());
        surf.Canvas.Translate(-chunkPos * ChunkyImage.FullChunkSize);
        skiaSurf.Canvas.RotateRadians((float)Data.Angle, (float)rect.Center.X, (float)rect.Center.Y);

        // draw fill
        if (Data.FillColor.Alpha > 0)
        {
            skiaSurf.Canvas.Save();
            skiaSurf.Canvas.ClipRect((SKRect)innerRect);
            skiaSurf.Canvas.DrawColor(Data.FillColor, Data.BlendMode);
            skiaSurf.Canvas.Restore();
        }

        // draw stroke
        skiaSurf.Canvas.Save();
        skiaSurf.Canvas.ClipRect((SKRect)rect);
        skiaSurf.Canvas.ClipRect((SKRect)innerRect, SKClipOperation.Difference);
        skiaSurf.Canvas.DrawColor(Data.StrokeColor, Data.BlendMode);
        skiaSurf.Canvas.Restore();

        surf.Canvas.Restore();
    }

    public HashSet<VecI> FindAffectedChunks()
    {
        if (Math.Abs(Data.Size.X) < 1 || Math.Abs(Data.Size.Y) < 1 || Data.StrokeColor.Alpha == 0 && Data.FillColor.Alpha == 0)
            return new();
        if (Data.FillColor.Alpha != 0 || Math.Abs(Data.Size.X) == 1 || Math.Abs(Data.Size.Y) == 1)
            return OperationHelper.FindChunksTouchingRectangle(Data.Center, Data.Size.Abs(), Data.Angle, ChunkPool.FullChunkSize);

        var chunks = OperationHelper.FindChunksTouchingRectangle(Data.Center, Data.Size.Abs(), Data.Angle, ChunkPool.FullChunkSize);
        chunks.ExceptWith(
            OperationHelper.FindChunksFullyInsideRectangle(
                Data.Center,
                Data.Size.Abs() - new VecD(Data.StrokeWidth * 2, Data.StrokeWidth * 2),
                Data.Angle,
                ChunkPool.FullChunkSize));
        return chunks;
    }

    public void Dispose() { }

    public IDrawOperation AsMirrored(int? verAxisX, int? horAxisY)
    {
        if (verAxisX is not null && horAxisY is not null)
            return new RectangleOperation(Data.AsMirroredAcrossHorAxis((int)horAxisY).AsMirroredAcrossVerAxis((int)verAxisX));
        else if (verAxisX is not null)
            return new RectangleOperation(Data.AsMirroredAcrossVerAxis((int)verAxisX));
        else if (horAxisY is not null)
            return new RectangleOperation(Data.AsMirroredAcrossHorAxis((int)horAxisY));
        return new RectangleOperation(Data);
    }
}
