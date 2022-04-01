using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

namespace PixiEditor.Zoombox
{
    [ContentProperty(nameof(AdditionalContent))]
    public partial class Zoombox : ContentControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty AdditionalContentProperty =
            DependencyProperty.Register(nameof(AdditionalContent), typeof(object), typeof(Zoombox),
              new PropertyMetadata(null));

        public static readonly DependencyProperty ZoomModeProperty =
            DependencyProperty.Register(nameof(ZoomMode), typeof(ZoomboxMode), typeof(Zoombox),
              new PropertyMetadata(ZoomboxMode.Normal, ZoomModeChanged));

        public static readonly DependencyProperty ZoomOutOnClickProperty =
            DependencyProperty.Register(nameof(ZoomOutOnClick), typeof(bool), typeof(Zoombox),
              new PropertyMetadata(false));

        public static readonly DependencyProperty UseTouchGesturesProperty =
            DependencyProperty.Register(nameof(UseTouchGestures), typeof(bool), typeof(Zoombox));

        public static readonly RoutedEvent ViewportMovedEvent = EventManager.RegisterRoutedEvent(
            nameof(ViewportMoved), RoutingStrategy.Bubble, typeof(EventHandler<ViewportRoutedEventArgs>), typeof(Zoombox));

        private const double zoomFactor = 1.09050773267; //2^(1/8)
        private const double maxZoom = 50;
        private double minZoom = -28;

        private double[] roundZoomValues = new double[] { .01, .02, .03, .04, .05, .06, .07, .08, .1, .13, .17, .2, .25, .33, .5, .67, 1, 1.5, 2, 3, 4, 5, 6, 8, 10, 12, 14, 16, 20, 24, 28, 32, 40, 48, 56, 64 };
        public object? AdditionalContent
        {
            get => GetValue(AdditionalContentProperty);
            set => SetValue(AdditionalContentProperty, value);
        }
        public ZoomboxMode ZoomMode
        {
            get => (ZoomboxMode)GetValue(ZoomModeProperty);
            set => SetValue(ZoomModeProperty, value);
        }

        public bool ZoomOutOnClick
        {
            get => (bool)GetValue(ZoomOutOnClickProperty);
            set => SetValue(ZoomOutOnClickProperty, value);
        }

        public bool UseTouchGestures
        {
            get => (bool)GetValue(UseTouchGesturesProperty);
            set => SetValue(UseTouchGesturesProperty, value);
        }

        public event EventHandler<ViewportRoutedEventArgs> ViewportMoved
        {
            add => AddHandler(ViewportMovedEvent, value);
            remove => RemoveHandler(ViewportMovedEvent, value);
        }

        public double Zoom => Math.Pow(zoomFactor, zoomPower);

        private Point spaceOriginPos;
        internal Point SpaceOriginPos
        {
            get => spaceOriginPos;
            set
            {
                spaceOriginPos = value;
                Canvas.SetLeft(mainGrid, spaceOriginPos.X);
                Canvas.SetTop(mainGrid, spaceOriginPos.Y);
                RaiseViewportEvent();
            }
        }

        private double zoomPower;
        internal double ZoomPowerClamped
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
                RaiseViewportEvent();
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
                RaiseViewportEvent();
            }
        }

        private IDragOperation? activeDragOperation = null;
        private MouseButtonEventArgs? activeMouseDownEventArgs = null;
        private Point activeMouseDownPos;

        public event PropertyChangedEventHandler? PropertyChanged;

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

        private void RaiseViewportEvent()
        {
            Point center = ToZoomboxSpace(new(mainCanvas.ActualWidth / 2, mainCanvas.ActualHeight / 2));
            Point topLeft = ToZoomboxSpace(new(0, 0));
            Point bottomRight = ToZoomboxSpace(new(mainCanvas.ActualWidth, mainCanvas.ActualHeight));

            RaiseEvent(new ViewportRoutedEventArgs(
                ViewportMovedEvent,
                new(center.X, center.Y),
                new(bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y),
                new(mainCanvas.ActualWidth, mainCanvas.ActualHeight),
                0));
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

        public void ZoomIntoCenter(double delta, bool round)
        {
            ZoomInto(new Point(mainCanvas.ActualWidth / 2, mainCanvas.ActualHeight / 2), delta, round);
        }

        public void ZoomInto(Point mousePos, double delta, bool round = false)
        {
            if (delta == 0)
                return;
            var oldZoomboxMousePos = ToZoomboxSpace(mousePos);

            if (round)
            {
                int curIndex = GetClosestRoundZoomValueIndex(Zoom);
                if (curIndex == 0 && delta < 0 || curIndex == roundZoomValues.Length - 1 && delta > 0)
                    return;
                int nextIndex = delta < 0 ? curIndex - 1 : curIndex + 1;
                double newZoom = roundZoomValues[nextIndex];
                ZoomPowerClamped = Math.Log(newZoom, zoomFactor);
            }
            else
            {
                ZoomPowerClamped += delta;
            }

            if (Math.Abs(ZoomPowerClamped) < 1) ZoomPowerClamped = 0;

            var shiftedMousePos = ToScreenSpace(oldZoomboxMousePos);
            var deltaMousePos = mousePos - shiftedMousePos;
            SpaceOriginPos = SpaceOriginPos + deltaMousePos;
        }

        private int GetClosestRoundZoomValueIndex(double value)
        {
            int index = -1;
            double delta = double.MaxValue;
            for (int i = 0; i < roundZoomValues.Length; i++)
            {
                double curDelta = Math.Abs(roundZoomValues[i] - value);
                if (curDelta < delta)
                {
                    delta = curDelta;
                    index = i;
                }
            }
            return index;
        }

        private void RecalculateMinZoomLevel(object sender, SizeChangedEventArgs args)
        {
            double fraction = Math.Max(
                mainCanvas.ActualWidth / mainGrid.ActualWidth,
                mainCanvas.ActualHeight / mainGrid.ActualHeight);
            minZoom = Math.Min(0, Math.Log(fraction / 8, zoomFactor));
        }

        internal Point ToScreenSpace(Point p)
        {
            double zoom = Zoom;
            p.X *= zoom;
            p.Y *= zoom;
            p += (Vector)SpaceOriginPos;
            return p;
        }

        internal Point ToZoomboxSpace(Point mousePos)
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
            if (ZoomMode == ZoomboxMode.Normal)
                return;

            activeDragOperation?.Terminate();

            if (ZoomMode == ZoomboxMode.Move)
                activeDragOperation = new MoveDragOperation(this);
            else if (ZoomMode == ZoomboxMode.Zoom)
                activeDragOperation = new ZoomDragOperation(this);
            else
                throw new InvalidOperationException("Unknown zoombox mode");

            activeDragOperation.Start(e);
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
                return;
            if (activeDragOperation is not null)
            {
                activeDragOperation.Terminate();
                activeDragOperation = null;
            }
            else
            {
                if (ZoomMode == ZoomboxMode.Zoom && e.ChangedButton == MouseButton.Left)
                    ZoomInto(e.GetPosition(mainCanvas), ZoomOutOnClick ? -1 : 1, true);
            }
            activeMouseDownEventArgs = null;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (activeDragOperation is null && activeMouseDownEventArgs is not null)
            {
                var cur = e.GetPosition(mainCanvas);

                if (Math.Abs(cur.X - activeMouseDownPos.X) > 3)
                    InitiateDrag(activeMouseDownEventArgs);
            }
            activeDragOperation?.Update(e);
        }

        private void OnScroll(object sender, MouseWheelEventArgs e)
        {
            for (int i = 0; i < Math.Abs(e.Delta / 100); i++)
            {
                ZoomInto(e.GetPosition(mainCanvas), e.Delta / 100, true);
            }
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
