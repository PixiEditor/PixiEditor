using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ChunkyImageLib.DataHolders;
using PixiEditor.Zoombox;
using PixiEditorPrototype.Models;
using PixiEditorPrototype.ViewModels;

namespace PixiEditorPrototype.UserControls.Viewport;

internal partial class Viewport : UserControl, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public static readonly DependencyProperty FlipXProperty =
        DependencyProperty.Register(nameof(FlipX), typeof(bool), typeof(Viewport), new(false));

    public static readonly DependencyProperty FlipYProperty =
        DependencyProperty.Register(nameof(FlipY), typeof(bool), typeof(Viewport), new(false));

    public static readonly DependencyProperty ZoomModeProperty =
        DependencyProperty.Register(nameof(ZoomMode), typeof(ZoomboxMode), typeof(Viewport), new(ZoomboxMode.Normal));

    public static readonly DependencyProperty DocumentProperty =
        DependencyProperty.Register(nameof(Document), typeof(DocumentViewModel), typeof(Viewport), new(null, OnDocumentChange));

    public static readonly DependencyProperty MouseDownCommandProperty =
        DependencyProperty.Register(nameof(MouseDownCommand), typeof(ICommand), typeof(Viewport), new(null));

    public static readonly DependencyProperty MouseMoveCommandProperty =
        DependencyProperty.Register(nameof(MouseMoveCommand), typeof(ICommand), typeof(Viewport), new(null));

    public static readonly DependencyProperty MouseUpCommandProperty =
            DependencyProperty.Register(nameof(MouseUpCommand), typeof(ICommand), typeof(Viewport), new(null));

    private static readonly DependencyProperty BitmapsProperty =
        DependencyProperty.Register(nameof(Bitmaps), typeof(Dictionary<ChunkResolution, WriteableBitmap>), typeof(Viewport), new(null, OnBitmapsChange));

    public static readonly DependencyProperty DelayedProperty = DependencyProperty.Register(
        nameof(Delayed), typeof(bool), typeof(Viewport), new PropertyMetadata(false));

    public bool Delayed
    {
        get => (bool)GetValue(DelayedProperty);
        set => SetValue(DelayedProperty, value);
    }
    
    public Dictionary<ChunkResolution, WriteableBitmap>? Bitmaps
    {
        get => (Dictionary<ChunkResolution, WriteableBitmap>?)GetValue(BitmapsProperty);
        set => SetValue(BitmapsProperty, value);
    }

    public ICommand? MouseDownCommand
    {
        get => (ICommand?)GetValue(MouseDownCommandProperty);
        set => SetValue(MouseDownCommandProperty, value);
    }

    public ICommand? MouseMoveCommand
    {
        get => (ICommand?)GetValue(MouseMoveCommandProperty);
        set => SetValue(MouseMoveCommandProperty, value);
    }

    public ICommand? MouseUpCommand
    {
        get => (ICommand?)GetValue(MouseUpCommandProperty);
        set => SetValue(MouseUpCommandProperty, value);
    }


    public DocumentViewModel? Document
    {
        get => (DocumentViewModel)GetValue(DocumentProperty);
        set => SetValue(DocumentProperty, value);
    }

    public ZoomboxMode ZoomMode
    {
        get => (ZoomboxMode)GetValue(ZoomModeProperty);
        set => SetValue(ZoomModeProperty, value);
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

    private double angle = 0;
    public double Angle
    {
        get => angle;
        set
        {
            angle = value;
            PropertyChanged?.Invoke(this, new(nameof(Angle)));
            Document?.AddOrUpdateViewport(GetLocation());
        }
    }

    private VecD center = new(32, 32);
    public VecD Center
    {
        get => center;
        set
        {
            center = value;
            PropertyChanged?.Invoke(this, new(nameof(Center)));
            Document?.AddOrUpdateViewport(GetLocation());
        }
    }

    private VecD realDimensions = new(double.MaxValue, double.MaxValue);
    public VecD RealDimensions
    {
        get => realDimensions;
        set
        {
            var oldRes = CalculateResolution();
            realDimensions = value;
            var newRes = CalculateResolution();

            PropertyChanged?.Invoke(this, new(nameof(RealDimensions)));
            Document?.AddOrUpdateViewport(GetLocation());

            if (oldRes != newRes)
                PropertyChanged?.Invoke(this, new(nameof(TargetBitmap)));
        }
    }

    private VecD dimensions = new(64, 64);
    public VecD Dimensions
    {
        get => dimensions;
        set
        {
            var oldRes = CalculateResolution();
            dimensions = value;
            var newRes = CalculateResolution();

            PropertyChanged?.Invoke(this, new(nameof(Dimensions)));
            Document?.AddOrUpdateViewport(GetLocation());

            if (oldRes != newRes)
                PropertyChanged?.Invoke(this, new(nameof(TargetBitmap)));
        }
    }

    public WriteableBitmap? TargetBitmap
    {
        get
        {
            return Document?.Bitmaps.TryGetValue(CalculateResolution(), out var value) == true ? value : null;
        }
    }

    public Zoombox Zoombox => zoombox;

    public Guid GuidValue { get; } = Guid.NewGuid();

    public Viewport()
    {
        InitializeComponent();

        Binding binding = new Binding();
        binding.Source = this;
        binding.Path = new PropertyPath("Document.Bitmaps");
        SetBinding(BitmapsProperty, binding);

        Loaded += OnLoad;
        Unloaded += OnUnload;
    }

    private Image? GetImage() => (Image?)((Grid?)((Border?)zoombox.AdditionalContent)?.Child)?.Children[0];
    private void ForceRefreshFinalImage()
    {
        GetImage()?.InvalidateVisual();
    }

    private void OnUnload(object sender, RoutedEventArgs e)
    {
        Document?.RemoveViewport(GuidValue);
    }

    private void OnLoad(object sender, RoutedEventArgs e)
    {
        Document?.AddOrUpdateViewport(GetLocation());
    }

    private static void OnDocumentChange(DependencyObject viewportObj, DependencyPropertyChangedEventArgs args)
    {
        var oldDoc = (DocumentViewModel?)args.OldValue;
        var newDoc = (DocumentViewModel?)args.NewValue;
        var viewport = (Viewport)viewportObj;
        oldDoc?.RemoveViewport(viewport.GuidValue);
        newDoc?.AddOrUpdateViewport(viewport.GetLocation());
    }

    private static void OnBitmapsChange(DependencyObject viewportObj, DependencyPropertyChangedEventArgs args)
    {
        ((Viewport)viewportObj).PropertyChanged?.Invoke(viewportObj, new(nameof(TargetBitmap)));
    }

    private ChunkResolution CalculateResolution()
    {
        VecD densityVec = Dimensions.Divide(RealDimensions);
        double density = Math.Min(densityVec.X, densityVec.Y);
        if (density > 8.01)
            return ChunkResolution.Eighth;
        else if (density > 4.01)
            return ChunkResolution.Quarter;
        else if (density > 2.01)
            return ChunkResolution.Half;
        return ChunkResolution.Full;
    }

    private ViewportInfo GetLocation()
    {
        return new(Angle, Center, RealDimensions / 2, Dimensions / 2, CalculateResolution(), GuidValue, Delayed, ForceRefreshFinalImage);
    }
}
