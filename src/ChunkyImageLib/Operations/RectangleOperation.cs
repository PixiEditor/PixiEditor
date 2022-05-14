using ChunkyImageLib.DataHolders;
using SkiaSharp;

namespace ChunkyImageLib.Operations;

internal record class RectangleOperation : IDrawOperation
{
    public RectangleOperation(ShapeData rect)
    {
        Data = rect;
    }

    public ShapeData Data { get; }

    public bool IgnoreEmptyChunks => false;

    public void DrawOnChunk(Chunk chunk, Vector2i chunkPos)
    {
        var skiaSurf = chunk.Surface.SkiaSurface;
        // use a clipping rectangle with 2x stroke width to make sure stroke doesn't stick outside rect bounds
        skiaSurf.Canvas.Save();

        var convertedPos = OperationHelper.ConvertForResolution(-Data.Size / 2, chunk.Resolution);
        var convertedCenter = OperationHelper.ConvertForResolution(Data.Center, chunk.Resolution) - chunkPos.Multiply(chunk.PixelSize);

        var convertedSize = OperationHelper.ConvertForResolution(Data.Size, chunk.Resolution);
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

    public HashSet<Vector2i> FindAffectedChunks()
    {
        if (Data.Size.X < 1 || Data.Size.Y < 1 || Data.StrokeColor.Alpha == 0 && Data.FillColor.Alpha == 0)
            return new();
        if (Data.FillColor.Alpha != 0 || Data.Size.X == 1 || Data.Size.Y == 1)
            return OperationHelper.FindChunksTouchingRectangle(Data.Center, Data.Size, Data.Angle, ChunkPool.FullChunkSize);

        var chunks = OperationHelper.FindChunksTouchingRectangle(Data.Center, Data.Size, Data.Angle, ChunkPool.FullChunkSize);
        chunks.ExceptWith(
            OperationHelper.FindChunksFullyInsideRectangle(
                Data.Center,
                Data.Size - new Vector2d(Data.StrokeWidth * 2, Data.StrokeWidth * 2),
                Data.Angle,
                ChunkPool.FullChunkSize));
        return chunks;
    }

    public void Dispose() { }
}
