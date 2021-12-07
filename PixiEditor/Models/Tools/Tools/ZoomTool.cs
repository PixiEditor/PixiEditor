using PixiEditor.Models.Controllers;
using PixiEditor.Models.Position;
using System.Collections.Generic;
using System.Windows.Input;

namespace PixiEditor.Models.Tools.Tools
{
    public class ZoomTool : ReadonlyTool
    {
        private BitmapManager BitmapManager { get; }
        private string defaultActionDisplay = "Click and move to zoom. Click to zoom in, hold ctrl and click to zoom out.";

        public ZoomTool(BitmapManager bitmapManager)
        {
            CanStartOutsideCanvas = true;
            ActionDisplay = defaultActionDisplay;
            BitmapManager = bitmapManager;
        }

        public override bool HideHighlight => true;

        public override string Tooltip => "Zooms viewport (Z). Click to zoom in, hold alt and click to zoom out.";

        public override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key is Key.LeftCtrl or Key.RightCtrl)
            {
                ActionDisplay = "Click and move to zoom. Click to zoom out, release ctrl and click to zoom in.";
            }
        }

        public override void OnKeyUp(KeyEventArgs e)
        {
            if (e.Key is Key.LeftCtrl or Key.RightCtrl)
            {
                ActionDisplay = defaultActionDisplay;
            }
        }

        public override void Use(List<Coordinates> pixels)
        {
            // Implemented inside Zoombox.xaml.cs
        }
    }
}
