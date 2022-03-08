using PixiEditor.Models.Position;
using System.Collections.Generic;
using System.Windows.Input;

namespace PixiEditor.Models.Tools.Tools
{
    public class MoveViewportTool : ReadonlyTool
    {
        public MoveViewportTool()
        {
            Cursor = Cursors.SizeAll;
            ActionDisplay = "Click and move to pan viewport.";
        }

        public override bool HideHighlight => true;
        public override string Tooltip => $"Move viewport. ({ShortcutKey})"; 

        public override void Use(IReadOnlyList<Coordinates> pixels)
        {
            // Implemented inside Zoombox.xaml.cs
        }
    }
}
