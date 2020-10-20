using PixiEditor.Models.Position;

namespace PixiEditor.Models.ImageManipulation
{
    public static class Transform
    {
        /// <summary>
        ///     Returns translation between two coordinates.
        /// </summary>
        /// <param name="from">Starting coordinate</param>
        /// <param name="to">New coordinate</param>
        /// <returns>Translation as coordinate</returns>
        public static Coordinates GetTranslation(Coordinates from, Coordinates to)
        {
            int translationX = to.X - from.X;
            int translationY = to.Y - from.Y;
            return new Coordinates(translationX, translationY);
        }

        public static Coordinates[] Translate(Coordinates[] points, Coordinates vector)
        {
            Coordinates[] translatedPoints = new Coordinates[points.Length];
            for (int i = 0; i < translatedPoints.Length; i++)
            {
                translatedPoints[i] = new Coordinates(points[i].X + vector.X, points[i].Y + vector.Y);
            }

            return translatedPoints;
        }
    }
}