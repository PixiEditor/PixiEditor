using System;
using System.Collections.Generic;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Metadata;
using Drawie.Backend.Core.Numerics;
using Drawie.Numerics;
using PixiEditor.Zoombox.Operations;
using Point = Avalonia.Point;

namespace PixiEditor.Zoombox;

public partial class Zoombox : ContentControl, INotifyPropertyChanged
{
    public static readonly StyledProperty<ZoomboxMode> ZoomModeProperty =
        AvaloniaProperty.Register<Zoombox, ZoomboxMode>(nameof(ZoomMode), defaultValue: ZoomboxMode.Normal);

    public static readonly StyledProperty<bool> ZoomOutOnClickProperty =
        AvaloniaProperty.Register<Zoombox, bool>(nameof(ZoomOutOnClick), defaultValue: false);

    public static readonly StyledProperty<bool> UseTouchGesturesProperty =
        AvaloniaProperty.Register<Zoombox, bool>(nameof(UseTouchGestures));

    public static readonly StyledProperty<double> ScaleProperty =
        AvaloniaProperty.Register<Zoombox, double>(nameof(Scale), defaultValue: 1.0);

    public static readonly StyledProperty<VecD> CenterProperty =
        AvaloniaProperty.Register<Zoombox, VecD>(nameof(Center), defaultValue: new VecD(0, 0));

    public static readonly StyledProperty<VecD> DimensionsProperty =
        AvaloniaProperty.Register<Zoombox, VecD>(nameof(Dimensions));

    public static readonly StyledProperty<VecD> RealDimensionsProperty =
        AvaloniaProperty.Register<Zoombox, VecD>(nameof(RealDimensions));

    public static readonly StyledProperty<VecD> ContentDimensionsProperty = AvaloniaProperty.Register<Zoombox, VecD>(
        nameof(ContentDimensions),
        defaultValue: new VecD(0, 0));

    public VecD ContentDimensions
    {
        get => GetValue(ContentDimensionsProperty);
        set => SetValue(ContentDimensionsProperty, value);
    }

    public static readonly StyledProperty<double> AngleRadiansProperty =
        AvaloniaProperty.Register<Zoombox, double>(nameof(AngleRadians), defaultValue: 0.0);

    public static readonly StyledProperty<bool> FlipXProperty =
        AvaloniaProperty.Register<Zoombox, bool>(nameof(FlipX), defaultValue: false);

    public static readonly StyledProperty<bool> FlipYProperty =
        AvaloniaProperty.Register<Zoombox, bool>(nameof(FlipY), defaultValue: false);

    public static readonly RoutedEvent<ViewportRoutedEventArgs> ViewportMovedEvent = RoutedEvent.Register<Zoombox, ViewportRoutedEventArgs>(
        nameof(ViewportMoved), RoutingStrategies.Bubble);

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

    public double AngleRadians
    {
        get => (double)GetValue(AngleRadiansProperty);
        set => SetValue(AngleRadiansProperty, value);
    }

    public VecD Center
    {
        get => (VecD)GetValue(CenterProperty);
        set => SetValue(CenterProperty, value);
    }

    public VecD Dimensions
    {
        get => (VecD)GetValue(DimensionsProperty);
        set => SetValue(DimensionsProperty, value);
    }

    public VecD RealDimensions
    {
        get => (VecD)GetValue(RealDimensionsProperty);
        set => SetValue(RealDimensionsProperty, value);
    }

    public event EventHandler<ViewportRoutedEventArgs> ViewportMoved
    {
        add => AddHandler(ViewportMovedEvent, value);
        remove => RemoveHandler(ViewportMovedEvent, value);
    }

    public event Action<double>? ScaleChanged;

    public VecD CanvasPos => ToScreenSpace(VecD.Zero);
    public double CanvasX => ToScreenSpace(VecD.Zero).X;
    public double CanvasY => ToScreenSpace(VecD.Zero).Y;

    public VecD Pan { get; internal set; } = VecD.Zero;

    public TransformGroup CanvasTransform => new TransformGroup
    {
        Children =
        {
            new ScaleTransform(Scale, Scale),
            new RotateTransform(AngleRadians * 180 / Math.PI),
            new ScaleTransform(FlipX ? -1 : 1, FlipY ? -1 : 1),
            new TranslateTransform(CanvasX, CanvasY),
        },
    };

    public double ScaleTransformXY => Scale;
    public double FlipTransformX => FlipX ? -1 : 1;
    public double FlipTransformY => FlipY ? -1 : 1;
    public double RotateTransformAngle => AngleRadians * 180 / Math.PI;
    internal const double MaxScale = 384;

    internal double MinScale
    {
        get
        {
            double fraction = Math.Max(Bounds.Width / ContentDimensions.X, Bounds.Height / ContentDimensions.Y);
            return Math.Min(fraction / 8, 0.1);
        }
    }

    internal const double ScaleFactor = 1.09050773267; //2^(1/8)
    internal const double ScrollStep = 0.5;

    public VecD ToScreenSpace(VecD p)
    {
        VecD delta = p - Center;
        delta = delta.Rotate(AngleRadians) * Scale;
        if (FlipX)
            delta.X = -delta.X;
        if (FlipY)
            delta.Y = -delta.Y;
        delta += new VecD(Bounds.Width / 2f, Bounds.Height / 2f);
        return delta;
    }

    public VecD ToZoomboxSpace(VecD mousePos)
    {
        VecD delta = mousePos - new VecD(Bounds.Width / 2, Bounds.Height / 2);
        if (FlipX)
            delta.X = -delta.X;
        if (FlipY)
            delta.Y = -delta.Y;
        delta = (delta / Scale).Rotate(-AngleRadians);
        return delta + Center;
    }

    private IDragOperation? activeDragOperation = null;
    private PointerEventArgs? activeMouseDownEventArgs = null;
    private VecD activeMouseDownPos;

    public event PropertyChangedEventHandler? PropertyChanged;

    private static void ZoomModeChanged(AvaloniaPropertyChangedEventArgs<ZoomboxMode> e)
    {
        Zoombox sender = (Zoombox)e.Sender;

        bool reset = sender.activeDragOperation != null;

        if (reset)
        {
            sender.activeDragOperation?.Terminate();
            sender.activeDragOperation = null;
            sender.activeMouseDownEventArgs = null;
        }
    }

    private double[]? zoomValues;

    static Zoombox()
    {
        ZoomModeProperty.Changed.Subscribe(ZoomModeChanged);
        ScaleProperty.Changed.Subscribe(OnPropertyChange);
        AngleRadiansProperty.Changed.Subscribe(OnPropertyChange);
        FlipXProperty.Changed.Subscribe(OnPropertyChange);
        FlipYProperty.Changed.Subscribe(OnPropertyChange);
        CenterProperty.Changed.Subscribe(OnPropertyChange);
    }

    public Zoombox()
    {
        CalculateZoomValues();
        PointerWheelChanged += OnScroll;
        PointerPressed += OnMouseDown;
        PointerReleased += OnMouseUp;
        PointerMoved += OnMouseMove;
        Loaded += (_, _) => OnPropertyChange(this);
    }

    private void CalculateZoomValues()
    {
        Span<double> roundZoomValues = stackalloc[]
        {
            .01,
            .02,
            .03,
            .04,
            .05,
            .06,
            .07,
            .08,
            .1,
            .13,
            .17,
            .2,
            .25,
            .33,
            .5,
            .67,
            1,
            1.5,
            2,
            3,
            4,
            5,
            6,
            8,
            10,
            12,
            14,
            16,
            20,
            24,
            28,
            32,
            40,
            48,
            56,
            64,
            80,
            96,
            112,
            128,
            160,
            192,
            224,
            256,
            320,
            384,
        };
        List<double> interpolatedValues = new();
        for (int i = 0; i < roundZoomValues.Length - 1; i++)
        {
            double cur = roundZoomValues[i];
            double next = roundZoomValues[i + 1];
            const int steps = 3;
            for (int j = 0; j < steps; j++)
            {
                double fraction = j / (double)steps;
                interpolatedValues.Add((next - cur) * fraction + cur);
            }
        }
        interpolatedValues.Add(roundZoomValues[^1]);
        zoomValues = interpolatedValues.ToArray();
    }

    private void RaiseViewportEvent()
    {
        VecD realDim = new VecD(Bounds.Width, Bounds.Height);
        RealDimensions = realDim;
        ScaleChanged?.Invoke(Scale);
        RaiseEvent(new ViewportRoutedEventArgs(
            ViewportMovedEvent,
            Center,
            Dimensions,
            realDim,
            AngleRadians));
    }

    public void CenterContent() => CenterContent(new(ContentDimensions.X, ContentDimensions.Y));

    public void CenterContent(VecD newSize)
    {
        const double marginFactor = 1.1;
        double scaleFactor = Math.Max(
            newSize.X * marginFactor / Bounds.Width,
            newSize.Y * marginFactor / Bounds.Height);

        AngleRadians = 0;
        FlipX = false;
        FlipY = false;
        Scale = Math.Clamp(1 / scaleFactor, MinScale, MaxScale);
        Center = newSize / 2;
        Pan = VecD.Zero;
    }

    public void ZoomIntoCenter(double delta)
    {
        ZoomInto(new VecD(Bounds.Width / 2, Bounds.Height / 2), delta);
    }

    public void ZoomInto(VecD position, double delta)
    {
        if (delta == 0)
            return;
        VecD oldZoomboxPosition = ToZoomboxSpace(position);

        int curIndex = GetClosestRoundZoomValueIndex(Scale);
        int nextIndex = curIndex;
        if (!((curIndex == 0 && delta < 0) || (curIndex == zoomValues!.Length - 1 && delta > 0)))
            nextIndex = delta < 0 ? curIndex - 1 : curIndex + 1;
        double newScale = zoomValues![nextIndex];

        if (Math.Abs(newScale - 1) < 0.1) newScale = 1;
        newScale = Math.Clamp(newScale, MinScale, MaxScale);
        Scale = newScale;

        VecD newZoomboxMousePosition = ToZoomboxSpace(position);
        VecD deltaCenter = oldZoomboxPosition - newZoomboxMousePosition;
        Center += deltaCenter;
    }

    private int GetClosestRoundZoomValueIndex(double value)
    {
        int index = -1;
        double delta = double.MaxValue;
        for (int i = 0; i < zoomValues!.Length; i++)
        {
            double curDelta = Math.Abs(zoomValues[i] - value);
            if (curDelta < delta)
            {
                delta = curDelta;
                index = i;
            }
        }
        return index;
    }

    private void OnMouseDown(object? sender, PointerPressedEventArgs e)
    {
        MouseButton but = e.GetCurrentPoint(this).Properties.PointerUpdateKind switch
        {
            PointerUpdateKind.LeftButtonPressed => MouseButton.Left,
            PointerUpdateKind.RightButtonPressed => MouseButton.Right,
            PointerUpdateKind.MiddleButtonPressed => MouseButton.Middle,
            _ => MouseButton.None,
        };

        if (but == MouseButton.Right)
            return;
        activeMouseDownEventArgs = e;
        activeMouseDownPos = ToVecD(e.GetPosition(this));
        Focus();
    }

    private void InitiateDrag(PointerEventArgs e)
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

    private void OnMouseUp(object? sender, PointerReleasedEventArgs e)
    {
        if (e.InitialPressMouseButton == MouseButton.Right)
            return;
        if (activeDragOperation is not null)
        {
            activeDragOperation.Terminate();
            activeDragOperation = null;
        }
        else
        {
            if (ZoomMode == ZoomboxMode.Zoom && e.InitialPressMouseButton == MouseButton.Left)
                ZoomInto(ToVecD(e.GetPosition(this)), ZoomOutOnClick ? -1 : 1);
        }
        activeMouseDownEventArgs = null;
    }

    private void OnMouseMove(object? sender, PointerEventArgs e)
    {
        if (activeDragOperation is null && activeMouseDownEventArgs is not null)
        {
            VecD cur = ToVecD(e.GetPosition(this));

            if ((cur - activeMouseDownPos).TaxicabLength > 3)
                InitiateDrag(activeMouseDownEventArgs);
        }
        activeDragOperation?.Update(e);
    }

    private void OnScroll(object? sender, PointerWheelEventArgs e)
    {
        double abs = Math.Abs(e.Delta.Y / ScrollStep);
        for (int i = 0; i < abs; i++)
        {
            ZoomInto(ToVecD(e.GetPosition(this)), e.Delta.Y / ScrollStep);
        }
    }


    private ManipulationOperation? activeManipulationOperation;

    /* TODO: Avalonia uses Pointer events for both mouse and touch, so we can't use this, would be cool to Implement UseTouchGestures
    private void OnManipulationStarted(object? sender, ManipulationStartedEventArgs e)
    {
        if (!UseTouchGestures || activeManipulationOperation is not null)
            return;
        activeManipulationOperation = new ManipulationOperation(this);
        activeManipulationOperation.Start();
    }

    private void OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
    {
        if (!UseTouchGestures || activeManipulationOperation is null)
            return;
        activeManipulationOperation.Update(e);
    }

    private void OnManipulationCompleted(object? sender, ManipulationCompletedEventArgs e)
    {
        if (!UseTouchGestures || activeManipulationOperation is null)
            return;
        activeManipulationOperation = null;
    }
    */

    internal static VecD ToVecD(Point point) => new VecD(point.X, point.Y);

    private static void OnPropertyChange(AvaloniaPropertyChangedEventArgs e)
    {
        Zoombox? zoombox = (Zoombox)e.Sender;
        OnPropertyChange(zoombox);
    }

    private static void OnPropertyChange(Zoombox zoombox)
    {
        VecD topLeft = zoombox.ToZoomboxSpace(VecD.Zero).Rotate(zoombox.AngleRadians);
        VecD bottomRight = zoombox.ToZoomboxSpace(new(zoombox.Bounds.Width, zoombox.Bounds.Height))
            .Rotate(zoombox.AngleRadians);

        zoombox.Dimensions = (bottomRight - topLeft).Abs();
        zoombox.PropertyChanged?.Invoke(zoombox, new(nameof(zoombox.ScaleTransformXY)));
        zoombox.PropertyChanged?.Invoke(zoombox, new(nameof(zoombox.RotateTransformAngle)));
        zoombox.PropertyChanged?.Invoke(zoombox, new(nameof(zoombox.FlipTransformX)));
        zoombox.PropertyChanged?.Invoke(zoombox, new(nameof(zoombox.FlipTransformY)));
        zoombox.PropertyChanged?.Invoke(zoombox, new(nameof(zoombox.CanvasX)));
        zoombox.PropertyChanged?.Invoke(zoombox, new(nameof(zoombox.CanvasY)));
        zoombox.PropertyChanged?.Invoke(zoombox, new(nameof(zoombox.CanvasPos)));
        zoombox.PropertyChanged?.Invoke(zoombox, new(nameof(zoombox.CanvasTransform)));
        zoombox.RaiseViewportEvent();
    }
}
