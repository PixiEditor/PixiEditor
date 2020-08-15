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
        public override ToolType ToolType => ToolType.Zoom;
        private double _startingX;

        public ZoomTool()
        {
            HideHighlight = true;
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
                double zoomModifier;
                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Alt))
                {
                    zoomModifier =  85;
                }
                else
                {
                    zoomModifier = 115;
                }
                ViewModelMain.Current.ZoomPercentage = zoomModifier;
            }
        }

        public override void Use(Coordinates[] pixels)
        {
            double xPos = MousePositionConverter.GetCursorPosition().X;

            ViewModelMain.Current.ZoomPercentage = 100 + ((xPos - _startingX) / 5.0);
        }
    }
}
