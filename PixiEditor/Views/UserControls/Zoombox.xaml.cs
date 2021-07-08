using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

namespace PixiEditor.Views.UserControls
{
    [ContentProperty(nameof(AdditionalContent))]
    public partial class Zoombox : ContentControl, INotifyPropertyChanged
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
                prevMousePos = curMousePos;
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
                initZoomPower = parent.ZoomPowerClamped;
                initSpaceOriginPos = parent.SpaceOriginPos;
                parent.mainCanvas.CaptureMouse();
            }

            public void Update(MouseEventArgs e)
            {
                var curScreenPos = e.GetPosition(parent.mainCanvas);
                double deltaX = screenZoomOrigin.X - curScreenPos.X;
                double deltaPower = deltaX / 10.0;
                parent.ZoomPowerClamped = initZoomPower - deltaPower;

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

        public static readonly DependencyProperty UseTouchGesturesProperty =
            DependencyProperty.Register(nameof(UseTouchGestures), typeof(bool), typeof(Zoombox));

        private const double zoomFactor = 1.1;
        private const double maxZoom = 50;
        private double minZoom = -28;
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

        public bool UseTouchGestures
        {
            get => (bool)GetValue(UseTouchGesturesProperty);
            set => SetValue(UseTouchGesturesProperty, value);
        }

        public double Zoom => Math.Pow(zoomFactor, zoomPower);

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
        private double ZoomPowerClamped
        {
            get => zoomPower;
            set
            {
                value = Math.Clamp(value, minZoom, maxZoom);
                if (value == zoomPower)
                    return;
                zoomPower = value;
                var mult = Zoom;
                scaleTransform.ScaleX = mult;
                scaleTransform.ScaleY = mult;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Zoom)));
            }
        }
        private double ZoomPowerTopCapped
        {
            get => zoomPower;
            set
            {
                if (value > maxZoom)
                    value = maxZoom;
                if (value == zoomPower)
                    return;
                zoomPower = value;
                var mult = Zoom;
                scaleTransform.ScaleX = mult;
                scaleTransform.ScaleY = mult;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Zoom)));
            }
        }

        private IDragOperation activeDragOperation = null;
        private MouseButtonEventArgs activeMouseDownEventArgs = null;
        private Point activeMouseDownPos;

        public event PropertyChangedEventHandler PropertyChanged;

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

        public void CenterContent() => CenterContent(new Size(mainGrid.ActualWidth, mainGrid.ActualHeight));

        public void CenterContent(Size newSize)
        {
            const double marginFactor = 1.1;
            double scaleFactor = Math.Max(
                newSize.Width * marginFactor / mainCanvas.ActualWidth,
                newSize.Height * marginFactor / mainCanvas.ActualHeight);
            ZoomPowerTopCapped = -Math.Log(scaleFactor, zoomFactor);
            SpaceOriginPos = new Point(
                mainCanvas.ActualWidth / 2 - newSize.Width * Zoom / 2,
                mainCanvas.ActualHeight / 2 - newSize.Height * Zoom / 2);
        }

        public void ZoomIntoCenter(double delta)
        {
            ZoomInto(new Point(mainCanvas.ActualWidth / 2, mainCanvas.ActualHeight / 2), delta);
        }

        public void ZoomInto(Point mousePos, double delta)
        {
            var oldZoomboxMousePos = ToZoomboxSpace(mousePos);

            ZoomPowerClamped += delta;

            if (Math.Abs(ZoomPowerClamped) < 1) ZoomPowerClamped = 0;

            var shiftedMousePos = ToScreenSpace(oldZoomboxMousePos);
            var deltaMousePos = mousePos - shiftedMousePos;
            SpaceOriginPos = SpaceOriginPos + deltaMousePos;
        }

        private void RecalculateMinZoomLevel(object sender, SizeChangedEventArgs args)
        {
            double fraction = Math.Max(
                mainCanvas.ActualWidth / mainGrid.ActualWidth,
                mainCanvas.ActualHeight / mainGrid.ActualHeight);
            minZoom = Math.Min(0, Math.Log(fraction / 8, zoomFactor));
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
            Keyboard.Focus(this);
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
                activeDragOperation.Terminate();
                activeDragOperation = null;
            }
            else
            {
                if (ZoomMode == Mode.ZoomTool && e.ChangedButton == MouseButton.Left)
                    ZoomInto(e.GetPosition(mainCanvas), Keyboard.IsKeyDown(Key.LeftCtrl) ? -1 : 1);
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

        private void OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            if (e.Handled = UseTouchGestures)
            {
                ZoomInto(e.ManipulationOrigin, e.DeltaManipulation.Expansion.X / 5.0);
                SpaceOriginPos += e.DeltaManipulation.Translation;
            }
        }
    }
}
