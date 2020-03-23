using PixiEditor.Models.Layers;
using System.Windows;

namespace PixiEditor.Models.Position
{
    public static class MousePositionConverter
    {
        public static Coordinates CurrentCoordinates { get; set; }

        public static Coordinates MousePositionToCoordinates(Layer baseLayer, Point mousePosition)
        {
            int xCoord = (int)(mousePosition.X / baseLayer.Width);
            int yCoord = (int)(mousePosition.Y / baseLayer.Height);
            return new Coordinates(xCoord, yCoord);
        }
    }
}
