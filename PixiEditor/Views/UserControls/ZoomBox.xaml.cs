using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PixiEditor.Views.UserControls
{
    public partial class ZoomBox : ContentControl
    {
        public enum Mode
        {
            Normal, Move, Zoom
        }

        private interface IDragOperation
        {
            void Start(MouseButtonEventArgs e);

            void Update(MouseEventArgs e);

            void Terminate();
        }

        private class MoveDragOperation : IDragOperation
        {
            private ZoomBox parent;
            private Point prevMousePos;

            public MoveDragOperation(ZoomBox zoomBox)
            {
                parent = zoomBox;
            }
            public void Start(MouseButtonEventArgs e)
            {
                prevMousePos = e.GetPosition(parent.mainCanvas);
                parent.mainCanvas.CaptureMouse();
            }

            public void Update(MouseEventArgs e)
            {
                var curMousePos = e.GetPosition(parent.mainCanvas);
                parent.SpaceOriginPos += curMousePos - prevMousePos;
                prevMousePos = e.GetPosition(parent.mainCanvas);
            }

            public void Terminate()
            {
                parent.mainCanvas.ReleaseMouseCapture();
            }
        }

        private class ZoomDragOperation : IDragOperation
        {
            private ZoomBox parent;

            private int initZoomPower;
            private Point initSpaceOriginPos;

            private Point zoomOrigin;
            private Point screenZoomOrigin;

            public ZoomDragOperation(ZoomBox zoomBox)
            {
                parent = zoomBox;
            }
            public void Start(MouseButtonEventArgs e)
            {
                screenZoomOrigin = e.GetPosition(parent.mainCanvas);
                zoomOrigin = parent.ToZoomboxSpace(screenZoomOrigin);
                initZoomPower = parent.ZoomPower;
                initSpaceOriginPos = parent.SpaceOriginPos;
                parent.mainCanvas.CaptureMouse();
            }

            public void Update(MouseEventArgs e)
            {
                var curScreenPos = e.GetPosition(parent.mainCanvas);
                double deltaX = screenZoomOrigin.X - curScreenPos.X;
                int deltaPower = (int)(deltaX / 10.0);
                parent.ZoomPower = initZoomPower - deltaPower;

                parent.SpaceOriginPos = initSpaceOriginPos;
                var shiftedOriginPos = parent.ToScreenSpace(zoomOrigin);
                var deltaOriginPos = shiftedOriginPos - screenZoomOrigin;
                parent.SpaceOriginPos = initSpaceOriginPos - deltaOriginPos;
            }

            public void Terminate()
            {
                parent.mainCanvas.ReleaseMouseCapture();
            }
        }

        private Point spaceOriginPos;
        private Point SpaceOriginPos
        {
            get => spaceOriginPos;
            set
            {
                spaceOriginPos = value;
                Canvas.SetLeft(mainGrid, spaceOriginPos.X);
                Canvas.SetTop(mainGrid, spaceOriginPos.Y);
            }
        }

        private int zoomPower;
        private int ZoomPower
        {
            get => zoomPower;
            set
            {
                zoomPower = value;
                var mult = Zoom;
                scaleTransform.ScaleX = mult;
                scaleTransform.ScaleY = mult;
            }
        }
        private double Zoom => Math.Pow(1.1, zoomPower);
        private Mode mode;

        private IDragOperation activeDragOperation = null;

        public ZoomBox()
        {
            InitializeComponent();
            mode = Mode.Zoom;
        }

        private Point ToScreenSpace(Point p)
        {
            double zoom = Zoom;
            p.X *= zoom;
            p.Y *= zoom;
            p += (Vector)SpaceOriginPos;
            return p;
        }

        private Point ToZoomboxSpace(Point mousePos)
        {
            double zoom = Zoom;
            mousePos -= (Vector)SpaceOriginPos;
            mousePos.X /= zoom;
            mousePos.Y /= zoom;
            return mousePos;
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (mode == Mode.Normal)
                return;

            activeDragOperation?.Terminate();

            if (mode == Mode.Move)
                activeDragOperation = new MoveDragOperation(this);
            else if (mode == Mode.Zoom)
                activeDragOperation = new ZoomDragOperation(this);

            activeDragOperation.Start(e);
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            activeDragOperation?.Terminate();
            activeDragOperation = null;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            activeDragOperation?.Update(e);
        }

        private void OnScroll(object sender, MouseWheelEventArgs e)
        {
            var oldMousePos = e.GetPosition(mainCanvas);
            var oldZoomboxMousePos = ToZoomboxSpace(oldMousePos);

            ZoomPower += e.Delta / 100;

            var shiftedMousePos = ToScreenSpace(oldZoomboxMousePos);
            var deltaMousePos = oldMousePos - shiftedMousePos;
            SpaceOriginPos = SpaceOriginPos + deltaMousePos;
        }
    }
}
