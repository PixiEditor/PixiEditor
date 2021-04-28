using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools.ToolSettings.Toolbars;

namespace PixiEditor.Models.Tools
{
    public abstract class ShapeTool : BitmapOperationTool
    {
        public ShapeTool()
        {
            RequiresPreviewLayer = true;
            Cursor = Cursors.Cross;
            Toolbar = new BasicShapeToolbar();
        }

        // TODO: Add cache for lines 31, 32 (hopefully it would speed up calculation)
        public abstract override LayerChange[] Use(Layer layer, Coordinates[] coordinates, Color color);

        protected IEnumerable<Coordinates> GetThickShape(IEnumerable<Coordinates> shape, int thickness)
        {
            List<Coordinates> output = new List<Coordinates>();
            foreach (Coordinates item in shape)
            {
                output.AddRange(
                    CoordinatesCalculator.RectangleToCoordinates(
                        CoordinatesCalculator.CalculateThicknessCenter(item, thickness)));
            }

            return output.Distinct();
        }

        protected DoubleCords CalculateCoordinatesForShapeRotation(
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
    }
}