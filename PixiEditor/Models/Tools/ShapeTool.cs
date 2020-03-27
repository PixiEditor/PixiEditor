using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace PixiEditor.Models.Tools
{
    public abstract class ShapeTool : Tool
    {
        public override abstract ToolType ToolType { get; }

        public abstract override BitmapPixelChanges Use(Layer layer, Coordinates[] coordinates, Color color, int toolSize);

        public ShapeTool()
        {
            RequiresPreviewLayer = true;
        }

        protected Coordinates[] BresenhamLine(int x1, int y1, int x2, int y2)
        {
            List<Coordinates> coordinates = new List<Coordinates>();
            if(x1 == x2 && y1 == y2)
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

        protected DoubleCords CalculateCoordinatesForShapeRotation(Coordinates startingCords, Coordinates secondCoordinates)
        {
            Coordinates currentCoordinates = secondCoordinates;

            if (startingCords.X > currentCoordinates.X && startingCords.Y > currentCoordinates.Y)
            {
                return new DoubleCords(new Coordinates(currentCoordinates.X, currentCoordinates.Y), new Coordinates(startingCords.X, startingCords.Y));
            }
            else if (startingCords.X < currentCoordinates.X && startingCords.Y < currentCoordinates.Y)
            {
                return new DoubleCords(new Coordinates(startingCords.X, startingCords.Y), new Coordinates(currentCoordinates.X, currentCoordinates.Y));
            }
            else if (startingCords.Y > currentCoordinates.Y)
            {
                return new DoubleCords(new Coordinates(startingCords.X, currentCoordinates.Y), new Coordinates(currentCoordinates.X, startingCords.Y));
            }
            else if(startingCords.X > currentCoordinates.X && startingCords.Y <= currentCoordinates.Y)
            {
                return new DoubleCords(new Coordinates(currentCoordinates.X, startingCords.Y), new Coordinates(startingCords.X, currentCoordinates.Y));
            }
            else
            {
                return new DoubleCords(startingCords, secondCoordinates);
            }
        }
    }
}
