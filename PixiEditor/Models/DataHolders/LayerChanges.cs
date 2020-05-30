using PixiEditor.Models.Tools;

namespace PixiEditor.Models.DataHolders
{
    public class LayerChanges
    {
        public BitmapPixelChanges PixelChanges { get; set; }
        public int LayerIndex { get; set; }

        public LayerChanges(BitmapPixelChanges pixelChanges, int layerIndex)
        {
            PixelChanges = pixelChanges;
            LayerIndex = layerIndex;
        }
    }
}
