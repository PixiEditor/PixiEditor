using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PixiEditorDotNetCore3.Models.Tools.Tools
{
    public class LineTool : Tool
    {
        public override ToolType ToolType => ToolType.Line;

        public LineTool()
        {
            ExecutesItself = true;
            IsShapeCreating = true;
        }

        public override BitmapPixelChanges Use(Layer layer, Coordinates startingCoords, Color color, int toolSize)
        {
            Line(layer, startingCoords, color, toolSize);
            return new BitmapPixelChanges();
        }

        public void Line(Layer layer, Coordinates coordinates, Color color, int size)
        {           
            layer.LayerBitmap.DrawLineBresenham(coordinates.X, coordinates.Y, MousePositionConverter.CurrentCoordinates.X,
                MousePositionConverter.CurrentCoordinates.Y, color);


        }
    }
}
