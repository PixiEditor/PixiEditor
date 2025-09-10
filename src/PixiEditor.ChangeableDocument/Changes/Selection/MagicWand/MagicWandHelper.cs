using System.Collections;
using ChunkyImageLib.Operations;
using Drawie.Backend.Core.Bridge;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.Changes.Drawing.FloodFill;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changes.Selection.MagicWand;
internal class MagicWandHelper
{
    private static readonly VecI Up = new VecI(0, -1);
    private static readonly VecI Down = new VecI(0, 1);
    private static readonly VecI Left = new VecI(-1, 0);
    private static readonly VecI Right = new VecI(1, 0);

    //private static MagicWandVisualizer visualizer = new MagicWandVisualizer(Path.Combine("Debugging", "MagicWand"));

    private class UnvisitedStack
    {
        private int chunkSize;
        private readonly VecI imageSize;
        private Stack<(VecI chunkPos, VecI posOnChunk)> likelyUnvisited = new();
        private HashSet<VecI> certainlyVisited = new();

        public UnvisitedStack(int chunkSize, VecI imageSize)
        {
            this.chunkSize = chunkSize;
            this.imageSize = imageSize;
        }

        public void PushAll(VecI chunkPos)
        {
            VecI chunkOffset = chunkPos * chunkSize;
            for (int i = 0; i < chunkSize; i++)
            {
                // separated into a function to prevent stackalloc stackoverflow
                PushArrayIteration(i);
            }
            void PushArrayIteration(int i)
            {
                Span<(VecI, VecI, VecI)> options = stackalloc (VecI, VecI, VecI)[]
                {
                    (new(chunkPos.X, chunkPos.Y - 1), new(i, chunkSize - 1), new(i, 0)), // Top
                    (new(chunkPos.X, chunkPos.Y + 1), new(i, 0), new(i, chunkSize - 1)), // Bottom
                    (new(chunkPos.X - 1, chunkPos.Y), new(chunkSize - 1, i), new(0, i)), // Left
                    (new(chunkPos.X + 1, chunkPos.Y), new(0, i), new(chunkSize - 1, i)) // Right
                };

                foreach (var (otherChunkPos, otherPosInChunk, refPos) in options)
                {
                    VecI global = otherChunkPos * chunkSize + otherPosInChunk;
                    if (global.X < 0 || global.Y < 0 || global.X >= imageSize.X || global.Y >= imageSize.Y)
                        continue;
                    likelyUnvisited.Push((otherChunkPos, otherPosInChunk));
                    certainlyVisited.Add(chunkOffset + refPos);
                }
            }
        }

        public void Push(VecI chunkPos, VecI posOnChunk)
        {
            likelyUnvisited.Push((chunkPos, posOnChunk));
        }

        public void Push(VecI chunkPos, bool[] visitedArray)
        {
            VecI chunkOffset = chunkPos * chunkSize;
            for (int i = 0; i < chunkSize; i++)
            {
                // separated into a function to prevent stackalloc stackoverflow
                PushArrayIteration(i);
            }
            void PushArrayIteration(int i)
            {
                Span<(int, VecI, VecI, VecI)> options = stackalloc (int, VecI, VecI, VecI)[]
                {
                    (i, new(chunkPos.X, chunkPos.Y - 1), new(i, chunkSize - 1), new(i, 0)), // Top
                    (chunkSize * (chunkSize - 1) + i, new(chunkPos.X, chunkPos.Y + 1), new(i, 0), new(i, chunkSize - 1)), // Bottom
                    (i * chunkSize, new(chunkPos.X - 1, chunkPos.Y), new(chunkSize - 1, i), new(0, i)), // Left
                    (i * chunkSize + (chunkSize - 1), new(chunkPos.X + 1, chunkPos.Y), new(0, i), new(chunkSize - 1, i)) // Right
                };

                foreach (var (refIndex, otherChunkPos, otherPosInChunk, refPos) in options)
                {
                    VecI otherGlobal = otherChunkPos * chunkSize + otherPosInChunk;
                    if (!visitedArray[refIndex] || otherGlobal.X < 0 || otherGlobal.Y < 0 || otherGlobal.X >= imageSize.X || otherGlobal.Y >= imageSize.Y)
                        continue;
                    certainlyVisited.Add(chunkOffset + refPos);
                    likelyUnvisited.Push((otherChunkPos, otherPosInChunk));
                }
            }
        }

        public (VecI chunkPos, VecI posOnChunk)? PopUnvisited()
        {
            while (likelyUnvisited.Count > 0)
            {
                var (chunkPos, posOnChunk) = likelyUnvisited.Pop();
                VecI global = chunkPos * chunkSize + posOnChunk;
                if (certainlyVisited.Contains(global))
                    continue;
                return (chunkPos, posOnChunk);
            }
            return null;
        }
    }

    public static VectorPath DoMagicWandFloodFill(VecI startingPos, HashSet<Guid> membersToFloodFill,
        double tolerance,
        IReadOnlyDocument document, int frame)
    {
        if (startingPos.X < 0 || startingPos.Y < 0 || startingPos.X >= document.Size.X || startingPos.Y >= document.Size.Y)
            return new VectorPath();
        
        tolerance = Math.Clamp(tolerance, 0, 1);

        int chunkSize = ChunkResolution.Full.PixelSize();

        FloodFillChunkCache cache = FloodFillHelper.CreateCache(membersToFloodFill, document, frame);

        VecI initChunkPos = OperationHelper.GetChunkPos(startingPos, chunkSize);
        VecI imageSizeInChunks = (VecI)(document.Size / (double)chunkSize).Ceiling();
        VecI initPosOnChunk = startingPos - initChunkPos * chunkSize;


        Color colorToReplace = cache.GetChunk(initChunkPos).Match(
            (Chunk chunk) => chunk.Surface.GetRawPixel(initPosOnChunk),
            static (EmptyChunk _) => Colors.Transparent
        );

        ColorBounds colorRange = new(colorToReplace, tolerance);

        HashSet<VecI> processedEmptyChunks = new();

        UnvisitedStack positionsToFloodFill = new(chunkSize, document.Size);

        Lines lines = new();
        VectorPath selection = new();

        positionsToFloodFill.Push(initChunkPos, initPosOnChunk);
        while (true)
        {
            (VecI initChunkPos, VecI initPosOnChunk)? popped = positionsToFloodFill.PopUnvisited();
            if (popped is null)
                break;
            var (chunkPos, posOnChunk) = popped.Value;
            var referenceChunk = cache.GetChunk(chunkPos);

            // don't call floodfill if the chunk is empty
            if (referenceChunk.IsT1)
            {
                if (colorToReplace.A == 0 && !processedEmptyChunks.Contains(chunkPos))
                {
                    AddLinesForEmptyChunk(lines, chunkPos, document.Size, chunkSize);
                    positionsToFloodFill.PushAll(chunkPos);
                    processedEmptyChunks.Add(chunkPos);
                }
                continue;
            }

            // use regular flood fill for chunks that have something in them
            var reallyReferenceChunk = referenceChunk.AsT0;

            VecI globalPos = chunkPos * chunkSize + posOnChunk;
            //visualizer.CurrentContext = $"FloodFill_{chunkPos}";
            var maybeArray = AddLinesForChunkViaFloodFill(
                reallyReferenceChunk,
                chunkSize,
                chunkPos * chunkSize,
                document.Size,
                posOnChunk,
                colorRange, lines);

            if (maybeArray is null)
                continue;
            positionsToFloodFill.Push(chunkPos, maybeArray);
        }

        if (lines.Count > 0)
        {
            selection = BuildContour(lines);
        }

        //visualizer.GenerateVisualization(document.Size.X, document.Size.Y, 500, 500);

        return selection;
    }

    private static void AddLinesForEmptyChunk(Lines lines, VecI chunkPos, VecI imageSize, int chunkSize)
    {
        //visualizer.CurrentContext = "EmptyChunk";

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
        while (current is not null)
        {
            (previous, current) = ((Line)current, allLines.RemoveLineAt(current.Value.End, current.Value.NormalizedDirection));
        }

        return previous.End;
    }

    private static void FollowPath(Lines allLines, Line startingLine, VectorPath path)
    {
        path.MoveTo(startingLine.Start);

        Line? current = startingLine;
        while (current is not null)
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
        while (current is not null)
        {
            FollowPath(lines, (Line)current, selection);
            current = lines.PopLine();
        }

        return selection;
    }

    private static unsafe bool[]? AddLinesForChunkViaFloodFill(
        Chunk referenceChunk,
        int chunkSize,
        VecI chunkOffset,
        VecI documentSize,
        VecI pos,
        ColorBounds bounds, Lines lines)
    {
        using var ctx = DrawingBackendApi.Current.RenderingDispatcher.EnsureContext();
        if (!bounds.IsWithinBounds(referenceChunk.Surface.GetRawPixel(pos)))
        {
            return null;
        }

        bool[] pixelVisitedStates = new bool[chunkSize * chunkSize];

        using var refPixmap = referenceChunk.Surface.PeekPixels();
        Half* refArray = (Half*)refPixmap.GetPixels();

        Stack<VecI> toVisit = new();
        toVisit.Push(pos);

        while (toVisit.Count > 0)
        {
            VecI curPos = toVisit.Pop();

            int pixelOffset = curPos.X + curPos.Y * chunkSize;
            if (pixelVisitedStates[pixelOffset])
                continue;

            VecI globalPos = curPos + chunkOffset;
            Half* refPixel = refArray + pixelOffset * 4;

            if (!bounds.IsWithinBounds(refPixel))
                continue;

            pixelVisitedStates[pixelOffset] = true;

            //visualizer.CurrentContext = "AddFillContourLines";
            AddFillContourLines(chunkSize, chunkOffset, bounds, lines, curPos, pixelVisitedStates, pixelOffset, refPixel, toVisit, globalPos, documentSize);
        }

        return pixelVisitedStates;
    }

    private static unsafe void AddFillContourLines(int chunkSize, VecI chunkOffset, ColorBounds bounds, Lines lines,
        VecI curPos, bool[] pixelStates, int pixelOffset, Half* refPixel, Stack<VecI> toVisit, VecI globalPos,
        VecI documentSize)
    {
        // Left pixel
        bool leftEdgePresent = curPos.X == 0 || globalPos.X == 0 || !bounds.IsWithinBounds(refPixel - 4);
        if (!leftEdgePresent && !pixelStates[pixelOffset - 1])
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
        if (!rightEdgePresent && !pixelStates[pixelOffset + 1])
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
        if (!topEdgePresent && !pixelStates[pixelOffset - chunkSize])
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
        if (!bottomEdgePresent && !pixelStates[pixelOffset + chunkSize])
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
                //visualizer.Steps.Add(new Step(line, StepType.CancelLine));
                return;
            }

            LineDicts[line.NormalizedDirection][line.Start] = line;
            //visualizer.Steps.Add(new Step(line));
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
