using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ChunkyImageLib.DataHolders;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Helpers;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.Events;
using PixiEditor.Models.Position;
using PixiEditor.Numerics;
using PixiEditor.ViewModels.SubViewModels.Document;
using PixiEditor.Zoombox;
using Point = System.Windows.Point;

namespace PixiEditor.Views.UserControls;

#nullable enable
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

    public static readonly DependencyProperty GridLinesVisibleProperty =
        DependencyProperty.Register(nameof(GridLinesVisible), typeof(bool), typeof(Viewport), new(false));

    public static readonly DependencyProperty ZoomOutOnClickProperty =
        DependencyProperty.Register(nameof(ZoomOutOnClick), typeof(bool), typeof(Viewport), new(false));

    public static readonly DependencyProperty ZoomViewportTriggerProperty =
        DependencyProperty.Register(nameof(ZoomViewportTrigger), typeof(ExecutionTrigger<double>), typeof(Viewport), new(null, ZoomViewportTriggerChanged));

    public static readonly DependencyProperty CenterViewportTriggerProperty =
        DependencyProperty.Register(nameof(CenterViewportTrigger), typeof(ExecutionTrigger<VecI>), typeof(Viewport), new(null, CenterViewportTriggerChanged));

    public static readonly DependencyProperty UseTouchGesturesProperty =
        DependencyProperty.Register(nameof(UseTouchGestures), typeof(bool), typeof(Viewport), new(false));

    public static readonly DependencyProperty StylusButtonDownCommandProperty =
        DependencyProperty.Register(nameof(StylusButtonDownCommand), typeof(ICommand), typeof(Viewport), new(null));

    public static readonly DependencyProperty StylusButtonUpCommandProperty =
        DependencyProperty.Register(nameof(StylusButtonUpCommand), typeof(ICommand), typeof(Viewport), new(null));

    public static readonly DependencyProperty StylusGestureCommandProperty =
        DependencyProperty.Register(nameof(StylusGestureCommand), typeof(ICommand), typeof(Viewport), new(null));

    public static readonly DependencyProperty StylusOutOfRangeCommandProperty =
        DependencyProperty.Register(nameof(StylusOutOfRangeCommand), typeof(ICommand), typeof(Viewport), new(null));

    public static readonly DependencyProperty MiddleMouseClickedCommandProperty =
        DependencyProperty.Register(nameof(MiddleMouseClickedCommand), typeof(ICommand), typeof(Viewport), new(null));

    public ICommand? MiddleMouseClickedCommand
    {
        get => (ICommand?)GetValue(MiddleMouseClickedCommandProperty);
        set => SetValue(MiddleMouseClickedCommandProperty, value);
    }

    public ICommand? StylusOutOfRangeCommand
    {
        get => (ICommand?)GetValue(StylusOutOfRangeCommandProperty);
        set => SetValue(StylusOutOfRangeCommandProperty, value);
    }

    public ICommand? StylusGestureCommand
    {
        get => (ICommand?)GetValue(StylusGestureCommandProperty);
        set => SetValue(StylusGestureCommandProperty, value);
    }

    public ICommand? StylusButtonUpCommand
    {
        get => (ICommand?)GetValue(StylusButtonUpCommandProperty);
        set => SetValue(StylusButtonUpCommandProperty, value);
    }

    public ICommand? StylusButtonDownCommand
    {
        get => (ICommand?)GetValue(StylusButtonDownCommandProperty);
        set => SetValue(StylusButtonDownCommandProperty, value);
    }

    public bool UseTouchGestures
    {
        get => (bool)GetValue(UseTouchGesturesProperty);
        set => SetValue(UseTouchGesturesProperty, value);
    }

    public ExecutionTrigger<VecI>? CenterViewportTrigger
    {
        get => (ExecutionTrigger<VecI>)GetValue(CenterViewportTriggerProperty);
        set => SetValue(CenterViewportTriggerProperty, value);
    }

    public ExecutionTrigger<double>? ZoomViewportTrigger
    {
        get => (ExecutionTrigger<double>)GetValue(ZoomViewportTriggerProperty);
        set => SetValue(ZoomViewportTriggerProperty, value);
    }

    public bool ZoomOutOnClick
    {
        get => (bool)GetValue(ZoomOutOnClickProperty);
        set => SetValue(ZoomOutOnClickProperty, value);
    }

    public bool GridLinesVisible
    {
        get => (bool)GetValue(GridLinesVisibleProperty);
        set => SetValue(GridLinesVisibleProperty, value);
    }

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

    public double ZoomboxScale
    {
        get => zoombox.Scale;
        // ReSharper disable once ValueParameterNotUsed
        set
        {
            PropertyChanged?.Invoke(this, new(nameof(ReferenceLayerScale)));
        }
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
            Document?.Operations.AddOrUpdateViewport(GetLocation());
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
            Document?.Operations.AddOrUpdateViewport(GetLocation());
        }
    }

    private VecD realDimensions = new(double.MaxValue, double.MaxValue);

    public VecD RealDimensions
    {
        get => realDimensions;
        set
        {
            ChunkResolution oldRes = CalculateResolution();
            realDimensions = value;
            ChunkResolution newRes = CalculateResolution();

            PropertyChanged?.Invoke(this, new(nameof(RealDimensions)));
            Document?.Operations.AddOrUpdateViewport(GetLocation());

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
            ChunkResolution oldRes = CalculateResolution();
            dimensions = value;
            ChunkResolution newRes = CalculateResolution();

            PropertyChanged?.Invoke(this, new(nameof(Dimensions)));
            Document?.Operations.AddOrUpdateViewport(GetLocation());

            if (oldRes != newRes)
                PropertyChanged?.Invoke(this, new(nameof(TargetBitmap)));
        }
    }

    public WriteableBitmap? TargetBitmap
    {
        get
        {
            return Document?.LazyBitmaps.TryGetValue(CalculateResolution(), out WriteableBitmap? value) == true ? value : null;
        }
    }

    public double ReferenceLayerScale =>
        ZoomboxScale * ((Document?.ReferenceLayerViewModel.ReferenceBitmap != null && Document?.ReferenceLayerViewModel.ReferenceShapeBindable != null)
            ? (Document.ReferenceLayerViewModel.ReferenceShapeBindable.RectSize.X / (double)Document.ReferenceLayerViewModel.ReferenceBitmap.PixelWidth)
            : 1);

    public PixiEditor.Zoombox.Zoombox Zoombox => zoombox;

    public Guid GuidValue { get; } = Guid.NewGuid();

    private MouseUpdateController? mouseUpdateController;

    public Viewport()
    {
        InitializeComponent();

        Binding binding = new Binding { Source = this, Path = new PropertyPath($"{nameof(Document)}.{nameof(Document.LazyBitmaps)}") };
        SetBinding(BitmapsProperty, binding);

        MainImage!.Loaded += OnImageLoaded;
        Loaded += OnLoad;
        Unloaded += OnUnload;
    }

    public Image? MainImage => (Image?)((Grid?)((Border?)zoombox.AdditionalContent)?.Child)?.Children[1];
    public Grid BackgroundGrid => mainGrid;

    private void ForceRefreshFinalImage()
    {
        MainImage?.InvalidateVisual();
    }

    private void OnUnload(object sender, RoutedEventArgs e)
    {
        Document?.Operations.RemoveViewport(GuidValue);
        mouseUpdateController?.Dispose();
    }

    private void OnLoad(object sender, RoutedEventArgs e)
    {
        Document?.Operations.AddOrUpdateViewport(GetLocation());
        mouseUpdateController = new MouseUpdateController(this, Image_MouseMove);
    }

    private static void OnDocumentChange(DependencyObject viewportObj, DependencyPropertyChangedEventArgs args)
    {
        DocumentViewModel? oldDoc = (DocumentViewModel?)args.OldValue;
        DocumentViewModel? newDoc = (DocumentViewModel?)args.NewValue;
        Viewport? viewport = (Viewport)viewportObj;
        oldDoc?.Operations.RemoveViewport(viewport.GuidValue);
        newDoc?.Operations.AddOrUpdateViewport(viewport.GetLocation());
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
        return new(Angle, Center, RealDimensions, Dimensions, CalculateResolution(), GuidValue, Delayed, ForceRefreshFinalImage);
    }

    private void OnReferenceImageSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        PropertyChanged?.Invoke(this, new(nameof(ReferenceLayerScale)));
    }

    private void Image_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (MouseDownCommand is null)
            return;
        Point pos = e.GetPosition(MainImage);
        VecD conv = new VecD(pos.X, pos.Y);
        MouseOnCanvasEventArgs? parameter = new MouseOnCanvasEventArgs(e.ChangedButton, conv);

        if (MouseDownCommand.CanExecute(parameter))
            MouseDownCommand.Execute(parameter);
    }

    private void Image_MouseMove(object sender, MouseEventArgs e)
    {
        if (MouseMoveCommand is null)
            return;
        Point pos = e.GetPosition(MainImage);
        VecD conv = new VecD(pos.X, pos.Y);

        if (MouseMoveCommand.CanExecute(conv))
            MouseMoveCommand.Execute(conv);
    }

    private void Image_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (MouseUpCommand is null)
            return;
        if (MouseUpCommand.CanExecute(e.ChangedButton))
            MouseUpCommand.Execute(e.ChangedButton);
    }

    private void CenterZoomboxContent(object? sender, VecI args)
    {
        zoombox.CenterContent(args);
    }

    private void ZoomZoomboxContent(object? sender, double delta)
    {
        zoombox.ZoomIntoCenter(delta);
    }

    private void OnImageLoaded(object sender, EventArgs e)
    {
        zoombox.CenterContent();
    }
    
    private void ResetViewportClicked(object sender, RoutedEventArgs e)
    {
        zoombox.AngleRadians = 0;
        zoombox.CenterContent();
    }

    private static void CenterViewportTriggerChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        Viewport? viewport = (Viewport)sender;
        if (args.OldValue != null)
            ((ExecutionTrigger<VecI>)args.OldValue).Triggered -= viewport.CenterZoomboxContent;
        if (args.NewValue != null)
            ((ExecutionTrigger<VecI>)args.NewValue).Triggered += viewport.CenterZoomboxContent;
    }

    private static void ZoomViewportTriggerChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        Viewport? viewport = (Viewport)sender;
        if (args.OldValue != null)
            ((ExecutionTrigger<double>)args.OldValue).Triggered -= viewport.ZoomZoomboxContent;
        if (args.NewValue != null)
            ((ExecutionTrigger<double>)args.NewValue).Triggered += viewport.ZoomZoomboxContent;
    }

    private void Grid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (MiddleMouseClickedCommand is null)
            return;
        if (Mouse.MiddleButton == MouseButtonState.Pressed && MiddleMouseClickedCommand.CanExecute(null))
            MiddleMouseClickedCommand.Execute(null);
    }
}
