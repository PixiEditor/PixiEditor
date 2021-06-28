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

        private const double zoomFactor = 1.1;
        private const double maxZoom = 50;
        private const double minZoom = -20;
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
                if (value > maxZoom)
                    return;
                if (value < minZoom)
                    return;
                zoomPower = value;
                var mult = Zoom;
                scaleTransform.ScaleX = mult;
                scaleTransform.ScaleY = mult;
            }
        }

        private double Zoom => Math.Pow(zoomFactor, zoomPower);

        private IDragOperation activeDragOperation = null;
        private MouseButtonEventArgs activeMouseDownEventArgs = null;
        private Point activeMouseDownPos;

        private static void ZoomModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Zoombox sender = (Zoombox)d;
            sender.activeDragOperation?.Terminate();
            sender.activeDragOperation = null;
            sender.activeMouseDownEventArgs = null;
        }

        public Zoombox()
        {
            InitializeComponent();
        }

        public void CenterContent()
        {
            const double marginFactor = 1.1;
            double scaleFactor = Math.Max(
                mainGrid.ActualWidth * marginFactor / mainCanvas.ActualWidth,
                mainGrid.ActualHeight * marginFactor / mainCanvas.ActualHeight);
            ZoomPower = -Math.Log(scaleFactor, zoomFactor);
            SpaceOriginPos = new Point(
                mainCanvas.ActualWidth / 2 - mainGrid.ActualWidth * Zoom / 2,
                mainCanvas.ActualHeight / 2 - mainGrid.ActualHeight * Zoom / 2);
        }

        public void ZoomIntoCenter(double delta)
        {
            ZoomInto(new Point(mainCanvas.ActualWidth / 2, mainCanvas.ActualHeight / 2), delta);
        }
        public void ZoomInto(Point mousePos, double delta)
        {
            var oldZoomboxMousePos = ToZoomboxSpace(mousePos);

            ZoomPower += delta;

            if (Math.Abs(ZoomPower) < 1) ZoomPower = 0;

            var shiftedMousePos = ToScreenSpace(oldZoomboxMousePos);
            var deltaMousePos = mousePos - shiftedMousePos;
            SpaceOriginPos = SpaceOriginPos + deltaMousePos;
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
            if (e.ChangedButton == MouseButton.Right)
                return;
            activeMouseDownEventArgs = e;
            activeMouseDownPos = e.GetPosition(mainCanvas);
        }

        private void InitiateDrag(MouseButtonEventArgs e)
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
            if (e.ChangedButton == MouseButton.Right)
                return;
            if (activeDragOperation != null)
            {
                activeDragOperation?.Terminate();
                activeDragOperation = null;
            }
            else
            {
                if (ZoomMode == Mode.ZoomTool)
                    ZoomInto(e.GetPosition(mainCanvas), Keyboard.IsKeyDown(Key.LeftAlt) ? -1 : 1);
            }
            activeMouseDownEventArgs = null;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (activeDragOperation == null && activeMouseDownEventArgs != null)
            {
                var cur = e.GetPosition(mainCanvas);

                if (Math.Abs(cur.X - activeMouseDownPos.X) > 3)
                    InitiateDrag(activeMouseDownEventArgs);
            }
            activeDragOperation?.Update(e);
        }

        private void OnScroll(object sender, MouseWheelEventArgs e)
        {
            ZoomInto(e.GetPosition(mainCanvas), e.Delta / 100);
        }
    }
}
