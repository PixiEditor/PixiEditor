using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PixiEditor.Models
{
    public static class MousePositionConverter
    {
        public static Coordinates MousePositionToCoordinates(Layer baseLayer, Point mousePosition)
        {
            int xCoord = (int)(mousePosition.X / baseLayer.Width);
            int yCoord = (int)(mousePosition.Y / baseLayer.Height);
            return new Coordinates(xCoord, yCoord);
        }
    }
}
