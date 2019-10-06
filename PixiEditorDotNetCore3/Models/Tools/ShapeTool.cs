using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace PixiEditorDotNetCore3.Models.Tools
{
    public abstract class ShapeTool : Tool
    {
        public override abstract ToolType ToolType { get; }

        public ShapeTool()
        {
            ExecutesItself = true;
        }

        public abstract override BitmapPixelChanges Use(Layer layer, Coordinates startingCoords, Color color, int toolSize);

        protected DoubleCords CalculateCoordinatesForShapeRotation(Coordinates startingCords)
        {
            Coordinates currentCoordinates = MousePositionConverter.CurrentCoordinates;

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
            else
            {
                return new DoubleCords(new Coordinates(currentCoordinates.X, startingCords.Y), new Coordinates(startingCords.X, currentCoordinates.Y));
            }
        }
    }
}
