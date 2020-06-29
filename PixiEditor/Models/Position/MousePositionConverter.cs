using System.Runtime.InteropServices;
using System.Windows;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Layers;

namespace PixiEditor.Models.Position
{
    public static class MousePositionConverter
    {
        public static Coordinates CurrentCoordinates { get; set; }

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out System.Drawing.Point point);

        public static System.Drawing.Point GetCursorPosition()
        {
            System.Drawing.Point point;
            GetCursorPos(out point);
            return point;
        }
    }
}