using System.Windows.Media;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;

namespace PixiEditor.Models.Tools
{
    public abstract class BitmapOperationTool : Tool
    {
        public bool RequiresPreviewLayer { get; set; }

        public bool UseDefaultUndoMethod { get; set; } = true;

        public abstract LayerChange[] Use(Layer layer, Coordinates[] mouseMove, Color color);

        protected LayerChange[] Only(BitmapPixelChanges changes, Layer layer)
        {
            return new[] { new LayerChange(changes, layer) };
        }

        protected LayerChange[] Only(BitmapPixelChanges changes, int layerIndex)
        {
            return new[] { new LayerChange(changes, layerIndex) };
        }
    }
}