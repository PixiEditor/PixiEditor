using System;
using System.Collections.Generic;
using System.Windows.Documents;
using System.Windows.Media;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;

namespace PixiEditor.Models.Tools
{
    public abstract class BitmapOperationTool : Tool
    {
        public bool RequiresPreviewLayer { get; set; }

        public bool ClearPreviewLayerOnEachIteration { get; set; } = true;

        public bool UseDefaultUndoMethod { get; set; } = true;
        public virtual bool UsesShift => true;

        public abstract void Use(Layer layer, List<Coordinates> mouseMove, Color color);
    }
}
