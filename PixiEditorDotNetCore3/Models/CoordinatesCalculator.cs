using System;
using System.Collections.Generic;
using System.Text;

namespace PixiEditorDotNetCore3.Models
{
    public static class CoordinatesCalculator
    {
        /// <summary>
        /// Calculates center of thickness * thickness rectangle
        /// </summary>
        /// <param name="startPosition">Top left position of rectangle</param>
        /// <param name="thickness">Thickness of rectangle</param>
        /// <returns></returns>
        public static DoubleCords CalculateThicknessCenter(Coordinates startPosition, int thickness)
        {
            int x1, x2, y1, y2;
            if (thickness % 2 == 0)
            {
                x2 = startPosition.X + thickness / 2;
                y2 = startPosition.Y + thickness / 2;
                x1 = x2 - thickness;
                y1 = y2 - thickness;
            }
            else
            {
                x2 = startPosition.X + (((thickness - 1) / 2) + 1);
                y2 = startPosition.Y + (((thickness - 1) / 2) + 1);
                x1 = x2 - thickness;
                y1 = y2 - thickness;
            }
            return new DoubleCords(new Coordinates(x1, y1), new Coordinates(x2, y2));
        }

        public static Coordinates[] RectangleToCoordinates(int x1, int y1, int x2, int y2)
        {
            List<Coordinates> coordinates = new List<Coordinates>();
            for (int y = y1; y < y1 + (y2 - y1); y++)
            {
                for (int x = x1; x < x1 + (x2 - x1); x++)
                {
                    coordinates.Add(new Coordinates(x, y));
                }
            }
            return coordinates.ToArray();
        }
    }
}