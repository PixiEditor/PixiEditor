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

        var convertedPos = OperationHelper.ConvertForResolution(Data.Pos, chunk.Resolution);
        var convertedSize = OperationHelper.ConvertForResolution(Data.Size, chunk.Resolution);
        int convertedStroke = (int)Math.Round(chunk.Resolution.Multiplier() * Data.StrokeWidth);

        var rect = SKRect.Create(convertedPos - chunkPos.Multiply(chunk.PixelSize), convertedSize);
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
            return GetChunksForFilled(ChunkPool.FullChunkSize);
        return GetChunksForStroke(ChunkPool.FullChunkSize);
    }

    private static (int, int)? Inset(int min, int max, int inset)
    {
        int insetMin = Math.Min(min + inset - 1, max);
        int insetMax = Math.Max(max - inset + 1, min);
        //is rectangle fully filled by the stroke
        if (insetMin + 1 >= insetMax)
            return null;
        return (insetMin, insetMax);
    }

    private HashSet<Vector2i> GetChunksForStroke(int chunkSize)
    {
        //we need to account for wide strokes covering multiple chunks
        //find inner stroke boudaries in pixel coords
        var xInset = Inset(Data.Pos.X, Data.MaxPos.X, Data.StrokeWidth);
        var yInset = Inset(Data.Pos.Y, Data.MaxPos.Y, Data.StrokeWidth);
        if (xInset is null || yInset is null)
            return GetChunksForFilled(chunkSize);

        //find two chunk rectanges, outer and inner
        Vector2i min = OperationHelper.GetChunkPos(Data.Pos, chunkSize);
        Vector2i max = OperationHelper.GetChunkPos(Data.MaxPos, chunkSize);
        Vector2i minInset = OperationHelper.GetChunkPos(new(xInset.Value.Item1, yInset.Value.Item1), chunkSize);
        Vector2i maxInset = OperationHelper.GetChunkPos(new(xInset.Value.Item2, yInset.Value.Item2), chunkSize);

        //fill in sides
        HashSet<Vector2i> chunks = new();
        AddRectangle(min, new(max.X, minInset.Y), chunks); //top
        AddRectangle(new(min.X, minInset.Y + 1), new(minInset.X, maxInset.Y - 1), chunks); //left
        AddRectangle(new(maxInset.X, minInset.Y + 1), new(max.X, maxInset.Y - 1), chunks); //right
        AddRectangle(new(min.X, maxInset.Y), max, chunks); //bottom
        return chunks;
    }

    private HashSet<Vector2i> GetChunksForFilled(int chunkSize)
    {
        Vector2i min = OperationHelper.GetChunkPos(Data.Pos, chunkSize);
        Vector2i max = OperationHelper.GetChunkPos(Data.MaxPos, chunkSize);
        HashSet<Vector2i> output = new();
        AddRectangle(min, max, output);
        return output;
    }

    private static void AddRectangle(Vector2i min, Vector2i max, HashSet<Vector2i> set)
    {
        for (int x = min.X; x <= max.X; x++)
        {
            for (int y = min.Y; y <= max.Y; y++)
            {
                set.Add(new(x, y));
            }
        }
    }

    public void Dispose() { }
}
