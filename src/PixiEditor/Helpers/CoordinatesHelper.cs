using PixiEditor.Models.Position;
using System;
using System.Collections.Generic;

namespace PixiEditor.Helpers
{
    internal class CoordinatesHelper
    {
        public static (Coordinates, Coordinates) GetSquareOrLineCoordinates(IReadOnlyList<Coordinates> coords)
        {
            if (DoCoordsFormLine(coords))
            {
                return GetLineCoordinates(coords);
            }
            return GetSquareCoordiantes(coords);
        }

        private static bool DoCoordsFormLine(IReadOnlyList<Coordinates> coords)
        {
            var p1 = coords[0];
            var p2 = coords[^1];
            //find delta and mirror to first quadrant
            float dX = Math.Abs(p2.X - p1.X);
            float dY = Math.Abs(p2.Y - p1.Y);

            //normalize
            float length = (float)Math.Sqrt(dX * dX + dY * dY);
            if (length == 0)
                return false;
            dX = dX / length;
            dY = dY / length;

            return dX < 0.25f || dY < 0.25f; //angle < 15 deg or angle > 75 deg (sin 15 ~= 0.25)
        }

        public static (Coordinates, Coordinates) GetLineCoordinates(IReadOnlyList<Coordinates> mouseMoveCords)
        {
            int xStart = mouseMoveCords[0].X;
            int yStart = mouseMoveCords[0].Y;

            int xEnd = mouseMoveCords[^1].X;
            int yEnd = mouseMoveCords[^1].Y;


            if (Math.Abs(xStart - xEnd) > Math.Abs(yStart - yEnd))
            {
                yEnd = yStart;
            }
            else
            {
                xEnd = xStart;
            }
            return (new(xStart, yStart), new(xEnd, yEnd));
        }

        /// <summary>
        ///     Extracts square from rectangle mouse drag, used to draw symmetric shapes.
        /// </summary>
        public static (Coordinates, Coordinates) GetSquareCoordiantes(IReadOnlyList<Coordinates> mouseMoveCords)
        {
            var end = mouseMoveCords[^1];
            var start = mouseMoveCords[0];

            //find delta and mirror to first quadrant
            var dX = Math.Abs(start.X - end.X);
            var dY = Math.Abs(start.Y - end.Y);

            float sqrt2 = (float)Math.Sqrt(2);
            //vector of length 1 at 45 degrees;
            float diagX, diagY;
            diagX = diagY = 1 / sqrt2;

            //dot product of delta and diag, returns length of [delta projected onto diag]
            float projectedLength = diagX * dX + diagY * dY;
            //project above onto axes
            float axisLength = projectedLength / sqrt2;

            //final coords
            float x = -Math.Sign(start.X - end.X) * axisLength;
            float y = -Math.Sign(start.Y - end.Y) * axisLength;
            end = new Coordinates((int)x + start.X, (int)y + start.Y);
            return (start, end);
        }
    }
}
