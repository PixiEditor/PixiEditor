using PixiEditorDotNetCore3.Models.Layers;
using PixiEditorDotNetCore3.Models.Position;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace PixiEditorDotNetCore3.Models.Tools
{
    public abstract class Tool
    {
        public abstract BitmapPixelChanges Use(Layer layer, Coordinates startingCoords, Color color, int toolSize);
        public abstract ToolType ToolType { get; }
        public bool ExecutesItself = false;
    }
}
