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
    public static SKMatrix CreateMatrixFromPoints(SKPoint topLeft, SKPoint topRight, SKPoint botRight, SKPoint botLeft, float width, float height)
    {
        (float a, float b) = (topLeft.X, topLeft.Y);
        (float c, float d) = (topRight.X, topRight.Y);
        (float e, float f) = (botRight.X, botRight.Y);
        (float g, float h) = (botLeft.X, botLeft.Y);
        (float w_1, float h_1) = (width, height);

        float scaleX = (b * c * g - a * d * g + a * f * g - c * f * g - b * c * e + a * d * e - a * h * e + c * h * e) / (c * f * w_1 + d * g * w_1 - f * g * w_1 - c * h * w_1 - d * w_1 * e + h * w_1 * e);
        float skewX = (-a * c * f - b * c * g + c * f * g + a * c * h + a * d * e + b * g * e - d * g * e - a * h * e) / (c * f * h_1 + d * g * h_1 - f * g * h_1 - c * h * h_1 - d * h_1 * e + h * h_1 * e);
        float transX = a;
        float skewY = (-b * c * f + a * d * f + b * f * g - d * f * g + b * c * h - a * d * h - b * h * e + d * h * e) / (c * f * w_1 + d * g * w_1 - f * g * w_1 - c * h * w_1 - d * w_1 * e + h * w_1 * e);
        float scaleY = (-b * c * f - b * d * g + b * f * g + a * d * h - a * f * h + c * f * h + b * d * e - d * h * e) / (c * f * h_1 + d * g * h_1 - f * g * h_1 - c * h * h_1 - d * h_1 * e + h * h_1 * e);
        float transY = b;
        float persp0 = (a * f - c * f + b * g - d * g - a * h + c * h - b * e + d * e) / (c * f * w_1 + d * g * w_1 - f * g * w_1 - c * h * w_1 - d * w_1 * e + h * w_1 * e);
        float persp1 = (-b * c + a * d - a * f - d * g + f * g + c * h + b * e - h * e) / (c * f * h_1 + d * g * h_1 - f * g * h_1 - c * h * h_1 - d * h_1 * e + h * h_1 * e);
        float persp2 = 1;

        return new SKMatrix(scaleX, skewX, transX, skewY, scaleY, transY, persp0, persp1, persp2);
    }

    public static HashSet<VecI> FindChunksTouchingQuadrilateral(ShapeCorners corners, int chunkSize)
    {
        if (corners.HasNaNOrInfinity ||
            (corners.BottomLeft - corners.TopRight).Length > chunkSize * 40 * 20 ||
            (corners.TopLeft - corners.BottomRight).Length > chunkSize * 40 * 20)
            return new HashSet<VecI>();
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
