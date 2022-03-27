using ChunkyImageLib.DataHolders;

namespace ChunkyImageLib.Operations
{
    public static class OperationHelper
    {
        public static Vector2i ConvertForResolution(Vector2i pixelPos, ChunkResolution resolution)
        {
            var mult = resolution.Multiplier();
            return new((int)Math.Round(pixelPos.X * mult), (int)Math.Round(pixelPos.Y * mult));
        }

        public static Vector2i GetChunkPos(Vector2i pixelPos, int chunkSize)
        {
            return new Vector2i()
            {
                X = (int)MathF.Floor(pixelPos.X / (float)chunkSize),
                Y = (int)MathF.Floor(pixelPos.Y / (float)chunkSize)
            };
        }

        /// <summary>
        /// Finds chunks that at least partially lie inside of a rectangle
        /// </summary>
        public static HashSet<Vector2i> FindChunksTouchingRectangle(Vector2d center, Vector2d size, double angle, int chunkSize)
        {
            if (size.X == 0 || size.Y == 0)
                return new HashSet<Vector2i>();
            // draw a line on the outside of each side
            var corners = FindRectangleCorners(center, size, angle);
            List<Vector2i>[] lines = new List<Vector2i>[] {
                FindChunksAlongLine(corners.Item2, corners.Item1, chunkSize),
                FindChunksAlongLine(corners.Item3, corners.Item2, chunkSize),
                FindChunksAlongLine(corners.Item4, corners.Item3, chunkSize),
                FindChunksAlongLine(corners.Item1, corners.Item4, chunkSize)
            };

            //find min and max X for each Y in lines
            var ySel = (Vector2i vec) => vec.Y;
            int minY = Math.Min(lines[0].Min(ySel), lines[2].Min(ySel));
            int maxY = Math.Max(lines[0].Max(ySel), lines[2].Max(ySel));

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
            HashSet<Vector2i> output = new();
            for (int i = 0; i < minXValues.Length; i++)
            {
                int minX = minXValues[i];
                int maxX = maxXValues[i];
                for (int x = minX; x <= maxX; x++)
                    output.Add(new(x, i + minY));
            }

            return output;
        }

        public static HashSet<Vector2i> FindChunksFullyInsideRectangle(Vector2i pos, Vector2i size)
        {
            Vector2i startChunk = GetChunkPos(pos, ChunkPool.FullChunkSize);
            Vector2i endChunk = GetChunkPosBiased(pos + size, false, false, ChunkPool.FullChunkSize);
            HashSet<Vector2i> output = new();
            for (int x = startChunk.X; x <= endChunk.X; x++)
            {
                for (int y = startChunk.Y; y <= endChunk.Y; y++)
                {
                    output.Add(new Vector2i(x, y));
                }
            }
            return output;
        }

        public static HashSet<Vector2i> FindChunksFullyInsideRectangle(Vector2d center, Vector2d size, double angle, int chunkSize)
        {
            if (size.X < chunkSize || size.Y < chunkSize)
                return new HashSet<Vector2i>();
            // draw a line on the inside of each side
            var corners = FindRectangleCorners(center, size, angle);
            List<Vector2i>[] lines = new List<Vector2i>[] {
                FindChunksAlongLine(corners.Item1, corners.Item2, chunkSize),
                FindChunksAlongLine(corners.Item2, corners.Item3, chunkSize),
                FindChunksAlongLine(corners.Item3, corners.Item4, chunkSize),
                FindChunksAlongLine(corners.Item4, corners.Item1, chunkSize)
            };

            //find min and max X for each Y in lines
            var ySel = (Vector2i vec) => vec.Y;
            int minY = Math.Min(lines[0].Min(ySel), lines[2].Min(ySel));
            int maxY = Math.Max(lines[0].Max(ySel), lines[2].Max(ySel));

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

            //draw a line from min X to max X for each Y and exclude lines
            HashSet<Vector2i> output = new();
            for (int i = 0; i < minXValues.Length; i++)
            {
                int minX = minXValues[i];
                int maxX = maxXValues[i];
                for (int x = minX; x <= maxX; x++)
                    output.Add(new(x, i + minY));
            }
            for (int i = 0; i < lines.Length; i++)
            {
                output.ExceptWith(lines[i]);
            }

            return output;
        }

        private static void UpdateMinXValues(List<Vector2i> line, int[] minXValues, int minY)
        {
            for (int i = 0; i < line.Count; i++)
            {
                if (line[i].X < minXValues[line[i].Y - minY])
                    minXValues[line[i].Y - minY] = line[i].X;
            }
        }

        private static void UpdateMaxXValues(List<Vector2i> line, int[] maxXValues, int minY)
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
        public static List<Vector2i> FindChunksAlongLine(Vector2d p1, Vector2d p2, int chunkSize)
        {
            if (p1 == p2)
                return new List<Vector2i>();

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

            List<Vector2i> output = new();
            //vertical line
            if (p1.X == p2.X)
            {
                //if exactly on a chunk boundary, pick the chunk on the top-left
                Vector2i start = GetChunkPosBiased(p1, false, true, chunkSize);
                //if exactly on chunk boundary, pick the chunk on the bottom-left
                Vector2i end = GetChunkPosBiased(p2, false, false, chunkSize);
                for (int y = start.Y; y <= end.Y; y++)
                    output.Add(new(start.X, y));
            }
            //horizontal line
            else if (p1.Y == p2.Y)
            {
                //if exactly on a chunk boundary, pick the chunk on the top-right
                Vector2i start = GetChunkPosBiased(p1, true, true, chunkSize);
                //if exactly on chunk boundary, pick the chunk on the top-left
                Vector2i end = GetChunkPosBiased(p2, false, true, chunkSize);
                for (int x = start.X; x <= end.X; x++)
                    output.Add(new(x, start.Y));
            }
            //all other lines
            else
            {
                //y = mx + b
                double m = (p2.Y - p1.Y) / (p2.X - p1.X);
                double b = p1.Y - (p1.X * m);
                Vector2i cur = GetChunkPosBiased(p1, true, true, chunkSize);
                output.Add(cur);
                if (LineEq(m, cur.X * chunkSize + chunkSize, b) > cur.Y * chunkSize + chunkSize)
                    cur.X--;
                Vector2i end = GetChunkPosBiased(p2, false, false, chunkSize);
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

        public static Vector2i GetChunkPosBiased(Vector2d pos, bool positiveX, bool positiveY, int chunkSize)
        {
            pos /= chunkSize;
            return new Vector2i()
            {
                X = positiveX ? (int)Math.Floor(pos.X) : (int)Math.Ceiling(pos.X) - 1,
                Y = positiveY ? (int)Math.Floor(pos.Y) : (int)Math.Ceiling(pos.Y) - 1,
            };
        }

        /// <summary>
        /// Returns corners in ccw direction (assuming y points up)
        /// </summary>
        private static (Vector2d, Vector2d, Vector2d, Vector2d) FindRectangleCorners(Vector2d center, Vector2d size, double angle)
        {
            Vector2d right = Vector2d.FromAngleAndLength(angle, size.X / 2);
            Vector2d up = Vector2d.FromAngleAndLength(angle + Math.PI / 2, size.Y / 2);
            return (
                center + right + up,
                center - right + up,
                center - right - up,
                center + right - up
                );
        }
    }
}
