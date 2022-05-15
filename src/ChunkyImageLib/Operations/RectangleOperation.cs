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
        // use a clipping rectangle with 2x stroke width to make sure stroke doesn't stick outside rect bounds
        skiaSurf.Canvas.Save();

        var convertedPos = OperationHelper.ConvertForResolution(-Data.Size.Abs() / 2, chunk.Resolution);
        var convertedCenter = OperationHelper.ConvertForResolution(Data.Center, chunk.Resolution) - chunkPos.Multiply(chunk.PixelSize);

        var convertedSize = OperationHelper.ConvertForResolution(Data.Size.Abs(), chunk.Resolution);
        int convertedStroke = (int)Math.Round(chunk.Resolution.Multiplier() * Data.StrokeWidth);

        var rect = SKRect.Create((SKPoint)convertedPos, (SKSize)convertedSize);

        skiaSurf.Canvas.Translate((SKPoint)convertedCenter);
        skiaSurf.Canvas.RotateRadians((float)Data.Angle);
        skiaSurf.Canvas.ClipRect(rect);

        // draw fill
        using SKPaint paint = new()
        {
            Color = Data.FillColor,
            Style = SKPaintStyle.Fill,
            BlendMode = Data.BlendMode,
        };

        if (Data.FillColor.Alpha > 0)
            skiaSurf.Canvas.DrawRect(rect, paint);

        // draw stroke
        paint.Color = Data.StrokeColor;
        paint.Style = SKPaintStyle.Stroke;
        paint.StrokeWidth = convertedStroke * 2;

        skiaSurf.Canvas.DrawRect(rect, paint);

        // get rid of the clipping rectangle
        skiaSurf.Canvas.Restore();
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
