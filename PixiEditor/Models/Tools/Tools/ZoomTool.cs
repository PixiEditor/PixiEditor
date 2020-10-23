using System;
using System.Windows;
using System.Windows.Input;
using PixiEditor.Models.Position;
using PixiEditor.ViewModels;

namespace PixiEditor.Models.Tools.Tools
{
    public class ZoomTool : ReadonlyTool
    {
        public const float ZoomSensitivityMultiplier = 30f;

        private double startingX;

        private double workAreaWidth = SystemParameters.WorkArea.Width;
        private double pixelsPerZoomMultiplier;

        public ZoomTool()
        {
            HideHighlight = true;
            CanStartOutsideCanvas = true;
            Tooltip = "Zooms viewport (Z). Click to zoom in, hold alt and click to zoom out.";
            pixelsPerZoomMultiplier = workAreaWidth / ZoomSensitivityMultiplier;
        }

        public override ToolType ToolType => ToolType.Zoom;

        public override void OnRecordingLeftMouseDown(MouseEventArgs e)
        {
            startingX = MousePositionConverter.GetCursorPosition().X;
            ViewModelMain.Current.ZoomPercentage = 100; // This resest the value, so callback in MainDrawingPanel can fire again later
        }

        public override void OnMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                double xPos = MousePositionConverter.GetCursorPosition().X;

                double rawPercentDifference = (xPos - startingX) / pixelsPerZoomMultiplier; // negative - zoom out, positive - zoom in, linear
                double finalPercentDifference = Math.Pow(2, rawPercentDifference) * 100.0; // less than 100 - zoom out, greater than 100 - zoom in
                Zoom(finalPercentDifference);
            }
        }

        public override void OnStoppedRecordingMouseUp(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released && e.RightButton == MouseButtonState.Released &&
                startingX == MousePositionConverter.GetCursorPosition().X)
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