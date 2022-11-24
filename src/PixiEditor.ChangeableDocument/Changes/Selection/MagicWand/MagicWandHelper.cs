using System.Collections;
using ChunkyImageLib.Operations;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.Changes.Drawing.FloodFill;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface.Vector;

namespace PixiEditor.ChangeableDocument.Changes.Selection.MagicWand;
internal class MagicWandHelper
{
    private const byte Visited = 2;

    private static readonly VecI Up = new VecI(0, -1);
    private static readonly VecI Down = new VecI(0, 1);
    private static readonly VecI Left = new VecI(-1, 0);
    private static readonly VecI Right = new VecI(1, 0);

    private static MagicWandVisualizer visualizer = new MagicWandVisualizer(Path.Combine("Debugging", "MagicWand"));

    public static VectorPath DoMagicWandFloodFill(VecI startingPos, HashSet<Guid> membersToFloodFill,
        IReadOnlyDocument document)
    {
        if (startingPos.X < 0 || startingPos.Y < 0 || startingPos.X >= document.Size.X || startingPos.Y >= document.Size.Y)
            return new VectorPath();

        int chunkSize = ChunkResolution.Full.PixelSize();

        FloodFillChunkCache cache = FloodFillHelper.CreateCache(membersToFloodFill, document);

        VecI initChunkPos = OperationHelper.GetChunkPos(startingPos, chunkSize);
        VecI imageSizeInChunks = (VecI)(document.Size / (double)chunkSize).Ceiling();
        VecI initPosOnChunk = startingPos - initChunkPos * chunkSize;


        Color colorToReplace = cache.GetChunk(initChunkPos).Match(
            (Chunk chunk) => chunk.Surface.GetSRGBPixel(initPosOnChunk),
            static (EmptyChunk _) => Colors.Transparent
        );

        ColorBounds colorRange = new(colorToReplace);

        HashSet<VecI> processedEmptyChunks = new();
        HashSet<VecI> processedPositions = new();
        Stack<(VecI chunkPos, VecI posOnChunk)> positionsToFloodFill = new();
        positionsToFloodFill.Push((initChunkPos, initPosOnChunk));

        Lines lines = new();

        VectorPath selection = new();
        while (positionsToFloodFill.Count > 0)
        {
            var (chunkPos, posOnChunk) = positionsToFloodFill.Pop();
            var referenceChunk = cache.GetChunk(chunkPos);

            // don't call floodfill if the chunk is empty
            if (referenceChunk.IsT1)
            {
                if (colorToReplace.A == 0 && !processedEmptyChunks.Contains(chunkPos))
                {
                    AddLinesForEmptyChunk(lines, chunkPos, document.Size, imageSizeInChunks, chunkSize);
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

            VecI globalPos = chunkPos * chunkSize + posOnChunk;

            if (processedPositions.Contains(globalPos))
                continue;

            visualizer.CurrentContext = $"FloodFill_{chunkPos}";
            var maybeArray = AddLinesForChunkViaFloodFill(
                reallyReferenceChunk,
                chunkSize,
                chunkPos * chunkSize,
                document.Size,
                posOnChunk,
                colorRange, lines, processedPositions);

            if (maybeArray is null)
                continue;
            for (int i = 0; i < chunkSize; i++)
            {
                if (chunkPos.Y > 0 && maybeArray[i] == Visited) //Top
                    positionsToFloodFill.Push((new(chunkPos.X, chunkPos.Y - 1), new(i, chunkSize - 1)));
                if (chunkPos.Y < imageSizeInChunks.Y - 1 && maybeArray[chunkSize * (chunkSize - 1) + i] == Visited) // Bottom
                    positionsToFloodFill.Push((new(chunkPos.X, chunkPos.Y + 1), new(i, 0)));
                if (chunkPos.X > 0 && maybeArray[i * chunkSize] == Visited) // Left
                    positionsToFloodFill.Push((new(chunkPos.X - 1, chunkPos.Y), new(chunkSize - 1, i)));
                if (chunkPos.X < imageSizeInChunks.X - 1 && maybeArray[i * chunkSize + (chunkSize - 1)] == Visited) // Right
                    positionsToFloodFill.Push((new(chunkPos.X + 1, chunkPos.Y), new(0, i)));
            }
        }

        if (lines.Count > 0)
        {
            selection = BuildContour(lines);
        }

        visualizer.GenerateVisualization(document.Size.X, document.Size.Y, 500, 500);

        return selection;
    }

    private static void AddLinesForEmptyChunk(Lines lines, VecI chunkPos, VecI imageSize,
        VecI imageSizeInChunks, int chunkSize)
    {
        visualizer.CurrentContext = "EmptyChunk";

        RectI chunkRect = new RectI(chunkPos * chunkSize, new(chunkSize));
        chunkRect = chunkRect.Intersect(new RectI(VecI.Zero, imageSize));
        if (chunkRect.IsZeroOrNegativeArea)
            return;

        for (int i = chunkRect.Left; i < chunkRect.Right; i++)
        {
            lines.AddLine(new Line(new(i, chunkRect.Top), new(i + 1, chunkRect.Top))); // Top
            lines.AddLine(new Line(new(i + 1, chunkRect.Bottom), new(i, chunkRect.Bottom))); // Bottom
        }

        for (int j = chunkRect.Top; j < chunkRect.Bottom; j++)
        {
            lines.AddLine(new Line(new(chunkRect.Left, j + 1), new(chunkRect.Left, j))); // Left
            lines.AddLine(new Line(new(chunkRect.Right, j), new(chunkRect.Right, j + 1))); // Right
        }
    }

    private static VecI GoStraight(Lines allLines, Line firstLine)
    {
        Line previous = default;
        Line? current = firstLine;
        while (current != null)
        {
            (previous, current) = ((Line)current, allLines.RemoveLineAt(current.Value.End, current.Value.NormalizedDirection));
        }

        return previous.End;
    }

    private static void FollowPath(Lines allLines, Line startingLine, VectorPath path)
    {
        path.MoveTo(startingLine.Start);

        Line? current = startingLine;
        while (current != null)
        {
            VecI straightPathEnd = GoStraight(allLines, (Line)current);
            path.LineTo(straightPathEnd);
            current = allLines.RemoveLineAt(straightPathEnd);
        }
    }

    private static VectorPath BuildContour(Lines lines)
    {
        VectorPath selection = new();

        Line? current = lines.PopLine();
        while (current != null)
        {
            FollowPath(lines, (Line)current, selection);
            current = lines.PopLine();
        }

        return selection;
    }

    private static unsafe byte[]? AddLinesForChunkViaFloodFill(
        Chunk referenceChunk,
        int chunkSize,
        VecI chunkOffset,
        VecI documentSize,
        VecI pos,
        ColorBounds bounds, Lines lines, HashSet<VecI> processedPositions)
    {
        if (!bounds.IsWithinBounds(referenceChunk.Surface.GetSRGBPixel(pos)))
        {
            return null;
        }

        byte[] pixelStates = new byte[chunkSize * chunkSize];

        using var refPixmap = referenceChunk.Surface.DrawingSurface.PeekPixels();
        Half* refArray = (Half*)refPixmap.GetPixels();

        Stack<VecI> toVisit = new();
        toVisit.Push(pos);

        while (toVisit.Count > 0)
        {
            VecI curPos = toVisit.Pop();

            int pixelOffset = curPos.X + curPos.Y * chunkSize;
            VecI globalPos = curPos + chunkOffset;
            Half* refPixel = refArray + pixelOffset * 4;

            if (!bounds.IsWithinBounds(refPixel))
            {
                processedPositions.Add(globalPos);
                continue;
            }

            pixelStates[pixelOffset] = Visited;

            visualizer.CurrentContext = "AddFillContourLines";
            AddFillContourLines(chunkSize, chunkOffset, bounds, lines, curPos, pixelStates, pixelOffset, refPixel, toVisit, globalPos, documentSize, processedPositions);

            processedPositions.Add(globalPos);
        }

        return pixelStates;
    }

    private static unsafe void AddFillContourLines(int chunkSize, VecI chunkOffset, ColorBounds bounds, Lines lines,
        VecI curPos, byte[] pixelStates, int pixelOffset, Half* refPixel, Stack<VecI> toVisit, VecI globalPos,
        VecI documentSize, HashSet<VecI> processedPositions)
    {

        if (processedPositions.Contains(globalPos)) return;

        // Left pixel
        bool leftEdgePresent = curPos.X == 0 || globalPos.X == 0 || !bounds.IsWithinBounds(refPixel - 4);
        if (!leftEdgePresent && pixelStates[pixelOffset - 1] != Visited)
        {
            toVisit.Push(new(curPos.X - 1, curPos.Y));
        }
        else if (leftEdgePresent)
        {
            lines.AddLine(new Line(
                new VecI(curPos.X, curPos.Y + 1) + chunkOffset,
                new VecI(curPos.X, curPos.Y) + chunkOffset));
        }

        // Right pixel
        bool rightEdgePresent = globalPos.X == documentSize.X - 1 || curPos.X == chunkSize - 1 || !bounds.IsWithinBounds(refPixel + 4);
        if (!rightEdgePresent && pixelStates[pixelOffset + 1] != Visited)
        {
            toVisit.Push(new(curPos.X + 1, curPos.Y));
        }
        else if (rightEdgePresent)
        {
            lines.AddLine(new Line(
                new VecI(curPos.X + 1, curPos.Y) + chunkOffset,
                new VecI(curPos.X + 1, curPos.Y + 1) + chunkOffset));
        }

        // Top pixel
        bool topEdgePresent = curPos.Y == 0 || globalPos.Y == 0 || !bounds.IsWithinBounds(refPixel - 4 * chunkSize);
        if (!topEdgePresent && pixelStates[pixelOffset - chunkSize] != Visited)
        {
            toVisit.Push(new(curPos.X, curPos.Y - 1));
        }
        else if (topEdgePresent)
        {
            lines.AddLine(new Line(
                new VecI(curPos.X, curPos.Y) + chunkOffset,
                new VecI(curPos.X + 1, curPos.Y) + chunkOffset));
        }

        //Bottom pixel
        bool bottomEdgePresent = globalPos.Y == documentSize.Y - 1 || curPos.Y == chunkSize - 1 || !bounds.IsWithinBounds(refPixel + 4 * chunkSize);
        if (!bottomEdgePresent && pixelStates[pixelOffset + chunkSize] != Visited)
        {
            toVisit.Push(new(curPos.X, curPos.Y + 1));
        }
        else if (bottomEdgePresent)
        {
            lines.AddLine(new Line(
                new VecI(curPos.X + 1, curPos.Y + 1) + chunkOffset,
                new VecI(curPos.X, curPos.Y + 1) + chunkOffset));
        }
    }

    internal struct Line
    {
        public bool Equals(Line other)
        {
            return Start.Equals(other.Start) && End.Equals(other.End) && NormalizedDirection.Equals(other.NormalizedDirection);
        }

        public override bool Equals(object? obj)
        {
            return obj is Line other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Start, End);
        }

        public VecI Start { get; set; }
        public VecI End { get; set; }
        public VecI NormalizedDirection { get; }

        public Line(VecI start, VecI end)
        {
            Start = start;
            End = end;
            NormalizedDirection = (VecI)(end - start).Normalized();
        }

        public static bool operator ==(Line a, Line b)
        {
            return a.Start == b.Start && a.End == b.End;
        }

        public static bool operator !=(Line a, Line b)
        {
            return !(a == b);
        }

        public override string ToString()
        {
            string dir = (NormalizedDirection.X, NormalizedDirection.Y) switch
            {
                ( > 0, _) => "Right",
                ( < 0, _) => "Left",
                (_, < 0) => "Up",
                (_, > 0) => "Down",
                _ => "Weird dir"
            };
            return $"{Start}, {dir}";
        }
    }

    internal class Lines : IEnumerable<Line>
    {
        private Dictionary<VecI, Dictionary<VecI, Line>> LineDicts { get; set; } = new Dictionary<VecI, Dictionary<VecI, Line>>();

        public int Count => LineDicts.Aggregate(0, (acc, cur) => acc += cur.Value.Count);

        public Lines()
        {
            LineDicts[Right] = new Dictionary<VecI, Line>();
            LineDicts[Down] = new Dictionary<VecI, Line>();
            LineDicts[Left] = new Dictionary<VecI, Line>();
            LineDicts[Up] = new Dictionary<VecI, Line>();
        }

        public Line? RemoveLineAt(VecI start)
        {
            foreach (var (_, lineDict) in LineDicts)
            {
                if (lineDict.Remove(start, out Line value))
                    return value;
            }
            return null;
        }

        public Line? RemoveLineAt(VecI start, VecI direction)
        {
            if (LineDicts[direction].Remove(start, out Line value))
                return value;
            return null;
        }

        public Line? PopLine()
        {
            foreach (var (_, lineDict) in LineDicts)
            {
                if (lineDict.Count == 0)
                    continue;
                var result = lineDict.First();
                lineDict.Remove(result.Key);
                return result.Value;
            }
            return null;
        }

        public void AddLine(Line line)
        {
            // cancel out line
            if (LineDicts[-line.NormalizedDirection].ContainsKey(line.End))
            {
                LineDicts[-line.NormalizedDirection].Remove(line.End);
                visualizer.Steps.Add(new Step(line, StepType.CancelLine));
                return;
            }

            LineDicts[line.NormalizedDirection][line.Start] = line;
            visualizer.Steps.Add(new Step(line));
        }

        public IEnumerator<Line> GetEnumerator()
        {
            foreach (var upLines in LineDicts[Up])
            {
                yield return upLines.Value;
            }

            foreach (var rightLines in LineDicts[Right])
            {
                yield return rightLines.Value;
            }

            foreach (var downLines in LineDicts[Down])
            {
                yield return downLines.Value;
            }

            foreach (var leftLines in LineDicts[Left])
            {
                yield return leftLines.Value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
