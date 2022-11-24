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

    public static VectorPath GetFloodFillSelection(VecI startingPos, HashSet<Guid> membersToFloodFill,
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
                    ProcessEmptySelectionChunk(lines, chunkPos, document.Size, imageSizeInChunks, chunkSize);
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
            var maybeArray = GetChunkFloodFill(
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

        if (lines.LineDict.Any(x => x.Value.Count > 0))
        {
            selection = BuildContour(lines);
        }

        visualizer.GenerateVisualization(document.Size.X, document.Size.Y, 500, 500);

        return selection;
    }

    private static void ProcessEmptySelectionChunk(Lines lines, VecI chunkPos, VecI realSize,
        VecI imageSizeInChunks, int chunkSize)
    {
        bool isEdgeChunk = chunkPos.X == 0 || chunkPos.Y == 0 || chunkPos.X == imageSizeInChunks.X - 1 ||
                           chunkPos.Y == imageSizeInChunks.Y - 1;

        visualizer.CurrentContext = "EmptyChunk";
        if (isEdgeChunk)
        {
            bool isTopEdge = chunkPos.Y == 0;
            bool isBottomEdge = chunkPos.Y == imageSizeInChunks.Y - 1;
            bool isLeftEdge = chunkPos.X == 0;
            bool isRightEdge = chunkPos.X == imageSizeInChunks.X - 1;

            int posX = chunkPos.X * chunkSize;
            int posY = chunkPos.Y * chunkSize;

            int endX = posX + chunkSize;
            int endY = posY + chunkSize;

            endX = Math.Clamp(endX, 0, realSize.X);
            endY = Math.Clamp(endY, 0, realSize.Y);


            if (isTopEdge)
            {
                AddLine(new(new(posX, posY), new(endX, posY)), lines, Right);
            }

            if (isBottomEdge)
            {
                AddLine(new(new(endX, endY), new(posX, endY)), lines, Left);
            }

            if (isLeftEdge)
            {
                AddLine(new(new(posX, endY), new(posX, posY)), lines, Up);
            }

            if (isRightEdge)
            {
                AddLine(new(new(endX, posY), new(endX, endY)), lines, Down);
            }
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

    private static unsafe byte[]? GetChunkFloodFill(
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

            visualizer.CurrentContext = "AddCornerLines";
            AddCornerLines(documentSize, chunkOffset, lines, curPos, chunkSize, processedPositions);

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
        if (curPos.X > 0 && pixelStates[pixelOffset - 1] != Visited)
        {
            if (bounds.IsWithinBounds(refPixel - 4) && globalPos.X - 1 >= 0)
            {
                toVisit.Push(new(curPos.X - 1, curPos.Y));
            }
            else
            {
                AddLine(
                    new Line(
                        new VecI(curPos.X, curPos.Y + 1) + chunkOffset,
                        new VecI(curPos.X, curPos.Y) + chunkOffset), lines, Up);
            }
        }

        // Right pixel
        if (curPos.X < chunkSize - 1 && pixelStates[pixelOffset + 1] != Visited)
        {
            if (bounds.IsWithinBounds(refPixel + 4) && globalPos.X + 1 < documentSize.X)
            {
                toVisit.Push(new(curPos.X + 1, curPos.Y));
            }
            else
            {
                AddLine(
                    new Line(
                        new VecI(curPos.X + 1, curPos.Y) + chunkOffset,
                        new VecI(curPos.X + 1, curPos.Y + 1) + chunkOffset), lines, Down);
            }
        }

        // Top pixel
        if (curPos.Y > 0 && pixelStates[pixelOffset - chunkSize] != Visited)
        {
            if (bounds.IsWithinBounds(refPixel - 4 * chunkSize) && globalPos.Y - 1 >= 0)
            {
                toVisit.Push(new(curPos.X, curPos.Y - 1));
            }
            else
            {
                AddLine(
                    new Line(
                        new VecI(curPos.X + 1, curPos.Y) + chunkOffset,
                        new VecI(curPos.X, curPos.Y) + chunkOffset), lines, Right);
            }
        }

        //Bottom pixel
        if (curPos.Y < chunkSize - 1 && pixelStates[pixelOffset + chunkSize] != Visited)
        {
            if (bounds.IsWithinBounds(refPixel + 4 * chunkSize) && globalPos.Y + 1 < documentSize.Y)
            {
                toVisit.Push(new(curPos.X, curPos.Y + 1));
            }
            else
            {
                AddLine(
                    new Line(
                        new VecI(curPos.X + 1, curPos.Y + 1) + chunkOffset,
                        new VecI(curPos.X, curPos.Y + 1) + chunkOffset), lines, Left);
            }
        }
    }

    private static void AddCornerLines(VecI documentSize, VecI chunkOffset, Lines lines, VecI curPos, int chunkSize, HashSet<VecI> processedPositions)
    {
        VecI clampedPos = new(
            Math.Clamp(curPos.X, 0, documentSize.X - 1),
            Math.Clamp(curPos.Y, 0, documentSize.Y - 1));

        if (curPos.X == 0)
        {
            AddLine(
                new Line(
                    new VecI(clampedPos.X, clampedPos.Y + 1) + chunkOffset,
                    new VecI(clampedPos.X, clampedPos.Y) + chunkOffset), lines, Up);
        }

        if (curPos.X == chunkSize - 1)
        {
            AddLine(
                new Line(
                    new VecI(clampedPos.X + 1, clampedPos.Y) + chunkOffset,
                    new VecI(clampedPos.X + 1, clampedPos.Y + 1) + chunkOffset), lines, Down);
        }

        if (curPos.Y == 0)
        {
            AddLine(
                new Line(
                    new VecI(clampedPos.X, clampedPos.Y) + chunkOffset,
                    new VecI(clampedPos.X + 1, clampedPos.Y) + chunkOffset), lines, Right);
        }

        if (curPos.Y == chunkSize - 1)
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

        // if line in opposite direction exists, remove it

        if (lines.TryCancelLine(line, direction))
        {
            visualizer.Steps.Add(new Step(line, StepType.CancelLine));
            return;
        }

        if (calculatedDir == direction)
        {
            lines.LineDict[direction][line.Start] = line;
            visualizer.Steps.Add(new Step(line));
        }
        else if (calculatedDir == -direction)
        {
            Line fixedLine = new Line(line.End, line.Start);
            lines.LineDict[direction][line.End] = fixedLine;
            visualizer.Steps.Add(new Step(fixedLine));
        }
        else
        {
            throw new Exception(
                $"Line direction {calculatedDir} is not perpendicular to the direction of the requested line direction {direction}");
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
            return HashCode.Combine(Start, End, NormalizedDirection);
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
        public Dictionary<VecI, Dictionary<VecI, Line>> LineDict { get; set; } = new Dictionary<VecI, Dictionary<VecI, Line>>();

        public Lines()
        {
            LineDict[Right] = new Dictionary<VecI, Line>();
            LineDict[Down] = new Dictionary<VecI, Line>();
            LineDict[Left] = new Dictionary<VecI, Line>();
            LineDict[Up] = new Dictionary<VecI, Line>();
        }

        public Line? RemoveLineAt(VecI start)
        {
            foreach (var (_, lineDict) in LineDict)
            {
                if (lineDict.Remove(start, out Line value))
                    return value;
            }
            return null;
        }

        public Line? RemoveLineAt(VecI start, VecI direction)
        {
            if (LineDict[direction].Remove(start, out Line value))
                return value;
            return null;
        }

        public Line? PopLine()
        {
            foreach (var (_, lineDict) in LineDict)
            {
                if (lineDict.Count == 0)
                    continue;
                var result = lineDict.First();
                lineDict.Remove(result.Key);
                return result.Value;
            }
            return null;
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

        public void RemoveLine(Line line)
        {
            foreach (var lineDict in LineDict.Values)
            {
                VecI dictDir = lineDict == LineDict[Up] ? Up : lineDict == LineDict[Right] ? Right : lineDict == LineDict[Down] ? Down : Left;
                if (line.NormalizedDirection != dictDir) continue;
                lineDict.Remove(line.Start);
            }
        }

        public bool TryCancelLine(Line line, VecI direction)
        {
            bool cancelingLineExists = false;

            LineDict[-direction].TryGetValue(line.End, out Line cancelingLine);
            if (cancelingLine != default && cancelingLine.End == line.Start)
            {
                cancelingLineExists = true;
                LineDict[-direction].Remove(line.End);
            }

            LineDict[direction].TryGetValue(line.Start, out cancelingLine);
            if (cancelingLine != default && cancelingLine.End == line.End)
            {
                cancelingLineExists = true;
                LineDict[-direction].Remove(line.Start);
            }

            return cancelingLineExists;
        }
    }
}
