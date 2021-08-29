using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace PixiEditor.Models.Tools
{
    public abstract class BitmapOperationTool : Tool
    {
        public bool RequiresPreviewLayer { get; set; }

        public bool ClearPreviewLayerOnEachIteration { get; set; } = true;

        public bool UseDefaultUndoMethod { get; set; } = true;

        private readonly LayerChange[] onlyLayerArr = new LayerChange[] { new LayerChange(BitmapPixelChanges.Empty, Guid.Empty) };

        public abstract LayerChange[] Use(Layer layer, List<Coordinates> mouseMove, SKColor color);

        protected LayerChange[] Only(BitmapPixelChanges changes, Layer layer)
        {
            onlyLayerArr[0] = new LayerChange(changes, layer);
            return onlyLayerArr;
        }

        protected LayerChange[] Only(BitmapPixelChanges changes, Guid layerGuid)
        {
            onlyLayerArr[0] = new LayerChange(changes, layerGuid);
            return onlyLayerArr;
        }
    }
}
