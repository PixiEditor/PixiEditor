using PixiEditor.Helpers;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools;
using PixiEditor.Models.Tools.Tools;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace PixiEditor.Models.ImageManipulation
{
    public static class ToolCalculator
    {
        /// <summary>
        /// This function calculates fill and outputs it into Coordinates list.
        /// </summary>
        /// <remarks>All coordinates are calculated without layer offset.
        /// If you want to take consideration offset, just do it in CalculateBresenhamLine in PerformLinearFill function.</remarks>
        /// <param name="layer">Layer to calculate fill from.</param>
        /// <param name="startingCoords">Starting fill coordinates</param>
        /// <param name="maxWidth">Maximum fill width</param>
        /// <param name="maxHeight">Maximum fill height</param>
        /// <param name="newColor">Replacement color to stop on</param>
        /// <param name="output">List with output coordinates.</param>
        public static void GetLinearFillAbsolute(Layer layer, Coordinates startingCoords, int maxWidth, int maxHeight, SKColor newColor, List<Coordinates> output)
        {
            Queue<FloodFillRange> floodFillQueue = new Queue<FloodFillRange>();
            SKColor colorToReplace = layer.GetPixelWithOffset(startingCoords.X, startingCoords.Y);
            if ((colorToReplace.Alpha == 0 && newColor.Alpha == 0) ||
                colorToReplace == newColor)
                return;

            int width = maxWidth;
            int height = maxHeight;
            if (startingCoords.X < 0 || startingCoords.Y < 0 || startingCoords.X >= width || startingCoords.Y >= height)
                return;
            var visited = new bool[width * height];

            Int32Rect dirtyRect = new Int32Rect(startingCoords.X, startingCoords.Y, 1, 1);

            PerformLinearFill(layer, floodFillQueue, startingCoords, width, colorToReplace, ref dirtyRect, visited, output);
            PerformFloodFIll(layer, floodFillQueue, colorToReplace, ref dirtyRect, width, height, visited, output);
        }

        public static void GenerateEllipseNonAlloc(Coordinates start, Coordinates end, bool fill,
            List<Coordinates> output)
        {
            DoubleCoords fixedCoordinates = CalculateCoordinatesForShapeRotation(start, end);
            
            EllipseGenerator.GenerateEllipseFromRect(fixedCoordinates, output);
            if (fill)
            {
                CalculateFillForEllipse(output);
            }
        }

        public static void GenerateRectangleNonAlloc(
            Coordinates start,
            Coordinates end, bool fill, int thickness, List<Coordinates> output)
        {
            DoubleCoords fixedCoordinates = CalculateCoordinatesForShapeRotation(start, end);
            CalculateRectanglePoints(fixedCoordinates, output);

            for (int i = 1; i < (int)Math.Floor(thickness / 2f) + 1; i++)
            {
                CalculateRectanglePoints(
                    new DoubleCoords(
                    new Coordinates(fixedCoordinates.Coords1.X - i, fixedCoordinates.Coords1.Y - i),
                    new Coordinates(fixedCoordinates.Coords2.X + i, fixedCoordinates.Coords2.Y + i)), output);
            }

            for (int i = 1; i < (int)Math.Ceiling(thickness / 2f); i++)
            {
                CalculateRectanglePoints(
                    new DoubleCoords(
                    new Coordinates(fixedCoordinates.Coords1.X + i, fixedCoordinates.Coords1.Y + i),
                    new Coordinates(fixedCoordinates.Coords2.X - i, fixedCoordinates.Coords2.Y - i)), output);
            }

            if (fill)
            {
                CalculateRectangleFillNonAlloc(start, end, thickness, output);
            }
        }

        public static DoubleCoords CalculateCoordinatesForShapeRotation(
            Coordinates startingCords,
            Coordinates secondCoordinates)
        {
            Coordinates currentCoordinates = secondCoordinates;

            if (startingCords.X > currentCoordinates.X && startingCords.Y > currentCoordinates.Y)
            {
                return new DoubleCoords(
                    new Coordinates(currentCoordinates.X, currentCoordinates.Y),
                    new Coordinates(startingCords.X, startingCords.Y));
            }

            if (startingCords.X < currentCoordinates.X && startingCords.Y < currentCoordinates.Y)
            {
                return new DoubleCoords(
                    new Coordinates(startingCords.X, startingCords.Y),
                    new Coordinates(currentCoordinates.X, currentCoordinates.Y));
            }

            if (startingCords.Y > currentCoordinates.Y)
            {
                return new DoubleCoords(
                    new Coordinates(startingCords.X, currentCoordinates.Y),
                    new Coordinates(currentCoordinates.X, startingCords.Y));
            }

            if (startingCords.X > currentCoordinates.X && startingCords.Y <= currentCoordinates.Y)
            {
                return new DoubleCoords(
                    new Coordinates(currentCoordinates.X, startingCords.Y),
                    new Coordinates(startingCords.X, currentCoordinates.Y));
            }

            return new DoubleCoords(startingCords, secondCoordinates);
        }

        private static void PerformLinearFill(
            Layer layer, Queue<FloodFillRange> floodFillQueue,
            Coordinates coords, int width, SKColor colorToReplace, ref Int32Rect dirtyRect, bool[] visited, List<Coordinates> output)
        {
            // Find the Left Edge of the Color Area
            int fillXLeft = coords.X;
            while (true)
            {
                // Indicate that this pixel has been checked
                int pixelIndex = (coords.Y * width) + fillXLeft;
                visited[pixelIndex] = true;

                // Move one pixel to the left
                fillXLeft--;
                // Exit the loop if we're at edge of the bitmap or the color area
                if (fillXLeft < 0 || visited[pixelIndex - 1] || layer.GetPixelWithOffset(fillXLeft, coords.Y) != colorToReplace)
                    break;
            }
            int lastCheckedPixelLeft = fillXLeft + 1;

            // Find the Right Edge of the Color Area
            int fillXRight = coords.X;
            while (true)
            {
                int pixelIndex = (coords.Y * width) + fillXRight;
                visited[pixelIndex] = true;

                fillXRight++;
                if (fillXRight >= width || visited[pixelIndex + 1] || layer.GetPixelWithOffset(fillXRight, coords.Y) != colorToReplace)
                    break;
            }
            int lastCheckedPixelRight = fillXRight - 1;

            int relativeY = coords.Y;
            LineTool.CalculateBresenhamLine(new Coordinates(lastCheckedPixelLeft, relativeY), new Coordinates(lastCheckedPixelRight, relativeY), output);
            dirtyRect = dirtyRect.Expand(new Int32Rect(lastCheckedPixelLeft, coords.Y, lastCheckedPixelRight - lastCheckedPixelLeft + 1, 1));

            FloodFillRange range = new FloodFillRange(lastCheckedPixelLeft, lastCheckedPixelRight, coords.Y);
            floodFillQueue.Enqueue(range);
        }

        private static void PerformFloodFIll(
            Layer layer, Queue<FloodFillRange> floodFillQueue,
            SKColor colorToReplace, ref Int32Rect dirtyRect, int width, int height, bool[] pixelsVisited, List<Coordinates> output)
        {
            while (floodFillQueue.Count > 0)
            {
                FloodFillRange range = floodFillQueue.Dequeue();

                //START THE LOOP UPWARDS AND DOWNWARDS
                int upY = range.Y - 1; //so we can pass the y coord by ref
                int downY = range.Y + 1;
                int downPixelxIndex = (width * (range.Y + 1)) + range.StartX;
                int upPixelIndex = (width * (range.Y - 1)) + range.StartX;
                for (int i = range.StartX; i <= range.EndX; i++)
                {
                    //START LOOP UPWARDS
                    //if we're not above the top of the bitmap and the pixel above this one is within the color tolerance
                    if (range.Y > 0 && (!pixelsVisited[upPixelIndex]) && layer.GetPixelWithOffset(i, upY) == colorToReplace)
                        PerformLinearFill(layer, floodFillQueue, new Coordinates(i, upY), width, colorToReplace, ref dirtyRect, pixelsVisited, output);
                    //START LOOP DOWNWARDS
                    if (range.Y < (height - 1) && (!pixelsVisited[downPixelxIndex]) && layer.GetPixelWithOffset(i, downY) == colorToReplace)
                        PerformLinearFill(layer, floodFillQueue, new Coordinates(i, downY), width, colorToReplace, ref dirtyRect, pixelsVisited, output);
                    downPixelxIndex++;
                    upPixelIndex++;
                }
            }
        }

        private static void CalculateFillForEllipse(List<Coordinates> outlineCoordinates)
        {
            if (!outlineCoordinates.Any())
                return;

            var lines = EllipseGenerator.SplitEllipseIntoLines(outlineCoordinates);
            foreach (var line in lines)
            {
                for (int i = line.Coords1.X; i <= line.Coords2.X; i++)
                {
                    outlineCoordinates.Add(new Coordinates(i, line.Coords1.Y));
                }
            }
        }

        private static void CalculateRectangleFillNonAlloc(Coordinates start, Coordinates end, int thickness, List<Coordinates> output)
        {
            int offset = (int)Math.Ceiling(thickness / 2f);
            DoubleCoords fixedCords = CalculateCoordinatesForShapeRotation(start, end);

            DoubleCoords innerCords = new DoubleCoords
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

        private static void CalculateRectanglePoints(DoubleCoords coordinates, List<Coordinates> output)
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
