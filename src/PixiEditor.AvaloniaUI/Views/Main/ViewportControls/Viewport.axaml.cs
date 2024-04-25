using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Core;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using PixiEditor.AvaloniaUI.Helpers.UI;
using PixiEditor.AvaloniaUI.Models.Controllers.InputDevice;
using PixiEditor.AvaloniaUI.Models.DocumentModels;
using PixiEditor.AvaloniaUI.Models.Position;
using PixiEditor.AvaloniaUI.ViewModels.Document;
using PixiEditor.AvaloniaUI.Views.Visuals;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.Zoombox;
using Point = Avalonia.Point;

namespace PixiEditor.AvaloniaUI.Views.Main.ViewportControls;

#nullable enable
internal partial class Viewport : UserControl, INotifyPropertyChanged
{
    //TODO: IDK where to write this, but on close zoom level, when I drag line handle, it doesn't update the canvas
    public event PropertyChangedEventHandler? PropertyChanged;

    public static readonly StyledProperty<bool> FlipXProperty =
        AvaloniaProperty.Register<Viewport, bool>(nameof(FlipX), false);

    public static readonly StyledProperty<bool> FlipYProperty =
        AvaloniaProperty.Register<Viewport, bool>(nameof(FlipY), false);

    public static readonly StyledProperty<ZoomboxMode> ZoomModeProperty =
        AvaloniaProperty.Register<Viewport, ZoomboxMode>(nameof(ZoomMode), ZoomboxMode.Normal);

    public static readonly StyledProperty<DocumentViewModel> DocumentProperty =
        AvaloniaProperty.Register<Viewport, DocumentViewModel>(nameof(Document), null);

    public static readonly StyledProperty<ICommand> MouseDownCommandProperty =
        AvaloniaProperty.Register<Viewport, ICommand>(nameof(MouseDownCommand), null);

    public static readonly StyledProperty<ICommand> MouseMoveCommandProperty =
        AvaloniaProperty.Register<Viewport, ICommand>(nameof(MouseMoveCommand), null);

    public static readonly StyledProperty<ICommand> MouseUpCommandProperty =
        AvaloniaProperty.Register<Viewport, ICommand>(nameof(MouseUpCommand), null);

    public static readonly StyledProperty<bool> DelayedProperty =
        AvaloniaProperty.Register<Viewport, bool>(nameof(Delayed), false);

    public static readonly StyledProperty<bool> GridLinesVisibleProperty =
        AvaloniaProperty.Register<Viewport, bool>(nameof(GridLinesVisible), false);

    public static readonly StyledProperty<bool> ZoomOutOnClickProperty =
        AvaloniaProperty.Register<Viewport, bool>(nameof(ZoomOutOnClick), false);

    public static readonly StyledProperty<ExecutionTrigger<double>> ZoomViewportTriggerProperty =
        AvaloniaProperty.Register<Viewport, ExecutionTrigger<double>>(nameof(ZoomViewportTrigger), null);

    public static readonly StyledProperty<ExecutionTrigger<VecI>> CenterViewportTriggerProperty =
        AvaloniaProperty.Register<Viewport, ExecutionTrigger<VecI>>(nameof(CenterViewportTrigger), null);

    public static readonly StyledProperty<bool> UseTouchGesturesProperty =
        AvaloniaProperty.Register<Viewport, bool>(nameof(UseTouchGestures), false);

    public static readonly StyledProperty<ICommand> StylusButtonDownCommandProperty =
        AvaloniaProperty.Register<Viewport, ICommand>(nameof(StylusButtonDownCommand), null);

    public static readonly StyledProperty<ICommand> StylusButtonUpCommandProperty =
        AvaloniaProperty.Register<Viewport, ICommand>(nameof(StylusButtonUpCommand), null);

    public static readonly StyledProperty<ICommand> StylusGestureCommandProperty =
        AvaloniaProperty.Register<Viewport, ICommand>(nameof(StylusGestureCommand), null);

    public static readonly StyledProperty<ICommand> StylusOutOfRangeCommandProperty =
        AvaloniaProperty.Register<Viewport, ICommand>(nameof(StylusOutOfRangeCommand), null);

    public static readonly StyledProperty<ICommand> MiddleMouseClickedCommandProperty =
        AvaloniaProperty.Register<Viewport, ICommand>(nameof(MiddleMouseClickedCommand), null);

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
        get => zoombox?.Scale ?? 1;
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

    public Surface? TargetBitmap
    {
        get
        {
            return Document?.Surfaces.TryGetValue(CalculateResolution(), out Surface? value) == true ? value : null;
        }
    }

    public double ReferenceLayerScale =>
        ZoomboxScale * ((Document?.ReferenceLayerViewModel.ReferenceBitmap != null && Document?.ReferenceLayerViewModel.ReferenceShapeBindable != null)
            ? (Document.ReferenceLayerViewModel.ReferenceShapeBindable.RectSize.X / (double)Document.ReferenceLayerViewModel.ReferenceBitmap.Size.X)
            : 1);

    public PixiEditor.Zoombox.Zoombox Zoombox => zoombox;

    public Guid GuidValue { get; } = Guid.NewGuid();

    private MouseUpdateController? mouseUpdateController;

    static Viewport()
    {
        DocumentProperty.Changed.Subscribe(OnDocumentChange);
        ZoomViewportTriggerProperty.Changed.Subscribe(ZoomViewportTriggerChanged);
        CenterViewportTriggerProperty.Changed.Subscribe(CenterViewportTriggerChanged);
    }

    public Viewport()
    {
        InitializeComponent();

        MainImage!.Loaded += OnImageLoaded;
        MainImage.SizeChanged += OnMainImageSizeChanged;
        Loaded += OnLoad;
        Unloaded += OnUnload;

        //TODO: It's weird that I had to do it this way, right click didn't raise Image_MouseUp otherwise.
        viewportGrid.AddHandler(PointerReleasedEvent, Image_MouseUp, RoutingStrategies.Tunnel);
        viewportGrid.AddHandler(PointerPressedEvent, Image_MouseDown, RoutingStrategies.Bubble);
    }

    public Panel? MainImage => zoombox != null ? (Panel?)((Grid?)((Border?)zoombox.AdditionalContent)?.Child)?.Children[0] : null;
    public Scene Scene => scene;
    public Grid BackgroundGrid => viewportGrid;

    private void ForceRefreshFinalImage()
    {
        Scene.InvalidateVisual();
        MainImage?.InvalidateVisual();
    }

    private void OnUnload(object? sender, RoutedEventArgs e)
    {
        Document?.Operations.RemoveViewport(GuidValue);
        mouseUpdateController?.Dispose();
    }

    private void OnLoad(object? sender, RoutedEventArgs e)
    {
        InitializeOverlays();
        Document?.Operations.AddOrUpdateViewport(GetLocation());
        mouseUpdateController = new MouseUpdateController(this, Image_MouseMove);
    }

    private void InitializeOverlays()
    {
        /*brushShapeOverlay.MouseEventSource = BackgroundGrid;
        brushShapeOverlay.MouseReference = MainImage;
        brushShapeOverlay.Initialize();*/
    }

    private static void OnDocumentChange(AvaloniaPropertyChangedEventArgs<DocumentViewModel> e)
    {
        Viewport? viewport = (Viewport)e.Sender;

        DocumentViewModel? oldDoc = e.OldValue.Value;
        if (oldDoc != null)
        {
            oldDoc.SizeChanged -= viewport.OnImageSizeChanged;
        }
        DocumentViewModel? newDoc = e.NewValue.Value;
        if (newDoc != null)
        {
            newDoc.SizeChanged += viewport.OnImageSizeChanged;
        }

        oldDoc?.Operations.RemoveViewport(viewport.GuidValue);
        newDoc?.Operations.AddOrUpdateViewport(viewport.GetLocation());
    }

    private void OnImageSizeChanged(object? sender, DocumentSizeChangedEventArgs e)
    {
        PropertyChanged?.Invoke(this, new(nameof(TargetBitmap)));
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

    private void OnReferenceImageSizeChanged(object? sender, SizeChangedEventArgs sizeChangedEventArgs)
    {
        PropertyChanged?.Invoke(this, new(nameof(ReferenceLayerScale)));
    }

    private void Image_MouseDown(object? sender, PointerPressedEventArgs e)
    {
        bool isMiddle = e.GetCurrentPoint(this).Properties.IsMiddleButtonPressed;
        HandleMiddleMouse(isMiddle);

        if (MouseDownCommand is null)
            return;

        MouseButton mouseButton = e.GetCurrentPoint(this).Properties.PointerUpdateKind switch
        {
            PointerUpdateKind.LeftButtonPressed => MouseButton.Left,
            PointerUpdateKind.RightButtonPressed => MouseButton.Right,
            _ => MouseButton.Middle
        };

        Point pos = e.GetPosition(MainImage);
        VecD conv = new VecD(pos.X, pos.Y);
        MouseOnCanvasEventArgs? parameter = new MouseOnCanvasEventArgs(mouseButton, conv);

        if (MouseDownCommand.CanExecute(parameter))
            MouseDownCommand.Execute(parameter);
    }

    private void Image_MouseMove(PointerEventArgs e)
    {
        if (MouseMoveCommand is null)
            return;
        Point pos = e.GetPosition(MainImage);
        VecD conv = new VecD(pos.X, pos.Y);

        MouseButton mouseButton = e.GetCurrentPoint(this).Properties.PointerUpdateKind switch
        {
            PointerUpdateKind.LeftButtonPressed => MouseButton.Left,
            PointerUpdateKind.RightButtonPressed => MouseButton.Right,
            _ => MouseButton.Middle
        };

        MouseOnCanvasEventArgs parameter = new(mouseButton, conv);

        if (MouseMoveCommand.CanExecute(parameter))
            MouseMoveCommand.Execute(parameter);
    }

    private void Image_MouseUp(object? sender, PointerReleasedEventArgs e)
    {
        if (MouseUpCommand is null)
            return;

        Point pos = e.GetPosition(MainImage);
        MouseOnCanvasEventArgs parameter = new(e.InitialPressMouseButton, new VecD(pos.X, pos.Y));
        if (MouseUpCommand.CanExecute(parameter))
            MouseUpCommand.Execute(parameter);
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

    private void OnMainImageSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (zoombox.Dimensions is { X: 0, Y: 0 }) return;
        zoombox.CenterContent(new VecD(e.NewSize.Width, e.NewSize.Height));
    }
    
    private void ResetViewportClicked(object sender, RoutedEventArgs e)
    {
        zoombox.Angle = 0;
        zoombox.CenterContent();
    }

    private static void CenterViewportTriggerChanged(AvaloniaPropertyChangedEventArgs<ExecutionTrigger<VecI>> e)
    {
        Viewport? viewport = (Viewport)e.Sender;
        if (e.OldValue.Value != null)
            e.OldValue.Value.Triggered -= viewport.CenterZoomboxContent;
        if (e.NewValue.Value != null)
            e.NewValue.Value.Triggered += viewport.CenterZoomboxContent;
    }

    private static void ZoomViewportTriggerChanged(AvaloniaPropertyChangedEventArgs<ExecutionTrigger<double>> e)
    {
        Viewport? viewport = (Viewport)e.Sender;
        if (e.OldValue.Value != null)
            e.OldValue.Value.Triggered -= viewport.ZoomZoomboxContent;
        if (e.NewValue.Value != null)
            e.NewValue.Value.Triggered += viewport.ZoomZoomboxContent;
    }

    private void HandleMiddleMouse(bool isMiddle)
    {
        if (MiddleMouseClickedCommand is null)
            return;
        if (isMiddle && MiddleMouseClickedCommand.CanExecute(null))
            MiddleMouseClickedCommand.Execute(null);
    }
}
