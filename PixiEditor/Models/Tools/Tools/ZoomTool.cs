using PixiEditor.Models.Position;
using System.Collections.Generic;
using System.Windows.Input;

namespace PixiEditor.Models.Tools.Tools
{
    public class ZoomTool : ReadonlyTool
    {
        public ZoomTool()
        {
            HideHighlight = true;
            CanStartOutsideCanvas = true;
            ActionDisplay = "Click and move to zoom. Click to zoom in, hold alt and click to zoom out.";
            Tooltip = "Zooms viewport (Z). Click to zoom in, hold alt and click to zoom out.";
        }

        public override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.LeftAlt)
            {
                ActionDisplay = "Click and move to zoom. Click to zoom out, release alt and click to zoom in.";
            }
        }

        public override void OnKeyUp(KeyEventArgs e)
        {
            if (e.Key == Key.LeftAlt)
            {
                ActionDisplay = "Click and move to zoom. Click to zoom in, hold alt and click to zoom out.";
            }
        }

        public override void Use(List<Coordinates> pixels)
        {
        }
    }
}
