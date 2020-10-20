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

        public abstract override ToolType ToolType { get; }

        public abstract override LayerChange[] Use(Layer layer, Coordinates[] coordinates, Color color);

        protected IEnumerable<Coordinates> GetThickShape(IEnumerable<Coordinates> shape, int thickness)
        {
            var output = new List<Coordinates>();
            foreach (var item in shape)
                output.AddRange(
                    CoordinatesCalculator.RectangleToCoordinates(
                        CoordinatesCalculator.CalculateThicknessCenter(item, thickness)));
            return output.Distinct();
        }


        protected DoubleCords CalculateCoordinatesForShapeRotation(Coordinates startingCords,
            Coordinates secondCoordinates)
        {
            var currentCoordinates = secondCoordinates;

            if (startingCords.X > currentCoordinates.X && startingCords.Y > currentCoordinates.Y)
                return new DoubleCords(new Coordinates(currentCoordinates.X, currentCoordinates.Y),
                    new Coordinates(startingCords.X, startingCords.Y));
            if (startingCords.X < currentCoordinates.X && startingCords.Y < currentCoordinates.Y)
                return new DoubleCords(new Coordinates(startingCords.X, startingCords.Y),
                    new Coordinates(currentCoordinates.X, currentCoordinates.Y));
            if (startingCords.Y > currentCoordinates.Y)
                return new DoubleCords(new Coordinates(startingCords.X, currentCoordinates.Y),
                    new Coordinates(currentCoordinates.X, startingCords.Y));
            if (startingCords.X > currentCoordinates.X && startingCords.Y <= currentCoordinates.Y)
                return new DoubleCords(new Coordinates(currentCoordinates.X, startingCords.Y),
                    new Coordinates(startingCords.X, currentCoordinates.Y));
            return new DoubleCords(startingCords, secondCoordinates);
        }
    }
}