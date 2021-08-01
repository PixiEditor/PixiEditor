using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools.ToolSettings.Toolbars;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;

namespace PixiEditor.Models.Tools
{
    public abstract class ShapeTool : BitmapOperationTool
    {
        public static DoubleCords CalculateCoordinatesForShapeRotation(
            Coordinates startingCords,
            Coordinates secondCoordinates)
        {
            Coordinates currentCoordinates = secondCoordinates;

            if (startingCords.X > currentCoordinates.X && startingCords.Y > currentCoordinates.Y)
            {
                return new DoubleCords(
                    new Coordinates(currentCoordinates.X, currentCoordinates.Y),
                    new Coordinates(startingCords.X, startingCords.Y));
            }

            if (startingCords.X < currentCoordinates.X && startingCords.Y < currentCoordinates.Y)
            {
                return new DoubleCords(
                    new Coordinates(startingCords.X, startingCords.Y),
                    new Coordinates(currentCoordinates.X, currentCoordinates.Y));
            }

            if (startingCords.Y > currentCoordinates.Y)
            {
                return new DoubleCords(
                    new Coordinates(startingCords.X, currentCoordinates.Y),
                    new Coordinates(currentCoordinates.X, startingCords.Y));
            }

            if (startingCords.X > currentCoordinates.X && startingCords.Y <= currentCoordinates.Y)
            {
                return new DoubleCords(
                    new Coordinates(currentCoordinates.X, startingCords.Y),
                    new Coordinates(startingCords.X, currentCoordinates.Y));
            }

            return new DoubleCords(startingCords, secondCoordinates);
        }

        public ShapeTool()
        {
            RequiresPreviewLayer = true;
            Cursor = Cursors.Cross;
            Toolbar = new BasicShapeToolbar();
        }

        // TODO: Add cache for lines 31, 32 (hopefully it would speed up calculation)
        public abstract override void Use(Layer layer, List<Coordinates> coordinates, Color color);

        protected static void ThickenShape(Layer layer, Color color, IEnumerable<Coordinates> shape, int thickness)
        {
            foreach (Coordinates item in shape)
            {
                var changes = BitmapPixelChanges.FromSingleColoredArray(
                    CoordinatesCalculator.RectangleToCoordinates(
                   CoordinatesCalculator.CalculateThicknessCenter(item, thickness)), color);
                layer.SetPixels(changes);
            }
        }
    }
}
