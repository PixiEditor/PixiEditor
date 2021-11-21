using PixiEditor.Models.Position;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PixiEditor.Models.ImageManipulation
{
    public static class ShapeCalculator
    {
        public static void GenerateEllipseNonAlloc(Coordinates start, Coordinates end, bool fill,
            List<Coordinates> output)
        {

            DoubleCords fixedCoordinates = CalculateCoordinatesForShapeRotation(start, end);

            CreateEllipse(fixedCoordinates.Coords1, fixedCoordinates.Coords2, output);

            if(fill)
            {
                CalculateFillForEllipse(output);
            }
        }

        public static void GenerateRectangleNonAlloc(
            Coordinates start,
            Coordinates end, bool fill, int thickness, List<Coordinates> output)
        {
            DoubleCords fixedCoordinates = CalculateCoordinatesForShapeRotation(start, end);
            CalculateRectanglePoints(fixedCoordinates, output);

            for (int i = 1; i < (int)Math.Floor(thickness / 2f) + 1; i++)
            {
                CalculateRectanglePoints(
                    new DoubleCords(
                    new Coordinates(fixedCoordinates.Coords1.X - i, fixedCoordinates.Coords1.Y - i),
                    new Coordinates(fixedCoordinates.Coords2.X + i, fixedCoordinates.Coords2.Y + i)), output);
            }

            for (int i = 1; i < (int)Math.Ceiling(thickness / 2f); i++)
            {
                CalculateRectanglePoints(
                    new DoubleCords(
                    new Coordinates(fixedCoordinates.Coords1.X + i, fixedCoordinates.Coords1.Y + i),
                    new Coordinates(fixedCoordinates.Coords2.X - i, fixedCoordinates.Coords2.Y - i)), output);
            }

            if (fill)
            {
                CalculateRectangleFillNonAlloc(start, end, thickness, output);
            }
        }

        public static DoubleCords CalculateCoordinatesForShapeRotation(
            Coordinates startingCords,
            Coordinates secondCoordinates)
        {
            Coordinates currentCoordinates = secondCoordinates;

            if (startingCords.X > currentCoordinates.X && startingCords.Y > currentCoordinates.Y)
            {
                return new DoubleCords(
                    new Coordinates(currentCoordinates.X, currentCoordinates.Y),
                    new Coordinates(startingCords.X, startingCords.Y));
            }

            if (startingCords.X < currentCoordinates.X && startingCords.Y < currentCoordinates.Y)
            {
                return new DoubleCords(
                    new Coordinates(startingCords.X, startingCords.Y),
                    new Coordinates(currentCoordinates.X, currentCoordinates.Y));
            }

            if (startingCords.Y > currentCoordinates.Y)
            {
                return new DoubleCords(
                    new Coordinates(startingCords.X, currentCoordinates.Y),
                    new Coordinates(currentCoordinates.X, startingCords.Y));
            }

            if (startingCords.X > currentCoordinates.X && startingCords.Y <= currentCoordinates.Y)
            {
                return new DoubleCords(
                    new Coordinates(currentCoordinates.X, startingCords.Y),
                    new Coordinates(startingCords.X, currentCoordinates.Y));
            }

            return new DoubleCords(startingCords, secondCoordinates);
        }

        private static void CalculateFillForEllipse(List<Coordinates> outlineCoordinates)
        {
            if (!outlineCoordinates.Any())
            {
                return;
            }

            int bottom = outlineCoordinates.Max(x => x.Y);
            int top = outlineCoordinates.Min(x => x.Y);
            for (int i = top + 1; i < bottom; i++)
            {
                IEnumerable<Coordinates> rowCords = outlineCoordinates.Where(x => x.Y == i);
                int right = rowCords.Max(x => x.X);
                int left = rowCords.Min(x => x.X);
                for (int j = left + 1; j < right; j++)
                {
                    outlineCoordinates.Add(new Coordinates(j, i));
                }
            }
        }


        /// <summary>
        ///     Calculates ellipse points for specified coordinates and thickness.
        /// </summary>
        /// <param name="startCoordinates">Top left coordinate of ellipse.</param>
        /// <param name="endCoordinates">Bottom right coordinate of ellipse.</param>
        private static void CreateEllipse(Coordinates startCoordinates, Coordinates endCoordinates, List<Coordinates> output)
        {
            double radiusX = (endCoordinates.X - startCoordinates.X) / 2.0;
            double radiusY = (endCoordinates.Y - startCoordinates.Y) / 2.0;
            double centerX = (startCoordinates.X + endCoordinates.X + 1) / 2.0;
            double centerY = (startCoordinates.Y + endCoordinates.Y + 1) / 2.0;

            MidpointEllipse(radiusX, radiusY, centerX, centerY, output);
        }

        private static void MidpointEllipse(double halfWidth, double halfHeight, double centerX, double centerY, List<Coordinates> output)
        {
            if (halfWidth < 1 || halfHeight < 1)
            {
                FallbackRectangle(halfWidth, halfHeight, centerX, centerY, output);
            }

            // ellipse formula: halfHeight^2 * x^2 + halfWidth^2 * y^2 - halfHeight^2 * halfWidth^2 = 0

            // Make sure we are always at the center of a pixel
            double currentX = Math.Ceiling(centerX - 0.5) + 0.5;
            double currentY = centerY + halfHeight;

            double currentSlope;

            // from PI/2 to middle
            do
            {
                GetRegionPoints(currentX, centerX, currentY, centerY, output);

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

            // from middle to 0
            while (currentY - centerY >= 0)
            {
                GetRegionPoints(currentX, centerX, currentY, centerY, output);

                currentY--;
                if ((Math.Pow(halfHeight, 2) * Math.Pow(currentX - centerX + 0.5, 2)) +
                    (Math.Pow(halfWidth, 2) * Math.Pow(currentY - centerY, 2)) -
                    (Math.Pow(halfWidth, 2) * Math.Pow(halfHeight, 2)) < 0)
                {
                    currentX++;
                }
            }
        }

        private static void FallbackRectangle(double halfWidth, double halfHeight, double centerX, double centerY, List<Coordinates> output)
        {
            for (double x = centerX - halfWidth; x <= centerX + halfWidth; x++)
            {
                output.Add(new Coordinates((int)x, (int)(centerY - halfHeight)));
                output.Add(new Coordinates((int)x, (int)(centerY + halfHeight)));
            }

            for (double y = centerY - halfHeight + 1; y <= centerY + halfHeight - 1; y++)
            {
                output.Add(new Coordinates((int)(centerX - halfWidth), (int)y));
                output.Add(new Coordinates((int)(centerX + halfWidth), (int)y));
            }
        }

        private static void GetRegionPoints(double x, double xc, double y, double yc, List<Coordinates> output)
        {
            output.Add(new Coordinates((int)Math.Floor(x), (int)Math.Floor(y)));
            output.Add(new Coordinates((int)Math.Floor(-(x - xc) + xc), (int)Math.Floor(y)));
            output.Add(new Coordinates((int)Math.Floor(x), (int)Math.Floor(-(y - yc) + yc)));
            output.Add(new Coordinates((int)Math.Floor(-(x - xc) + xc), (int)Math.Floor(-(y - yc) + yc)));
        }

        private static void CalculateRectangleFillNonAlloc(Coordinates start, Coordinates end, int thickness, List<Coordinates> output)
        {
            int offset = (int)Math.Ceiling(thickness / 2f);
            DoubleCords fixedCords = CalculateCoordinatesForShapeRotation(start, end);

            DoubleCords innerCords = new DoubleCords
            {
                Coords1 = new Coordinates(fixedCords.Coords1.X + offset, fixedCords.Coords1.Y + offset),
                Coords2 = new Coordinates(fixedCords.Coords2.X - (offset - 1), fixedCords.Coords2.Y - (offset - 1))
            };

            int height = innerCords.Coords2.Y - innerCords.Coords1.Y;
            int width = innerCords.Coords2.X - innerCords.Coords1.X;

            if (height < 1 || width < 1)
            {
                return;
            }

            int i = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    output.Add(new Coordinates(innerCords.Coords1.X + x, innerCords.Coords1.Y + y));
                    i++;
                }
            }
        }

        private static void CalculateRectanglePoints(DoubleCords coordinates, List<Coordinates> output)
        {
            for (int i = coordinates.Coords1.X; i < coordinates.Coords2.X + 1; i++)
            {
                output.Add(new Coordinates(i, coordinates.Coords1.Y));
                output.Add(new Coordinates(i, coordinates.Coords2.Y));
            }

            for (int i = coordinates.Coords1.Y + 1; i <= coordinates.Coords2.Y - 1; i++)
            {
                output.Add(new Coordinates(coordinates.Coords1.X, i));
                output.Add(new Coordinates(coordinates.Coords2.X, i));
            }
        }
    }
}
