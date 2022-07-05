using System.Runtime.CompilerServices;
using ChunkyImageLib.Operations;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using SkiaSharp;

namespace PixiEditor.ChangeableDocument.Changes.Drawing.FloodFill;

internal static class FloodFillHelper
{
    private const byte InSelection = 1;
    private const byte Visited = 2;

    private static FloodFillChunkCache CreateCache(HashSet<Guid> membersToFloodFill, IReadOnlyDocument document)
    {
        if (membersToFloodFill.Count == 1)
        {
            Guid guid = membersToFloodFill.First();
            var member = document.FindMemberOrThrow(guid);
            if (member is IReadOnlyFolder folder)
                return new FloodFillChunkCache(membersToFloodFill, document.StructureRoot);
            return new FloodFillChunkCache(((IReadOnlyLayer)member).LayerImage);
        }
        return new FloodFillChunkCache(membersToFloodFill, document.StructureRoot);
    }

    public static Dictionary<VecI, Chunk> FloodFill(
        HashSet<Guid> membersToFloodFill,
        IReadOnlyDocument document,
        SKPath? selection,
        VecI startingPos,
        SKColor drawingColor)
    {
        int chunkSize = ChunkResolution.Full.PixelSize();

        FloodFillChunkCache cache = CreateCache(membersToFloodFill, document);

        VecI initChunkPos = OperationHelper.GetChunkPos(startingPos, chunkSize);
        VecI imageSizeInChunks = (VecI)(document.Size / (double)chunkSize).Ceiling();
        VecI initPosOnChunk = startingPos - initChunkPos * chunkSize;
        SKColor colorToReplace = cache.GetChunk(initChunkPos).Match(
            (Chunk chunk) => chunk.Surface.GetSRGBPixel(initPosOnChunk),
            static (EmptyChunk _) => SKColors.Transparent
        );

        if (colorToReplace.Alpha == 0 && drawingColor.Alpha == 0 || colorToReplace == drawingColor)
            return new();

        RectI globalSelectionBounds = (RectI?)selection?.TightBounds ?? new RectI(VecI.Zero, document.Size);

        // Pre-multiplies the color and convert it to floats. Since floats are imprecise, a range is used.
        // Used for faster pixel checking
        FloodFillColorRange colorRange = new(colorToReplace);
        ulong uLongColor = ToULong(drawingColor);

        Dictionary<VecI, Chunk> drawingChunks = new();
        HashSet<VecI> processedEmptyChunks = new();
        // flood fill chunks using a basic 4-way approach with a stack (each chunk is kinda like a pixel)
        // once the chunk is filled all places where it spills over to neighboring chunks are saved in the stack
        Stack<(VecI chunkPos, VecI posOnChunk)> positionsToFloodFill = new();
        positionsToFloodFill.Push((initChunkPos, initPosOnChunk));
        while (positionsToFloodFill.Count > 0)
        {
            var (chunkPos, posOnChunk) = positionsToFloodFill.Pop();

            if (!drawingChunks.ContainsKey(chunkPos))
            {
                var chunk = Chunk.Create();
                chunk.Surface.SkiaSurface.Canvas.Clear(SKColors.Transparent);
                drawingChunks[chunkPos] = chunk;
            }
            var drawingChunk = drawingChunks[chunkPos];
            var referenceChunk = cache.GetChunk(chunkPos);

            // don't call floodfill if the chunk is empty
            if (referenceChunk.IsT1)
            {
                if (colorToReplace.Alpha == 0 && !processedEmptyChunks.Contains(chunkPos))
                {
                    drawingChunk.Surface.SkiaSurface.Canvas.Clear(drawingColor);
                    for (int i = 0; i < chunkSize; i++)
                    {
                        if (chunkPos.Y > 0)
                            positionsToFloodFill.Push((new(chunkPos.X, chunkPos.Y - 1), new(i, chunkSize - 1)));
                        if (chunkPos.Y < imageSizeInChunks.Y - 1)
                            positionsToFloodFill.Push((new(chunkPos.X, chunkPos.Y + 1), new(i, 0)));
                        if (chunkPos.X > 0)
                            positionsToFloodFill.Push((new(chunkPos.X - 1, chunkPos.Y), new(chunkSize - 1, i)));
                        if (chunkPos.X < imageSizeInChunks.X - 1)
                            positionsToFloodFill.Push((new(chunkPos.X + 1, chunkPos.Y), new(0, i)));
                    }
                    processedEmptyChunks.Add(chunkPos);
                }
                continue;
            }

            // use regular flood fill for chunks that have something in them
            var reallyReferenceChunk = referenceChunk.AsT0;
            var maybeArray = FloodFillChunk(
                reallyReferenceChunk,
                drawingChunk,
                selection,
                globalSelectionBounds,
                chunkPos,
                chunkSize,
                uLongColor,
                drawingColor,
                posOnChunk,
                colorRange);

            if (maybeArray is null)
                continue;
            for (int i = 0; i < chunkSize; i++)
            {
                if (chunkPos.Y > 0 && maybeArray[i] == Visited)
                    positionsToFloodFill.Push((new(chunkPos.X, chunkPos.Y - 1), new(i, chunkSize - 1)));
                if (chunkPos.Y < imageSizeInChunks.Y - 1 && maybeArray[chunkSize * (chunkSize - 1) + i] == Visited)
                    positionsToFloodFill.Push((new(chunkPos.X, chunkPos.Y + 1), new(i, 0)));
                if (chunkPos.X > 0 && maybeArray[i * chunkSize] == Visited)
                    positionsToFloodFill.Push((new(chunkPos.X - 1, chunkPos.Y), new(chunkSize - 1, i)));
                if (chunkPos.X < imageSizeInChunks.X - 1 && maybeArray[i * chunkSize + (chunkSize - 1)] == Visited)
                    positionsToFloodFill.Push((new(chunkPos.X + 1, chunkPos.Y), new(0, i)));
            }
        }
        return drawingChunks;
    }

    private static unsafe ulong ToULong(SKColor color)
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

    private static unsafe bool IsWithinBounds(ref FloodFillColorRange bounds, Half* pixel)
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

    private static unsafe byte[]? FloodFillChunk(
        Chunk referenceChunk,
        Chunk drawingChunk,
        SKPath? selection,
        RectI globalSelectionBounds,
        VecI chunkPos,
        int chunkSize,
        ulong colorBits,
        SKColor color,
        VecI pos,
        FloodFillColorRange bounds)
    {
        if (referenceChunk.Surface.GetSRGBPixel(pos) == color || drawingChunk.Surface.GetSRGBPixel(pos) == color)
            return null;

        byte[] pixelStates = new byte[chunkSize * chunkSize];
        DrawSelection(pixelStates, selection, globalSelectionBounds, chunkPos, chunkSize);

        using var refPixmap = referenceChunk.Surface.SkiaSurface.PeekPixels();
        Half* refArray = (Half*)refPixmap.GetPixels();

        using var drawPixmap = drawingChunk.Surface.SkiaSurface.PeekPixels();
        Half* drawArray = (Half*)drawPixmap.GetPixels();

        Stack<VecI> toVisit = new();
        toVisit.Push(pos);

        while (toVisit.Count > 0)
        {
            VecI curPos = toVisit.Pop();
            int pixelOffset = curPos.X + curPos.Y * chunkSize;
            Half* drawPixel = drawArray + pixelOffset * 4;
            Half* refPixel = refArray + pixelOffset * 4;
            *(ulong*)drawPixel = colorBits;
            pixelStates[pixelOffset] = Visited;

            if (curPos.X > 0 && pixelStates[pixelOffset - 1] == InSelection && IsWithinBounds(ref bounds, refPixel - 4))
                toVisit.Push(new(curPos.X - 1, curPos.Y));
            if (curPos.X < chunkSize - 1 && pixelStates[pixelOffset + 1] == InSelection && IsWithinBounds(ref bounds, refPixel + 4))
                toVisit.Push(new(curPos.X + 1, curPos.Y));
            if (curPos.Y > 0 && pixelStates[pixelOffset - chunkSize] == InSelection && IsWithinBounds(ref bounds, refPixel - 4 * chunkSize))
                toVisit.Push(new(curPos.X, curPos.Y - 1));
            if (curPos.Y < chunkSize - 1 && pixelStates[pixelOffset + chunkSize] == InSelection && IsWithinBounds(ref bounds, refPixel + 4 * chunkSize))
                toVisit.Push(new(curPos.X, curPos.Y + 1));
        }
        return pixelStates;
    }

    /// <summary>
    /// Use skia to set all pixels in array that are inside selection to InSelection
    /// </summary>
    private static unsafe void DrawSelection(byte[] array, SKPath? selection, RectI globalBounds, VecI chunkPos, int chunkSize)
    {
        if (selection is null)
        {
            fixed (byte* arr = array)
            {
                Unsafe.InitBlockUnaligned(arr, InSelection, (uint)(chunkSize * chunkSize));
            }
            return;
        }

        RectI localBounds = globalBounds.Offset(-chunkPos * chunkSize).Intersect(new(0, 0, chunkSize, chunkSize));
        if (localBounds.IsZeroOrNegativeArea)
            return;
        SKPath shiftedSelection = new SKPath(selection);
        shiftedSelection.Transform(SKMatrix.CreateTranslation(-chunkPos.X * chunkSize, -chunkPos.Y * chunkSize));

        fixed (byte* arr = array)
        {
            using SKSurface drawingSurface = SKSurface.Create(
                new SKImageInfo(localBounds.Right, localBounds.Bottom, SKColorType.Gray8, SKAlphaType.Opaque), (IntPtr)arr, chunkSize);
            drawingSurface.Canvas.ClipPath(shiftedSelection);
            drawingSurface.Canvas.Clear(new SKColor(InSelection, InSelection, InSelection));
            drawingSurface.Canvas.Flush();
        }
    }
}
