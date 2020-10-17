using PixiEditor.Models.Position;
using PixiEditor.ViewModels;
using System.Drawing;
using System.Windows.Input;

namespace PixiEditor.Models.Tools.Tools
{
    public class MoveViewportTool : ReadonlyTool
    {
        public override ToolType ToolType => ToolType.MoveViewport;

        public MoveViewportTool()
        {
            HideHighlight = true;
            Cursor = Cursors.SizeAll;
        }

        public override void OnMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var point = MousePositionConverter.GetCursorPosition();
                ViewModelMain.Current.ViewportPosition = new System.Windows.Point(point.X, point.Y);
            }
        }

        public override void Use(Coordinates[] pixels)
        {
        }
    }
}
