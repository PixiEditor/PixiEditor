using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Enums;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools.ToolSettings.Settings;
using PixiEditor.Models.Tools.ToolSettings.Toolbars;

namespace PixiEditor.Models.Tools.Tools
{
    public class LineTool : ShapeTool
    {
        public LineTool()
        {
            Tooltip = "Draws line on canvas (L). Hold Shift to draw even line.";
            Toolbar = new BasicToolbar();
        }

        public override ToolType ToolType => ToolType.Line;

        public override LayerChange[] Use(Layer layer, Coordinates[] coordinates, Color color)
        {
            var pixels =
                BitmapPixelChanges.FromSingleColoredArray(
                    CreateLine(coordinates,
                        Toolbar.GetSetting<SizeSetting>("ToolSize").Value, CapType.Square, CapType.Square), color);
            return Only(pixels, layer);
        }

        public IEnumerable<Coordinates> CreateLine(Coordinates start, Coordinates end, int thickness)
        {
            return CreateLine(new[] {end, start}, thickness, CapType.Square, CapType.Square);
        }

        public IEnumerable<Coordinates> CreateLine(Coordinates start, Coordinates end, int thickness, CapType startCap,
            CapType endCap)
        {
            return CreateLine(new[] {end, start}, thickness, startCap, endCap);
        }

        private IEnumerable<Coordinates> CreateLine(Coordinates[] coordinates, int thickness, CapType startCap, CapType endCap)
        {
            var startingCoordinates = coordinates[^1];
            var latestCoordinates = coordinates[0];
            if (thickness == 1)
                return BresenhamLine(startingCoordinates.X, startingCoordinates.Y, latestCoordinates.X,
                    latestCoordinates.Y);
            return GetLinePoints(startingCoordinates, latestCoordinates, thickness, startCap, endCap);
        }

        private IEnumerable<Coordinates> GetLinePoints(Coordinates start, Coordinates end, int thickness, CapType startCap, CapType endCap)
        {
            var startingCap = GetCapCoordinates(startCap, start, thickness);
            if (start == end) return startingCap;

            var line = BresenhamLine(start.X, start.Y, end.X, end.Y);

            var output = new List<Coordinates>(startingCap);

            output.AddRange(GetCapCoordinates(endCap, end, thickness));
            if (line.Count() > 2) output.AddRange(GetThickShape(line.Except(new[] {start, end}).ToArray(), thickness));

            return output.Distinct();
        }

        private IEnumerable<Coordinates> GetCapCoordinates(CapType cap, Coordinates position, int thickness)
        {
            switch (cap)
            {
                case CapType.Round:
                {
                    return GetRoundCap(position, thickness); // Round cap is not working very well, circle tool must be improved
                }
                default:
                    return GetThickShape(new[] {position}, thickness);
            }
        }

        /// <summary>
        ///     Gets points for rounded cap on specified position and thickness
        /// </summary>
        /// <param name="position">Starting position of cap</param>
        /// <param name="thickness">Thickness of cap</param>
        /// <returns></returns>
        private IEnumerable<Coordinates> GetRoundCap(Coordinates position, int thickness)
        {
            var circle = new CircleTool();
            var rectangleCords = CoordinatesCalculator.RectangleToCoordinates(
                CoordinatesCalculator.CalculateThicknessCenter(position, thickness));
            return circle.CreateEllipse(rectangleCords[0], rectangleCords[^1], 1, true);
        }

        private IEnumerable<Coordinates> BresenhamLine(int x1, int y1, int x2, int y2)
        {
            var coordinates = new List<Coordinates>();
            if (x1 == x2 && y1 == y2) return new[] {new Coordinates(x1, y1)};

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

            return coordinates;
        }
    }
}