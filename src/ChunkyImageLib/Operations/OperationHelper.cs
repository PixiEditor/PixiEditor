using ChunkyImageLib.DataHolders;

namespace ChunkyImageLib.Operations
{
    public static class OperationHelper
    {
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
        public static HashSet<Vector2i> FindChunksTouchingRectangle(Vector2d center, Vector2d size, float angle, int chunkSize)
        {
            var corners = FindRectangleCorners(center, size, angle);
            double minX = Math.Min(Math.Min(corners.Item1.X, corners.Item2.X), Math.Min(corners.Item3.X, corners.Item4.X));
            double maxX = Math.Max(Math.Max(corners.Item1.X, corners.Item2.X), Math.Max(corners.Item3.X, corners.Item4.X));
            double minY = Math.Min(Math.Min(corners.Item1.Y, corners.Item2.Y), Math.Min(corners.Item3.Y, corners.Item4.Y));
            double maxY = Math.Max(Math.Max(corners.Item1.Y, corners.Item2.Y), Math.Max(corners.Item3.Y, corners.Item4.Y));

            //(int leftChunkX, int leftChunkY) = GetChunkPos(minX, )
            throw new NotImplementedException();
        }


        private static (Vector2d, Vector2d, Vector2d, Vector2d) FindRectangleCorners(Vector2d center, Vector2d size, float angle)
        {
            Vector2d right = Vector2d.FromAngleAndLength(angle, size.X / 2);
            Vector2d up = Vector2d.FromAngleAndLength(angle + Math.PI / 2, size.Y / 2);
            return (
                center + right + up,
                center - right + up,
                center + right - up,
                center - right - up
                );
        }
    }
}
