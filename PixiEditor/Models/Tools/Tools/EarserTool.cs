using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools.ToolSettings;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace PixiEditor.Models.Tools.Tools
{
    public class EarserTool : Tool
    {
        public override ToolType ToolType => ToolType.Earser;

        public EarserTool()
        {
            Tooltip = "Earsers color from pixel (E)";
            Toolbar = new BasicToolbar();
        }

        public override BitmapPixelChanges Use(Layer layer, Coordinates[] coordinates, Color color)
        {
            PenTool pen = new PenTool();
            return pen.Draw(coordinates[0], System.Windows.Media.Colors.Transparent, (int)Toolbar.GetSetting("ToolSize").Value);
        }
    }
}
