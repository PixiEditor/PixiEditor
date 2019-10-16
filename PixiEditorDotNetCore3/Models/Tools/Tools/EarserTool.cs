using PixiEditorDotNetCore3.Models.Layers;
using PixiEditorDotNetCore3.Models.Position;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace PixiEditorDotNetCore3.Models.Tools.Tools
{
    public class EarserTool : Tool
    {
        public override ToolType ToolType => ToolType.Earser;

        public override BitmapPixelChanges Use(Layer layer, Coordinates startingCoords, Color color, int toolSize)
        {
            PenTool pen = new PenTool();
            return pen.Draw(startingCoords, System.Windows.Media.Colors.Transparent, toolSize);
        }
    }
}
