using PixiEditor.Models.Controllers;
using PixiEditor.Models.Position;
using PixiEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace PixiEditor.Models.Tools.Tools
{
    public class ZoomTool : ReadonlyTool
    {
        public const float ZoomSensitivityMultiplier = 30f;
        public override ToolType ToolType => ToolType.Zoom;
        private double _startingX;
        private double _workAreaWidth = SystemParameters.WorkArea.Width;
        private double _pixelsPerZoomMultiplier;

        public ZoomTool()
        {
            HideHighlight = true;
            CanStartOutsideCanvas = true;
            Tooltip = "Zooms viewport (Z). Click to zoom in, hold alt and click to zoom out.";
            _pixelsPerZoomMultiplier = _workAreaWidth / ZoomSensitivityMultiplier;
        }

        public override void OnMouseDown(MouseEventArgs e)
        {
            _startingX = MousePositionConverter.GetCursorPosition().X;
            ViewModelMain.Current.ZoomPercentage = 100; //This resest the value, so callback in MainDrawingPanel can fire again later
        }

        public override void OnMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                double xPos = MousePositionConverter.GetCursorPosition().X;

                double rawPercentDifference = (xPos - _startingX) / _pixelsPerZoomMultiplier; //negative - zoom out, positive - zoom in, linear
                double finalPercentDifference = Math.Pow(2, rawPercentDifference) * 100.0; //less than 100 - zoom out, greater than 100 - zoom in
                Zoom(finalPercentDifference);
            }
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
        }
    }
}
