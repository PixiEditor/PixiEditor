using PixiEditor.Models.Position;
using PixiEditor.ViewModels;
using System.Drawing;
using System.Windows.Input;

namespace PixiEditor.Models.Tools.Tools
{
    public class MoveViewportTool : ReadonlyTool
    {
        public override ToolType ToolType => ToolType.MoveViewport;
        private Point _clickPoint;

        public MoveViewportTool()
        {
            HideHighlight = true;
            Cursor = Cursors.SizeAll;
        }

        public override void OnMouseDown(MouseEventArgs e)
        {
             _clickPoint = MousePositionConverter.GetCursorPosition();
        }

        public override void OnMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var point = MousePositionConverter.GetCursorPosition();
                ViewModelMain.Current.ViewportPosition = new System.Windows.Point(point.X - _clickPoint.X, 
                    point.Y - _clickPoint.Y);
            }
        }

        public override void Use(Coordinates[] pixels)
        {
        }
    }
}
