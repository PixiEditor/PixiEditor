using PixiEditor.Models.DataHolders;
using System;
using System.Collections.Generic;

namespace PixiEditor.Models.Position
{
    public static class CoordinatesCalculator
    {
        /// <summary>
        ///     Calculates center of thickness * thickness rectangle.
        /// </summary>
        /// <param name="startPosition">Top left position of rectangle.</param>
        /// <param name="thickness">Thickness of rectangle.</param>
        public static DoubleCords CalculateThicknessCenter(Coordinates startPosition, int thickness)
        {
            int x1, x2, y1, y2;
            if (thickness % 2 == 0)
            {
                x2 = startPosition.X + (thickness / 2);
                y2 = startPosition.Y + (thickness / 2);
                x1 = x2 - thickness;
                y1 = y2 - thickness;
            }
            else
            {
                x2 = startPosition.X + ((thickness - 1) / 2) + 1;
                y2 = startPosition.Y + ((thickness - 1) / 2) + 1;
                x1 = x2 - thickness;
                y1 = y2 - thickness;
            }

            return new DoubleCords(new Coordinates(x1, y1), new Coordinates(x2 - 1, y2 - 1));
        }

        public static Coordinates GetCenterPoint(Coordinates startingPoint, Coordinates endPoint)
        {
            int x = (int)Math.Floor((startingPoint.X + endPoint.X) / 2f);
            int y = (int)Math.Floor((startingPoint.Y + endPoint.Y) / 2f);
            return new Coordinates(x, y);
        }

        /// <summary>
        ///     Calculates coordinates of rectangle by edge points x1, y1, x2, y2.
        /// </summary>
        /// <param name="x1">Top left x point.</param>
        /// <param name="y1">Top left y position.</param>
        /// <param name="x2">Bottom right x position.</param>
        /// <param name="y2">Bottom right Y position.</param>
        public static IEnumerable<Coordinates> RectangleToCoordinates(int x1, int y1, int x2, int y2)
        {
            x2++;
            y2++;
            List<Coordinates> coordinates = new List<Coordinates>();
            for (int y = y1; y < y1 + (y2 - y1); y++)
            {
                for (int x = x1; x < x1 + (x2 - x1); x++)
                {
                    coordinates.Add(new Coordinates(x, y));
                }
            }

            return coordinates;
        }

        public static void DrawRectangle(Layer layer, Color color, int x1, int y1, int x2, int y2)
        {
            using var ctx = layer.LayerBitmap.GetBitmapContext();
            x2++;
            y2++;
            for (int y = y1; y < y1 + (y2 - y1); y++)
            {
                for (int x = x1; x < x1 + (x2 - x1); x++)
                {
                    layer.SetPixelWithOffset(x, y, color);
                }
            }
        }

        public static IEnumerable<Coordinates> RectangleToCoordinates(DoubleCords coordinates)
        {
            return RectangleToCoordinates(coordinates.Coords1.X, coordinates.Coords1.Y, coordinates.Coords2.X, coordinates.Coords2.Y);
        }

        /// <summary>
        ///     Returns first pixel coordinates in bitmap that is most top left on canvas.
        /// </summary>
        public static Coordinates FindMinEdgeNonTransparentPixel(Surface bitmap)
        {
            return new Coordinates(FindMinXNonTransparent(bitmap), FindMinYNonTransparent(bitmap));
        }

        /// <summary>
        ///     Returns last pixel coordinates that is most bottom right.
        /// </summary>
        public static Coordinates FindMostEdgeNonTransparentPixel(Surface bitmap)
        {
            return new Coordinates(FindMaxXNonTransparent(bitmap), FindMaxYNonTransparent(bitmap));
        }

        public static int FindMinYNonTransparent(Surface bitmap)
        {
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    if (bitmap.GetSRGBPixel(x, y).Alpha != 0)
                    {
                        return y;
                    }
                }
            }

            return -1;
        }

        public static int FindMinXNonTransparent(Surface bitmap)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    if (bitmap.GetSRGBPixel(x, y).Alpha != 0)
                    {
                        return x;
                    }
                }
            }

            return -1;
        }

        public static int FindMaxYNonTransparent(Surface bitmap)
        {
            for (int y = bitmap.Height - 1; y >= 0; y--)
            {
                for (int x = bitmap.Width - 1; x >= 0; x--)
                {
                    if (bitmap.GetSRGBPixel(x, y).Alpha != 0)
                    {
                        return y;
                    }
                }
            }

            return -1;
        }

        public static int FindMaxXNonTransparent(Surface bitmap)
        {
            for (int x = bitmap.Width - 1; x >= 0; x--)
            {
                for (int y = bitmap.Height - 1; y >= 0; y--)
                {
                    if (bitmap.GetSRGBPixel(x, y).Alpha != 0)
                    {
                        return x;
                    }
                }
            }

            return -1;
        }
    }
}
