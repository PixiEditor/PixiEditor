using ChunkyImageLib.Operations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Enums;
using BlendMode = Drawie.Backend.Core.Surfaces.BlendMode;

namespace PixiEditor.ChangeableDocument.Changes.Drawing.FloodFill;

public static class FloodFillHelper
{
    private const byte InSelection = 1;
    private const byte Visited = 2;

    private static readonly VecI Up = new VecI(0, -1);
    private static readonly VecI Down = new VecI(0, 1);
    private static readonly VecI Left = new VecI(-1, 0);
    private static readonly VecI Right = new VecI(1, 0);

    internal static FloodFillChunkCache CreateCache(HashSet<Guid> membersToFloodFill, IReadOnlyDocument document,
        int frame, string renderOutput)
    {
        if (membersToFloodFill?.Count == 1)
        {
            Guid guid = membersToFloodFill.First();
            var member = document.FindMemberOrThrow(guid);
            if (member is IReadOnlyFolderNode)
                return new FloodFillChunkCache(membersToFloodFill, document, renderOutput, frame);

            if (member is IReadOnlyImageNode rasterLayer)
            {
                return new FloodFillChunkCache(rasterLayer.GetLayerImageAtFrame(frame));
            }
        }

        return new FloodFillChunkCache(membersToFloodFill, document, renderOutput, frame);
    }

    public static Dictionary<VecI, Chunk> FloodFill(
        HashSet<Guid> membersToFloodFill,
        IReadOnlyDocument document,
        VectorPath? selection,
        VecI startingPos,
        Color drawingColor,
        float tolerance,
        int frame, bool lockTransparency, FloodFillMode fillMode, bool contiguous, string renderOutput)
    {
        if (selection is not null && !selection.Contains(startingPos.X + 0.5f, startingPos.Y + 0.5f))
            return new();

        int chunkSize = ChunkResolution.Full.PixelSize();
        VecI targetSize = document.Size;
        if(!string.IsNullOrEmpty(renderOutput))
        {
            targetSize = document.GetRenderOutputSize(renderOutput);
        }

        using var ctx = DrawingBackendApi.Current.RenderingDispatcher.EnsureContext();

        FloodFillChunkCache cache = CreateCache(membersToFloodFill, document, frame, renderOutput);

        List<Chunk> usedChunks = new();
        VecI initChunkPos = OperationHelper.GetChunkPos(startingPos, chunkSize);
        VecI imageSizeInChunks = (VecI)(document.Size / (double)chunkSize).Ceiling();
        VecI initPosOnChunk = startingPos - initChunkPos * chunkSize;
        var chunkAtPos = cache.GetChunk(initChunkPos);
        ColorF colorToReplace = chunkAtPos.Match(
            (Chunk chunk) => chunk.Surface.GetSrgbPixel(initPosOnChunk),
            static (EmptyChunk _) => Colors.Transparent
        );

        if(chunkAtPos.IsT0)
            usedChunks.Add(chunkAtPos.AsT0);

        var fixedColor = drawingColor;

        var uLongColor = fixedColor.ToULong();
        var colorSpaceCorrectedColor = fixedColor;

        if ((colorSpaceCorrectedColor.A == 0 && fillMode == FloodFillMode.Overlay) ||
            (colorToReplace == colorSpaceCorrectedColor && fillMode == FloodFillMode.Replace))
        {
            cache.Dispose();
            return new();
        }

        if (colorToReplace.A == 0 && lockTransparency)
        {
            cache.Dispose();
            return new();
        }

        RectI globalSelectionBounds = (RectI?)selection?.TightBounds ?? new RectI(VecI.Zero, targetSize);

        // Pre-multiplies the color and convert it to floats. Since floats are imprecise, a range is used.
        // Used for faster pixel checking
        ColorBounds colorRange = new(colorToReplace, tolerance);

        Dictionary<VecI, Chunk> drawingChunks = new();
        HashSet<VecI> processedEmptyChunks = new();
        // flood fill chunks using a basic 4-way approach with a stack (each chunk is kinda like a pixel)
        // once the chunk is filled all places where it spills over to neighboring chunks are saved in the stack
        Stack<(VecI chunkPos, VecI posOnChunk)> positionsToFloodFill = new();
        positionsToFloodFill.Push((initChunkPos, initPosOnChunk));
        if (!contiguous)
        {
            // add all chunks within selection bounds

            for (int y = 0; y < imageSizeInChunks.Y; y++)
            {
                for (int x = 0; x < imageSizeInChunks.X; x++)
                {
                    if (x == initChunkPos.X && y == initChunkPos.Y)
                        continue;

                    bool intersectsWithSelection = SelectionIntersectsChunk(selection, new VecI(x, y), chunkSize);
                    if (!intersectsWithSelection)
                        continue;

                    positionsToFloodFill.Push((new VecI(x, y), VecI.Zero));
                }
            }
        }

        int iter = -1;
        while (positionsToFloodFill.Count > 0)
        {
            iter++;
            var (chunkPos, posOnChunk) = positionsToFloodFill.Pop();

            if (!drawingChunks.ContainsKey(chunkPos))
            {
                var chunk = Chunk.Create(ColorSpace.CreateSrgb());
                usedChunks.Add(chunk);

                if (fillMode == FloodFillMode.Replace)
                {
                    // For replace mode, copy original image data to avoid erasing unfilled pixels
                    var originalChunk = cache.GetChunk(chunkPos);
                    originalChunk.Switch(
                        (Chunk origChunk) =>
                            chunk.Surface.DrawingSurface.Canvas.DrawSurface(origChunk.Surface.DrawingSurface, 0, 0),
                        (EmptyChunk _) => chunk.Surface.DrawingSurface.Canvas.Clear(Colors.Transparent)
                    );
                }
                else
                {
                    // For overlay mode, start with transparent
                    chunk.Surface.DrawingSurface.Canvas.Clear(Colors.Transparent);
                }

                drawingChunks[chunkPos] = chunk;
            }

            var drawingChunk = drawingChunks[chunkPos];
            var referenceChunk = cache.GetChunk(chunkPos);

            // don't call floodfill if the chunk is empty
            if (referenceChunk.IsT1)
            {
                if (colorToReplace.A == 0 && !processedEmptyChunks.Contains(chunkPos))
                {
                    int saved = drawingChunk.Surface.DrawingSurface.Canvas.Save();
                    if (selection is not null && !selection.IsEmpty)
                    {
                        using VectorPath localSelection = new VectorPath(selection);
                        localSelection.Transform(Matrix3X3.CreateTranslation(-chunkPos.X * chunkSize,
                            -chunkPos.Y * chunkSize));

                        drawingChunk.Surface.DrawingSurface.Canvas.ClipPath(localSelection);
                        if (SelectionIntersectsChunk(selection, chunkPos, chunkSize))
                        {
                            drawingChunk.Surface.DrawingSurface.Canvas.Clear(drawingColor);
                        }
                    }
                    else
                    {
                        drawingChunk.Surface.DrawingSurface.Canvas.Clear(drawingColor);
                    }

                    drawingChunk.Surface.DrawingSurface.Canvas.RestoreToCount(saved);
                    if (contiguous)
                    {
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
                    }

                    processedEmptyChunks.Add(chunkPos);
                }

                continue;
            }

            // use regular flood fill for chunks that have something in them
            var reallyReferenceChunk = referenceChunk.AsT0;
            usedChunks.Add(reallyReferenceChunk);
            var maybeArray = FloodFillChunk(
                reallyReferenceChunk,
                drawingChunk,
                selection,
                globalSelectionBounds,
                chunkPos,
                chunkSize,
                uLongColor,
                colorSpaceCorrectedColor,
                posOnChunk,
                colorRange,
                iter != 0, contiguous);

            if (maybeArray is null)
                continue;
            if (contiguous)
            {
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
        }

        foreach (var usedChunk in usedChunks)
        {
            if(!drawingChunks.ContainsValue(usedChunk))
                usedChunk.Dispose();
        }

        foreach (var cacheAcquiredChunk in cache.AcquiredChunks)
        {
            if (!drawingChunks.ContainsKey(cacheAcquiredChunk.Key))
                cacheAcquiredChunk.Value.AsT0?.Dispose();
        }

        return drawingChunks;
    }

    private static unsafe byte[]? FloodFillChunk(
        Chunk referenceChunk,
        Chunk drawingChunk,
        VectorPath? selection,
        RectI globalSelectionBounds,
        VecI chunkPos,
        int chunkSize,
        ulong colorBits,
        ColorF color,
        VecI pos,
        ColorBounds bounds,
        bool checkFirstPixel,
        bool contiguous)
    {
        if (contiguous)
        {
            var rawPixelRef = referenceChunk.Surface.GetSrgbPixel(pos);
            // color should be a fixed color
            if (rawPixelRef == (Color)color ||
                drawingChunk.Surface.GetSrgbPixel(pos) == color)
                return null;
            if (checkFirstPixel && !bounds.IsWithinBounds(rawPixelRef))
                return null;
        }

        if (!SelectionIntersectsChunk(selection, chunkPos, chunkSize))
            return null;

        byte[] pixelStates = new byte[chunkSize * chunkSize];
        DrawSelection(pixelStates, selection, globalSelectionBounds, chunkPos, chunkSize);

        using var refPixmap = referenceChunk.Surface.PeekPixels();
        Half* refArray = (Half*)refPixmap.GetPixels();

        using Surface cpuSurface = Surface.ForProcessing(new VecI(chunkSize), referenceChunk.Surface.ColorSpace);
        cpuSurface.DrawingSurface.Canvas.DrawSurface(drawingChunk.Surface.DrawingSurface, 0, 0);
        using var drawPixmap = cpuSurface.PeekPixels();
        Half* drawArray = (Half*)drawPixmap.GetPixels();

        if (contiguous)
        {
            FloodFillContiguous(chunkSize, colorBits, pos, bounds, drawArray, refArray, pixelStates);
        }
        else
        {
            FloodFillNonContiguous(chunkSize, colorBits, bounds, drawArray, refArray, pixelStates);
        }

        using Paint replacePaint = new Paint();
        replacePaint.BlendMode = BlendMode.Src;
        drawingChunk.Surface.DrawingSurface.Canvas.DrawSurface(cpuSurface.DrawingSurface, 0, 0, replacePaint);
        drawingChunk.Surface.DrawingSurface.Canvas.Flush();

        return pixelStates;
    }

    private static unsafe void FloodFillContiguous(int chunkSize, ulong colorBits, VecI pos, ColorBounds bounds,
        Half* drawArray, Half* refArray, byte[] pixelStates)
    {
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

            if (curPos.X > 0 && pixelStates[pixelOffset - 1] == InSelection && bounds.IsWithinBounds(refPixel - 4))
                toVisit.Push(new(curPos.X - 1, curPos.Y));
            if (curPos.X < chunkSize - 1 && pixelStates[pixelOffset + 1] == InSelection &&
                bounds.IsWithinBounds(refPixel + 4))
                toVisit.Push(new(curPos.X + 1, curPos.Y));
            if (curPos.Y > 0 && pixelStates[pixelOffset - chunkSize] == InSelection &&
                bounds.IsWithinBounds(refPixel - 4 * chunkSize))
                toVisit.Push(new(curPos.X, curPos.Y - 1));
            if (curPos.Y < chunkSize - 1 && pixelStates[pixelOffset + chunkSize] == InSelection &&
                bounds.IsWithinBounds(refPixel + 4 * chunkSize))
                toVisit.Push(new(curPos.X, curPos.Y + 1));
        }
    }

    private static unsafe void FloodFillNonContiguous(int chunkSize, ulong colorBits, ColorBounds bounds,
        Half* drawArray, Half* refArray, byte[] pixelStates)
    {
        for (int y = 0; y < chunkSize; y++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                int pixelOffset = x + y * chunkSize;
                if (pixelStates[pixelOffset] == InSelection && bounds.IsWithinBounds(refArray + pixelOffset * 4))
                {
                    *(ulong*)(drawArray + pixelOffset * 4) = colorBits;
                    pixelStates[pixelOffset] = Visited;
                }
            }
        }
    }

    public static Surface FillSelection(IReadOnlyDocument document, VectorPath selection)
    {
        Surface surface = Surface.ForProcessing(document.Size, document.ProcessingColorSpace);

        var inverse = new VectorPath();
        inverse.AddRect((RectD)new RectI(new(0, 0), document.Size));

        surface.DrawingSurface.Canvas.Clear(new Color(255, 255, 255, 255));
        surface.DrawingSurface.Canvas.Flush();
        surface.DrawingSurface.Canvas.ClipPath(inverse.Op(selection, VectorPathOp.Difference));
        surface.DrawingSurface.Canvas.Clear(new Color(0, 0, 0, 0));
        surface.DrawingSurface.Canvas.Flush();

        return surface;
    }

    /// <summary>
    /// Use skia to set all pixels in array that are inside selection to InSelection
    /// </summary>
    private static unsafe void DrawSelection(byte[] array, VectorPath? selection, RectI globalBounds, VecI chunkPos,
        int chunkSize)
    {
        if (selection is null)
        {
            selection = new VectorPath();
            selection.AddRect((RectD)globalBounds);
        }

        RectI localBounds = globalBounds.Offset(-chunkPos * chunkSize).Intersect(new(0, 0, chunkSize, chunkSize));
        if (localBounds.IsZeroOrNegativeArea)
            return;
        using VectorPath shiftedSelection = new VectorPath(selection);
        shiftedSelection.Transform(Matrix3X3.CreateTranslation(-chunkPos.X * chunkSize, -chunkPos.Y * chunkSize));

        fixed (byte* arr = array)
        {
            using DrawingSurface drawingSurface = DrawingSurface.Create(
                new ImageInfo(localBounds.Right, localBounds.Bottom, ColorType.Gray8, AlphaType.Opaque), (IntPtr)arr,
                chunkSize);
            drawingSurface.Canvas.ClipPath(shiftedSelection);
            drawingSurface.Canvas.Clear(new Color(InSelection, InSelection, InSelection));
            drawingSurface.Canvas.Flush();
        }
    }

    private static bool SelectionIntersectsChunk(VectorPath selection, VecI chunkPos, int chunkSize)
    {
        if (selection is null || selection.IsEmpty)
            return true;

        RectD chunkBounds = new(chunkPos * chunkSize, new VecI(chunkSize));
        return selection.Bounds.IntersectsWithInclusive(chunkBounds);
    }
}
