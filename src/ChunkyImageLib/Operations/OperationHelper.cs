using ChunkyImageLib.DataHolders;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Numerics;

namespace ChunkyImageLib.Operations;

public static class OperationHelper
{
    public static VecI ConvertForResolution(VecI pixelPos, ChunkResolution resolution)
    {
        var mult = resolution.Multiplier();
        return new((int)Math.Round(pixelPos.X * mult), (int)Math.Round(pixelPos.Y * mult));
    }

    public static VecD ConvertForResolution(VecD pixelPos, ChunkResolution resolution)
    {
        var mult = resolution.Multiplier();
        return new(pixelPos.X * mult, pixelPos.Y * mult);
    }

    /// <summary>
    /// toModify[x,y].Alpha = Math.Min(toModify[x,y].Alpha, toGetAlphaFrom[x,y].Alpha)
    /// </summary>
    public static unsafe void ClampAlpha(IPixelsMap toModify, IPixelsMap toGetAlphaFrom, RectI? clippingRect = null)
    {
        if (clippingRect is not null)
        {
            ClampAlphaWithClippingRect(toModify, toGetAlphaFrom, (RectI)clippingRect);
            return;
        }

        using Pixmap map = toModify.PeekPixels();
        using Pixmap refMap = toGetAlphaFrom.PeekPixels();
        long* pixels = (long*)map.GetPixels();
        long* refPixels = (long*)refMap.GetPixels();
        int size = map.Width * map.Height;
        if (map.Width != refMap.Width || map.Height != refMap.Height)
            throw new ArgumentException("The surfaces must have the same size");

        for (int i = 0; i < size; i++)
        {
            long* offset = pixels + i;
            long* refOffset = refPixels + i;
            Half* alpha = (Half*)offset + 3;
            Half* refAlpha = (Half*)refOffset + 3;
            if (*refAlpha < *alpha)
            {
                float a = (float)(*alpha);
                float r = (float)(*((Half*)offset)) / a;
                float g = (float)(*((Half*)offset + 1)) / a;
                float b = (float)(*((Half*)offset + 2)) / a;
                float newA = (float)(*refAlpha);
                Half newR = (Half)(r * newA);
                Half newG = (Half)(g * newA);
                Half newB = (Half)(b * newA);
                *offset = (*(ushort*)(&newR)) | ((long)*(ushort*)(&newG)) << 16 | ((long)*(ushort*)(&newB)) << 32 | ((long)*(ushort*)(refAlpha)) << 48;
            }
        }

        toModify.MarkPixelsChanged();
    }

    private static unsafe void ClampAlphaWithClippingRect(IPixelsMap toModify, IPixelsMap toGetAlphaFrom, RectI clippingRect)
    {
        using Pixmap map = toModify.PeekPixels();
        using Pixmap refMap = toGetAlphaFrom.PeekPixels();
        long* pixels = (long*)map.GetPixels();
        long* refPixels = (long*)refMap.GetPixels();
        int size = map.Width * map.Height;
        if (map.Width != refMap.Width || map.Height != refMap.Height)
            throw new ArgumentException("The surfaces must have the same size");
        RectI workingArea = clippingRect.Intersect(new RectI(0, 0, map.Width, map.Height));
        if (workingArea.IsZeroOrNegativeArea)
            return;

        for (int y = workingArea.Top; y < workingArea.Bottom; y++)
        {
            for (int x = workingArea.Left; x < workingArea.Right; x++)
            {
                int position = x + y * map.Width;
                long* offset = pixels + position;
                long* refOffset = refPixels + position;
                Half* alpha = (Half*)offset + 3;
                Half* refAlpha = (Half*)refOffset + 3;
                if (*refAlpha < *alpha)
                {
                    float a = (float)(*alpha);
                    float r = (float)(*((Half*)offset)) / a;
                    float g = (float)(*((Half*)offset + 1)) / a;
                    float b = (float)(*((Half*)offset + 2)) / a;
                    float newA = (float)(*refAlpha);
                    Half newR = (Half)(r * newA);
                    Half newG = (Half)(g * newA);
                    Half newB = (Half)(b * newA);
                    *offset = (*(ushort*)(&newR)) | ((long)*(ushort*)(&newG)) << 16 | ((long)*(ushort*)(&newB)) << 32 | ((long)*(ushort*)(refAlpha)) << 48;
                }
            }
        }

        toModify.MarkPixelsChanged();
    }

    public static ShapeCorners ConvertForResolution(ShapeCorners corners, ChunkResolution resolution)
    {
        return new ShapeCorners()
        {
            BottomLeft = ConvertForResolution(corners.BottomLeft, resolution),
            BottomRight = ConvertForResolution(corners.BottomRight, resolution),
            TopLeft = ConvertForResolution(corners.TopLeft, resolution),
            TopRight = ConvertForResolution(corners.TopRight, resolution),
        };
    }

    public static VecI GetChunkPos(VecI pixelPos, int chunkSize)
    {
        return new VecI()
        {
            X = (int)MathF.Floor(pixelPos.X / (float)chunkSize),
            Y = (int)MathF.Floor(pixelPos.Y / (float)chunkSize)
        };
    }

    public static Matrix3X3 CreateMatrixFromPoints(ShapeCorners corners, VecD size)
        => CreateMatrixFromPoints((VecF)corners.TopLeft, (VecF)corners.TopRight, (VecF)corners.BottomRight, (VecF)corners.BottomLeft, (float)size.X, (float)size.Y);

    // see https://stackoverflow.com/questions/48416118/perspective-transform-in-skia/72364829#72364829
    public static Matrix3X3 CreateMatrixFromPoints(VecF topLeft, VecF topRight, VecF botRight, VecF botLeft, double width, double height)
    {
        (double x1, double y1) = (topLeft.X, topLeft.Y);
        (double x2, double y2) = (topRight.X, topRight.Y);
        (double x3, double y3) = (botRight.X, botRight.Y);
        (double x4, double y4) = (botLeft.X, botLeft.Y);
        (double w, double h) = (width, height);

        double scaleX = (y1 * x2 * x4 - x1 * y2 * x4 + x1 * y3 * x4 - x2 * y3 * x4 - y1 * x2 * x3 + x1 * y2 * x3 - x1 * y4 * x3 + x2 * y4 * x3) / (x2 * y3 * w + y2 * x4 * w - y3 * x4 * w - x2 * y4 * w - y2 * w * x3 + y4 * w * x3);
        double skewX = (-x1 * x2 * y3 - y1 * x2 * x4 + x2 * y3 * x4 + x1 * x2 * y4 + x1 * y2 * x3 + y1 * x4 * x3 - y2 * x4 * x3 - x1 * y4 * x3) / (x2 * y3 * h + y2 * x4 * h - y3 * x4 * h - x2 * y4 * h - y2 * h * x3 + y4 * h * x3);
        double transX = x1;
        double skewY = (-y1 * x2 * y3 + x1 * y2 * y3 + y1 * y3 * x4 - y2 * y3 * x4 + y1 * x2 * y4 - x1 * y2 * y4 - y1 * y4 * x3 + y2 * y4 * x3) / (x2 * y3 * w + y2 * x4 * w - y3 * x4 * w - x2 * y4 * w - y2 * w * x3 + y4 * w * x3);
        double scaleY = (-y1 * x2 * y3 - y1 * y2 * x4 + y1 * y3 * x4 + x1 * y2 * y4 - x1 * y3 * y4 + x2 * y3 * y4 + y1 * y2 * x3 - y2 * y4 * x3) / (x2 * y3 * h + y2 * x4 * h - y3 * x4 * h - x2 * y4 * h - y2 * h * x3 + y4 * h * x3);
        double transY = y1;
        double persp0 = (x1 * y3 - x2 * y3 + y1 * x4 - y2 * x4 - x1 * y4 + x2 * y4 - y1 * x3 + y2 * x3) / (x2 * y3 * w + y2 * x4 * w - y3 * x4 * w - x2 * y4 * w - y2 * w * x3 + y4 * w * x3);
        double persp1 = (-y1 * x2 + x1 * y2 - x1 * y3 - y2 * x4 + y3 * x4 + x2 * y4 + y1 * x3 - y4 * x3) / (x2 * y3 * h + y2 * x4 * h - y3 * x4 * h - x2 * y4 * h - y2 * h * x3 + y4 * h * x3);
        double persp2 = 1;

        return new Matrix3X3((float)scaleX, (float)skewX, (float)transX, (float)skewY, (float)scaleY, (float)transY, (float)persp0, (float)persp1, (float)persp2);
    }

    public static (ShapeCorners, ShapeCorners) CreateStretchedHexagon(VecD centerPos, double hexagonSide, double stretchX)
    {
        ShapeCorners left = new ShapeCorners()
        {
            TopLeft = centerPos + VecD.FromAngleAndLength(Math.PI * 7 / 6, hexagonSide),
            TopRight = new VecD(centerPos.X, centerPos.Y - hexagonSide),
            BottomRight = new VecD(centerPos.X, centerPos.Y + hexagonSide),
            BottomLeft = centerPos + VecD.FromAngleAndLength(Math.PI * 5 / 6, hexagonSide),
        };
        left.TopLeft = new VecD((left.TopLeft.X - centerPos.X) * stretchX + centerPos.X, left.TopLeft.Y);
        left.BottomLeft = new VecD((left.BottomLeft.X - centerPos.X) * stretchX + centerPos.X, left.BottomLeft.Y);
        ShapeCorners right = new ShapeCorners()
        {
            TopRight = centerPos + VecD.FromAngleAndLength(Math.PI * 11 / 6, hexagonSide),
            TopLeft = new VecD(centerPos.X, centerPos.Y - hexagonSide),
            BottomLeft = new VecD(centerPos.X, centerPos.Y + hexagonSide),
            BottomRight = centerPos + VecD.FromAngleAndLength(Math.PI * 1 / 6, hexagonSide),
        };
        right.TopRight = new VecD((right.TopRight.X - centerPos.X) * stretchX + centerPos.X, right.TopRight.Y);
        right.BottomRight = new VecD((right.BottomRight.X - centerPos.X) * stretchX + centerPos.X, right.BottomRight.Y);
        return (left, right);
    }

    public static HashSet<VecI> FindChunksTouchingEllipse(VecD pos, double radiusX, double radiusY, int chunkSize)
    {
        const double sqrt3 = 1.73205080757;
        double hexagonSide = 2.0 / sqrt3 * radiusY;
        double stretchX = radiusX / radiusY;
        var (left, right) = CreateStretchedHexagon(pos, hexagonSide, stretchX);
        var chunks = FindChunksTouchingQuadrilateral(left, chunkSize);
        chunks.UnionWith(FindChunksTouchingQuadrilateral(right, chunkSize));
        return chunks;
    }

    public static HashSet<VecI> FindChunksFullyInsideEllipse(VecD pos, double radiusX, double radiusY, int chunkSize,
        double rotation)
    {
        double stretchX = radiusX / radiusY;
        var (left, right) = CreateStretchedHexagon(pos, radiusY, stretchX);
        left = left.AsRotated(rotation, pos);
        right = right.AsRotated(rotation, pos);
        
        var chunks = FindChunksFullyInsideQuadrilateral(left, chunkSize);
        chunks.UnionWith(FindChunksFullyInsideQuadrilateral(right, chunkSize));
        return chunks;
    }
    
    public static HashSet<VecI> FindChunksTouchingQuadrilateral(ShapeCorners corners, int chunkSize)
    {
        if (corners.IsRect && Math.Abs(corners.RectRotation) < 0.0001)
            return FindChunksTouchingRectangle((RectI)RectD.FromCenterAndSize(corners.RectCenter, corners.RectSize).RoundOutwards(), chunkSize);
        if (corners.HasNaNOrInfinity ||
            (corners.BottomLeft - corners.TopRight).Length > chunkSize * 40 * 20 ||
            (corners.TopLeft - corners.BottomRight).Length > chunkSize * 40 * 20)
            return new HashSet<VecI>();
        if (corners.IsInverted)
            corners = corners with { BottomLeft = corners.TopRight, TopRight = corners.BottomLeft };
        List<VecI>[] lines = new List<VecI>[] 
        {
            FindChunksAlongLine(corners.TopRight, corners.TopLeft, chunkSize),
            FindChunksAlongLine(corners.BottomRight, corners.TopRight, chunkSize),
            FindChunksAlongLine(corners.BottomLeft, corners.BottomRight, chunkSize),
            FindChunksAlongLine(corners.TopLeft, corners.BottomLeft, chunkSize)
        };
        return FillLines(lines);
    }

    public static HashSet<VecI> FindChunksFullyInsideQuadrilateral(ShapeCorners corners, int chunkSize)
    {
        if (corners.IsRect && Math.Abs(corners.RectRotation) < 0.0001)
            return FindChunksFullyInsideRectangle((RectI)RectD.FromCenterAndSize(corners.RectCenter, corners.RectSize).RoundOutwards(), chunkSize);
        if (corners.HasNaNOrInfinity ||
            (corners.BottomLeft - corners.TopRight).Length > chunkSize * 40 * 20 ||
            (corners.TopLeft - corners.BottomRight).Length > chunkSize * 40 * 20)
            return new HashSet<VecI>();
        if (corners.IsInverted)
            corners = corners with { BottomLeft = corners.TopRight, TopRight = corners.BottomLeft };
        List<VecI>[] lines = new List<VecI>[] {
            FindChunksAlongLine(corners.TopLeft, corners.TopRight, chunkSize),
            FindChunksAlongLine(corners.TopRight, corners.BottomRight, chunkSize),
            FindChunksAlongLine(corners.BottomRight, corners.BottomLeft, chunkSize),
            FindChunksAlongLine(corners.BottomLeft, corners.TopLeft, chunkSize)
        };

        var output = FillLines(lines);

        //exclude lines
        for (int i = 0; i < lines.Length; i++)
        {
            output.ExceptWith(lines[i]);
        }

        return output;
    }

    public static HashSet<VecI> FindChunksTouchingRectangle(RectI rect, int chunkSize)
    {
        if (rect.Width > chunkSize * 40 * 20 || rect.Height > chunkSize * 40 * 20 || rect.IsZeroOrNegativeArea)
            return new HashSet<VecI>();

        VecI min = GetChunkPos(rect.TopLeft, chunkSize);
        VecI max = GetChunkPosBiased(rect.BottomRight, false, false, chunkSize);
        HashSet<VecI> output = new();
        for (int x = min.X; x <= max.X; x++)
        {
            for (int y = min.Y; y <= max.Y; y++)
            {
                output.Add(new(x, y));
            }
        }
        return output;
    }

    /// <summary>
    /// Finds chunks that at least partially lie inside of a rectangle
    /// </summary>
    public static HashSet<VecI> FindChunksTouchingRectangle(VecD center, VecD size, double angle, int chunkSize)
    {
        if (angle == 0)
            return FindChunksTouchingRectangle((RectI)RectD.FromCenterAndSize(center, size).RoundOutwards(), chunkSize);
        if (size.X == 0 || size.Y == 0 || center.IsNaNOrInfinity() || size.IsNaNOrInfinity() || double.IsNaN(angle) || double.IsInfinity(angle))
            return new HashSet<VecI>();
        if (size.X > chunkSize * 40 * 20 || size.Y > chunkSize * 40 * 20)
            return new HashSet<VecI>();
        // draw a line on the outside of each side
        var corners = FindRectangleCorners(center, size, angle);
        List<VecI>[] lines = new List<VecI>[] {
            FindChunksAlongLine(corners.Item2, corners.Item1, chunkSize),
            FindChunksAlongLine(corners.Item3, corners.Item2, chunkSize),
            FindChunksAlongLine(corners.Item4, corners.Item3, chunkSize),
            FindChunksAlongLine(corners.Item1, corners.Item4, chunkSize)
        };
        if (lines[0].Count == 0 || lines[1].Count == 0 || lines[2].Count == 0 || lines[3].Count == 0)
            return new HashSet<VecI>();
        return FillLines(lines);
    }

    public static HashSet<VecI> FillLines(List<VecI>[] lines)
    {
        if (lines.Length == 0 || lines.Any(static line => line.Count == 0))
            return new HashSet<VecI>();

        //find min and max X for each Y in lines
        var ySel = (VecI vec) => vec.Y;
        int minY = int.MaxValue;
        int maxY = int.MinValue;
        foreach (var line in lines)
        {
            minY = Math.Min(line.Min(ySel), minY);
            maxY = Math.Max(line.Max(ySel), maxY);
        }

        int[] minXValues = new int[maxY - minY + 1];
        int[] maxXValues = new int[maxY - minY + 1];
        for (int i = 0; i < minXValues.Length; i++)
        {
            minXValues[i] = int.MaxValue;
            maxXValues[i] = int.MinValue;
        }

        for (int i = 0; i < lines.Length; i++)
        {
            UpdateMinXValues(lines[i], minXValues, minY);
            UpdateMaxXValues(lines[i], maxXValues, minY);
        }

        //draw a line from min X to max X for each Y
        HashSet<VecI> output = new();
        for (int i = 0; i < minXValues.Length; i++)
        {
            int minX = minXValues[i];
            int maxX = maxXValues[i];
            for (int x = minX; x <= maxX; x++)
                output.Add(new(x, i + minY));
        }

        return output;
    }

    public static HashSet<VecI> FindChunksFullyInsideRectangle(RectI rect, int chunkSize)
    {
        if (rect.Width > chunkSize * 40 * 20 || rect.Height > chunkSize * 40 * 20)
            return new HashSet<VecI>();
        VecI startChunk = GetChunkPosBiased(rect.TopLeft, false, false, ChunkPool.FullChunkSize) + new VecI(1, 1);
        VecI endChunk = GetChunkPosBiased(rect.BottomRight, true, true, chunkSize) - new VecI(1, 1);
        HashSet<VecI> output = new();
        for (int x = startChunk.X; x <= endChunk.X; x++)
        {
            for (int y = startChunk.Y; y <= endChunk.Y; y++)
            {
                output.Add(new VecI(x, y));
            }
        }
        return output;
    }

    public static HashSet<VecI> FindChunksFullyInsideRectangle(VecD center, VecD size, double angle, int chunkSize)
    {
        if (angle == 0)
            return FindChunksFullyInsideRectangle((RectI)RectD.FromCenterAndSize(center, size).RoundOutwards(), chunkSize);
        if (size.X < chunkSize || size.Y < chunkSize || center.IsNaNOrInfinity() || size.IsNaNOrInfinity() || double.IsNaN(angle) || double.IsInfinity(angle))
            return new HashSet<VecI>();
        if (size.X > chunkSize * 40 * 20 || size.Y > chunkSize * 40 * 20)
            return new HashSet<VecI>();
        // draw a line on the inside of each side
        var corners = FindRectangleCorners(center, size, angle);
        List<VecI>[] lines = new List<VecI>[] {
            FindChunksAlongLine(corners.Item1, corners.Item2, chunkSize),
            FindChunksAlongLine(corners.Item2, corners.Item3, chunkSize),
            FindChunksAlongLine(corners.Item3, corners.Item4, chunkSize),
            FindChunksAlongLine(corners.Item4, corners.Item1, chunkSize)
        };

        var output = FillLines(lines);

        //exclude lines
        for (int i = 0; i < lines.Length; i++)
        {
            output.ExceptWith(lines[i]);
        }

        return output;
    }

    private static void UpdateMinXValues(List<VecI> line, int[] minXValues, int minY)
    {
        for (int i = 0; i < line.Count; i++)
        {
            if (line[i].X < minXValues[line[i].Y - minY])
                minXValues[line[i].Y - minY] = line[i].X;
        }
    }

    private static void UpdateMaxXValues(List<VecI> line, int[] maxXValues, int minY)
    {
        for (int i = 0; i < line.Count; i++)
        {
            if (line[i].X > maxXValues[line[i].Y - minY])
                maxXValues[line[i].Y - minY] = line[i].X;
        }
    }

    /// <summary>
    /// Think of this function as a line drawing algorithm. 
    /// The chosen chunks are guaranteed to be on the left side of the line (assuming y going upwards and looking from p1 towards p2).
    /// This ensures that when you draw a filled shape all updated chunks will be covered (the filled part should go to the right of the line)
    /// No parts of the line will stick out to the left and be left uncovered
    /// </summary>
    public static List<VecI> FindChunksAlongLine(VecD p1, VecD p2, int chunkSize)
    {
        if (p1 == p2 || p1.IsNaNOrInfinity() || p2.IsNaNOrInfinity())
            return new List<VecI>();

        //rotate the line into the first quadrant of the coordinate plane
        int quadrant;
        if (p2.X >= p1.X && p2.Y >= p1.Y)
        {
            quadrant = 1;
        }
        else if (p2.X <= p1.X && p2.Y <= p1.Y)
        {
            quadrant = 3;
            p1 = -p1;
            p2 = -p2;
        }
        else if (p2.X < p1.X)
        {
            quadrant = 2;
            (p1.X, p1.Y) = (p1.Y, -p1.X);
            (p2.X, p2.Y) = (p2.Y, -p2.X);
        }
        else
        {
            quadrant = 4;
            (p1.X, p1.Y) = (-p1.Y, p1.X);
            (p2.X, p2.Y) = (-p2.Y, p2.X);
        }

        List<VecI> output = new();
        //vertical line
        if (p1.X == p2.X)
        {
            //if exactly on a chunk boundary, pick the chunk on the top-left
            VecI start = GetChunkPosBiased(p1, false, true, chunkSize);
            //if exactly on chunk boundary, pick the chunk on the bottom-left
            VecI end = GetChunkPosBiased(p2, false, false, chunkSize);
            for (int y = start.Y; y <= end.Y; y++)
                output.Add(new(start.X, y));
        }
        //horizontal line
        else if (p1.Y == p2.Y)
        {
            //if exactly on a chunk boundary, pick the chunk on the top-right
            VecI start = GetChunkPosBiased(p1, true, true, chunkSize);
            //if exactly on chunk boundary, pick the chunk on the top-left
            VecI end = GetChunkPosBiased(p2, false, true, chunkSize);
            for (int x = start.X; x <= end.X; x++)
                output.Add(new(x, start.Y));
        }
        //all other lines
        else
        {
            //y = mx + b
            double m = (p2.Y - p1.Y) / (p2.X - p1.X);
            double b = p1.Y - (p1.X * m);
            VecI cur = GetChunkPosBiased(p1, true, true, chunkSize);
            output.Add(cur);
            if (LineEq(m, cur.X * chunkSize + chunkSize, b) > cur.Y * chunkSize + chunkSize)
                cur.X--;
            VecI end = GetChunkPosBiased(p2, false, false, chunkSize);
            if (m < 1)
            {
                while (true)
                {
                    if (LineEq(m, cur.X * chunkSize + chunkSize * 2, b) > cur.Y * chunkSize + chunkSize)
                    {
                        cur.X++;
                        cur.Y++;
                    }
                    else
                    {
                        cur.X++;
                    }
                    if (cur.X >= end.X && cur.Y >= end.Y)
                        break;
                    output.Add(cur);
                }
                output.Add(end);
            }
            else
            {
                while (true)
                {
                    if (LineEq(m, cur.X * chunkSize + chunkSize, b) <= cur.Y * chunkSize + chunkSize)
                    {
                        cur.X++;
                        cur.Y++;
                    }
                    else
                    {
                        cur.Y++;
                    }
                    if (cur.X >= end.X && cur.Y >= end.Y)
                        break;
                    output.Add(cur);
                }
                output.Add(end);
            }
        }

        //rotate output back
        if (quadrant == 1)
            return output;
        if (quadrant == 3)
        {
            for (int i = 0; i < output.Count; i++)
                output[i] = new(-output[i].X - 1, -output[i].Y - 1);
            return output;
        }
        if (quadrant == 2)
        {
            for (int i = 0; i < output.Count; i++)
                output[i] = new(-output[i].Y - 1, output[i].X);
            return output;
        }
        for (int i = 0; i < output.Count; i++)
            output[i] = new(output[i].Y, -output[i].X - 1);
        return output;
    }

    private static double LineEq(double m, double x, double b)
    {
        return m * x + b;
    }

    /// <summary>
    /// "Bias" specifies how to handle whole values. This function behaves the same as GetChunkPos for fractional values.
    /// Examples if you pass (0, 0):
    /// If both positiveX and positiveY are true it behaves like GetChunkPos, you get chunk (0, 0)
    /// If both are false you'll get (-1, -1), because the right and bottom boundaries are now considered to be part of the chunk, and top and left aren't.
    /// </summary>
    public static VecI GetChunkPosBiased(VecD pos, bool positiveX, bool positiveY, int chunkSize)
    {
        pos /= chunkSize;
        return new VecI()
        {
            X = positiveX ? (int)Math.Floor(pos.X) : (int)Math.Ceiling(pos.X) - 1,
            Y = positiveY ? (int)Math.Floor(pos.Y) : (int)Math.Ceiling(pos.Y) - 1,
        };
    }

    /// <summary>
    /// Returns corners in ccw direction (assuming y points up)
    /// </summary>
    private static (VecD, VecD, VecD, VecD) FindRectangleCorners(VecD center, VecD size, double angle)
    {
        VecD right = VecD.FromAngleAndLength(angle, size.X / 2);
        VecD up = VecD.FromAngleAndLength(angle + Math.PI / 2, size.Y / 2);
        return (
            center + right + up,
            center - right + up,
            center - right - up,
            center + right - up
            );
    }
}
