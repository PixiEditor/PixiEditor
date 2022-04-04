using ChunkyImageLib.DataHolders;
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


        public static readonly DependencyProperty ScaleProperty =
            DependencyProperty.Register(nameof(Scale), typeof(double), typeof(Zoombox), new(1.0, OnPropertyChange));

        public static readonly DependencyProperty CenterProperty =
            DependencyProperty.Register(nameof(Center), typeof(Vector2d), typeof(Zoombox), new(new Vector2d(0, 0), OnPropertyChange));

        public static readonly DependencyProperty DimensionsProperty =
            DependencyProperty.Register(nameof(Dimensions), typeof(Vector2d), typeof(Zoombox));

        public static readonly DependencyProperty AngleProperty =
            DependencyProperty.Register(nameof(Angle), typeof(double), typeof(Zoombox), new(0.0, OnPropertyChange));

        public static readonly DependencyProperty FlipXProperty =
            DependencyProperty.Register(nameof(FlipX), typeof(bool), typeof(Zoombox), new(false, OnPropertyChange));

        public static readonly DependencyProperty FlipYProperty =
            DependencyProperty.Register(nameof(FlipY), typeof(bool), typeof(Zoombox), new(false, OnPropertyChange));

        public static readonly RoutedEvent ViewportMovedEvent = EventManager.RegisterRoutedEvent(
            nameof(ViewportMoved), RoutingStrategy.Bubble, typeof(EventHandler<ViewportRoutedEventArgs>), typeof(Zoombox));

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

        public bool FlipX
        {
            get => (bool)GetValue(FlipXProperty);
            set => SetValue(FlipXProperty, value);
        }

        public bool FlipY
        {
            get => (bool)GetValue(FlipYProperty);
            set => SetValue(FlipYProperty, value);
        }

        public double Scale
        {
            get => (double)GetValue(ScaleProperty);
            set => SetValue(ScaleProperty, value);
        }

        public double Angle
        {
            get => (double)GetValue(AngleProperty);
            set => SetValue(AngleProperty, value);
        }

        public Vector2d Center
        {
            get => (Vector2d)GetValue(CenterProperty);
            set => SetValue(CenterProperty, value);
        }

        public Vector2d Dimensions
        {
            get => (Vector2d)GetValue(DimensionsProperty);
            set => SetValue(DimensionsProperty, value);
        }

        public event EventHandler<ViewportRoutedEventArgs> ViewportMoved
        {
            add => AddHandler(ViewportMovedEvent, value);
            remove => RemoveHandler(ViewportMovedEvent, value);
        }

        public double CanvasX => ToScreenSpace(new(0, 0)).X;
        public double CanvasY => ToScreenSpace(new(0, 0)).Y;

        public double ScaleTransformXY => Scale;
        public double FlipTransformX => FlipX ? -1 : 1;
        public double FlipTransformY => FlipY ? -1 : 1;
        public double RotateTransformAngle => Angle * 180 / Math.PI;
        internal const double MaxScale = 70;
        internal double MinScale
        {
            get
            {
                double fraction = Math.Max(
                    mainCanvas.ActualWidth / mainGrid.ActualWidth,
                    mainCanvas.ActualHeight / mainGrid.ActualHeight);
                return Math.Min(fraction / 8, 0.1);
            }
        }

        internal const double ScaleFactor = 1.09050773267; //2^(1/8)

        private double[] roundZoomValues = new double[] { .01, .02, .03, .04, .05, .06, .07, .08, .1, .13, .17, .2, .25, .33, .5, .67, 1, 1.5, 2, 3, 4, 5, 6, 8, 10, 12, 14, 16, 20, 24, 28, 32, 40, 48, 56, 64 };

        internal Vector2d ToScreenSpace(Vector2d p)
        {
            Vector2d delta = p - Center;
            delta = delta.Rotate(Angle) * Scale;
            if (FlipX)
                delta.X = -delta.X;
            if (FlipY)
                delta.Y = -delta.Y;
            delta += new Vector2d(mainCanvas.ActualWidth / 2, mainCanvas.ActualHeight / 2);
            return delta;
        }

        internal Vector2d ToZoomboxSpace(Vector2d mousePos)
        {
            Vector2d delta = mousePos - new Vector2d(mainCanvas.ActualWidth / 2, mainCanvas.ActualHeight / 2);
            if (FlipX)
                delta.X = -delta.X;
            if (FlipY)
                delta.Y = -delta.Y;
            delta = (delta / Scale).Rotate(-Angle);
            return delta + Center;
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
            Loaded += (_, _) => OnPropertyChange(this, new DependencyPropertyChangedEventArgs());
        }

        private void RaiseViewportEvent()
        {
            RaiseEvent(new ViewportRoutedEventArgs(
                ViewportMovedEvent,
                Center,
                Dimensions,
                new(mainCanvas.ActualWidth, mainCanvas.ActualHeight),
                Angle));
        }

        public void CenterContent() => CenterContent(new(mainGrid.ActualWidth, mainGrid.ActualHeight));

        public void CenterContent(Vector2d newSize)
        {

            const double marginFactor = 1.1;
            double scaleFactor = Math.Max(
                newSize.X * marginFactor / mainCanvas.ActualWidth,
                newSize.Y * marginFactor / mainCanvas.ActualHeight);

            Angle = 0;
            FlipX = false;
            FlipY = false;
            Scale = scaleFactor;
            Center = newSize / 2;
        }

        public void ZoomIntoCenter(double delta)
        {
            ZoomInto(new Vector2d(mainCanvas.ActualWidth / 2, mainCanvas.ActualHeight / 2), delta);
        }

        public void ZoomInto(Vector2d mousePos, double delta)
        {
            if (delta == 0)
                return;
            var oldZoomboxMousePos = ToZoomboxSpace(mousePos);

            int curIndex = GetClosestRoundZoomValueIndex(Scale);
            int nextIndex = curIndex;
            if (!(curIndex == 0 && delta < 0 || curIndex == roundZoomValues.Length - 1 && delta > 0))
                nextIndex = delta < 0 ? curIndex - 1 : curIndex + 1;
            double newScale = roundZoomValues[nextIndex];

            if (Math.Abs(newScale - 1) < 0.1) newScale = 1;
            newScale = Math.Clamp(newScale, MinScale, MaxScale);
            Scale = newScale;

            var newZoomboxMousePos = ToZoomboxSpace(mousePos);
            var deltaCenter = oldZoomboxMousePos - newZoomboxMousePos;
            Center += deltaCenter;
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
            else if (ZoomMode == ZoomboxMode.Rotate)
                activeDragOperation = new RotateDragOperation(this);
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
                    ZoomInto(ToVector2d(e.GetPosition(mainCanvas)), ZoomOutOnClick ? -1 : 1);
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
                ZoomInto(ToVector2d(e.GetPosition(mainCanvas)), e.Delta / 100);
            }
        }

        private void OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            if (!UseTouchGestures)
                return;
            e.Handled = true;
            Vector2d screenTranslation = new(e.DeltaManipulation.Translation.X, e.DeltaManipulation.Translation.Y);
            Vector2d screenOrigin = new(e.ManipulationOrigin.X, e.ManipulationOrigin.Y);
            Manipulate(e.DeltaManipulation.Scale.X, screenTranslation, screenOrigin, e.DeltaManipulation.Rotation / 180 * Math.PI);
        }

        private void Manipulate(double deltaScale, Vector2d screenTranslation, Vector2d screenOrigin, double rotation)
        {
            double newScale = Math.Clamp(Scale * deltaScale, MinScale, MaxScale);
            double newAngle = Angle + rotation;

            Vector2d originalPos = ToZoomboxSpace(screenOrigin);
            Angle = newAngle;
            Scale = newScale;
            Vector2d newPos = ToZoomboxSpace(screenOrigin);
            Vector2d centerTranslation = originalPos - newPos;
            Center += centerTranslation;

            Vector2d translatedZoomboxPos = ToZoomboxSpace(screenOrigin + screenTranslation);
            Center -= translatedZoomboxPos - originalPos;
        }

        internal static Vector2d ToVector2d(Point point) => new Vector2d(point.X, point.Y);

        private static void OnPropertyChange(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var zoombox = (Zoombox)obj;

            Vector2d topLeft = zoombox.ToZoomboxSpace(new(0, 0)).Rotate(zoombox.Angle);
            Vector2d bottomRight = zoombox.ToZoomboxSpace(new(zoombox.mainCanvas.ActualWidth, zoombox.mainCanvas.ActualHeight)).Rotate(zoombox.Angle);

            zoombox.Dimensions = (bottomRight - topLeft).Abs();
            zoombox.PropertyChanged?.Invoke(zoombox, new(nameof(zoombox.ScaleTransformXY)));
            zoombox.PropertyChanged?.Invoke(zoombox, new(nameof(zoombox.RotateTransformAngle)));
            zoombox.PropertyChanged?.Invoke(zoombox, new(nameof(zoombox.FlipTransformX)));
            zoombox.PropertyChanged?.Invoke(zoombox, new(nameof(zoombox.FlipTransformY)));
            zoombox.PropertyChanged?.Invoke(zoombox, new(nameof(zoombox.CanvasX)));
            zoombox.PropertyChanged?.Invoke(zoombox, new(nameof(zoombox.CanvasY)));
            zoombox.RaiseViewportEvent();
        }

        private void OnMainCanvasSizeChanged(object sender, SizeChangedEventArgs e)
        {
            RaiseViewportEvent();
        }

        private void OnGridSizeChanged(object sender, SizeChangedEventArgs args)
        {
            RaiseViewportEvent();
        }
    }
}
