using PixiEditor.Models.Commands.Attributes;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.Position;
using System.Windows.Input;

namespace PixiEditor.Models.Tools.Tools
{
    [Command.Tool(Key = Key.Z)]
    internal class ZoomTool : ReadonlyTool
    {
        private BitmapManager BitmapManager { get; }
        private string defaultActionDisplay = "Click and move to zoom. Click to zoom in, hold ctrl and click to zoom out.";

        public ZoomTool(BitmapManager bitmapManager)
        {
            ActionDisplay = defaultActionDisplay;
            BitmapManager = bitmapManager;
        }

        public override bool HideHighlight => true;

        public override string Tooltip => $"Zooms viewport ({Shortcut}). Click to zoom in, hold alt and click to zoom out.";

        public override void OnKeyDown(Key key)
        {
            if (key is Key.LeftCtrl)
            {
                ActionDisplay = "Click and move to zoom. Click to zoom out, release ctrl and click to zoom in.";
            }
        }

        public override void OnKeyUp(Key key)
        {
            if (key is Key.LeftCtrl)
            {
                ActionDisplay = defaultActionDisplay;
            }
        }

        public override void Use(IReadOnlyList<Coordinates> pixels)
        {
            // Implemented inside Zoombox.xaml.cs
        }
    }
}
