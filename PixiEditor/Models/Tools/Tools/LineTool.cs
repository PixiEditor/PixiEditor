using PixiEditor.Models.ImageManipulation;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace PixiEditor.Models.Tools.Tools
{
	public class LineTool : ShapeTool
    {

        public override ToolType ToolType => ToolType.Line;

        public LineTool()
        {
            Tooltip = "Draws line on canvas (L)";
        }

        public override BitmapPixelChanges Use(Layer layer, Coordinates[] coordinates, Color color)
        {
            return BitmapPixelChanges.FromSingleColoredArray(CreateLine(coordinates, 1), color);
        }

        public Coordinates[] CreateLine(Coordinates[] coordinates, int thickness)
        {
            Coordinates startingCoordinates = coordinates[^1];
            Coordinates latestCoordinates = coordinates[0];
            if(thickness == 1)
            {
                return BresenhamLine(startingCoordinates.X, startingCoordinates.Y, latestCoordinates.X, latestCoordinates.Y);
            }
            return GetThickShape(BresenhamLine(startingCoordinates.X, startingCoordinates.Y, latestCoordinates.X, latestCoordinates.Y), thickness);
        }

        private Coordinates[] BresenhamLine(int x1, int y1, int x2, int y2)
        {
            List<Coordinates> coordinates = new List<Coordinates>();
            if (x1 == x2 && y1 == y2)
            {
                return new Coordinates[] { new Coordinates(x1, y1) };
            }

            int d, dx, dy, ai, bi, xi, yi;
            int x = x1, y = y1;

            if (x1 < x2)
            {
                xi = 1;
                dx = x2 - x1;
            }
            else
            {
                xi = -1;
                dx = x1 - x2;
            }

            if (y1 < y2)
            {
                yi = 1;
                dy = y2 - y1;
            }
            else
            {
                yi = -1;
                dy = y1 - y2;
            }
            coordinates.Add(new Coordinates(x, y));

            if (dx > dy)
            {
                ai = (dy - dx) * 2;
                bi = dy * 2;
                d = bi - dx;

                while (x != x2)
                {

                    if (d >= 0)
                    {
                        x += xi;
                        y += yi;
                        d += ai;
                    }
                    else
                    {
                        d += bi;
                        x += xi;
                    }
                    coordinates.Add(new Coordinates(x, y));
                }
            }
            else
            {
                ai = (dx - dy) * 2;
                bi = dx * 2;
                d = bi - dy;

                while (y != y2)
                {

                    if (d >= 0)
                    {
                        x += xi;
                        y += yi;
                        d += ai;
                    }
                    else
                    {
                        d += bi;
                        y += yi;
                    }
                    coordinates.Add(new Coordinates(x, y));
                }
            }

            return coordinates.ToArray();

        }

        private int[,] GetMaskForThickness(int thickness)
        {
            if(thickness == 2)
            {
                return new int[,] {
                {0,0,0 },
                {0,1,1 },
                {0,1,1 }
                };
            }
            int[,] mask = new int[thickness,thickness];

            for (int i = 0; i < thickness; i++)
            {
                for (int j = 0; j < thickness; j++)
                {
                    mask[i, j] = 1;
                }
            }
            return mask;
        }    
	}
}
