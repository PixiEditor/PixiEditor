using System.Collections;
using System.Numerics;
using System.Runtime.CompilerServices;
using ChunkyImageLib.Operations;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.ImageData;
using PixiEditor.DrawingApi.Core.Surface.Vector;

namespace PixiEditor.ChangeableDocument.Changes.Drawing.FloodFill;

internal static class FloodFillHelper
{
    private const byte InSelection = 1;
    private const byte Visited = 2;
    
    private static readonly VecI Up = new VecI(0, -1);
    private static readonly VecI Down = new VecI(0, 1);
    private static readonly VecI Left = new VecI(-1, 0);
    private static readonly VecI Right = new VecI(1, 0);

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
        VectorPath? selection,
        VecI startingPos,
        Color drawingColor)
    {
        if (selection is not null && !selection.Contains(startingPos.X + 0.5f, startingPos.Y + 0.5f))
            return new();

        int chunkSize = ChunkResolution.Full.PixelSize();

        FloodFillChunkCache cache = CreateCache(membersToFloodFill, document);

        VecI initChunkPos = OperationHelper.GetChunkPos(startingPos, chunkSize);
        VecI imageSizeInChunks = (VecI)(document.Size / (double)chunkSize).Ceiling();
        VecI initPosOnChunk = startingPos - initChunkPos * chunkSize;
        Color colorToReplace = cache.GetChunk(initChunkPos).Match(
            (Chunk chunk) => chunk.Surface.GetSRGBPixel(initPosOnChunk),
            static (EmptyChunk _) => Colors.Transparent
        );

        if ((colorToReplace.A == 0 && drawingColor.A == 0) || colorToReplace == drawingColor)
            return new();

        RectI globalSelectionBounds = (RectI?)selection?.TightBounds ?? new RectI(VecI.Zero, document.Size);

        // Pre-multiplies the color and convert it to floats. Since floats are imprecise, a range is used.
        // Used for faster pixel checking
        ColorBounds colorRange = new(colorToReplace);
        ulong uLongColor = drawingColor.ToULong();

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
                chunk.Surface.DrawingSurface.Canvas.Clear(Colors.Transparent);
                drawingChunks[chunkPos] = chunk;
            }
            var drawingChunk = drawingChunks[chunkPos];
            var referenceChunk = cache.GetChunk(chunkPos);

            // don't call floodfill if the chunk is empty
            if (referenceChunk.IsT1)
            {
                if (colorToReplace.A == 0 && !processedEmptyChunks.Contains(chunkPos))
                {
                    drawingChunk.Surface.DrawingSurface.Canvas.Clear(drawingColor);
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

    private static unsafe byte[]? FloodFillChunk(
        Chunk referenceChunk,
        Chunk drawingChunk,
        VectorPath? selection,
        RectI globalSelectionBounds,
        VecI chunkPos,
        int chunkSize,
        ulong colorBits,
        Color color,
        VecI pos,
        ColorBounds bounds)
    {
        if (referenceChunk.Surface.GetSRGBPixel(pos) == color || drawingChunk.Surface.GetSRGBPixel(pos) == color)
            return null;

        byte[] pixelStates = new byte[chunkSize * chunkSize];
        DrawSelection(pixelStates, selection, globalSelectionBounds, chunkPos, chunkSize);

        using var refPixmap = referenceChunk.Surface.DrawingSurface.PeekPixels();
        Half* refArray = (Half*)refPixmap.GetPixels();

        using var drawPixmap = drawingChunk.Surface.DrawingSurface.PeekPixels();
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

            if (curPos.X > 0 && pixelStates[pixelOffset - 1] == InSelection && bounds.IsWithinBounds(refPixel - 4))
                toVisit.Push(new(curPos.X - 1, curPos.Y));
            if (curPos.X < chunkSize - 1 && pixelStates[pixelOffset + 1] == InSelection && bounds.IsWithinBounds(refPixel + 4))
                toVisit.Push(new(curPos.X + 1, curPos.Y));
            if (curPos.Y > 0 && pixelStates[pixelOffset - chunkSize] == InSelection && bounds.IsWithinBounds(refPixel - 4 * chunkSize))
                toVisit.Push(new(curPos.X, curPos.Y - 1));
            if (curPos.Y < chunkSize - 1 && pixelStates[pixelOffset + chunkSize] == InSelection && bounds.IsWithinBounds(refPixel + 4 * chunkSize))
                toVisit.Push(new(curPos.X, curPos.Y + 1));
        }
        return pixelStates;
    }

    /// <summary>
    /// Use skia to set all pixels in array that are inside selection to InSelection
    /// </summary>
    private static unsafe void DrawSelection(byte[] array, VectorPath? selection, RectI globalBounds, VecI chunkPos, int chunkSize)
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
        VectorPath shiftedSelection = new VectorPath(selection);
        shiftedSelection.Transform(Matrix3X3.CreateTranslation(-chunkPos.X * chunkSize, -chunkPos.Y * chunkSize));

        fixed (byte* arr = array)
        {
            using DrawingSurface drawingSurface = DrawingSurface.Create(
                new ImageInfo(localBounds.Right, localBounds.Bottom, ColorType.Gray8, AlphaType.Opaque), (IntPtr)arr, chunkSize);
            drawingSurface.Canvas.ClipPath(shiftedSelection);
            drawingSurface.Canvas.Clear(new Color(InSelection, InSelection, InSelection));
            drawingSurface.Canvas.Flush();
        }
    }

    public static VectorPath GetFloodFillSelection(VecI startingPos, HashSet<Guid> membersToFloodFill,
        IReadOnlyDocument document)
    {
        int chunkSize = ChunkResolution.Full.PixelSize();

        FloodFillChunkCache cache = CreateCache(membersToFloodFill, document);

        VecI initChunkPos = OperationHelper.GetChunkPos(startingPos, chunkSize);
        VecI imageSizeInChunks = (VecI)(document.Size / (double)chunkSize).Ceiling();
        VecI initPosOnChunk = startingPos - initChunkPos * chunkSize;
        
        Color colorToReplace = cache.GetChunk(initChunkPos).Match(
            (Chunk chunk) => chunk.Surface.GetSRGBPixel(initPosOnChunk),
            static (EmptyChunk _) => Colors.Transparent
        );
        
        ColorBounds colorRange = new(colorToReplace);
        
        HashSet<VecI> processedEmptyChunks = new();
        HashSet<VecI> processedChunks = new();
        Stack<(VecI chunkPos, VecI posOnChunk)> positionsToFloodFill = new();
        positionsToFloodFill.Push((initChunkPos, initPosOnChunk));
        
        Lines lines = new();
        
        VectorPath selection = new();
        while (positionsToFloodFill.Count > 0)
        {
            var (chunkPos, posOnChunk) = positionsToFloodFill.Pop();
            var referenceChunk = cache.GetChunk(chunkPos);
            
            VecI realSize = new VecI(Math.Min(chunkSize, document.Size.X), Math.Min(chunkSize, document.Size.Y));

            // don't call floodfill if the chunk is empty
            if (referenceChunk.IsT1)
            {
                if (colorToReplace.A == 0 && !processedEmptyChunks.Contains(chunkPos))
                {
                    ProcessEmptySelectionChunk(lines, chunkPos, realSize, imageSizeInChunks);
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
            if(processedChunks.Contains(chunkPos))
                continue;

            var maybeArray = GetChunkFloodFill(
                reallyReferenceChunk,
                chunkSize,
                chunkPos * chunkSize,
                realSize,
                document.Size,
                posOnChunk,
                colorRange, lines);

            processedChunks.Add(chunkPos);
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

        if (lines.LineDict.Any(x => x.Value.Count > 0))
        {
            selection = BuildContour(lines);
        }

        return selection;
    }

    private static void ProcessEmptySelectionChunk(Lines lines, VecI chunkPos, VecI realSize,
        VecI imageSizeInChunks)
    {
        bool isEdgeChunk = chunkPos.X == 0 || chunkPos.Y == 0 || chunkPos.X == imageSizeInChunks.X - 1 ||
                           chunkPos.Y == imageSizeInChunks.Y - 1;
        if (isEdgeChunk)
        {
            bool isTopEdge = chunkPos.Y == 0;
            bool isBottomEdge = chunkPos.Y == imageSizeInChunks.Y - 1;
            bool isLeftEdge = chunkPos.X == 0;
            bool isRightEdge = chunkPos.X == imageSizeInChunks.X - 1;
            
            int posX = chunkPos.X * realSize.X;
            int posY = chunkPos.Y * realSize.Y;
            
            int endX = posX + realSize.X;
            int endY = posY + realSize.Y;

            if (isTopEdge)
            {
                AddLine(new(new(posX, posY), new(endX, posY)), lines, Right);
            }

            if (isBottomEdge)
            {
                AddLine(new(new(posX, endY), new(endX, endY)), lines, Left);
            }

            if (isLeftEdge)
            {
                AddLine(new(new(posX, posY), new(posX, endY)), lines, Up);
            }
            
            if (isRightEdge)
            {
                AddLine(new(new(endX, posY), new(endX, endY)), lines, Down);
            }
        }
    }

    public static VectorPath BuildContour(Lines lines)
    {
        VectorPath selection = new();

        VecI startingPoint = lines.LineDict[Up].First().Value.Start;
        VecI prevPos = startingPoint;
        selection.MoveTo(prevPos);
        foreach (var line in lines)
        {
            Line nextLine;
            if (!lines.TryGetLine(prevPos, out nextLine))
            {
                nextLine = line;
                selection.MoveTo(nextLine.Start);
                selection.LineTo(nextLine.End);
                prevPos = nextLine.End;
                lines.RemoveLine(line.Start);
                continue;
            }
            
            selection.LineTo(nextLine.End);
            lines.RemoveLine(prevPos);
            prevPos = nextLine.End;
        }
        
        /*selection.MoveTo(startLine.Start);
        VecI lastPos = startLine.End;
        VecI lastDir = startLine.End - startLine.Start;
        lines.RemoveAt(0);
        for (var i = 0; i < lines.Count; i++)
        {
            Line nextLine = FindNextLine(lines, lastPos);

            // Inner contour was found
            if (nextLine == default) 
            {
                nextLine = lines[i];

                selection.MoveTo(nextLine.Start);
                selection.LineTo(nextLine.End);
                lastPos = nextLine.End;
                lines.RemoveAt(i);
                i--;
                continue;
            }
            
            VecI nextPoint = nextLine.Start == lastPos ? nextLine.End : nextLine.Start;
            VecI nextDir = nextPoint - lastPos;

            // Draw only if direction changed, sometimes one more line at one of starting edges is added for some reason. I didn't fix it since it doesn't affect the result really
            // But it's still shouldn't happen, so feel free to fix it
            if (nextDir.Normalized() != lastDir.Normalized()) 
            {
                selection.LineTo(lastPos);
            }
            
            lastPos = nextPoint;
            lastDir = nextDir;
            lines.Remove(nextLine);
            i--;
        }*/

        return selection;
    }

    private static unsafe byte[]? GetChunkFloodFill(
        Chunk referenceChunk,
        int chunkSize,
        VecI chunkOffset,
        VecI maxSize,
        VecI documentSize,
        VecI pos,
        ColorBounds bounds, Lines lines)
    {
        byte[] pixelStates = new byte[chunkSize * chunkSize];

        using var refPixmap = referenceChunk.Surface.DrawingSurface.PeekPixels();
        Half* refArray = (Half*)refPixmap.GetPixels();
        
        Stack<VecI> toVisit = new();
        toVisit.Push(pos);

        while (toVisit.Count > 0)
        {
            VecI curPos = toVisit.Pop();
            VecI clampedPos = new VecI(
                Math.Clamp(curPos.X, 0, maxSize.X - 1),
                Math.Clamp(curPos.Y, 0, maxSize.Y - 1));
            
            int pixelOffset = curPos.X + curPos.Y * chunkSize;
            Half* refPixel = refArray + pixelOffset * 4;
            pixelStates[pixelOffset] = Visited;

            AddCornerLines(documentSize, chunkOffset, lines, curPos, clampedPos);
            AddFillContourLines(chunkSize, chunkOffset, bounds, lines, curPos, pixelStates, pixelOffset, refPixel, toVisit, clampedPos);
        }
        
        return pixelStates;
    }

    private static unsafe void AddFillContourLines(int chunkSize, VecI chunkOffset, ColorBounds bounds, Lines lines,
        VecI curPos, byte[] pixelStates, int pixelOffset, Half* refPixel, Stack<VecI> toVisit, VecI clampedPos)
    {
        // Left pixel
        if (curPos.X > 0 && pixelStates[pixelOffset - 1] != Visited)
        {
            if (bounds.IsWithinBounds(refPixel - 4))
            {
                toVisit.Push(new(curPos.X - 1, curPos.Y));
            }
            else
            {
                AddLine(
                    new Line(
                        new VecI(clampedPos.X, clampedPos.Y + 1) + chunkOffset,
                        new VecI(clampedPos.X, clampedPos.Y) + chunkOffset), lines, Up);
            }
        }

        // Right pixel
        if (curPos.X < chunkSize - 1 && pixelStates[pixelOffset + 1] != Visited)
        {
            if (bounds.IsWithinBounds(refPixel + 4))
            {
                toVisit.Push(new(curPos.X + 1, curPos.Y));
            }
            else
            {
                AddLine(
                    new Line(
                        new VecI(clampedPos.X + 1, clampedPos.Y) + chunkOffset,
                        new VecI(clampedPos.X + 1, clampedPos.Y + 1) + chunkOffset), lines, Down);
            }
        }

        // Top pixel
        if (curPos.Y > 0 && pixelStates[pixelOffset - chunkSize] != Visited)
        {
            if (bounds.IsWithinBounds(refPixel - 4 * chunkSize))
            {
                toVisit.Push(new(curPos.X, curPos.Y - 1));
            }
            else
            {
                AddLine(
                    new Line(
                        new VecI(clampedPos.X + 1, clampedPos.Y) + chunkOffset,
                        new VecI(clampedPos.X, clampedPos.Y) + chunkOffset), lines, Right);
            }
        }

        //Bottom pixel
        if (curPos.Y < chunkSize - 1 && pixelStates[pixelOffset + chunkSize] != Visited)
        {
            if (bounds.IsWithinBounds(refPixel + 4 * chunkSize))
            {
                toVisit.Push(new(curPos.X, curPos.Y + 1));
            }
            else
            {
                AddLine(
                    new Line(
                        new VecI(clampedPos.X + 1, clampedPos.Y + 1) + chunkOffset,
                        new VecI(clampedPos.X, clampedPos.Y + 1) + chunkOffset), lines, Left);
            }
        }
    }

    private static void AddCornerLines(VecI documentSize, VecI chunkOffset, Lines lines, VecI curPos, VecI clampedPos)
    {
        VecI globalPos = curPos + chunkOffset;
        if (globalPos.X == 0)
        {
            AddLine(
                new Line(
                    new VecI(clampedPos.X, clampedPos.Y + 1) + chunkOffset,
                    new VecI(clampedPos.X, clampedPos.Y) + chunkOffset), lines, Up);
        }

        if (globalPos.X == documentSize.X - 1)
        {
            AddLine(
                new Line(
                    new VecI(clampedPos.X + 1, clampedPos.Y) + chunkOffset,
                    new VecI(clampedPos.X + 1, clampedPos.Y + 1) + chunkOffset), lines, Down);
        }

        if (globalPos.Y == 0)
        {
            AddLine(
                new Line(
                    new VecI(clampedPos.X, clampedPos.Y) + chunkOffset,
                    new VecI(clampedPos.X + 1, clampedPos.Y) + chunkOffset), lines, Right);
        }

        if (globalPos.Y == documentSize.Y - 1)
        {
            AddLine(
                new Line(
                    new VecI(clampedPos.X + 1, clampedPos.Y + 1) + chunkOffset,
                    new VecI(clampedPos.X, clampedPos.Y + 1) + chunkOffset), lines, Left);
        }
    }

    private static void AddLine(Line line, Lines lines, VecI direction)
    {
        VecI calculatedDir = (VecI)(line.End - line.Start).Normalized();
        if (calculatedDir == direction)
        {
            lines.LineDict[direction][line.Start] = line;
        }
        else if(calculatedDir == -direction)
        {
            lines.LineDict[direction][line.End] = new Line(line.End, line.Start);
        }
        else
        {
            throw new Exception(
                $"Line direction {calculatedDir} is not perpendicular to the direction of the requested line direction {direction}");
        }
    }

    public struct Line
    {
        public VecI Start { get; set; }
        public VecI End { get; set; }

        public Line(VecI start, VecI end)
        {
            Start = start;
            End = end;
        }
        
        public Line Extended(VecI point)
        {
            VecI start = Start;
            VecI end = End;
            if (point.X < Start.X) start.X = point.X;
            if (point.Y < Start.Y) start.Y = point.Y;
            if (point.X > End.X) end.X = point.X;
            if (point.Y > End.Y) end.Y = point.Y;
            
            return new Line(start, end);
        }
        
        public static bool operator ==(Line a, Line b)
        {
            return a.Start == b.Start && a.End == b.End;
        }
        
        public static bool operator !=(Line a, Line b)
        {
            return !(a == b);
        }
    }

    internal class Lines : IEnumerable<Line>
    {
        public Dictionary<VecI, Dictionary<VecI, Line>> LineDict { get; set; } = new Dictionary<VecI, Dictionary<VecI, Line>>();

        public Lines()
        {
            LineDict[Right] = new Dictionary<VecI, Line>();
            LineDict[Down] = new Dictionary<VecI, Line>();
            LineDict[Left] = new Dictionary<VecI, Line>();
            LineDict[Up] = new Dictionary<VecI, Line>();
        }
        
        public bool TryGetLine(VecI start, out Line line)
        {
            foreach (var lineDict in LineDict.Values)
            {
                if (lineDict.TryGetValue(start, out line))
                {
                    return true;
                }
            }
            
            line = default;
            return false;
        }

        public IEnumerator<Line> GetEnumerator()
        {
            foreach (var upLines in LineDict[Up])
            {
                yield return upLines.Value;
            }
            
            foreach (var rightLines in LineDict[Right])
            {
                yield return rightLines.Value;
            }
            
            foreach (var downLines in LineDict[Down])
            {
                yield return downLines.Value;
            }
            
            foreach (var leftLines in LineDict[Left])
            {
                yield return leftLines.Value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void RemoveLine(VecI prevPos)
        {
            foreach (var lineDict in LineDict.Values)
            {
                lineDict.Remove(prevPos);
            }
        }
    }
}
