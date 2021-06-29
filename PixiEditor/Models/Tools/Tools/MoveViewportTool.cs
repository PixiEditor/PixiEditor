using PixiEditor.Models.Position;
using PixiEditor.ViewModels;
using System.Collections.Generic;
using System.Windows.Input;

namespace PixiEditor.Models.Tools.Tools
{
    public class MoveViewportTool : ReadonlyTool
    {
        public MoveViewportTool()
        {
            HideHighlight = true;
            Cursor = Cursors.SizeAll;
            ActionDisplay = "Click and move to pan viewport.";
            Tooltip = "Move viewport. (H)";
        }

        public override void OnMouseUp(MouseEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                ViewModelMain.Current.ToolsSubViewModel.SetActiveTool(ViewModelMain.Current.ToolsSubViewModel.LastActionTool);
            }
        }

        public override void Use(List<Coordinates> pixels)
        {
        }
    }
}
