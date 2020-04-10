using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PixiEditor.Models.Tools.Tools
{
    public class CircleTool : ShapeTool
    {
        public override ToolType ToolType => ToolType.Circle;

        public override BitmapPixelChanges Use(Layer layer, Coordinates[] coordinates, Color color, int toolSize)
        {
            DoubleCords fixedCoordinates = CalculateCoordinatesForShapeRotation(coordinates[^1], coordinates[0]);
            return BitmapPixelChanges.FromSingleColoredArray(CreateEllipse(fixedCoordinates.Coords1, fixedCoordinates.Coords2, true), color);
        }

        public Coordinates[] CreateEllipse(Coordinates startCoordinates, Coordinates endCoordinates, bool filled)
        {            
            Coordinates centerCoordinates = CoordinatesCalculator.GetCenterPoint(startCoordinates, endCoordinates);
            int radiusX = endCoordinates.X - centerCoordinates.X;
            int radiusY = endCoordinates.Y - centerCoordinates.Y;
            Coordinates[] ellipse = MidpointEllipse(radiusX, radiusY, centerCoordinates.X, centerCoordinates.Y);
            if (filled)
            {
                ellipse = ellipse.Concat(CalculateFillForEllipse(ellipse)).Distinct().ToArray();
            }
            return ellipse;
        }

        public Coordinates[] MidpointEllipse(double rx, double ry, double xc, double yc)
        {
            List<Coordinates> outputCoordinates = new List<Coordinates>();
            double dx, dy, d1, d2, x, y;
            x = 0;
            y = ry;

            d1 = (ry * ry) - (rx * rx * ry) + (0.25f * rx * rx);
            dx = 2 * ry * ry * x;
            dy = 2 * rx * rx * y;

            while (dx < dy)
            {
                outputCoordinates.AddRange(GetRegionPoints(x, xc, y, yc));
                if (d1 < 0)
                {
                    x++;
                    dx += (2 * ry * ry);
                    d1 = d1 + dx + (ry * ry);
                }
                else
                {
                    x++;
                    y--;
                    dx += (2 * ry * ry);
                    dy -= (2 * rx * rx);
                    d1 = d1 + dx - dy + (ry * ry);
                }
            }

            //Yeah, it scares me too
            d2 = (ry * ry * ((x + 0.5f) * (x + 0.5f))) + (rx * rx * ((y - 1) * (y - 1))) - (rx * rx * ry * ry);

            while(y >= 0)
            {
                outputCoordinates.AddRange(GetRegionPoints(x, xc, y, yc));

                if(d2 > 0)
                {
                    y--;
                    dy -= (2 * rx * rx);
                    d2 = d2 + (rx * rx) - dy;
                }
                else
                {
                    y--;
                    x++;
                    dx += (2 * ry * ry);
                    dy -= (2 * rx * rx);
                    d2 = d2 + dx - dy + (rx * rx);
                }
            }

            return outputCoordinates.Distinct().ToArray();

        }

        private Coordinates[] CalculateFillForEllipse(Coordinates[] outlineCoordinates)
        {
            List<Coordinates> finalCoordinates = new List<Coordinates>();
            int bottom = outlineCoordinates.Max(x => x.Y);
            int top = outlineCoordinates.Min(x => x.Y);
            for (int i = top + 1; i < bottom; i++)
            {
                var rowCords = outlineCoordinates.Where(x => x.Y == i);
                int right = rowCords.Max(x => x.X);
                int left = rowCords.Min(x => x.X);
                for (int j = left + 1; j < right; j++)
                {
                    finalCoordinates.Add(new Coordinates(j, i));
                }
            }
            return finalCoordinates.ToArray();
        }

        private Coordinates[] GetRegionPoints(double x, double xc, double y, double yc)
        {
            Coordinates[] outputCoordinates = new Coordinates[4];
            outputCoordinates[0] = (new Coordinates((int)x + (int)xc, (int)y + (int)yc));
            outputCoordinates[1] = (new Coordinates((int)-x + (int)xc, (int)y + (int)yc));
            outputCoordinates[2] = (new Coordinates((int)x + (int)xc, (int)-y + (int)yc));
            outputCoordinates[3] = (new Coordinates((int)-x + (int)xc, (int)-y + (int)yc));
            return outputCoordinates;
        }

        public Coordinates[] BresenhamCircle(Coordinates startCoordinates, Coordinates endCoordinates, int size)
        {
            List<Coordinates> outputCoordinates = new List<Coordinates>();
            Coordinates centerCoordinates = CoordinatesCalculator.GetCenterPoint(startCoordinates, endCoordinates);
            int radius = endCoordinates.X - centerCoordinates.X;
            int x = 0;
            int y = radius;
            int decision = 3 - 2 * radius;
            outputCoordinates.AddRange(GetPixelsForOctant(centerCoordinates.X, centerCoordinates.Y, x, y));

            while (x <= y)
            {
                if (decision <= 0)
                {
                    decision = decision + (4 * x) + 6;
                }
                else
                {
                    decision = decision + (4 * x) - (4 * y) + 10;
                    y--;
                }
                x++;
                outputCoordinates.AddRange(GetPixelsForOctant(centerCoordinates.X, centerCoordinates.Y, x, y));
            }
            return outputCoordinates.Distinct().ToArray();
        }

        private Coordinates[] GetPixelsForOctant(int xc, int yc, int x, int y)
        {
            Coordinates[] outputCords = new Coordinates[8];
            outputCords[0] = new Coordinates(x + xc, y + yc);
            outputCords[1] = new Coordinates(x + xc, -y + yc);
            outputCords[2] = new Coordinates(-x + xc, -y + yc);
            outputCords[3] = new Coordinates(-x + xc, y + yc);
            outputCords[4] = new Coordinates(y + xc, x + yc);
            outputCords[5] = new Coordinates(y + xc, -x + yc);
            outputCords[6] = new Coordinates(-y + xc, -x + yc);
            outputCords[7] = new Coordinates(-y + xc, x + yc);
            return outputCords;
        }

    }
}
