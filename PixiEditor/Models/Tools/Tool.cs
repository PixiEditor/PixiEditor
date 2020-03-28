using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using System.Windows.Input;
using System.Windows.Media;

namespace PixiEditor.Models.Tools
{
    public abstract class Tool
    {
        public abstract BitmapPixelChanges Use(Layer layer, Coordinates[] pixels, Color color, int toolSize);
        public abstract ToolType ToolType { get; }
        public bool RequiresPreviewLayer { get; set; }
        public Cursor Cursor { get; set; } = Cursors.Arrow;
    }
}
