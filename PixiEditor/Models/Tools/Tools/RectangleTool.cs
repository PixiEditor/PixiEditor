using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools.ToolSettings.Settings;

namespace PixiEditor.Models.Tools.Tools
{
    public class RectangleTool : ShapeTool
    {
        public RectangleTool()
        {
            ActionDisplay = "Click and move to draw a rectangle.  Hold Shift to draw square.";
        }

        public override string Tooltip => "Draws rectangle on canvas (R). Hold Shift to draw square.";

        public bool Filled { get; set; } = false;

        public override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift)
            {
                ActionDisplay = "Click and move to draw a square.";
            }
        }

        public override void OnKeyUp(KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift)
            {
                ActionDisplay = "Click and move to draw a rectangle.  Hold Shift to draw square.";
            }
        }

        public override void Use(Layer layer, List<Coordinates> coordinates, Color color)
        {
            int thickness = Toolbar.GetSetting<SizeSetting>("ToolSize").Value;
            CreateRectangle(layer, color, coordinates, thickness);
            if (Toolbar.GetSetting<BoolSetting>("Fill").Value)
            {
                Color fillColor = Toolbar.GetSetting<ColorSetting>("FillColor").Value;
                DrawRectangleFill(layer, color, coordinates[^1], coordinates[0], thickness);
            }
        }

        public void CreateRectangle(Layer layer, Color color, List<Coordinates> coordinates, int thickness)
        {
            DoubleCords fixedCoordinates = CalculateCoordinatesForShapeRotation(coordinates[^1], coordinates[0]);

            using var ctx = layer.LayerBitmap.GetBitmapContext();

            DrawRectangle(layer, color, fixedCoordinates);

            for (int i = 1; i < (int)Math.Floor(thickness / 2f) + 1; i++)
            {
                DrawRectangle(layer, color, new DoubleCords(
                    new Coordinates(fixedCoordinates.Coords1.X - i, fixedCoordinates.Coords1.Y - i),
                    new Coordinates(fixedCoordinates.Coords2.X + i, fixedCoordinates.Coords2.Y + i)));
            }

            for (int i = 1; i < (int)Math.Ceiling(thickness / 2f); i++)
            {
                DrawRectangle(layer, color, new DoubleCords(
                    new Coordinates(fixedCoordinates.Coords1.X + i, fixedCoordinates.Coords1.Y + i),
                    new Coordinates(fixedCoordinates.Coords2.X - i, fixedCoordinates.Coords2.Y - i)));
            }
        }

        public void CreateRectangle(Layer layer, Color color, Coordinates start, Coordinates end, int thickness)
        {
            CreateRectangle(layer, color, new() { end, start }, thickness);
        }

        public void DrawRectangleFill(Layer layer, Color color, Coordinates start, Coordinates end, int thickness)
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
                    layer.SetPixel(new Coordinates(innerCords.Coords1.X + x, innerCords.Coords1.Y + y), color);
                    i++;
                }
            }
        }

        private void DrawRectangle(Layer layer, Color color, DoubleCords coordinates)
        {
            for (int i = coordinates.Coords1.X; i < coordinates.Coords2.X + 1; i++)
            {
                layer.SetPixel(new Coordinates(i, coordinates.Coords1.Y), color);
                layer.SetPixel(new Coordinates(i, coordinates.Coords2.Y), color);
            }

            for (int i = coordinates.Coords1.Y + 1; i <= coordinates.Coords2.Y - 1; i++)
            {
                layer.SetPixel(new Coordinates(coordinates.Coords1.X, i), color);
                layer.SetPixel(new Coordinates(coordinates.Coords2.X, i), color);
            }
        }
    }
}