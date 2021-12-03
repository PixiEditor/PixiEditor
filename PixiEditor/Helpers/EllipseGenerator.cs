using PixiEditor.Models.Position;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixiEditor.Helpers
{
    internal static class EllipseGenerator
    {

        public static List<Coordinates> GenerateEllipseFromRect(DoubleCoords rect)
        {
            float radiusX = (rect.Coords2.X - rect.Coords1.X) / 2.0f;
            float radiusY = (rect.Coords2.Y - rect.Coords1.Y) / 2.0f;
            float centerX = (rect.Coords1.X + rect.Coords2.X + 1) / 2.0f;
            float centerY = (rect.Coords1.Y + rect.Coords2.Y + 1) / 2.0f;

            return GenerateMidpointEllipse(radiusX, radiusY, centerX, centerY);
        }

        /// <summary>
        /// Draws an ellipse using it's center and radii
        ///
        /// Here is a usage example:
        /// Let's say you want an ellipse that's 3 pixels wide and 3 pixels tall located in the top right corner of the canvas
        /// It's center is at (1.5; 1.5). That's in the middle of a pixel
        /// The radii are both equal to 1. Notice that it's 1 and not 1.5, since we want the ellipse to land in the middle of the pixel, not outside of it.
        /// See desmos (note the inverted y axis): https://www.desmos.com/calculator/tq9uqg0hcq
        ///
        /// Another example:
        /// 4x4 ellipse in the top right corner of the canvas
        /// Center is at (2; 2). It's a place where 4 pixels meet
        /// Both radii are 1.5. Making them 2 would make the ellipse touch the edges of pixels, whereas we want it to stay in the middle
        /// </summary>
        public static List<Coordinates> GenerateMidpointEllipse(double halfWidth, double halfHeight, double centerX, double centerY)
        {
            if (halfWidth < 1 || halfHeight < 1)
            {
                return GenerateFallbackRectangle(halfWidth, halfHeight, centerX, centerY);
            }

            // ellipse formula: halfHeight^2 * x^2 + halfWidth^2 * y^2 - halfHeight^2 * halfWidth^2 = 0

            // Make sure we are always at the center of a pixel
            double currentX = Math.Ceiling(centerX - 0.5) + 0.5;
            double currentY = centerY + halfHeight;

            List<Coordinates> outputCoordinates = new List<Coordinates>();

            double currentSlope;

            // from PI/2 to PI/4
            do
            {
                AddRegionPoints(outputCoordinates, currentX, centerX, currentY, centerY);

                // calculate next pixel coords
                currentX++;

                if ((Math.Pow(halfHeight, 2) * Math.Pow(currentX - centerX, 2)) +
                    (Math.Pow(halfWidth, 2) * Math.Pow(currentY - centerY - 0.5, 2)) -
                    (Math.Pow(halfWidth, 2) * Math.Pow(halfHeight, 2)) >= 0)
                {
                    currentY--;
                }

                // calculate how far we've advanced
                double derivativeX = 2 * Math.Pow(halfHeight, 2) * (currentX - centerX);
                double derivativeY = 2 * Math.Pow(halfWidth, 2) * (currentY - centerY);
                currentSlope = -(derivativeX / derivativeY);
            }
            while (currentSlope > -1 && currentY - centerY > 0.5);

            // from PI/4 to 0
            while (currentY - centerY >= 0)
            {
                AddRegionPoints(outputCoordinates, currentX, centerX, currentY, centerY);

                currentY--;
                if ((Math.Pow(halfHeight, 2) * Math.Pow(currentX - centerX + 0.5, 2)) +
                    (Math.Pow(halfWidth, 2) * Math.Pow(currentY - centerY, 2)) -
                    (Math.Pow(halfWidth, 2) * Math.Pow(halfHeight, 2)) < 0)
                {
                    currentX++;
                }
            }

            return outputCoordinates;
        }

        private static List<Coordinates> GenerateFallbackRectangle(double halfWidth, double halfHeight, double centerX, double centerY)
        {
            List<Coordinates> coordinates = new List<Coordinates>();

            int left = (int)Math.Floor(centerX - halfWidth);
            int top = (int)Math.Floor(centerY - halfHeight);
            int right = (int)Math.Floor(centerX + halfWidth);
            int bottom = (int)Math.Floor(centerY + halfHeight);

            for (int x = left; x <= right; x++)
            {
                coordinates.Add(new Coordinates(x, top));
                coordinates.Add(new Coordinates(x, bottom));
            }

            for (int y = top; y <= bottom; y++)
            {
                coordinates.Add(new Coordinates(left, y));
                coordinates.Add(new Coordinates(right, y));
            }

            return coordinates;
        }

        private static void AddRegionPoints(List<Coordinates> coordinates, double x, double xc, double y, double yc)
        {
            int xFloor = (int)Math.Floor(x);
            int yFloor = (int)Math.Floor(y);
            int xFloorInv = (int)Math.Floor(-x + 2 * xc);
            int yFloorInv = (int)Math.Floor(-y + 2 * yc);

            //top and bottom or left and right
            if (xFloor == xFloorInv || yFloor == yFloorInv)
            {
                coordinates.Add(new Coordinates(xFloor, yFloor));
                coordinates.Add(new Coordinates(xFloorInv, yFloorInv));
            }
            //part of the arc
            else
            {
                coordinates.Add(new Coordinates(xFloor, yFloor));
                coordinates.Add(new Coordinates(xFloorInv, yFloorInv));
                coordinates.Add(new Coordinates(xFloorInv, yFloor));
                coordinates.Add(new Coordinates(xFloor, yFloorInv));
            }
        }
    }
}
