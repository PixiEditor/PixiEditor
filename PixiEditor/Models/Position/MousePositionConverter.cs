using System.Drawing;
using System.Runtime.InteropServices;

namespace PixiEditor.Models.Position
{
    public static class MousePositionConverter
    {
        public static Coordinates CurrentCoordinates { get; set; }

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out Point point);

        public static Point GetCursorPosition()
        {
            GetCursorPos(out Point point);
            return point;
        }
    }
}