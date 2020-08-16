using PixiEditor.Models.Position;
using PixiEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace PixiEditor.Models.Tools.Tools
{
    public class ZoomTool : ReadonlyTool
    {
        public const double DragZoomSpeed = 4.0;
        public override ToolType ToolType => ToolType.Zoom;
        private double _startingX;

        public ZoomTool()
        {
            HideHighlight = true;
            CanStartOutsideCanvas = true;
            Tooltip = "Zooms viewport (Z). Click to zoom in, hold alt and click to zoom out.";            
        }

        public override void OnMouseDown(MouseEventArgs e)
        {
            _startingX = MousePositionConverter.GetCursorPosition().X;
            ViewModelMain.Current.ZoomPercentage = 100; //This resest the value, so callback in MainDrawingPanel can fire again later
        }

        public override void OnMouseUp(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released && e.RightButton == MouseButtonState.Released && 
                _startingX == MousePositionConverter.GetCursorPosition().X)
            {
                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Alt))
                {
                    Zoom(85);
                }
                else
                {
                    Zoom(115);
                }
            }
        }

        public void Zoom(double percentage)
        {
            ViewModelMain.Current.ZoomPercentage = percentage;
        }

        public override void Use(Coordinates[] pixels)
        {
            double xPos = MousePositionConverter.GetCursorPosition().X;

            ViewModelMain.Current.ZoomPercentage = 100 + -((_startingX - xPos) / DragZoomSpeed);
        }
    }
}
