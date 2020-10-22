using System.Drawing;
using System.Windows.Input;
using PixiEditor.Models.Position;
using PixiEditor.ViewModels;

namespace PixiEditor.Models.Tools.Tools
{
    public class MoveViewportTool : ReadonlyTool
    {
        public override ToolType ToolType => ToolType.MoveViewport;

        private Point clickPoint;

        public MoveViewportTool()
        {
            HideHighlight = true;
            Cursor = Cursors.SizeAll;
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
                ViewModelMain.Current.ViewportPosition = new System.Windows.Point(point.X - clickPoint.X,
                    point.Y - clickPoint.Y);
            }
        }

        public override void OnMouseUp(MouseEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                ViewModelMain.Current.SetActiveTool(ViewModelMain.Current.LastActionTool);
            }
        }

        public override void Use(Coordinates[] pixels)
        {
        }
    }
}