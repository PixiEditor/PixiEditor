using System;
using PixiEditor.Models.Layers;

namespace PixiEditor.Models.DataHolders
{
    public class LayerChange
    {
        public LayerChange(BitmapPixelChanges pixelChanges, Guid layerGuid)
        {
            PixelChanges = pixelChanges;
            LayerGuid = layerGuid;
        }

        public LayerChange(BitmapPixelChanges pixelChanges, Layer layer)
        {
            PixelChanges = pixelChanges;
            LayerGuid = layer.LayerGuid;
        }

        public BitmapPixelChanges PixelChanges { get; set; }

        public Guid LayerGuid { get; set; }
    }
}