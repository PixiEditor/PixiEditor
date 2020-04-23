using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace PixiEditor.Models.Tools.Tools
{
    public class ColorPickerTool : Tool
    {
        public override ToolType ToolType => ToolType.ColorPicker;

        public ColorPickerTool()
        {
            PerformsOperationOnBitmap = false;
            HideHighlight = true;
        }

        public override BitmapPixelChanges Use(Layer layer, Coordinates[] pixels, Color color)
        {
            return new BitmapPixelChanges();
        }
    }
}
