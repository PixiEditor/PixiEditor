using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools.ToolSettings.Toolbars;
using SkiaSharp;
using System.Collections.Generic;
using System.Windows.Input;

namespace PixiEditor.Models.Tools;

public abstract class ShapeTool : BitmapOperationTool
{
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

    public ShapeTool()
    {
        RequiresPreviewLayer = true;
        Cursor = Cursors.Cross;
        Toolbar = new BasicShapeToolbar();
    }

    public static void ThickenShape(Layer layer, SKColor color, IEnumerable<Coordinates> shape, int thickness)
    {
        foreach (Coordinates item in shape)
        {
            ThickenShape(layer, color, item, thickness);
        }
    }

    protected static void ThickenShape(Layer layer, SKColor color, Coordinates coords, int thickness)
    {
        var dcords = CoordinatesCalculator.CalculateThicknessCenter(coords, thickness);
        CoordinatesCalculator.DrawRectangle(layer, color, dcords.Coords1.X, dcords.Coords1.Y, dcords.Coords2.X, dcords.Coords2.Y);
    }
}