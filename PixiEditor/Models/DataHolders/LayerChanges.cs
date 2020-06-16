using PixiEditor.Models.Layers;
using PixiEditor.Models.Tools;
using PixiEditor.ViewModels;

namespace PixiEditor.Models.DataHolders
{
    public class LayerChange
    {
        public LayerChange(BitmapPixelChanges pixelChanges, int layerIndex)
        {
            PixelChanges = pixelChanges;
            LayerIndex = layerIndex;
        }

        public LayerChange(BitmapPixelChanges pixelChanges, Layer layer)
        {
            PixelChanges = pixelChanges;
            LayerIndex = ViewModelMain.Current.BitmapManager.ActiveDocument.Layers.IndexOf(layer);
        }

        public BitmapPixelChanges PixelChanges { get; set; }
        public int LayerIndex { get; set; }
    }
}