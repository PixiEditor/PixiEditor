using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PixiEditor.Views.UserControls
{
    /// <summary>
    /// Interaction logic for ZoomBox.xaml
    /// </summary>
    public partial class ZoomBox : ContentControl
    {
        private bool captured = false;
        private Point prevMousePos;

        private Point backingSpaceOriginPos;
        private Point SpaceOriginPos
        {
            get => backingSpaceOriginPos;
            set
            {
                backingSpaceOriginPos = value;
                Canvas.SetLeft(mainGrid, backingSpaceOriginPos.X);
                Canvas.SetTop(mainGrid, backingSpaceOriginPos.Y);
            }
        }

        private int backingZoomPower;
        private int ZoomPower
        {
            get => backingZoomPower;
            set
            {
                backingZoomPower = value;
                var mult = Zoom;
                scaleTransform.ScaleX = mult;
                scaleTransform.ScaleY = mult;
            }
        }
        private double Zoom => Math.Pow(1.1, backingZoomPower);

        public ZoomBox()
        {
            InitializeComponent();
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            mainCanvas.CaptureMouse();
            captured = true;
            prevMousePos = e.GetPosition(mainCanvas);
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            mainCanvas.ReleaseMouseCapture();
            captured = false;
            prevMousePos = e.GetPosition(mainCanvas);
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!captured)
                return;
            var curMousePos = e.GetPosition(mainCanvas);
            SpaceOriginPos += curMousePos - prevMousePos;

            prevMousePos = e.GetPosition(mainCanvas);
        }

        private Point ToScreenSpace(Point p)
        {
            double zoom = Zoom;
            p.X /= zoom;
            p.Y /= zoom;
            p.X += SpaceOriginPos.X;
            p.Y += SpaceOriginPos.Y;
            return p;
        }

        private Point ToZoomboxSpace(Point mousePos)
        {
            double zoom = Zoom;
            mousePos.X -= SpaceOriginPos.X;
            mousePos.Y -= SpaceOriginPos.Y;
            mousePos.X *= zoom;
            mousePos.Y *= zoom;
            return mousePos;
        }

        private void OnScroll(object sender, MouseWheelEventArgs e)
        {
            var oldMousePos = e.GetPosition(mainCanvas);
            var oldZoomboxMousePos = ToZoomboxSpace(oldMousePos);

            ZoomPower += e.Delta / 100;

            var shiftedMousePos = ToScreenSpace(oldZoomboxMousePos);
            var deltaMousePos = oldMousePos - shiftedMousePos;
            SpaceOriginPos = SpaceOriginPos - deltaMousePos;
        }
    }
}
