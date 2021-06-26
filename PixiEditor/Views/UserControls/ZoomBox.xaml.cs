using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

namespace PixiEditor.Views.UserControls
{
    [ContentProperty(nameof(AdditionalContent))]
    public partial class Zoombox : ContentControl
    {
        public enum Mode
        {
            Normal, MoveTool, ZoomTool
        }

        private interface IDragOperation
        {
            void Start(MouseButtonEventArgs e);

            void Update(MouseEventArgs e);

            void Terminate();
        }

        private class MoveDragOperation : IDragOperation
        {
            private Zoombox parent;
            private Point prevMousePos;

            public MoveDragOperation(Zoombox zoomBox)
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
            private Zoombox parent;

            private double initZoomPower;
            private Point initSpaceOriginPos;

            private Point zoomOrigin;
            private Point screenZoomOrigin;

            public ZoomDragOperation(Zoombox zoomBox)
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
                double deltaPower = deltaX / 10.0;
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

        public static readonly DependencyProperty AdditionalContentProperty =
            DependencyProperty.Register(nameof(AdditionalContent), typeof(object), typeof(Zoombox),
              new PropertyMetadata(null));

        public static readonly DependencyProperty ZoomModeProperty =
            DependencyProperty.Register(nameof(ZoomMode), typeof(Mode), typeof(Zoombox),
              new PropertyMetadata(Mode.Normal, ZoomModeChanged));
        public object AdditionalContent
        {
            get => GetValue(AdditionalContentProperty);
            set => SetValue(AdditionalContentProperty, value);
        }
        public Mode ZoomMode
        {
            get => (Mode)GetValue(ZoomModeProperty);
            set => SetValue(ZoomModeProperty, value);
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

        private double zoomPower;
        private double ZoomPower
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

        private IDragOperation activeDragOperation = null;

        private static void ZoomModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Zoombox sender = (Zoombox)d;
            sender.activeDragOperation?.Terminate();
            sender.activeDragOperation = null;
        }

        public Zoombox()
        {
            InitializeComponent();
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
            if (ZoomMode == Mode.Normal)
                return;

            activeDragOperation?.Terminate();

            if (ZoomMode == Mode.MoveTool)
                activeDragOperation = new MoveDragOperation(this);
            else if (ZoomMode == Mode.ZoomTool)
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

            if (Math.Abs(ZoomPower) < 1) ZoomPower = 0;

            var shiftedMousePos = ToScreenSpace(oldZoomboxMousePos);
            var deltaMousePos = oldMousePos - shiftedMousePos;
            SpaceOriginPos = SpaceOriginPos + deltaMousePos;
        }
    }
}
