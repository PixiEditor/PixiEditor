using PixiEditor.Models.Layers;
using PixiEditor.Models.Tools;
using PixiEditor.ViewModels;

namespace PixiEditor.Models.DataHolders
{
    public class LayerChange
    {
        public BitmapPixelChanges PixelChanges { get; set; }
        public int LayerIndex { get; set; }

        public LayerChange(BitmapPixelChanges pixelChanges, int layerIndex)
        {
            PixelChanges = pixelChanges;
            LayerIndex = layerIndex;
        }

        public LayerChange(BitmapPixelChanges pixelChanges, Layer layer)
        {
            PixelChanges = pixelChanges;

            // Layer implements IEquatable interface so IndexOf method cannot be used here as it
            // calls Equals that is very slow (compares bitmap pixel by pixel).
            for (var i = 0; i < ViewModelMain.Current.BitmapManager.ActiveDocument.Layers.Count; i++)
            {
                if (ViewModelMain.Current.BitmapManager.ActiveDocument.Layers[i] != layer) continue;

                LayerIndex = i;
                break;
            }
        }
    }
}