using System.Collections.Generic;
using System.Drawing;
using System.Windows.Input;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.Position;
using PixiEditor.ViewModels.SubViewModels.Main;

namespace PixiEditor.Models.Tools.Tools
{
    public class MoveViewportTool : ReadonlyTool
    {
        private Point clickPoint;

        public BitmapManager BitmapManager { get; set; }

        public ToolsViewModel ToolsViewModel { get; set; }

        public MoveViewportTool()
        {
            HideHighlight = true;
            Cursor = Cursors.SizeAll;
            ActionDisplay = "Click and move to pan viewport.";
            Tooltip = "Move viewport. (H)";
        }

        public override void OnMouseDown(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed || e.MiddleButton == MouseButtonState.Pressed)
            {
                clickPoint = MousePositionConverter.GetCursorPosition();
            }
        }

        public override void OnMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed || e.MiddleButton == MouseButtonState.Pressed)
            {
                var point = MousePositionConverter.GetCursorPosition();
                BitmapManager.ActiveDocument.ViewportPosition = new System.Windows.Point(
                    point.X - clickPoint.X,
                    point.Y - clickPoint.Y);
            }
        }

        public override void OnMouseUp(MouseEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                ToolsViewModel.SetActiveTool(ToolsViewModel.LastActionTool);
            }
        }

        public override void Use(List<Coordinates> pixels)
        {
        }
    }
}