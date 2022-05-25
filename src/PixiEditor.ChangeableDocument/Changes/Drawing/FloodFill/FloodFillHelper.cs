using ChunkyImageLib.Operations;
using SkiaSharp;

namespace PixiEditor.ChangeableDocument.Changes.Drawing.FloodFill;
internal class FloodFillHelper
{
    public static (HashSet<VecI>, CommittedChunkStorage) FloodFillAndCommit(ChunkyImage image, VecI pos, SKColor color)
    {
        int chunkSize = ChunkResolution.Full.PixelSize();

        using FloodFillChunkStorage storage = new(image);

        VecI initChunkPos = OperationHelper.GetChunkPos(pos, chunkSize);
        VecI imageSizeInChunks = (VecI)(image.LatestSize / (double)chunkSize).Ceiling();
        VecI initPosOnChunk = pos - initChunkPos * chunkSize;
        SKColor colorToReplace = storage.GetChunk(initChunkPos).Surface.GetSRGBPixel(initPosOnChunk);

        FloodFillColorBounds bounds = new(colorToReplace);
        ulong uLongColor = ToULong(color);

        Stack<(VecI chunkPos, VecI posOnChunk)> positionsToFloodFill = new();
        positionsToFloodFill.Push((initChunkPos, initPosOnChunk));
        while (positionsToFloodFill.Count > 0)
        {
            var (chunkPos, posOnChunk) = positionsToFloodFill.Pop();
            Chunk chunk = storage.GetChunk(chunkPos);
            var maybeArray = FloodFillChunk(chunk, chunkSize, uLongColor, color, posOnChunk, bounds);
            if (maybeArray is null)
                continue;
            for (int i = 0; i < chunkSize; i++)
            {
                if (chunkPos.Y > 0 && maybeArray[i])
                    positionsToFloodFill.Push((new(chunkPos.X, chunkPos.Y - 1), new(i, chunkSize - 1)));
                if (chunkPos.Y < imageSizeInChunks.Y - 1 && maybeArray[chunkSize * (chunkSize - 1) + i])
                    positionsToFloodFill.Push((new(chunkPos.X, chunkPos.Y + 1), new(i, 0)));
                if (chunkPos.X > 0 && maybeArray[i * chunkSize])
                    positionsToFloodFill.Push((new(chunkPos.X - 1, chunkPos.Y), new(chunkSize - 1, i)));
                if (chunkPos.X < imageSizeInChunks.X - 1 && maybeArray[i * chunkSize + (chunkSize - 1)])
                    positionsToFloodFill.Push((new(chunkPos.X + 1, chunkPos.Y), new(0, i)));
            }
        }
        storage.DrawOnImage();
        var affected = image.FindAffectedChunks();
        var affectedChunkStorage = new CommittedChunkStorage(image, affected);
        image.CommitChanges();
        return (affected, affectedChunkStorage);
    }

    private unsafe static ulong ToULong(SKColor color)
    {
        ulong result = 0;
        Half* ptr = (Half*)&result;
        float normalizedAlpha = color.Alpha / 255.0f;
        ptr[0] = (Half)(color.Red / 255f * normalizedAlpha);
        ptr[1] = (Half)(color.Green / 255f * normalizedAlpha);
        ptr[2] = (Half)(color.Blue / 255f * normalizedAlpha);
        ptr[3] = (Half)(normalizedAlpha);
        return result;
    }

    private unsafe static bool IsWithinBounds(ref FloodFillColorBounds bounds, Half* pixel)
    {
        float r = (float)pixel[0];
        float g = (float)pixel[1];
        float b = (float)pixel[2];
        float a = (float)pixel[3];
        if (r < bounds.LowerR || r > bounds.UpperR)
            return false;
        if (g < bounds.LowerG || g > bounds.UpperG)
            return false;
        if (b < bounds.LowerB || b > bounds.UpperB)
            return false;
        if (a < bounds.LowerA || a > bounds.UpperA)
            return false;
        return true;
    }

    private unsafe static bool[]? FloodFillChunk(Chunk chunk, int chunkSize, ulong colorBits, SKColor color, VecI pos, FloodFillColorBounds bounds)
    {
        if (chunk.Surface.GetSRGBPixel(pos) == color)
            return null;

        bool[] visited = new bool[chunkSize * chunkSize];
        using var pixmap = chunk.Surface.SkiaSurface.PeekPixels();
        Half* array = (Half*)pixmap.GetPixels();

        Stack<VecI> toVisit = new();
        toVisit.Push(pos);

        while (toVisit.Count > 0)
        {
            VecI curPos = toVisit.Pop();
            int pixelOffset = curPos.X + curPos.Y * chunkSize;
            Half* pixel = array + pixelOffset * 4;
            *(ulong*)pixel = colorBits;
            visited[pixelOffset] = true;

            if (curPos.X > 0 && !visited[pixelOffset - 1] && IsWithinBounds(ref bounds, pixel - 4))
                toVisit.Push(new(curPos.X - 1, curPos.Y));
            if (curPos.X < chunkSize - 1 && !visited[pixelOffset + 1] && IsWithinBounds(ref bounds, pixel + 4))
                toVisit.Push(new(curPos.X + 1, curPos.Y));
            if (curPos.Y > 0 && !visited[pixelOffset - chunkSize] && IsWithinBounds(ref bounds, pixel - 4 * chunkSize))
                toVisit.Push(new(curPos.X, curPos.Y - 1));
            if (curPos.Y < chunkSize - 1 && !visited[pixelOffset + chunkSize] && IsWithinBounds(ref bounds, pixel + 4 * chunkSize))
                toVisit.Push(new(curPos.X, curPos.Y + 1));
        }
        return visited;
    }
}
