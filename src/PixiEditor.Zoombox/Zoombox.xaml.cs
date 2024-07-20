﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using ChunkyImageLib.DataHolders;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Zoombox.Operations;

namespace PixiEditor.Zoombox;

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
        DependencyProperty.Register(nameof(Center), typeof(VecD), typeof(Zoombox), new(new VecD(0, 0), OnPropertyChange));

    public static readonly DependencyProperty DimensionsProperty =
        DependencyProperty.Register(nameof(Dimensions), typeof(VecD), typeof(Zoombox));

    public static readonly DependencyProperty RealDimensionsProperty =
        DependencyProperty.Register(nameof(RealDimensions), typeof(VecD), typeof(Zoombox));

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

    public double CanvasX => ToScreenSpace(VecD.Zero).X;
    public double CanvasY => ToScreenSpace(VecD.Zero).Y;

    public double ScaleTransformXY => Scale;
    public double FlipTransformX => FlipX ? -1 : 1;
    public double FlipTransformY => FlipY ? -1 : 1;
    public double RotateTransformAngle => Angle * 180 / Math.PI;
    internal const double MaxScale = 384;

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

    internal VecD ToScreenSpace(VecD p)
    {
        VecD delta = p - Center;
        delta = delta.Rotate(Angle) * Scale;
        if (FlipX)
            delta.X = -delta.X;
        if (FlipY)
            delta.Y = -delta.Y;
        delta += new VecD(mainCanvas.ActualWidth / 2, mainCanvas.ActualHeight / 2);
        return delta;
    }

    internal VecD ToZoomboxSpace(VecD mousePos)
    {
        VecD delta = mousePos - new VecD(mainCanvas.ActualWidth / 2, mainCanvas.ActualHeight / 2);
        if (FlipX)
            delta.X = -delta.X;
        if (FlipY)
            delta.Y = -delta.Y;
        delta = (delta / Scale).Rotate(-Angle);
        return delta + Center;
    }

    private IDragOperation? activeDragOperation = null;
    private MouseButtonEventArgs? activeMouseDownEventArgs = null;
    private VecD activeMouseDownPos;

    public event PropertyChangedEventHandler? PropertyChanged;

    private static void ZoomModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        Zoombox sender = (Zoombox)d;
        sender.activeDragOperation?.Terminate();
        sender.activeDragOperation = null;
        sender.activeMouseDownEventArgs = null;
    }

    private double[]? zoomValues;

    public Zoombox()
    {
        CalculateZoomValues();
        InitializeComponent();
        Loaded += (_, _) => OnPropertyChange(this, new DependencyPropertyChangedEventArgs());
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
        VecD realDim = new VecD(mainCanvas.ActualWidth, mainCanvas.ActualHeight);
        RealDimensions = realDim;
        RaiseEvent(new ViewportRoutedEventArgs(
            ViewportMovedEvent,
            Center,
            Dimensions,
            realDim,
            Angle));
    }

    public void CenterContent() => CenterContent(new(mainGrid.ActualWidth, mainGrid.ActualHeight));

    public void CenterContent(VecD newSize)
    {
        const double marginFactor = 1.1;
        double scaleFactor = Math.Max(
            newSize.X * marginFactor / mainCanvas.ActualWidth,
            newSize.Y * marginFactor / mainCanvas.ActualHeight);

        Angle = 0;
        FlipX = false;
        FlipY = false;
        Scale = Math.Clamp(1 / scaleFactor, MinScale, MaxScale);
        Center = newSize / 2;
    }

    public void ZoomIntoCenter(double delta)
    {
        ZoomInto(new VecD(mainCanvas.ActualWidth / 2, mainCanvas.ActualHeight / 2), delta);
    }

    public void ZoomInto(VecD mousePos, double delta)
    {
        if (delta == 0)
            return;
        VecD oldZoomboxMousePos = ToZoomboxSpace(mousePos);

        int curIndex = GetClosestRoundZoomValueIndex(Scale);
        int nextIndex = curIndex;
        if (!((curIndex == 0 && delta < 0) || (curIndex == zoomValues!.Length - 1 && delta > 0)))
            nextIndex = delta < 0 ? curIndex - 1 : curIndex + 1;
        double newScale = zoomValues![nextIndex];

        if (Math.Abs(newScale - 1) < 0.1) newScale = 1;
        newScale = Math.Clamp(newScale, MinScale, MaxScale);
        Scale = newScale;

        VecD newZoomboxMousePos = ToZoomboxSpace(mousePos);
        VecD deltaCenter = oldZoomboxMousePos - newZoomboxMousePos;
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

    private void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Right)
            return;
        activeMouseDownEventArgs = e;
        activeMouseDownPos = ToVecD(e.GetPosition(mainCanvas));
        Keyboard.Focus(this);
    }

    private void InitiateDrag(MouseButtonEventArgs e)
    {
        if (ZoomMode == ZoomboxMode.Normal)
            return;

        activeDragOperation?.Terminate();

        switch (ZoomMode)
        {
            case ZoomboxMode.Move:
                activeDragOperation = new MoveDragOperation(this); break;
            case  ZoomboxMode.Zoom:
                activeDragOperation = new ZoomDragOperation(this); break;
            case  ZoomboxMode.Rotate:
                activeDragOperation = new RotateDragOperation(this); break;
            default:
                throw new InvalidOperationException("Unknown zoombox mode");
        }
        
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
                ZoomInto(ToVecD(e.GetPosition(mainCanvas)), ZoomOutOnClick ? -1 : 1);
        }
        activeMouseDownEventArgs = null;
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (activeDragOperation is null && activeMouseDownEventArgs is not null)
        {
            VecD cur = ToVecD(e.GetPosition(mainCanvas));

            if ((cur - activeMouseDownPos).TaxicabLength > 3)
                InitiateDrag(activeMouseDownEventArgs);
        }
        activeDragOperation?.Update(e);
    }

    private void OnScroll(object sender, MouseWheelEventArgs e)
    {
        double abs = Math.Abs(e.Delta / 100.0);
        for (int i = 0; i < abs; i++)
        {
            ZoomInto(ToVecD(e.GetPosition(mainCanvas)), e.Delta / 100.0);
        }
    }


    private ManipulationOperation? activeManipulationOperation;

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

    internal static VecD ToVecD(Point point) => new VecD(point.X, point.Y);

    private static void OnPropertyChange(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
        Zoombox? zoombox = (Zoombox)obj;

        VecD topLeft = zoombox.ToZoomboxSpace(VecD.Zero).Rotate(zoombox.Angle);
        VecD bottomRight = zoombox.ToZoomboxSpace(new(zoombox.mainCanvas.ActualWidth, zoombox.mainCanvas.ActualHeight)).Rotate(zoombox.Angle);

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
        OnPropertyChange(this, new DependencyPropertyChangedEventArgs());
        RaiseViewportEvent();
    }

    private void OnGridSizeChanged(object sender, SizeChangedEventArgs args)
    {
        OnPropertyChange(this, new DependencyPropertyChangedEventArgs());
        RaiseViewportEvent();
    }
}
