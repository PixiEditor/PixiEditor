using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace PixiEditor.Models.Tools.Tools
{
    public class EarserTool : Tool
    {
        public override ToolType ToolType => ToolType.Earser;

        public override BitmapPixelChanges Use(Layer layer, Coordinates[] coordinates, Color color, int toolSize)
        {
            PenTool pen = new PenTool();
            return pen.Draw(coordinates[0], System.Windows.Media.Colors.Transparent, toolSize);
        }
    }
}
