using System;
using System.Collections.Generic;
using System.Drawing;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Media.Imaging;

namespace PixiEditor.Models.Position
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
                x2 = startPosition.X + (thickness - 1) / 2 + 1;
                y2 = startPosition.Y + (thickness - 1) / 2 + 1;
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

        public static Coordinates[] RectangleToCoordinates(int x1, int y1, int x2, int y2)
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
            return coordinates.ToArray();
        }

        public static Coordinates[] RectangleToCoordinates(DoubleCords coordinates)
        {
            return RectangleToCoordinates(coordinates.Coords1.X, coordinates.Coords1.Y, coordinates.Coords2.X, coordinates.Coords2.Y);
        }

        /// <summary>
        /// Returns first pixel coordinates in bitmap that is most top left on canvas
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static Coordinates FindMinEdgeNonTransparentPixel(WriteableBitmap bitmap)
        {
            return new Coordinates(FindMinXNonTransparent(bitmap), FindMinYNonTransparent(bitmap));
        }

        /// <summary>
        /// Returns last pixel coordinates that is most bottom right
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static Coordinates FindMostEdgeNonTransparentPixel(WriteableBitmap bitmap)
        {
            return new Coordinates(FindMaxXNonTransparent(bitmap), FindMaxYNonTransparent(bitmap));
        }

        public static int FindMinYNonTransparent(WriteableBitmap bitmap)
        {
            System.Windows.Media.Color transparent = System.Windows.Media.Color.FromArgb(0, 0, 0, 0);
            bitmap.Lock();
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    if (bitmap.GetPixel(x, y) != transparent)
                    {
                        bitmap.Unlock();
                        return y;
                    }
                }
            }
            bitmap.Unlock();
            return -1;
        }

        public static int FindMinXNonTransparent(WriteableBitmap bitmap)
        {
            System.Windows.Media.Color transparent = System.Windows.Media.Color.FromArgb(0, 0, 0, 0);
            bitmap.Lock();
            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    if (bitmap.GetPixel(x, y) != transparent)
                    {
                        bitmap.Unlock();
                        return x;
                    }
                }
            }
            bitmap.Unlock();
            return -1;
        }

        public static int FindMaxYNonTransparent(WriteableBitmap bitmap)
        {
            System.Windows.Media.Color transparent = System.Windows.Media.Color.FromArgb(0, 0, 0, 0);
            bitmap.Lock();
            for (int y = (int)bitmap.Height - 1; y >= 0; y--)
            {
                for (int x = (int)bitmap.Width - 1; x >= 0; x--)
                {
                    if (bitmap.GetPixel(x, y) != transparent)
                    {
                        bitmap.Unlock();
                        return y;
                    }
                }
            }
            bitmap.Unlock();
            return -1;
        }

        public static int FindMaxXNonTransparent(WriteableBitmap bitmap)
        {
            System.Windows.Media.Color transparent = System.Windows.Media.Color.FromArgb(0, 0, 0, 0);
            bitmap.Lock();
            for (int x = (int)bitmap.Width - 1; x >= 0; x--)
            {
                for (int y = (int)bitmap.Height - 1; y >= 0; y--)
                {
                    if (bitmap.GetPixel(x, y) != transparent)
                    {
                        bitmap.Unlock();
                        return x;
                    }
                }
            }
            bitmap.Unlock();
            return -1;
        }
    }
}