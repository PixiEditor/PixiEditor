using PixiEditor.Models.Position;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixiEditor.Models.ImageManipulation
{
    public static class ShapeCalculator
    {
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
