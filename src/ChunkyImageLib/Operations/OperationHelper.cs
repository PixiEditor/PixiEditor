using ChunkyImageLib.DataHolders;
using SkiaSharp;

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
    public unsafe static void ClampAlpha(SKSurface toModify, SKSurface toGetAlphaFrom)
    {
        using (var map = toModify.PeekPixels())
        {
            using (var refMap = toGetAlphaFrom.PeekPixels())
            {
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
                        *offset = ((long)*(ushort*)(&newR)) | ((long)*(ushort*)(&newG)) << 16 | ((long)*(ushort*)(&newB)) << 32 | ((long)*(ushort*)(refAlpha)) << 48;
                    }
                }
            }
        }
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

    public static SKMatrix CreateMatrixFromPoints(ShapeCorners corners, VecD size)
        => CreateMatrixFromPoints((SKPoint)corners.TopLeft, (SKPoint)corners.TopRight, (SKPoint)corners.BottomRight, (SKPoint)corners.BottomLeft, (float)size.X, (float)size.Y);

    // see https://stackoverflow.com/questions/48416118/perspective-transform-in-skia/72364829#72364829
    public static SKMatrix CreateMatrixFromPoints(SKPoint topLeft, SKPoint topRight, SKPoint botRight, SKPoint botLeft, float width, float height)
    {
        (float x1, float y1) = (topLeft.X, topLeft.Y);
        (float x2, float y2) = (topRight.X, topRight.Y);
        (float x3, float y3) = (botRight.X, botRight.Y);
        (float x4, float y4) = (botLeft.X, botLeft.Y);
        (float w, float h) = (width, height);

        float scaleX = (y1 * x2 * x4 - x1 * y2 * x4 + x1 * y3 * x4 - x2 * y3 * x4 - y1 * x2 * x3 + x1 * y2 * x3 - x1 * y4 * x3 + x2 * y4 * x3) / (x2 * y3 * w + y2 * x4 * w - y3 * x4 * w - x2 * y4 * w - y2 * w * x3 + y4 * w * x3);
        float skewX = (-x1 * x2 * y3 - y1 * x2 * x4 + x2 * y3 * x4 + x1 * x2 * y4 + x1 * y2 * x3 + y1 * x4 * x3 - y2 * x4 * x3 - x1 * y4 * x3) / (x2 * y3 * h + y2 * x4 * h - y3 * x4 * h - x2 * y4 * h - y2 * h * x3 + y4 * h * x3);
        float transX = x1;
        float skewY = (-y1 * x2 * y3 + x1 * y2 * y3 + y1 * y3 * x4 - y2 * y3 * x4 + y1 * x2 * y4 - x1 * y2 * y4 - y1 * y4 * x3 + y2 * y4 * x3) / (x2 * y3 * w + y2 * x4 * w - y3 * x4 * w - x2 * y4 * w - y2 * w * x3 + y4 * w * x3);
        float scaleY = (-y1 * x2 * y3 - y1 * y2 * x4 + y1 * y3 * x4 + x1 * y2 * y4 - x1 * y3 * y4 + x2 * y3 * y4 + y1 * y2 * x3 - y2 * y4 * x3) / (x2 * y3 * h + y2 * x4 * h - y3 * x4 * h - x2 * y4 * h - y2 * h * x3 + y4 * h * x3);
        float transY = y1;
        float persp0 = (x1 * y3 - x2 * y3 + y1 * x4 - y2 * x4 - x1 * y4 + x2 * y4 - y1 * x3 + y2 * x3) / (x2 * y3 * w + y2 * x4 * w - y3 * x4 * w - x2 * y4 * w - y2 * w * x3 + y4 * w * x3);
        float persp1 = (-y1 * x2 + x1 * y2 - x1 * y3 - y2 * x4 + y3 * x4 + x2 * y4 + y1 * x3 - y4 * x3) / (x2 * y3 * h + y2 * x4 * h - y3 * x4 * h - x2 * y4 * h - y2 * h * x3 + y4 * h * x3);
        float persp2 = 1;

        return new SKMatrix(scaleX, skewX, transX, skewY, scaleY, transY, persp0, persp1, persp2);
    }

    public static HashSet<VecI> FindChunksTouchingQuadrilateral(ShapeCorners corners, int chunkSize)
    {
        if (corners.HasNaNOrInfinity ||
            (corners.BottomLeft - corners.TopRight).Length > chunkSize * 40 * 20 ||
            (corners.TopLeft - corners.BottomRight).Length > chunkSize * 40 * 20)
            return new HashSet<VecI>();
        if (corners.IsInverted)
            corners = corners with { BottomLeft = corners.TopRight, TopRight = corners.BottomLeft };
        List<VecI>[] lines = new List<VecI>[] {
            FindChunksAlongLine(corners.TopRight, corners.TopLeft, chunkSize),
            FindChunksAlongLine(corners.BottomRight, corners.TopRight, chunkSize),
            FindChunksAlongLine(corners.BottomLeft, corners.BottomRight, chunkSize),
            FindChunksAlongLine(corners.TopLeft, corners.BottomLeft, chunkSize)
        };
        return FillLines(lines);
    }

    public static HashSet<VecI> FindChunksFullyInsideQuadrilateral(ShapeCorners corners, int chunkSize)
    {
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

    public static HashSet<VecI> FindChunksTouchingRectangle(VecI topLeft, VecI size, int chunkSize)
    {
        VecI min = GetChunkPos(topLeft, chunkSize);
        VecI max = GetChunkPosBiased(topLeft + size, false, false, chunkSize);
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
        if (lines.Length == 0)
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

    public static HashSet<VecI> FindChunksFullyInsideRectangle(VecI pos, VecI size, int chunkSize)
    {
        if (size.X > chunkSize * 40 * 20 || size.Y > chunkSize * 40 * 20)
            return new HashSet<VecI>();
        VecI startChunk = GetChunkPos(pos, ChunkPool.FullChunkSize);
        VecI endChunk = GetChunkPosBiased(pos + size, false, false, chunkSize);
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
